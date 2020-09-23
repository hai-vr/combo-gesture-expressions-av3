using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboGestureCompilerInternal
    {
        private const string EmptyClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_EmptyClip.anim";

        public const string GestureLeftWeight = "GestureLeftWeight";
        public const string GestureRightWeight = "GestureRightWeight";
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
        private readonly AvatarMask _expressionsAvatarMask;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly List<CurveKey> _blinkBlendshapes;
        private readonly AssetContainer _assetContainer;

        public ComboGestureCompilerInternal(string activityStageName,
            List<GestureComboStageMapper> comboLayers,
            RuntimeAnimatorController animatorController,
            AnimationClip customEmptyClip,
            float analogBlinkingUpperThreshold,
            FeatureToggles featuresToggles,
            ConflictPreventionMode compilerConflictPreventionMode,
            ConflictFxLayerMode compilerConflictFxLayerMode,
            AnimationClip compilerIgnoreParamList,
            AnimationClip compilerFallbackParamList,
            List<CurveKey> blinkBlendshapes,
            AvatarMask expressionsAvatarMask,
            AvatarMask logicalAvatarMask,
            AssetContainer assetContainer)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _animatorController = (AnimatorController) animatorController;
            _customEmptyClip = customEmptyClip;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _featuresToggles = featuresToggles;
            _conflictPrevention = ConflictPrevention.Of(compilerConflictPreventionMode);
            _compilerConflictFxLayerMode = compilerConflictFxLayerMode;
            _compilerIgnoreParamList = compilerIgnoreParamList;
            _compilerFallbackParamList = compilerFallbackParamList;
            _expressionsAvatarMask = expressionsAvatarMask;
            _logicalAvatarMask = logicalAvatarMask;
            _blinkBlendshapes = blinkBlendshapes;
            _animatorGenerator = new AnimatorGenerator(_animatorController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));
            _assetContainer = assetContainer;
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Starting", 0f);
            var emptyClip = GetOrCreateEmptyClip();

            CreateParameters();

            if (!Feature(FeatureToggles.DoNotGenerateControllerLayer))
            {
                CreateOrReplaceController(emptyClip);
            }

            CreateOrReplaceExpressionsView(emptyClip);

            if (!Feature(FeatureToggles.DoNotGenerateBlinkingOverrideLayer))
            {
                CreateOrReplaceBlinkingOverrideView(emptyClip);
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

        private void CreateParameters()
        {
            CreateParamIfNotExists("GestureLeft", AnimatorControllerParameterType.Int);
            CreateParamIfNotExists("GestureRight", AnimatorControllerParameterType.Int);
            CreateParamIfNotExists("GestureLeftWeight", AnimatorControllerParameterType.Float);
            CreateParamIfNotExists("GestureRightWeight", AnimatorControllerParameterType.Float);
            CreateParamIfNotExists(SharedLayerUtils.HaiGestureComboParamName, AnimatorControllerParameterType.Int);
            if (Feature(FeatureToggles.ExposeDisableExpressions)) {
                CreateParamIfNotExists(SharedLayerUtils.HaiGestureComboDisableExpressionsParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeDisableBlinkingOverride)) {
                CreateParamIfNotExists(SharedLayerUtils.HaiGestureComboDisableBlinkingOverrideParamName, AnimatorControllerParameterType.Int);
            }
            if (Feature(FeatureToggles.ExposeAreEyesClosed)) {
                CreateParamIfNotExists(SharedLayerUtils.HaiGestureComboAreEyesClosed, AnimatorControllerParameterType.Int);
            }

            if (_activityStageName != null)
            {
                CreateParamIfNotExists(_activityStageName, AnimatorControllerParameterType.Int);
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

        private void CreateParamIfNotExists(string paramName,
            AnimatorControllerParameterType type)
        {
            if (_animatorController.parameters.FirstOrDefault(param => param.name == paramName) == null)
            {
                _animatorController.AddParameter(paramName, type);
            }
        }

        private void CreateOrReplaceController(AnimationClip emptyClip)
        {
            new LayerForController(_animatorGenerator, _logicalAvatarMask, emptyClip).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip)
        {
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
                _blinkBlendshapes,
                _animatorController,
                _comboLayers
            ).Create();
        }

        private void CreateOrReplaceBlinkingOverrideView(AnimationClip emptyClip)
        {
            new LayerForBlinkingOverrideView(
                _activityStageName,
                _comboLayers,
                _analogBlinkingUpperThreshold,
                _featuresToggles,
                _logicalAvatarMask,
                _animatorGenerator,
                emptyClip
            ).Create();
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }
    }

    class ActivityManifest
    {
        public int StageValue { get; }
        public RawGestureManifest Manifest { get; }
        public int LayerOrdinal { get; }

        public ActivityManifest(int stageValue, RawGestureManifest manifest, int layerOrdinal)
        {
            StageValue = stageValue;
            Manifest = manifest;
            LayerOrdinal = layerOrdinal;
        }
    }
}
