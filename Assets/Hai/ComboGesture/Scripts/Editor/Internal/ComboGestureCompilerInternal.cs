using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
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
        private const string GesturePlayableLayerAvatarMaskPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_Nothing.mask";

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
        private AnimatorGenerator _animatorGenerator;
        private readonly AssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;
        private readonly AnimatorController _gesturePlayableLayerController;
        private readonly AvatarMask _gesturePlayableLayerExpressionsAvatarMask;
        private readonly AvatarMask _gesturePlayableLayerTechnicalAvatarMask;

        public ComboGestureCompilerInternal(
            ComboGestureCompiler compiler,
            AssetContainer assetContainer)
        {
            _activityStageName = compiler.activityStageName == "" ? null : compiler.activityStageName;
            _comboLayers = compiler.comboLayers;
            _animatorController = (AnimatorController)compiler.animatorController;
            _gesturePlayableLayerController = (AnimatorController)compiler.gesturePlayableLayerController;
            _customEmptyClip = compiler.customEmptyClip;
            _analogBlinkingUpperThreshold = compiler.analogBlinkingUpperThreshold;
            _featuresToggles = (compiler.exposeDisableExpressions ? FeatureToggles.ExposeDisableExpressions : 0)
                               | (compiler.exposeDisableBlinkingOverride ? FeatureToggles.ExposeDisableBlinkingOverride : 0)
                               | (compiler.exposeAreEyesClosed ? FeatureToggles.ExposeAreEyesClosed : 0)
                               | (compiler.doNotGenerateControllerLayer ? FeatureToggles.DoNotGenerateControllerLayer : 0)
                               | (compiler.forceGenerationOfControllerLayer ? FeatureToggles.ForceGenerationOfControllerLayer : 0)
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
            _gesturePlayableLayerExpressionsAvatarMask = compiler.gesturePlayableLayerExpressionsAvatarMask;
            _gesturePlayableLayerTechnicalAvatarMask = compiler.gesturePlayableLayerTechnicalAvatarMask;
            _integrateLimitedLipsync = compiler.integrateLimitedLipsync;
            _limitedLipsync = compiler.lipsyncForWideOpenMouth;
            _doNotIncludeBlinkBlendshapes = !compiler.WillUseBlinkBlendshapeCorrection();
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = compiler.WillUseGestureWeightCorrection();
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            _animatorGenerator = new AnimatorGenerator(_animatorController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));
            var emptyClip = GetOrCreateEmptyClip();

            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }

            if (!Feature(FeatureToggles.DoNotGenerateControllerLayer))
            {
                if (Feature(FeatureToggles.ForceGenerationOfControllerLayer))
                {
                    CreateOrReplaceController(emptyClip);
                }
                else
                {
                    DeleteController();
                }
            }

            if (!Feature(FeatureToggles.DoNotGenerateWeightCorrectionLayer))
            {
                if (_useGestureWeightCorrection)
                {
                    CreateOrReplaceWeightCorrection(_weightCorrectionAvatarMask, _animatorGenerator, _animatorController, emptyClip);
                }
                else
                {
                    DeleteWeightCorrection();
                }
            }

            var manifestBindings = CreateManifestBindings(emptyClip);

            CreateOrReplaceExpressionsView(emptyClip, manifestBindings);

            if (!Feature(FeatureToggles.DoNotGenerateBlinkingOverrideLayer))
            {
                CreateOrReplaceBlinkingOverrideView(emptyClip, manifestBindings);
            }

            if (!Feature(FeatureToggles.DoNotGenerateLipsyncOverrideLayer))
            {
                if (_integrateLimitedLipsync && _avatarDescriptor != null && _avatarDescriptor.VisemeSkinnedMesh != null)
                {
                    CreateOrReplaceLipsyncOverrideView(emptyClip, manifestBindings);
                }
                else
                {
                    DeleteLipsyncOverrideView();
                }
            }

            ReapAnimator();

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public void DoOverwriteAnimatorGesturePlayableLayer()
        {
            _animatorGenerator = new AnimatorGenerator(_gesturePlayableLayerController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));
            var emptyClip = GetOrCreateEmptyClip();

            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_gesturePlayableLayerController, _activityStageName, AnimatorControllerParameterType.Int);
            }

            if (!Feature(FeatureToggles.DoNotGenerateWeightCorrectionLayer))
            {
                if (_useGestureWeightCorrection)
                {
                    var technicalAvatarMask = _gesturePlayableLayerTechnicalAvatarMask
                        ? _gesturePlayableLayerTechnicalAvatarMask
                        : AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);
                    CreateOrReplaceWeightCorrection(technicalAvatarMask, _animatorGenerator, _gesturePlayableLayerController, emptyClip);
                }
                else
                {
                    DeleteWeightCorrection();
                }
            }

            var manifestBindings = CreateManifestBindings(emptyClip);

            CreateOrReplaceGesturePlayableLayerExpressionsView(emptyClip, manifestBindings);

            ReapAnimator();

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private List<ManifestBinding> CreateManifestBindings(AnimationClip emptyClip)
        {
            return _comboLayers
                .Select((mapper, layerOrdinal) => new ManifestBinding(
                    mapper.stageValue,
                    SharedLayerUtils.FromMapper(mapper, emptyClip),
                    layerOrdinal
                ))
                .ToList();
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

        private static void CreateOrReplaceWeightCorrection(AvatarMask weightCorrectionAvatarMask, AnimatorGenerator animatorGenerator, AnimatorController animatorController, AnimationClip emptyClip)
        {
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboLeftWeightProxy, AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboRightWeightProxy, AnimatorControllerParameterType.Float);
            new LayerForWeightCorrection(animatorGenerator, weightCorrectionAvatarMask).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            CreateExpressionsViewParameters(_animatorController, Feature(FeatureToggles.ExposeDisableExpressions), _activityStageName);
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
                _useGestureWeightCorrection,
                manifestBindings,
                ""
            ).Create();
        }

        private void CreateOrReplaceGesturePlayableLayerExpressionsView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            var gesturePlayableLayerExpressionsAvatarMask = _gesturePlayableLayerExpressionsAvatarMask
                ? _gesturePlayableLayerExpressionsAvatarMask
                : AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);

            CreateExpressionsViewParameters(_gesturePlayableLayerController, Feature(FeatureToggles.ExposeDisableExpressions), _activityStageName);
            new LayerForExpressionsView(
                _featuresToggles,
                _animatorGenerator,
                gesturePlayableLayerExpressionsAvatarMask,
                emptyClip,
                _activityStageName,
                ConflictPrevention.Of(ConflictPreventionMode.OnlyWriteDefaults),
                _assetContainer,
                ConflictFxLayerMode.KeepOnlyTransforms,
                _compilerIgnoreParamList,
                _compilerFallbackParamList,
                new List<CurveKey>(),
                _gesturePlayableLayerController,
                _comboLayers,
                _useGestureWeightCorrection,
                manifestBindings,
                "GPL"
            ).Create();
        }

        private static void CreateExpressionsViewParameters(AnimatorController animatorController, bool disableExpressions, string activityStageName)
        {
            if (activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(animatorController, activityStageName, AnimatorControllerParameterType.Int);
            }

            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
            if (disableExpressions)
            {
                SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboDisableExpressionsParamName, AnimatorControllerParameterType.Int);
            }
        }

        private void CreateOrReplaceBlinkingOverrideView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
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
                manifestBindings
            ).Create();
        }

        private void CreateOrReplaceLipsyncOverrideView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
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
                manifestBindings
            ).Create();
        }

        private void DeleteLipsyncOverrideView()
        {
            LayerForLipsyncOverrideView.Delete(_animatorGenerator);
        }

        private void DeleteController()
        {
            LayerForController.Delete(_animatorGenerator);
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

    class ManifestBinding
    {
        public int StageValue { get; }
        public IManifest Manifest { get; }
        public int LayerOrdinal { get; }

        public ManifestBinding(int stageValue, IManifest manifest, int layerOrdinal)
        {
            StageValue = stageValue;
            Manifest = manifest;
            LayerOrdinal = layerOrdinal;
        }
    }
}
