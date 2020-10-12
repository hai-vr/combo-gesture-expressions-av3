﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Infra;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboGestureCompilerInternal
    {
        private const string EmptyClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_EmptyClip.anim";

        public const string GestureLeft = "GestureLeft";
        public const string GestureRight = "GestureRight";

        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly AnimatorController _animatorController;
        private readonly AnimationClip _customEmptyClip;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly FeatureToggles _featuresToggles;
        private readonly ConflictPrevention _conflictPrevention;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly AnimationClip _compilerIgnoreParamList;
        private readonly AnimationClip _compilerFallbackParamList;
        private readonly VRCAvatarDescriptor _avatarDescriptor;
        private readonly AvatarMask _expressionsAvatarMask;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly bool _integrateLimitedLipsync;
        private readonly ComboGestureLimitedLipsync _limitedLipsync;
        private readonly bool _doNotIncludeBlinkBlendshapes;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;

        public ComboGestureCompilerInternal(
            ComboGestureCompiler compiler,
            AssetContainer assetContainer)
        {
            _activityStageName = compiler.activityStageName == "" ? null : compiler.activityStageName;
            _comboLayers = compiler.comboLayers;
            _animatorController = (AnimatorController)compiler.animatorController;
            _customEmptyClip = compiler.customEmptyClip;
            _analogBlinkingUpperThreshold = compiler.analogBlinkingUpperThreshold;
            _featuresToggles = (compiler.exposeDisableExpressions ? FeatureToggles.ExposeDisableExpressions : 0)
                               | (compiler.exposeDisableBlinkingOverride ? FeatureToggles.ExposeDisableBlinkingOverride : 0)
                               | (compiler.exposeAreEyesClosed ? FeatureToggles.ExposeAreEyesClosed : 0)
                               | (compiler.doNotGenerateControllerLayer ? FeatureToggles.DoNotGenerateControllerLayer : 0)
                               | (compiler.doNotGenerateBlinkingOverrideLayer ? FeatureToggles.DoNotGenerateBlinkingOverrideLayer : 0)
                               | (compiler.doNotGenerateLipsyncOverrideLayer ? FeatureToggles.DoNotGenerateLipsyncOverrideLayer : 0)
                               | (compiler.doNotGenerateWeightCorrectionLayer ? FeatureToggles.DoNotGenerateWeightCorrectionLayer : 0);
            _conflictPrevention = ConflictPrevention.Of(compiler.conflictPreventionMode);
            _compilerConflictFxLayerMode = compiler.conflictFxLayerMode;
            _compilerIgnoreParamList = compiler.ignoreParamList;
            _compilerFallbackParamList = compiler.fallbackParamList;
            _avatarDescriptor = compiler.avatarDescriptor;
            _expressionsAvatarMask = compiler.expressionsAvatarMask;
            _logicalAvatarMask = compiler.logicalAvatarMask;
            _weightCorrectionAvatarMask = compiler.weightCorrectionAvatarMask;
            _integrateLimitedLipsync = compiler.integrateLimitedLipsync;
            _limitedLipsync = compiler.lipsyncForWideOpenMouth;
            _doNotIncludeBlinkBlendshapes = !compiler.WillUseBlinkBlendshapeCorrection();
            _animatorGenerator = new AnimatorGenerator(_animatorController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = compiler.WillUseGestureWeightCorrection();
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            var emptyClip = GetOrCreateEmptyClip();

            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }

            if (!Feature(FeatureToggles.DoNotGenerateControllerLayer))
            {
                CreateOrReplaceController(emptyClip);
            }

            if (!Feature(FeatureToggles.DoNotGenerateWeightCorrectionLayer))
            {
                if (_useGestureWeightCorrection)
                {
                    CreateOrReplaceWeightCorrection(emptyClip);
                }
                else
                {
                    DeleteWeightCorrection();
                }
            }

            CreateOrReplaceExpressionsView(emptyClip);

            if (!Feature(FeatureToggles.DoNotGenerateBlinkingOverrideLayer))
            {
                CreateOrReplaceBlinkingOverrideView(emptyClip);
            }

            if (!Feature(FeatureToggles.DoNotGenerateLipsyncOverrideLayer))
            {
                if (_integrateLimitedLipsync && _avatarDescriptor != null && _avatarDescriptor.VisemeSkinnedMesh != null)
                {
                    CreateOrReplaceLipsyncOverrideView(emptyClip);
                }
                else
                {
                    DeleteLipsyncOverrideView();
                }
            }

            ReapAnimator();

            var isAssetRefreshingRequired = _conflictPrevention.ShouldGenerateAnimations;
            if (isAssetRefreshingRequired)
            {
                AssetDatabase.Refresh();
            }
            EditorUtility.ClearProgressBar();
        }

        private void ReapAnimator()
        {
            if (AssetDatabase.GetAssetPath(_animatorController) == "")
            {
                return;
            }

            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_animatorController));

            var reachableMotions = ConcatStateMachines()
                .SelectMany(machine => machine.states)
                .Select(state => state.state.motion)
                .ToList<Object>();
            Reap(allSubAssets, typeof(BlendTree), reachableMotions, o => o.name.StartsWith("autoBT_"));
        }

        private IEnumerable<AnimatorStateMachine> ConcatStateMachines()
        {
            return _animatorController.layers.Select(layer => layer.stateMachine)
                .Concat(_animatorController.layers.SelectMany(layer => layer.stateMachine.stateMachines).Select(machine => machine.stateMachine));
        }

        private static void Reap(Object[] allAssets, Type type, List<Object> existingAssets, Predicate<Object> predicate)
        {
            foreach (var o in allAssets)
            {
                if (o != null && (o.GetType() == type || o.GetType().IsSubclassOf(type)) && !existingAssets.Contains(o) && predicate.Invoke(o))
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        private AnimationClip GetOrCreateEmptyClip()
        {
            var emptyClip = _customEmptyClip;
            if (emptyClip == null)
            {
                emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EmptyClipPath);
            }
            if (emptyClip == null)
            {
                emptyClip = GenerateEmptyClipAsset();
            }

            return emptyClip;
        }

        private static AnimationClip GenerateEmptyClipAsset()
        {
            var emptyClip = new AnimationClip();
            var settings = AnimationUtility.GetAnimationClipSettings(emptyClip);
            settings.loopTime = false;
            Keyframe[] keyframes = {new Keyframe(0, 0), new Keyframe(1 / 60f, 0)};
            var curve = new AnimationCurve(keyframes);
            emptyClip.SetCurve("_ignored", typeof(GameObject), "m_IsActive", curve);

            if (!AssetDatabase.IsValidFolder("Assets/Hai"))
            {
                AssetDatabase.CreateFolder("Assets", "Hai");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Hai/ComboGesture"))
            {
                AssetDatabase.CreateFolder("Assets/Hai", "ComboGesture");
            }

            AssetDatabase.CreateAsset(emptyClip, EmptyClipPath);
            return emptyClip;
        }

        private void CreateOrReplaceController(AnimationClip emptyClip)
        {
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboParamName, AnimatorControllerParameterType.Int);
            new LayerForController(_animatorGenerator, _logicalAvatarMask, emptyClip).Create();
        }

        private void CreateOrReplaceWeightCorrection(AnimationClip emptyClip)
        {
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboLeftWeightProxy, AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboRightWeightProxy, AnimatorControllerParameterType.Float);
            new LayerForWeightCorrection(_animatorGenerator, _weightCorrectionAvatarMask, emptyClip).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip)
        {
            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboParamName, AnimatorControllerParameterType.Int);
            if (Feature(FeatureToggles.ExposeDisableExpressions))
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboDisableExpressionsParamName, AnimatorControllerParameterType.Int);
            }
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimBlink", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimLSWide", AnimatorControllerParameterType.Float);
            new LayerForExpressionsView(
                _featuresToggles,
                _animatorGenerator,
                _expressionsAvatarMask,
                emptyClip,
                _activityStageName,
                _conflictPrevention,
                _assetContainer,
                _compilerConflictFxLayerMode,
                _compilerIgnoreParamList,
                _compilerFallbackParamList,
                _doNotIncludeBlinkBlendshapes
                    ? new List<CurveKey>()
                    : new BlendshapesFinder(_avatarDescriptor).FindBlink().Select(key => key.AsCurveKey()).ToList(),
                _animatorController,
                _comboLayers,
                _useGestureWeightCorrection
            ).Create();
        }

        private void CreateOrReplaceBlinkingOverrideView(AnimationClip emptyClip)
        {
            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeDisableBlinkingOverride))
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboDisableBlinkingOverrideParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeAreEyesClosed))
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboAreEyesClosedParamName, AnimatorControllerParameterType.Int);
            }
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimBlink", AnimatorControllerParameterType.Float);
            new LayerForBlinkingOverrideView(
                _activityStageName,
                _comboLayers,
                _analogBlinkingUpperThreshold,
                _featuresToggles,
                _logicalAvatarMask,
                _animatorGenerator,
                emptyClip,
                _useGestureWeightCorrection
            ).Create();
        }

        private void CreateOrReplaceLipsyncOverrideView(AnimationClip emptyClip)
        {
            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeIsLipsyncLimited))
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboIsLipsyncLimitedParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeDisableLipsyncOverride))
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, SharedLayerUtils.HaiGestureComboDisableLipsyncOverrideParamName, AnimatorControllerParameterType.Int);
            }
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "Viseme", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimLSWide", AnimatorControllerParameterType.Float);
            new LayerForLipsyncOverrideView(
                _activityStageName,
                _comboLayers,
                _analogBlinkingUpperThreshold,
                _featuresToggles,
                _logicalAvatarMask,
                _animatorGenerator,
                _avatarDescriptor,
                _limitedLipsync,
                _assetContainer,
                emptyClip,
                _useGestureWeightCorrection
            ).Create();
        }

        private void DeleteLipsyncOverrideView()
        {
            LayerForLipsyncOverrideView.Delete(_animatorGenerator);
        }

        private void DeleteWeightCorrection()
        {
            LayerForWeightCorrection.Delete(_animatorGenerator);
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }
    }

    class ActivityManifest
    {
        public int StageValue { get; }
        public PermutationManifest Manifest { get; }
        public int LayerOrdinal { get; }

        public ActivityManifest(int stageValue, PermutationManifest manifest, int layerOrdinal)
        {
            StageValue = stageValue;
            Manifest = manifest;
            LayerOrdinal = layerOrdinal;
        }
    }
}
