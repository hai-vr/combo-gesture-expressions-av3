﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class CgeBlendTreeAutoWeightCorrector : List<ManifestBinding>
    {
        public const string AutoGestureWeightParam = "_AutoGestureWeight";
        private readonly List<ManifestBinding> _activityManifests;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;
        private readonly AssetContainer _assetContainer;

        public CgeBlendTreeAutoWeightCorrector(List<ManifestBinding> activityManifests, bool useGestureWeightCorrection, bool useSmoothing, AssetContainer assetContainer)
        {
            _activityManifests = activityManifests;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _assetContainer = assetContainer;
        }

        public List<ManifestBinding> MutateAndCorrectExistingBlendTrees()
        {
            var mappings = _activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Permutation || binding.Manifest.Kind() == ManifestKind.Massive)
                .SelectMany(binding => binding.Manifest.AllBlendTreesFoundRecursively())
                .Distinct()
                .Where(tree =>
                {
                    switch (tree.blendType)
                    {
                        case BlendTreeType.Simple1D:
                            return tree.blendParameter == AutoGestureWeightParam;
                        case BlendTreeType.SimpleDirectional2D:
                        case BlendTreeType.FreeformDirectional2D:
                        case BlendTreeType.FreeformCartesian2D:
                            return tree.blendParameter == AutoGestureWeightParam || tree.blendParameterY == AutoGestureWeightParam;
                        case BlendTreeType.Direct:
                            return tree.children.Any(motion => motion.directBlendParameter == AutoGestureWeightParam);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                })
                .Select(originalTree =>
                {
                    var newTreeForLeftSide = CopyTreeIdentically(originalTree, Side.Left);
                    var newTreeForRightSide = CopyTreeIdentically(originalTree, Side.Right);
                    _assetContainer.AddBlendTree(newTreeForLeftSide);
                    _assetContainer.AddBlendTree(newTreeForRightSide);
                    return new AutoWeightTreeMapping(originalTree, newTreeForLeftSide, newTreeForRightSide);
                })
                .ToDictionary(mapping => mapping.Original, mapping => mapping);


            return _activityManifests
                .Select(binding =>
                {
                    if (binding.Manifest.Kind() != ManifestKind.Permutation && binding.Manifest.Kind() != ManifestKind.Massive)
                    {
                        return binding;
                    }

                    return RemapManifest(binding, mappings);
                }).ToList();
        }

        private static ManifestBinding RemapManifest(ManifestBinding manifestBinding, Dictionary<BlendTree, AutoWeightTreeMapping> autoWeightRemapping)
        {
            var remappedManifest = manifestBinding.Manifest.UsingRemappedWeights(autoWeightRemapping);
            return new ManifestBinding(manifestBinding.StageValue, remappedManifest, manifestBinding.LayerOrdinal);
        }

        private BlendTree CopyTreeIdentically(BlendTree originalTree, Side side)
        {
            var newTree = new BlendTree();

            // Object.Instantiate(...) is triggering some weird issues about assertions failures.
            // Copy the blend tree manually
            newTree.name = "zAutogeneratedPup_" + originalTree.name + "_DO_NOT_EDIT";
            newTree.blendType = originalTree.blendType;
            newTree.blendParameter = HandleWeightCorrection(
                RemapAutoWeightOrElse(side, originalTree.blendParameter)
            );
            newTree.blendParameterY = HandleWeightCorrection(
                RemapAutoWeightOrElse(side, originalTree.blendParameterY)
            );
            newTree.minThreshold = originalTree.minThreshold;
            newTree.maxThreshold = originalTree.maxThreshold;
            newTree.useAutomaticThresholds = originalTree.useAutomaticThresholds;

            var copyOfChildren = originalTree.children;
            while (newTree.children.Length > 0) {
                newTree.RemoveChild(0);
            }

            newTree.children = copyOfChildren
                .Select(childMotion => new ChildMotion
                {
                    motion = childMotion.motion,
                    threshold = childMotion.threshold,
                    position = childMotion.position,
                    timeScale = childMotion.timeScale,
                    cycleOffset = childMotion.cycleOffset,
                    directBlendParameter = HandleWeightCorrection(RemapAutoWeightOrElse(side, childMotion.directBlendParameter)),
                    mirror = childMotion.mirror
                })
                .ToArray();

            return newTree;
        }

        private static string RemapAutoWeightOrElse(Side side, string originalParameterName)
        {
            if (originalParameterName == AutoGestureWeightParam)
            {
                return side == Side.Left ? "GestureLeftWeight" : "GestureRightWeight";
            }

            return originalParameterName;
        }

        private string HandleWeightCorrection(string originalTreeBlendParameter)
        {
            // FIXME this is duplicate code
            if (!_useGestureWeightCorrection)
            {
                return originalTreeBlendParameter;
            }

            switch (originalTreeBlendParameter)
            {
                case "GestureLeftWeight":
                    return _useSmoothing ? SharedLayerUtils.HaiGestureComboLeftWeightSmoothing : SharedLayerUtils.HaiGestureComboLeftWeightProxy;
                case "GestureRightWeight":
                    return _useSmoothing ? SharedLayerUtils.HaiGestureComboRightWeightSmoothing : SharedLayerUtils.HaiGestureComboRightWeightProxy;
                default:
                    return originalTreeBlendParameter;
            }
        }
    }

    internal enum Side
    {
        Left, Right
    }
}
