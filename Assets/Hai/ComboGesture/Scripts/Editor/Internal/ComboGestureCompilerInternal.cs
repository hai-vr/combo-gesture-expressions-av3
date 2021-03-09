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
        private readonly ConflictPrevention _conflictPreventionTempGestureLayer;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly AnimationClip _compilerIgnoreParamList;
        private readonly AnimationClip _compilerFallbackParamList;
        private readonly VRCAvatarDescriptor _avatarDescriptor;
        private readonly AvatarMask _expressionsAvatarMask;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly bool _integrateLimitedLipsync;
        private readonly ComboGestureLimitedLipsync _limitedLipsync;
        private AnimatorGenerator _animatorGenerator;
        private readonly AssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;
        private readonly AnimatorController _gesturePlayableLayerController;
        private readonly AvatarMask _gesturePlayableLayerExpressionsAvatarMask;
        private readonly AvatarMask _gesturePlayableLayerTechnicalAvatarMask;
        private readonly ParameterGeneration _parameterGeneration;
        private readonly bool _useSmoothing;

        public ComboGestureCompilerInternal(
            ComboGestureCompiler compiler,
            AssetContainer assetContainer)
        {
            _comboLayers = compiler.comboLayers;
            _parameterGeneration = _comboLayers.Count <= 1 ? ParameterGeneration.Unique : (compiler.parameterMode == ParameterMode.SingleInt ? ParameterGeneration.UserDefinedActivity : ParameterGeneration.VirtualActivity);
            switch (_parameterGeneration)
            {
                case ParameterGeneration.Unique:
                    _activityStageName = null;
                    break;
                case ParameterGeneration.UserDefinedActivity:
                    _activityStageName = compiler.activityStageName;
                    break;
                case ParameterGeneration.VirtualActivity:
                    _activityStageName = SharedLayerUtils.HaiVirtualActivity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _animatorController = (AnimatorController)compiler.animatorController;
            _gesturePlayableLayerController = compiler.gesturePlayableLayerController as AnimatorController;
            _customEmptyClip = compiler.customEmptyClip;
            _analogBlinkingUpperThreshold = compiler.analogBlinkingUpperThreshold;
            _featuresToggles = (compiler.doNotGenerateBlinkingOverrideLayer ? FeatureToggles.DoNotGenerateBlinkingOverrideLayer : 0)
                               | (compiler.doNotGenerateLipsyncOverrideLayer ? FeatureToggles.DoNotGenerateLipsyncOverrideLayer : 0)
                               | (compiler.doNotGenerateWeightCorrectionLayer ? FeatureToggles.DoNotGenerateWeightCorrectionLayer : 0)
                               | (compiler.doNotFixSingleKeyframes ? FeatureToggles.DoNotFixSingleKeyframes : 0);
            _conflictPrevention = ConflictPrevention.OfFxLayer(compiler.writeDefaultsRecommendationMode);
            _conflictPreventionTempGestureLayer = ConflictPrevention.OfGestureLayer(compiler.writeDefaultsRecommendationModeGesture, compiler.gestureLayerTransformCapture);
            _compilerConflictFxLayerMode = compiler.conflictFxLayerMode;
            _compilerIgnoreParamList = compiler.ignoreParamList;
            _compilerFallbackParamList = compiler.fallbackParamList;
            _avatarDescriptor = compiler.avatarDescriptor;
            _expressionsAvatarMask = compiler.expressionsAvatarMask ? compiler.expressionsAvatarMask : AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedLayerUtils.FxPlayableLayerAvatarMaskPath);
            _logicalAvatarMask = compiler.logicalAvatarMask ? compiler.logicalAvatarMask : AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedLayerUtils.FxPlayableLayerAvatarMaskPath);
            _weightCorrectionAvatarMask = compiler.weightCorrectionAvatarMask ? compiler.weightCorrectionAvatarMask : AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedLayerUtils.FxPlayableLayerAvatarMaskPath);
            _gesturePlayableLayerExpressionsAvatarMask = compiler.gesturePlayableLayerExpressionsAvatarMask ? compiler.gesturePlayableLayerExpressionsAvatarMask : AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);
            _gesturePlayableLayerTechnicalAvatarMask = compiler.gesturePlayableLayerTechnicalAvatarMask ? compiler.gesturePlayableLayerTechnicalAvatarMask : AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);
            _integrateLimitedLipsync = false; // For now, Limited Lipsync is disabled regardless of the compiler value
            _limitedLipsync = compiler.lipsyncForWideOpenMouth;
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = compiler.WillUseGestureWeightCorrection();
            _useSmoothing = _useGestureWeightCorrection;
        }

        public ComboGestureCompilerInternal(
            ComboGestureIntegrator integrator)
        {
            _animatorController = (AnimatorController)integrator.animatorController;
            _conflictPrevention = ConflictPrevention.OfIntegrator(integrator.writeDefaults);

            // FIXME: Incorrect pattern in use here, none of those are necessary
            _comboLayers = new List<GestureComboStageMapper>();
            _parameterGeneration = ParameterGeneration.Unique;
            _gesturePlayableLayerController = null;
            _customEmptyClip = null;
            _analogBlinkingUpperThreshold = 0f;
            _featuresToggles = 0;
            _conflictPreventionTempGestureLayer = ConflictPrevention.OfFxLayer(WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn);
            _compilerConflictFxLayerMode = ConflictFxLayerMode.KeepBoth;
            _compilerIgnoreParamList = new AnimationClip();
            _compilerFallbackParamList = new AnimationClip();
            _avatarDescriptor = null;
            _expressionsAvatarMask = null;
            _logicalAvatarMask = null;
            _weightCorrectionAvatarMask = null;
            _gesturePlayableLayerExpressionsAvatarMask = null;
            _gesturePlayableLayerTechnicalAvatarMask = null;
            _integrateLimitedLipsync = false;
            _limitedLipsync = null;
            _assetContainer = null;
            _useGestureWeightCorrection = false;
            _useSmoothing = _useGestureWeightCorrection;
        }

        enum ParameterGeneration
        {
            Unique, UserDefinedActivity, VirtualActivity
        }

        public void IntegrateWeightCorrection()
        {
            _animatorGenerator = new AnimatorGenerator(_animatorController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));

            var avatarMaskPath = AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);
            CreateOrReplaceWeightCorrection(
                avatarMaskPath,
                _animatorGenerator,
                _animatorController,
                _conflictPrevention
            );
            CreateOrReplaceSmoothing(avatarMaskPath, _animatorGenerator, _animatorController, _conflictPrevention);

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            _animatorGenerator = new AnimatorGenerator(_animatorController, new StatefulEmptyClipProvider(new ClipGenerator(_customEmptyClip, EmptyClipPath, "ComboGesture")));
            var emptyClip = GetOrCreateEmptyClip();

            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }

            DeleteDeprecatedControllerLayer();

            if (_parameterGeneration == ParameterGeneration.VirtualActivity)
            {
                CreateOrReplaceBooleansToVirtualActivityMenu();
            }
            else
            {
                DeleteBooleansToVirtualActivityMenu();
            }

            if (!Feature(FeatureToggles.DoNotGenerateWeightCorrectionLayer))
            {
                if (_useGestureWeightCorrection)
                {
                    CreateOrReplaceWeightCorrection(_weightCorrectionAvatarMask, _animatorGenerator, _animatorController, _conflictPrevention);
                    if (_useSmoothing)
                    {
                        CreateOrReplaceSmoothing(_weightCorrectionAvatarMask, _animatorGenerator, _animatorController, _conflictPrevention);
                    }
                    else
                    {
                        DeleteSmoothing();
                    }
                }
                else
                {
                    DeleteWeightCorrection();
                    DeleteSmoothing();
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

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void CreateOrReplaceSmoothing(AvatarMask weightCorrectionAvatarMask, AnimatorGenerator animatorGenerator, AnimatorController animatorController, ConflictPrevention conflictPrevention)
        {
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboLeftWeightSmoothing, AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboRightWeightSmoothing, AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboSmoothingFactor, AnimatorControllerParameterType.Float);
            new LayerForAnalogFistSmoothing(animatorGenerator, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, animatorController).Create();
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
                    CreateOrReplaceWeightCorrection(_gesturePlayableLayerTechnicalAvatarMask, _animatorGenerator, _gesturePlayableLayerController, _conflictPreventionTempGestureLayer);
                    if (_useSmoothing)
                    {
                        CreateOrReplaceSmoothing(_weightCorrectionAvatarMask, _animatorGenerator, _gesturePlayableLayerController, _conflictPreventionTempGestureLayer);
                    }
                    else
                    {
                        DeleteSmoothing();
                    }
                }
                else
                {
                    DeleteWeightCorrection();
                    DeleteSmoothing();
                }
            }

            var manifestBindings = CreateManifestBindings(emptyClip);

            CreateOrReplaceGesturePlayableLayerExpressionsView(emptyClip, manifestBindings);

            ReapAnimator(_gesturePlayableLayerController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private List<ManifestBinding> CreateManifestBindings(AnimationClip emptyClip)
        {
            return _comboLayers
                .Select((mapper, layerOrdinal) => new ManifestBinding(
                    _parameterGeneration == ParameterGeneration.VirtualActivity ? mapper.internalVirtualStageValue : mapper.stageValue,
                    SharedLayerUtils.FromMapper(mapper, emptyClip),
                    layerOrdinal
                ))
                .ToList();
        }

        private static void ReapAnimator(AnimatorController animatorController)
        {
            if (AssetDatabase.GetAssetPath(animatorController) == "")
            {
                return;
            }

            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));

            var reachableMotions = SharedLayerUtils.FindAllReachableClipsAndBlendTrees(animatorController)
                .ToList<Object>();
            Reap(allSubAssets, typeof(BlendTree), reachableMotions, o => o.name.StartsWith("autoBT_"));
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


        private void CreateOrReplaceBooleansToVirtualActivityMenu()
        {
            foreach (var mapper in _comboLayers)
            {
                if (!string.IsNullOrEmpty(mapper.booleanParameterName))
                {
                    SharedLayerUtils.CreateParamIfNotExists(_animatorController, mapper.booleanParameterName, AnimatorControllerParameterType.Bool);
                }
            }

            new LayerForBooleansToVirtualActivity(_animatorGenerator, _logicalAvatarMask, _conflictPrevention.ShouldWriteDefaults, _comboLayers).Create();
        }

        private void DeleteBooleansToVirtualActivityMenu()
        {
            LayerForBooleansToVirtualActivity.Delete(_animatorGenerator);
        }

        private static void CreateOrReplaceWeightCorrection(AvatarMask weightCorrectionAvatarMask, AnimatorGenerator animatorGenerator, AnimatorController animatorController, ConflictPrevention conflictPrevention)
        {
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboLeftWeightProxy, AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, SharedLayerUtils.HaiGestureComboRightWeightProxy, AnimatorControllerParameterType.Float);
            new LayerForWeightCorrection(animatorGenerator, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            CreateExpressionsViewParameters(_animatorController, _activityStageName);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimBlink", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimLSWide", AnimatorControllerParameterType.Float);

            var avatarFallbacks = new CgeAvatarSnapshot(_avatarDescriptor, _compilerFallbackParamList).CaptureFallbacks();
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
                avatarFallbacks,
                new List<CurveKey>(),
                _animatorController,
                _comboLayers,
                _useGestureWeightCorrection,
                _useSmoothing,
                manifestBindings,
                ""
            ).Create();
        }

        private void CreateOrReplaceGesturePlayableLayerExpressionsView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            var gesturePlayableLayerExpressionsAvatarMask = _gesturePlayableLayerExpressionsAvatarMask
                ? _gesturePlayableLayerExpressionsAvatarMask
                : AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);

            CreateExpressionsViewParameters(_gesturePlayableLayerController, _activityStageName);

            var avatarFallbacks = new CgeAvatarSnapshot(_avatarDescriptor, _compilerFallbackParamList).CaptureFallbacks();
            new LayerForExpressionsView(
                _featuresToggles,
                _animatorGenerator,
                gesturePlayableLayerExpressionsAvatarMask,
                emptyClip,
                _activityStageName,
                _conflictPreventionTempGestureLayer,
                _assetContainer,
                ConflictFxLayerMode.KeepOnlyTransforms,
                _compilerIgnoreParamList,
                avatarFallbacks,
                new List<CurveKey>(),
                _gesturePlayableLayerController,
                _comboLayers,
                _useGestureWeightCorrection,
                _useSmoothing,
                manifestBindings,
                "GPL"
            ).Create();
        }

        private static void CreateExpressionsViewParameters(AnimatorController animatorController, string activityStageName)
        {
            if (activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(animatorController, activityStageName, AnimatorControllerParameterType.Int);
            }

            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeft", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRight", AnimatorControllerParameterType.Int);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureLeftWeight", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(animatorController, "GestureRightWeight", AnimatorControllerParameterType.Float);
        }

        private void CreateOrReplaceBlinkingOverrideView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_animatorController, _activityStageName, AnimatorControllerParameterType.Int);
            }
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimBlink", AnimatorControllerParameterType.Float);
            new LayerForBlinkingOverrideView(
                _activityStageName,
                _comboLayers,
                _analogBlinkingUpperThreshold,
                _logicalAvatarMask,
                _animatorGenerator,
                emptyClip,
                manifestBindings,
                _conflictPrevention.ShouldWriteDefaults).Create();
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
            new LayerForLipsyncOverrideView(_analogBlinkingUpperThreshold,
                _logicalAvatarMask,
                _animatorGenerator,
                _avatarDescriptor,
                _limitedLipsync,
                _assetContainer,
                emptyClip,
                manifestBindings,
                _conflictPrevention.ShouldWriteDefaults).Create();
        }

        private void DeleteLipsyncOverrideView()
        {
            LayerForLipsyncOverrideView.Delete(_animatorGenerator);
        }

        private void DeleteDeprecatedControllerLayer()
        {
            LayerForController.Delete(_animatorGenerator);
        }

        private void DeleteWeightCorrection()
        {
            LayerForWeightCorrection.Delete(_animatorGenerator);
        }

        private void DeleteSmoothing()
        {
            LayerForAnalogFistSmoothing.Delete(_animatorGenerator);
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
