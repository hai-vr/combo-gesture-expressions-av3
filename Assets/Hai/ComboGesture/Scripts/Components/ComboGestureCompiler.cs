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

        public bool integrateLimitedLipsync;
        public ComboGestureLimitedLipsync lipsyncForWideOpenMouth;

        public bool exposeDisableExpressions; // Deprecated
        public bool exposeDisableBlinkingOverride; // Deprecated
        public bool exposeAreEyesClosed; // Deprecated

        public bool doNotGenerateControllerLayer; // Deprecated
        public bool forceGenerationOfControllerLayer; // Deprecated
        public bool doNotGenerateBlinkingOverrideLayer;
        public bool doNotGenerateLipsyncOverrideLayer;
        public bool doNotGenerateWeightCorrectionLayer;

        public AvatarMask expressionsAvatarMask;
        public AvatarMask logicalAvatarMask;
        public AvatarMask weightCorrectionAvatarMask;
        public AvatarMask gesturePlayableLayerExpressionsAvatarMask;
        public AvatarMask gesturePlayableLayerTechnicalAvatarMask;

        public ConflictPreventionMode conflictPreventionMode; // Deprecated
        public WriteDefaultsRecommendationMode writeDefaultsRecommendationMode = WriteDefaultsRecommendationMode.FollowVrChatRecommendationWriteDefaultsOff;
        public WriteDefaultsRecommendationMode writeDefaultsRecommendationModeGesture = WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn;
        public GestureLayerTransformCapture gestureLayerTransformCapture = GestureLayerTransformCapture.CaptureDefaultTransformsFromAvatar;
        public ConflictPreventionMode conflictPreventionTempGestureLayerMode = ConflictPreventionMode.UseRecommendedConfiguration; // Deprecated
        public ConflictFxLayerMode conflictFxLayerMode = ConflictFxLayerMode.RemoveTransformsAndMuscles;
        public WeightCorrectionMode weightCorrectionMode = WeightCorrectionMode.UseRecommendedConfiguration;
        public BlinkCorrectionMode blinkCorrectionMode = BlinkCorrectionMode.UseRecommendedConfiguration; // Deprecated

        public AvatarMask generatedAvatarMask;

        public bool editorAdvancedFoldout;

        public AnimationClip ignoreParamList;
        public AnimationClip fallbackParamList;
        public bool doNotFixSingleKeyframes;

        public VRCAvatarDescriptor avatarDescriptor;
        public bool bypassMandatoryAvatarDescriptor;

        public ParameterMode parameterMode;

        public bool WillUseGestureWeightCorrection()
        {
            return weightCorrectionMode != WeightCorrectionMode.UseNativeWeight;
        }
    }

    [System.Serializable]
    public struct GestureComboStageMapper
    {
        public GestureComboStageKind kind;
        public ComboGestureActivity activity; // This can be null even when the kind is an Activity
        public ComboGesturePuppet puppet; // This can be null
        public int stageValue;
        public string booleanParameterName;
        public int internalVirtualStageValue; // This is overwritten by the compiler process
    }

    [System.Serializable]
    public enum GestureComboStageKind
    {
        Activity, Puppet
    }

    [System.Serializable]
    public enum ConflictPreventionMode
    {
        UseRecommendedConfiguration, OnlyWriteDefaults, GenerateAnimationsWithWriteDefaults, GenerateAnimationsWithoutWriteDefaults
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
    public enum BlinkCorrectionMode
    {
        UseRecommendedConfiguration, UseBlinkCorrection, DoNotUseBlinkCorrection
    }

    [System.Serializable]
    public enum ParameterMode
    {
        MultipleBools, SingleInt
    }
}
