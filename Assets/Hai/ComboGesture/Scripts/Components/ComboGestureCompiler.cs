#if UNITY_EDITOR
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Internal;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    internal class ComboGestureCompiler : MonoBehaviour
    {
        public string activityStageName;
        public List<GestureComboStageMapper> comboLayers;
        public AnimatorController animatorController;
        public AnimationClip customEmptyClip;
    }
}
#endif