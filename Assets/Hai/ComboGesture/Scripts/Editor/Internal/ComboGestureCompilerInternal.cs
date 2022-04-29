using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboGestureCompilerInternal
    {
        private const string GesturePlayableLayerAvatarMaskPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_Nothing.mask";

        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly AnimatorController _animatorController;
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
        private readonly AssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;
        private readonly AnimatorController _gesturePlayableLayerController;
        private readonly AvatarMask _gesturePlayableLayerExpressionsAvatarMask;
        private readonly AvatarMask _gesturePlayableLayerTechnicalAvatarMask;
        private readonly ParameterGeneration _parameterGeneration;
        private readonly bool _useSmoothing;
        private readonly bool _universalAnalogSupport;

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
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = compiler.WillUseGestureWeightCorrection();
            _useSmoothing = _useGestureWeightCorrection;
            _universalAnalogSupport = compiler.useViveAdvancedControlsForNonFistAnalog;
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
            _assetContainer = null;
            _useGestureWeightCorrection = false;
            _useSmoothing = _useGestureWeightCorrection;
            _universalAnalogSupport = false;
        }

        enum ParameterGeneration
        {
            Unique, UserDefinedActivity, VirtualActivity
        }

        public void IntegrateWeightCorrection()
        {
            var avatarMaskPath = AssetDatabase.LoadAssetAtPath<AvatarMask>(GesturePlayableLayerAvatarMaskPath);
            CreateOrReplaceWeightCorrection(
                avatarMaskPath,
                _assetContainer,
                _animatorController,
                _conflictPrevention,
                _universalAnalogSupport
            );
            CreateOrReplaceSmoothing(avatarMaskPath, _assetContainer, _animatorController, _conflictPrevention);

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public void DoOverwriteAnimatorFxLayer()
        {
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
                    CreateOrReplaceWeightCorrection(_weightCorrectionAvatarMask, _assetContainer, _animatorController, _conflictPrevention, _universalAnalogSupport);
                    if (_useSmoothing)
                    {
                        CreateOrReplaceSmoothing(_weightCorrectionAvatarMask, _assetContainer, _animatorController, _conflictPrevention);
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

            var emptyClip = _assetContainer.ExposeAac().DummyClipLasting(1, AacFlUnit.Frames).Clip;

            var manifestBindings = CreateManifestBindings(emptyClip);

            CreateOrReplaceExpressionsView(emptyClip, manifestBindings);

            if (!Feature(FeatureToggles.DoNotGenerateBlinkingOverrideLayer))
            {
                CreateOrReplaceBlinkingOverrideView(manifestBindings);
            }

            if (!Feature(FeatureToggles.DoNotGenerateLipsyncOverrideLayer))
            {
                DeleteLipsyncOverrideView();
            }

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void CreateOrReplaceSmoothing(AvatarMask weightCorrectionAvatarMask, AssetContainer assetContainer, AnimatorController animatorController, ConflictPrevention conflictPrevention)
        {
            new LayerForAnalogFistSmoothing(assetContainer, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, animatorController).Create();
        }

        public void DoOverwriteAnimatorGesturePlayableLayer()
        {
            var emptyClip = _assetContainer.ExposeAac().DummyClipLasting(1, AacFlUnit.Frames).Clip;

            if (_activityStageName != null)
            {
                SharedLayerUtils.CreateParamIfNotExists(_gesturePlayableLayerController, _activityStageName, AnimatorControllerParameterType.Int);
            }

            if (!Feature(FeatureToggles.DoNotGenerateWeightCorrectionLayer))
            {
                if (_useGestureWeightCorrection)
                {
                    CreateOrReplaceWeightCorrection(_gesturePlayableLayerTechnicalAvatarMask, _assetContainer, _gesturePlayableLayerController, _conflictPreventionTempGestureLayer, _universalAnalogSupport);
                    if (_useSmoothing)
                    {
                        CreateOrReplaceSmoothing(_weightCorrectionAvatarMask, _assetContainer, _gesturePlayableLayerController, _conflictPreventionTempGestureLayer);
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
                    ToParameterGeneration(mapper),
                    SharedLayerUtils.FromMapper(mapper, emptyClip, _universalAnalogSupport),
                    layerOrdinal
                ))
                .ToList();
        }

        private int ToParameterGeneration(GestureComboStageMapper mapper)
        {
            switch (_parameterGeneration)
            {
                case ParameterGeneration.VirtualActivity:
                    return mapper.internalVirtualStageValue;
                case ParameterGeneration.Unique:
                    return 0;
                case ParameterGeneration.UserDefinedActivity:
                    return mapper.stageValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private void CreateOrReplaceBooleansToVirtualActivityMenu()
        {
            foreach (var mapper in _comboLayers)
            {
                if (!string.IsNullOrEmpty(mapper.booleanParameterName))
                {
                    SharedLayerUtils.CreateParamIfNotExists(_animatorController, mapper.booleanParameterName, AnimatorControllerParameterType.Bool);
                }
            }

            new LayerForBooleansToVirtualActivity(_assetContainer, _animatorController, _logicalAvatarMask, _conflictPrevention.ShouldWriteDefaults, _comboLayers).Create();
        }

        private void DeleteBooleansToVirtualActivityMenu()
        {
            LayerForBooleansToVirtualActivity.Delete(_assetContainer, _animatorController);
        }

        private static void CreateOrReplaceWeightCorrection(AvatarMask weightCorrectionAvatarMask, AssetContainer assetContainer, AnimatorController animatorController, ConflictPrevention conflictPrevention, bool universalAnalogSupport)
        {
            new LayerForWeightCorrection(assetContainer, animatorController, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, universalAnalogSupport).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip, List<ManifestBinding> manifestBindings)
        {
            CreateExpressionsViewParameters(_animatorController, _activityStageName);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimBlink", AnimatorControllerParameterType.Float);
            SharedLayerUtils.CreateParamIfNotExists(_animatorController, "_Hai_GestureAnimLSWide", AnimatorControllerParameterType.Float);

            var avatarFallbacks = new CgeAvatarSnapshot(_avatarDescriptor, _compilerFallbackParamList).CaptureFallbacks();
            new LayerForExpressionsView(
                _featuresToggles,
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
                gesturePlayableLayerExpressionsAvatarMask,
                emptyClip,
                _activityStageName,
                _conflictPreventionTempGestureLayer,
                _assetContainer,
                ConflictFxLayerMode.KeepOnlyTransforms,
                _compilerIgnoreParamList,
                avatarFallbacks,
                new List<CurveKey>(),
                _animatorController,
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

        private void CreateOrReplaceBlinkingOverrideView(List<ManifestBinding> manifestBindings)
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
                _animatorController,
                _assetContainer,
                manifestBindings,
                _conflictPrevention.ShouldWriteDefaults).Create();
        }

        private void DeleteLipsyncOverrideView()
        {
            LayerForLipsyncOverrideView.Delete(_assetContainer, _animatorController);
        }

        private void DeleteDeprecatedControllerLayer()
        {
            LayerForController.Delete(_assetContainer, _animatorController);
        }

        private void DeleteWeightCorrection()
        {
            LayerForWeightCorrection.Delete(_assetContainer, _animatorController);
        }

        private void DeleteSmoothing()
        {
            LayerForAnalogFistSmoothing.Delete(_assetContainer, _animatorController);
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
