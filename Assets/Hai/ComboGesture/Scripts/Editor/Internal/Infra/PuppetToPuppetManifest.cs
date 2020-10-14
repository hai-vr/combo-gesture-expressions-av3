using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Infra
{
    public class PuppetToPuppetManifest
    {
        public static IManifest FromPuppet(ComboGesturePuppet puppet)
        {
            var animations = puppet.AllDistinctAnimations();
            var qualifications = animations
                .Select(clip => new QualifiedAnimation(
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
                PuppetAnimatedBehavior.Of(puppet.mainTree, qualifications)
            );
        }

        private static bool LimitedLipsyncHasWideOpenMouth(ComboGesturePuppet puppet, AnimationClip clip)
        {
            return puppet.limitedLipsync.Contains(new ComboGestureActivity.LimitedLipsyncAnimation{clip = clip, limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth});
        }
    }
}
