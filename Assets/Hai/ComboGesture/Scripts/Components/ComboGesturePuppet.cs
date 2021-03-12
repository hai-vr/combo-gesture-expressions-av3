using System.Collections.Generic;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePuppet : MonoBehaviour
    {
        public float transitionDuration = 0.1f;

        public Motion mainTree;
        public PuppetIntent intent;

        public List<AnimationClip> blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> limitedLipsync;

        public ExpressionEditorPreviewable previewSetup;
        public bool editorLegacyFoldout;
        public bool editorTool;

        [System.Serializable]
        public enum PuppetIntent
        {
            DirectionalPuppet
        }
    }
}
