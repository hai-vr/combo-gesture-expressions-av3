using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePuppet : MonoBehaviour
    {
        public float transitionDuration = 0.1f;

        public BlendTree mainTree;
        public PuppetIntent intent;

        public List<AnimationClip> blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> limitedLipsync;

        public ComboGesturePreviewSetup previewSetup;
        public bool editorLegacyFoldout;
        public bool editorTool;

        [System.Serializable]
        public enum PuppetIntent
        {
            DirectionalPuppet
        }

        public List<AnimationClip> AllDistinctAnimations()
        {
            if (mainTree == null)
            {
                return new List<AnimationClip>();
            }

            return AllAnimationsOf(mainTree);
        }

        public static List<AnimationClip> AllAnimationsOf(BlendTree tree)
        {
            return tree.children
                .Select(childMotion => childMotion.motion)
                .Where(motion => motion != null)
                .SelectMany(motion =>
                {
                    switch (motion)
                    {
                        case AnimationClip clip:
                            return new[] {clip}.ToList();
                        case BlendTree subTree:
                            return AllAnimationsOf(subTree);
                        default:
                            throw new ArgumentException();
                    }
                })
                .Distinct()
                .ToList();
        }
    }
}
