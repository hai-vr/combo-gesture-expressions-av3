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
    }
    
    [System.Serializable]
    public struct GestureComboStageMapper
    {
        public ComboGestureActivity activity; // This can be null
        public int stageValue;
    }
}