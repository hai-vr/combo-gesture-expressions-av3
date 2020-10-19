using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Processing
{
    public static class ManifestFromPuppet
    {
        public static IManifest FromPuppet(ComboGesturePuppet puppet)
        {
            var animations = AllDistinctAnimations(puppet);
            var qualifications = animations.Select(clip => new QualifiedAnimation(
                    clip,
                    new Qualification(
                        puppet.blinking.Contains(clip),
                        LimitedLipsyncHasWideOpenMouth(puppet, clip)
                            ? QualifiedLimitation.Wide
                            : QualifiedLimitation.None
                    )))
                .Distinct()
                .ToList();

            return new PuppetManifest(
                puppet.transitionDuration,
                PuppetAnimatedBehavior.Of((BlendTree)puppet.mainTree, qualifications)
            );
        }

        private static bool LimitedLipsyncHasWideOpenMouth(ComboGesturePuppet puppet, AnimationClip clip)
        {
            return puppet.limitedLipsync.Contains(new ComboGestureActivity.LimitedLipsyncAnimation{clip = clip, limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth});
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
