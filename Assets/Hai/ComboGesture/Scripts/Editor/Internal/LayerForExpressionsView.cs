﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForExpressionsView
    {
        private readonly FeatureToggles _featuresToggles;
        private readonly AvatarMask _expressionsAvatarMask;
        private readonly AnimationClip _emptyClip;
        private readonly string _activityStageName;
        private readonly ConflictPrevention _conflictPrevention;
        private readonly AssetContainer _assetContainer;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly AnimationClip _compilerIgnoreParamList;
        private readonly AnimationClip _compilerFallbackParamList;
        private readonly AnimatorController _animatorController;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;
        private readonly List<ManifestBinding> _manifestBindings;

        public LayerForExpressionsView(FeatureToggles featuresToggles,
            AvatarMask expressionsAvatarMask,
            AnimationClip emptyClip,
            string activityStageName,
            ConflictPrevention conflictPrevention,
            AssetContainer assetContainer,
            ConflictFxLayerMode compilerConflictFxLayerMode,
            AnimationClip compilerIgnoreParamList,
            AnimationClip compilerFallbackParamList,
            AnimatorController animatorController,
            bool useGestureWeightCorrection,
            bool useSmoothing,
            List<ManifestBinding> manifestBindings)
        {
            _featuresToggles = featuresToggles;
            _expressionsAvatarMask = expressionsAvatarMask;
            _emptyClip = emptyClip;
            _activityStageName = activityStageName;
            _conflictPrevention = conflictPrevention;
            _assetContainer = assetContainer;
            _compilerConflictFxLayerMode = compilerConflictFxLayerMode;
            _compilerIgnoreParamList = compilerIgnoreParamList;
            _compilerFallbackParamList = compilerFallbackParamList;
            _animatorController = animatorController;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _manifestBindings = manifestBindings;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Generating animations", 0f);

            var activityManifests = _manifestBindings;
            var animationNeutralizer = new AnimationNeutralizer(
                activityManifests,
                _compilerConflictFxLayerMode,
                _compilerIgnoreParamList,
                _compilerFallbackParamList,
                _assetContainer,
                _conflictPrevention.ShouldGenerateExhaustiveAnimations,
                _emptyClip,
                Feature(FeatureToggles.DoNotFixSingleKeyframes)
            );

            var avatarMaskNullable = animationNeutralizer.GenerateAvatarMaskInsideContainerIfApplicableOrNull();

            var layer = ReinitializeLayer(avatarMaskNullable);

            var defaultState = layer.NewState("Default")
                .WithWriteDefaultsSetTo(_conflictPrevention.ShouldWriteDefaults);

            activityManifests = animationNeutralizer.NeutralizeManifestAnimationsInsideContainer();

            AssetContainer.GlobalSave();

            // The blend tree auto weight corrector assumes that all of the Manifests' blend trees have been autogenerated.
            // This remains true as long as the Animation Neutralizer's "NeutralizeManifestAnimationsInsideContainer" function is executed before this.
            activityManifests = new CgeBlendTreeAutoWeightCorrector(activityManifests, _useGestureWeightCorrection, _useSmoothing, _assetContainer)
                .MutateAndCorrectExistingBlendTrees();

            foreach (var parameter in AllParametersUsedByManifests(activityManifests))
            {
                layer.FloatParameter(parameter);
            }

            var combinator = new IntermediateCombinator(activityManifests);

            new GestureCExpressionCombiner(_assetContainer,
                layer,
                combinator.ComposedBehaviours,
                _activityStageName,
                _conflictPrevention.ShouldWriteDefaults,
                _useGestureWeightCorrection,
                _useSmoothing,
                defaultState
            ).Populate();
        }

        private static List<string> AllParametersUsedByManifests(List<ManifestBinding> activityManifests)
        {
            return AllParametersUsedByBlendTrees(activityManifests)
                .Concat(AllParametersUsedByMassiveBlends(activityManifests))
                .ToList();
        }

        private static List<string> AllParametersUsedByBlendTrees(List<ManifestBinding> activityManifests)
        {
            return activityManifests
                .SelectMany(binding => binding.Manifest.AllBlendTreesFoundRecursively())
                .SelectMany(FindParametersOfBlendTree)
                .Distinct()
                .ToList();
        }

        private static List<string> AllParametersUsedByMassiveBlends(List<ManifestBinding> activityManifests)
        {
            return activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Massive)
                .SelectMany(binding =>
                {
                    var manifest = (MassiveBlendManifest)binding.Manifest;
                    switch (manifest.Mode)
                    {
                        case CgeMassiveBlendMode.Simple:
                        case CgeMassiveBlendMode.TwoDirections:
                            return new List<string> { manifest.SimpleParameterName };
                        case CgeMassiveBlendMode.ComplexBlendTree:
                            return FindParametersOfBlendTree(manifest.BlendTree);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                })
                .Distinct()
                .ToList();
        }

        private static IEnumerable<string> FindParametersOfBlendTree(BlendTree tree)
        {
            switch (tree.blendType)
            {
                case BlendTreeType.Simple1D:
                    return new List<string> {tree.blendParameter};
                case BlendTreeType.SimpleDirectional2D:
                    return new List<string> {tree.blendParameter, tree.blendParameterY};
                case BlendTreeType.FreeformDirectional2D:
                    return new List<string> {tree.blendParameter, tree.blendParameterY};
                case BlendTreeType.FreeformCartesian2D:
                    return new List<string> {tree.blendParameter, tree.blendParameterY};
                case BlendTreeType.Direct:
                    return tree.children.Select(motion => motion.directBlendParameter).ToList();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgeAacFlLayer ReinitializeLayer(AvatarMask avatarMaskNullable)
        {
            return _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureExp")
                .WithAvatarMask(avatarMaskNullable != null ? avatarMaskNullable : _expressionsAvatarMask);
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }
    }
}
