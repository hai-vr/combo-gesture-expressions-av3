using System.Collections.Generic;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePuppet : ComboGestureMoodSet
    {
        public float transitionDuration = 0.1f;

        public Motion mainTree;
        public PuppetIntent intent;

        public List<AnimationClip> blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> limitedLipsync;

        public bool editorLegacyFoldout;
        public bool editorTool;

        public Animator previewAnimator;

        [System.Serializable]
        public enum PuppetIntent
        {
            DirectionalPuppet
        }
    }
}
