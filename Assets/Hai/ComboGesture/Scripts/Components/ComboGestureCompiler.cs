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
        public bool doNotGenerateBlinkingOverrideLayer;
        public bool doNotGenerateLipsyncOverrideLayer;
        public bool doNotGenerateWeightCorrectionLayer;

        public AvatarMask expressionsAvatarMask;
        public AvatarMask logicalAvatarMask;
        public AvatarMask weightCorrectionAvatarMask;

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
        public ComboGestureActivity activity; // This can be null
        public int stageValue;
    }

    [System.Serializable]
    public enum ConflictPreventionMode
    {
        UseRecommendedConfiguration, OnlyWriteDefaults, GenerateAnimationsWithWriteDefaults, GenerateAnimationsWithoutWriteDefaults
    }

    [System.Serializable]
    public enum ConflictFxLayerMode
    {
        RemoveTransformsAndMuscles, KeepBoth, KeepOnlyTransformsAndMuscles
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
