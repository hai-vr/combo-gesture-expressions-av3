using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboGestureCompilerInternal
    {
        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly AnimatorController _animatorController;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly CgeFeatureToggles _featuresToggles;
        private readonly CgeConflictPrevention _conflictPrevention;
        private readonly CgeConflictPrevention _conflictPreventionTempGestureLayer;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly AnimationClip _compilerIgnoreParamList;
        private readonly AnimationClip _compilerFallbackParamList;
        private readonly VRCAvatarDescriptor _avatarDescriptor;
        private readonly AvatarMask _expressionsAvatarMask;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly CgeAssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;
        private readonly AnimatorController _gesturePlayableLayerController;
        private readonly AvatarMask _gesturePlayableLayerExpressionsAvatarMask;
        private readonly AvatarMask _gesturePlayableLayerTechnicalAvatarMask;
        private readonly ParameterGeneration _parameterGeneration;
        private readonly bool _useSmoothing;
        private readonly bool _universalAnalogSupport;
        private readonly AvatarMask _nothingMask;
        private readonly ComboGestureDynamicsItem[] _dynamicsLayers;
        private readonly bool _doNotForceBlinkBlendshapes;

        public ComboGestureCompilerInternal(
            ComboGestureCompiler compiler,
            CgeAssetContainer assetContainer)
        {
            _comboLayers = compiler.comboLayers;
            _dynamicsLayers = compiler.dynamics != null ? compiler.dynamics.items : new ComboGestureDynamicsItem[] { };
            _parameterGeneration = _comboLayers.Count <= 1 ? ParameterGeneration.Unique : (compiler.parameterMode == ParameterMode.SingleInt ? ParameterGeneration.UserDefinedActivity : ParameterGeneration.VirtualActivity);
            switch (_parameterGeneration)
            {
                case ParameterGeneration.Unique:
                case ParameterGeneration.VirtualActivity:
                    _activityStageName = CgeSharedLayerUtils.HaiVirtualActivity;
                    break;
                case ParameterGeneration.UserDefinedActivity:
                    _activityStageName = compiler.activityStageName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _animatorController = (AnimatorController)compiler.animatorController;
            _gesturePlayableLayerController = compiler.gesturePlayableLayerController as AnimatorController;
            _analogBlinkingUpperThreshold = compiler.analogBlinkingUpperThreshold;
            _featuresToggles = (compiler.doNotGenerateBlinkingOverrideLayer ? CgeFeatureToggles.DoNotGenerateBlinkingOverrideLayer : 0)
                               | (compiler.doNotGenerateWeightCorrectionLayer ? CgeFeatureToggles.DoNotGenerateWeightCorrectionLayer : 0)
                               | (compiler.doNotFixSingleKeyframes ? CgeFeatureToggles.DoNotFixSingleKeyframes : 0);
            _conflictPrevention = CgeConflictPrevention.OfFxLayer(compiler.writeDefaultsRecommendationMode);
            _conflictPreventionTempGestureLayer = CgeConflictPrevention.OfGestureLayer(compiler.writeDefaultsRecommendationModeGesture, compiler.gestureLayerTransformCapture);
            _compilerConflictFxLayerMode = compiler.conflictFxLayerMode;
            _compilerIgnoreParamList = compiler.ignoreParamList;
            _compilerFallbackParamList = compiler.fallbackParamList;
            _avatarDescriptor = compiler.avatarDescriptor;

            _nothingMask = CreateNothingMask();
            assetContainer.AddAvatarMask(_nothingMask);

            var noTransformsMask = CreateNoTransformsMask();
            assetContainer.AddAvatarMask(noTransformsMask);

            _expressionsAvatarMask = compiler.expressionsAvatarMask ? compiler.expressionsAvatarMask : noTransformsMask;
            _logicalAvatarMask = compiler.logicalAvatarMask ? compiler.logicalAvatarMask : noTransformsMask;
            _weightCorrectionAvatarMask = compiler.weightCorrectionAvatarMask ? compiler.weightCorrectionAvatarMask : noTransformsMask;
            _gesturePlayableLayerExpressionsAvatarMask = compiler.gesturePlayableLayerExpressionsAvatarMask ? compiler.gesturePlayableLayerExpressionsAvatarMask : _nothingMask;
            _gesturePlayableLayerTechnicalAvatarMask = compiler.gesturePlayableLayerTechnicalAvatarMask ? compiler.gesturePlayableLayerTechnicalAvatarMask : _nothingMask;
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = compiler.WillUseGestureWeightCorrection();
            _useSmoothing = _useGestureWeightCorrection;
            _universalAnalogSupport = compiler.useViveAdvancedControlsForNonFistAnalog;
            _doNotForceBlinkBlendshapes = compiler.doNotForceBlinkBlendshapes;
        }

        public ComboGestureCompilerInternal(
            ComboGestureIntegrator integrator)
        {
            _animatorController = (AnimatorController)integrator.animatorController;
            _conflictPrevention = CgeConflictPrevention.OfIntegrator(integrator.writeDefaults);

            // FIXME: Incorrect pattern in use here, none of those are necessary
            _comboLayers = new List<GestureComboStageMapper>();
            _parameterGeneration = ParameterGeneration.Unique;
            _gesturePlayableLayerController = null;
            _analogBlinkingUpperThreshold = 0f;
            _featuresToggles = 0;
            _conflictPreventionTempGestureLayer = CgeConflictPrevention.OfFxLayer(WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn);
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
            CreateOrReplaceWeightCorrection(
                _nothingMask,
                _assetContainer,
                _animatorController,
                _conflictPrevention,
                _universalAnalogSupport
            );
            CreateOrReplaceSmoothing(_nothingMask, _assetContainer, _animatorController, _conflictPrevention);

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static AvatarMask CreateNothingMask()
        {
            var avatarMask = new AvatarMask();
            for (var i = 0; i < (int) AvatarMaskBodyPart.LastBodyPart; i++)
            {
                avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart) i, false);
            }

            return avatarMask;
        }

        private static AvatarMask CreateNoTransformsMask()
        {
            var avatarMask = new AvatarMask();
            if (true)
            {
                avatarMask.transformCount = 1;
                avatarMask.SetTransformActive(0, false);
                avatarMask.SetTransformPath(0, "_ignored");
            }

            for (int i = 0; i < (int) AvatarMaskBodyPart.LastBodyPart; i++)
            {
                avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart) i, false);
            }

            return avatarMask;
        }

        public void DoOverwriteAnimatorFxLayer()
        {
            DeleteDeprecatedControllerLayer();

            if (_parameterGeneration == ParameterGeneration.VirtualActivity)
            {
                CreateOrReplaceBooleansToVirtualActivityMenu();
            }
            else
            {
                DeleteBooleansToVirtualActivityMenu();
            }

            if (!Feature(CgeFeatureToggles.DoNotGenerateWeightCorrectionLayer))
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

            var emptyClip = _assetContainer.ExposeCgeAac().DummyClipLasting(1, CgeAacFlUnit.Frames).Clip;

            var manifestBindings = CreateManifestBindings(emptyClip);

            CreateOrReplaceExpressionsView(emptyClip, manifestBindings);

            if (!Feature(CgeFeatureToggles.DoNotGenerateBlinkingOverrideLayer))
            {
                CreateOrReplaceBlinkingOverrideView(manifestBindings);
            }

            DeleteDeprecatedLipsyncOverrideView();

            ReapAnimator(_animatorController);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void CreateOrReplaceSmoothing(AvatarMask weightCorrectionAvatarMask, CgeAssetContainer assetContainer, AnimatorController animatorController, CgeConflictPrevention conflictPrevention)
        {
            new CgeLayerForAnalogFistSmoothing(assetContainer, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, animatorController).Create();
        }

        public void DoOverwriteAnimatorGesturePlayableLayer()
        {
            var emptyClip = _assetContainer.ExposeCgeAac().DummyClipLasting(1, CgeAacFlUnit.Frames).Clip;

            if (!Feature(CgeFeatureToggles.DoNotGenerateWeightCorrectionLayer))
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

        private List<CgeManifestBinding> CreateManifestBindings(AnimationClip emptyClip)
        {
            var comboLayers = _comboLayers
                .Select(mapper => CgeManifestBinding.FromActivity(ToParameterGeneration(mapper),
                    CgeSharedLayerUtils.FromMapper(mapper, emptyClip, _universalAnalogSupport)))
                .ToList();
            var dynamicsLayers = _dynamicsLayers
                .SelectMany((simpleDynamics, rank) =>
                {
                    var descriptor = simpleDynamics.ToDescriptor();
                    if (descriptor.parameterType == ComboGestureDynamicsParameterType.Float && !descriptor.isHardThreshold)
                    {
                        return comboLayers.Select(binding => CgeManifestBinding.FromActivityBoundAvatarDynamics(
                            new CgeDynamicsRankedDescriptor
                            {
                                descriptor = descriptor,
                                rank = rank
                            }, CgeSharedLayerUtils.FromMassiveSimpleDynamics(simpleDynamics, emptyClip, _universalAnalogSupport, binding.Manifest),
                            binding.StageValue
                        )).ToArray();
                    }

                    return new[]
                    {
                        CgeManifestBinding.FromAvatarDynamics(
                            new CgeDynamicsRankedDescriptor
                            {
                                descriptor = descriptor,
                                rank = rank,
                            }, CgeSharedLayerUtils.FromSimpleDynamics(simpleDynamics, emptyClip, _universalAnalogSupport)
                        )
                    };
                })
                .ToList();
            var comboDynamicsLayers = _comboLayers
                .SelectMany((mapper, index) =>
                {
                    var binding = comboLayers[index]; // Can't use a .Where before the SelectMany because we need the index to match
                    if (mapper.dynamics == null)
                    {
                        return new CgeManifestBinding[0];
                    }

                    return mapper.dynamics.items
                        .Select((simpleDynamics, rank) =>
                        {
                            var descriptor = simpleDynamics.ToDescriptor();
                            if (descriptor.parameterType == ComboGestureDynamicsParameterType.Float && !descriptor.isHardThreshold)
                            {
                                return CgeManifestBinding.FromActivityBoundAvatarDynamics(
                                    new CgeDynamicsRankedDescriptor
                                    {
                                        descriptor = descriptor,
                                        rank = rank
                                    }, CgeSharedLayerUtils.FromMassiveSimpleDynamics(simpleDynamics, emptyClip, _universalAnalogSupport, binding.Manifest),
                                    binding.StageValue
                                );
                            }

                            return CgeManifestBinding.FromAvatarDynamics(
                                new CgeDynamicsRankedDescriptor
                                {
                                    descriptor = descriptor,
                                    rank = rank,
                                }, CgeSharedLayerUtils.FromSimpleDynamics(simpleDynamics, emptyClip, _universalAnalogSupport)
                            );
                        })
                        .ToArray();
                })
                .ToList();

            // Dynamics layers must be above combo layers -- This will affect layer generation order later on.
            return dynamicsLayers.Concat(comboDynamicsLayers).Concat(comboLayers).ToList();
        }

        private int ToParameterGeneration(GestureComboStageMapper mapper)
        {
            switch (_parameterGeneration)
            {
                case ParameterGeneration.VirtualActivity:
                case ParameterGeneration.UserDefinedActivity:
                    return mapper.internalVirtualStageValue;
                case ParameterGeneration.Unique:
                    return 0;
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

            var reachableMotions = CgeSharedLayerUtils.FindAllReachableClipsAndBlendTrees(animatorController)
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
            new CgeLayerForVirtualActivity(_assetContainer, _animatorController, _logicalAvatarMask, _conflictPrevention.ShouldWriteDefaults, _comboLayers).Create();
        }

        private void DeleteBooleansToVirtualActivityMenu()
        {
            CgeLayerForVirtualActivity.Delete(_assetContainer, _animatorController);
        }

        private static void CreateOrReplaceWeightCorrection(AvatarMask weightCorrectionAvatarMask, CgeAssetContainer assetContainer, AnimatorController animatorController, CgeConflictPrevention conflictPrevention, bool universalAnalogSupport)
        {
            new CgeLayerForWeightCorrection(assetContainer, animatorController, weightCorrectionAvatarMask, conflictPrevention.ShouldWriteDefaults, universalAnalogSupport).Create();
        }

        private void CreateOrReplaceExpressionsView(AnimationClip emptyClip, List<CgeManifestBinding> manifestBindings)
        {
            var avatarFallbacks = new CgeAvatarSnapshot(_avatarDescriptor, _compilerFallbackParamList).CaptureFallbacks();
            new CgeLayerForExpressionsView(
                _featuresToggles,
                _expressionsAvatarMask,
                emptyClip,
                _activityStageName,
                _conflictPrevention,
                _assetContainer,
                _compilerConflictFxLayerMode,
                _compilerIgnoreParamList,
                avatarFallbacks,
                _animatorController,
                _useGestureWeightCorrection,
                _useSmoothing,
                manifestBindings,
                _avatarDescriptor,
                _doNotForceBlinkBlendshapes
            ).Create();
        }

        private void CreateOrReplaceGesturePlayableLayerExpressionsView(AnimationClip emptyClip, List<CgeManifestBinding> manifestBindings)
        {
            var gesturePlayableLayerExpressionsAvatarMask = _gesturePlayableLayerExpressionsAvatarMask
                ? _gesturePlayableLayerExpressionsAvatarMask
                : _nothingMask;

            var avatarFallbacks = new CgeAvatarSnapshot(_avatarDescriptor, _compilerFallbackParamList).CaptureFallbacks();
            new CgeLayerForExpressionsView(
                _featuresToggles,
                gesturePlayableLayerExpressionsAvatarMask,
                emptyClip,
                _activityStageName,
                _conflictPreventionTempGestureLayer,
                _assetContainer,
                ConflictFxLayerMode.KeepOnlyTransforms,
                _compilerIgnoreParamList,
                avatarFallbacks,
                _animatorController,
                _useGestureWeightCorrection,
                _useSmoothing,
                manifestBindings,
                _avatarDescriptor,
                _doNotForceBlinkBlendshapes
            ).Create();
        }

        private void CreateOrReplaceBlinkingOverrideView(List<CgeManifestBinding> manifestBindings)
        {
            new CgeLayerForBlinkingOverrideView(_analogBlinkingUpperThreshold,
                _logicalAvatarMask,
                _animatorController,
                _assetContainer,
                manifestBindings,
                _conflictPrevention.ShouldWriteDefaults).Create();
        }

        private void DeleteDeprecatedLipsyncOverrideView()
        {
            _assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureLipsync");
        }

        private void DeleteDeprecatedControllerLayer()
        {
            _assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureCtrl");
        }

        private void DeleteWeightCorrection()
        {
            CgeLayerForWeightCorrection.Delete(_assetContainer, _animatorController);
        }

        private void DeleteSmoothing()
        {
            CgeLayerForAnalogFistSmoothing.Delete(_assetContainer, _animatorController);
        }

        private bool Feature(CgeFeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }
    }

    struct CgeManifestBinding
    {
        public bool IsActivityBound;
        public int StageValue;
        public ICgeManifest Manifest;
        public bool IsAvatarDynamics;
        public CgeDynamicsRankedDescriptor DynamicsDescriptor;

        public static CgeManifestBinding FromActivity(int stageValue, ICgeManifest manifest)
        {
            return new CgeManifestBinding
            {
                IsActivityBound = true,
                StageValue = stageValue,
                Manifest = manifest
            };
        }

        public static CgeManifestBinding FromAvatarDynamics(CgeDynamicsRankedDescriptor dynamicsDescriptor, ICgeManifest manifest)
        {
            return new CgeManifestBinding
            {
                IsAvatarDynamics = true,
                DynamicsDescriptor = dynamicsDescriptor,
                Manifest = manifest
            };
        }

        public static CgeManifestBinding FromActivityBoundAvatarDynamics(CgeDynamicsRankedDescriptor dynamicsDescriptor, ICgeManifest manifest, int stageValue)
        {
            return new CgeManifestBinding
            {
                IsActivityBound = true,
                StageValue = stageValue,
                IsAvatarDynamics = true,
                DynamicsDescriptor = dynamicsDescriptor,
                Manifest = manifest
            };
        }

        public static CgeManifestBinding Remapping(CgeManifestBinding original, ICgeManifest newManifest)
        {
            return new CgeManifestBinding
            {
                IsActivityBound = original.IsActivityBound,
                StageValue = original.StageValue,
                Manifest = newManifest,
                IsAvatarDynamics = original.IsAvatarDynamics,
                DynamicsDescriptor = original.DynamicsDescriptor
            };
        }
    }
}
