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

        public bool exposeDisableExpressions;
        public bool exposeDisableBlinkingOverride;
        public bool exposeAreEyesClosed;

        public bool doNotGenerateControllerLayer;
        public bool forceGenerationOfControllerLayer;
        public bool doNotGenerateBlinkingOverrideLayer;
        public bool doNotGenerateLipsyncOverrideLayer;
        public bool doNotGenerateWeightCorrectionLayer;

        public AvatarMask expressionsAvatarMask;
        public AvatarMask logicalAvatarMask;
        public AvatarMask weightCorrectionAvatarMask;
        public AvatarMask gesturePlayableLayerExpressionsAvatarMask;
        public AvatarMask gesturePlayableLayerTechnicalAvatarMask;

        public ConflictPreventionMode conflictPreventionMode = ConflictPreventionMode.GenerateAnimationsWithWriteDefaults;
        public ConflictFxLayerMode conflictFxLayerMode = ConflictFxLayerMode.RemoveTransformsAndMuscles;
        public WeightCorrectionMode weightCorrectionMode = WeightCorrectionMode.UseRecommendedConfiguration;
        public BlinkCorrectionMode blinkCorrectionMode = BlinkCorrectionMode.UseRecommendedConfiguration;

        public bool editorAdvancedFoldout;

        public AnimationClip ignoreParamList;
        public AnimationClip fallbackParamList;

        public VRCAvatarDescriptor avatarDescriptor;
        public bool bypassMandatoryAvatarDescriptor;

        public bool WillUseBlinkBlendshapeCorrection()
        {
            return blinkCorrectionMode == BlinkCorrectionMode.UseBlinkCorrection;
        }

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
}
