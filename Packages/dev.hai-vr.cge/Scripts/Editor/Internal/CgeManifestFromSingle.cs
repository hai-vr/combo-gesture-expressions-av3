using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public static class CgeManifestFromSingle
    {
        public static ICgeManifest FromPuppet(ComboGesturePuppet puppet)
        {
            var animations = AllDistinctAnimations(puppet);
            var qualifications = animations.Select(clip => new CgeQualifiedAnimation(
                    clip,
                    new Qualification(puppet.blinking.Contains(clip))
                ))
                .Distinct()
                .ToList();

            return new CgeSingleManifest(
                puppet.transitionDuration,
                CgePuppetAnimatedBehavior.Of((BlendTree)puppet.mainTree, qualifications)
            );
        }

        public static ICgeManifest FromAnim(AnimationClip clip, bool bothEyesClosed, float transitionDuration)
        {
            return new CgeSingleManifest(
                transitionDuration,
                CgeSingleAnimatedBehavior.Of(new CgeQualifiedAnimation(clip, new Qualification(bothEyesClosed)))
            );
        }

        public static List<AnimationClip> AllDistinctAnimations(ComboGesturePuppet cgp)
        {
            if (cgp.mainTree == null)
            {
                return new List<AnimationClip>();
            }

            return AllAnimationsOf((BlendTree)cgp.mainTree);
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
