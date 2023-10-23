using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureCompiler : MonoBehaviour
    {
        public string activityStageName;
        public List<GestureComboStageMapper> comboLayers;
        public RuntimeAnimatorController animatorController;
        public bool useGesturePlayableLayer;
        public RuntimeAnimatorController gesturePlayableLayerController;
        public RuntimeAnimatorController folderToGenerateNeutralizedAssetsIn;
        public RuntimeAnimatorController assetContainer;
        public bool generateNewContainerEveryTime;

        public AnimationClip customEmptyClip;
        public float analogBlinkingUpperThreshold = 0.95f;

        public bool doNotGenerateBlinkingOverrideLayer;
        public bool doNotGenerateWeightCorrectionLayer;

        public AvatarMask expressionsAvatarMask;
        public AvatarMask logicalAvatarMask;
        public AvatarMask weightCorrectionAvatarMask;
        public AvatarMask gesturePlayableLayerExpressionsAvatarMask;
        public AvatarMask gesturePlayableLayerTechnicalAvatarMask;

        public WriteDefaultsRecommendationMode writeDefaultsRecommendationMode = WriteDefaultsRecommendationMode.FollowVrChatRecommendationWriteDefaultsOff;
        public WriteDefaultsRecommendationMode writeDefaultsRecommendationModeGesture = WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn;
        public GestureLayerTransformCapture gestureLayerTransformCapture = GestureLayerTransformCapture.CaptureDefaultTransformsFromAvatar;
        public ConflictFxLayerMode conflictFxLayerMode = ConflictFxLayerMode.RemoveTransformsAndMuscles;
        public WeightCorrectionMode weightCorrectionMode = WeightCorrectionMode.UseRecommendedConfiguration;

        public bool useViveAdvancedControlsForNonFistAnalog;

        public AvatarMask generatedAvatarMask;

        public bool editorAdvancedFoldout;

        public AnimationClip ignoreParamList;
        public AnimationClip fallbackParamList;
        public bool doNotFixSingleKeyframes;

        public VRCAvatarDescriptor avatarDescriptor;
        public bool bypassMandatoryAvatarDescriptor;

        public ParameterMode parameterMode;
        public ComboGestureDynamics dynamics;
        public bool doNotForceBlinkBlendshapes;

        public string mmdCompatibilityToggleParameter;
        public int totalNumberOfGenerations;

        public string eyeTrackingEnabledParameter;
        public EyeTrackingParameterType eyeTrackingParameterType;

        public CgeStrategy playableLayerStrategy;

        public bool WillUseGestureWeightCorrection()
        {
            return weightCorrectionMode != WeightCorrectionMode.UseNativeWeight;
        }
    }

    public enum CgeStrategy
    {
        ModernStyle,
        OldStyle
    }

    [System.Serializable]
    public struct GestureComboStageMapper
    {
        public GestureComboStageKind kind;
        public ComboGestureActivity activity; // This can be null even when the kind is an Activity
        public ComboGesturePuppet puppet; // This can be null
        public ComboGestureMassiveBlend massiveBlend; // This can be null
        public ComboGestureDynamics dynamics; // This can be null
        public int stageValue;
        public string booleanParameterName;
        public int internalVirtualStageValue; // This is overwritten by the compiler process

        public string SimpleName()
        {
            switch (kind)
            {
                case GestureComboStageKind.Activity:
                    return activity != null ? activity.name : "";
                case GestureComboStageKind.Puppet:
                    return puppet != null ? puppet.name : "";
                case GestureComboStageKind.Massive:
                    return massiveBlend != null ? massiveBlend.name : "";
                default:
                    return "";
            }
        }
    }

    [System.Serializable]
    public enum GestureComboStageKind
    {
        Activity, Puppet, Massive
    }

    [System.Serializable]
    public enum WriteDefaultsRecommendationMode
    {
        FollowVrChatRecommendationWriteDefaultsOff, UseUnsupportedWriteDefaultsOn
    }

    [System.Serializable]
    public enum GestureLayerTransformCapture
    {
        CaptureDefaultTransformsFromAvatar, DoNotCaptureTransforms
    }
    [System.Serializable]
    public enum ConflictFxLayerMode
    {
        RemoveTransformsAndMuscles, KeepBoth, KeepOnlyTransformsAndMuscles, KeepOnlyTransforms
    }

    [System.Serializable]
    public enum WeightCorrectionMode
    {
        UseRecommendedConfiguration, UseWeightCorrection, UseNativeWeight
    }

    [System.Serializable]
    public enum ParameterMode
    {
        MultipleBools, SingleInt
    }

    [System.Serializable]
    public enum EyeTrackingParameterType
    {
        Modern, LegacyBool
    }
}
