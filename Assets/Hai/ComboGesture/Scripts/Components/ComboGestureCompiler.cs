using System.Collections.Generic;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureCompiler : MonoBehaviour
    {
        public string activityStageName;
        public List<GestureComboStageMapper> comboLayers;
        public RuntimeAnimatorController animatorController;
        public AnimationClip customEmptyClip;
        public float analogBlinkingUpperThreshold = 0.7f;

        public bool exposeDisableExpressions;
        public bool exposeDisableBlinkingOverride;
        public bool exposeAreEyesClosed;

        public bool doNotGenerateControllerLayer;
        public bool doNotGenerateBlinkingOverrideLayer;

        public ConflictPreventionMode conflictPreventionMode = ConflictPreventionMode.GenerateAnimationsWithWriteDefaults;
        public ConflictFxLayerMode conflictFxLayerMode = ConflictFxLayerMode.RemoveTransformsAndMuscles;

        public bool editorAdvancedFoldout;

        public AnimationClip ignoreParamList;
        public AnimationClip fallbackParamList;
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
}
