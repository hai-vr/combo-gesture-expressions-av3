using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Infra
{
    public class ActivityToPermutationManifest
    {
        public static PermutationManifest FromNothing(AnimationClip defaultClip)
        {
            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            for (HandPose left = HandPose.H0; left <= HandPose.H7; left++)
            {
                for (HandPose right = HandPose.H0; right <= HandPose.H7; right++)
                {
                    poses.Add(Permutation.LeftRight(left, right), SingleAnimatedBehavior.Of(new QualifiedAnimation(defaultClip, new Qualification(false, QualifiedLimitation.None))));
                }
            }

            return new PermutationManifest(poses, 0f);
        }

        public static PermutationManifest FromActivity(ComboGestureActivity activity, AnimationClip defaultClip)
        {
            AnimationClip Just(AnimationClip anim)
            {
                return anim ? anim : defaultClip;
            }

            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim00))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim00)), Qualify(activity, Just(activity.anim01))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim02))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim03))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim04))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim05))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim06))));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim07))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H1), DualAnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim00)), Qualify(activity, Just(activity.anim11_L)), Qualify(activity, Just(activity.anim11_R)), Qualify(activity, Just(activity.anim11))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H2), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim02)), Qualify(activity, Just(activity.anim12))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H3), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim03)), Qualify(activity, Just(activity.anim13))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H4), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim04)), Qualify(activity, Just(activity.anim14))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H5), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim05)), Qualify(activity, Just(activity.anim15))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H6), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim06)), Qualify(activity, Just(activity.anim16))));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H7), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim07)), Qualify(activity, Just(activity.anim17))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim22))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim23))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim24))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim25))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim26))));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim27))));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim33))));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim34))));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim35))));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim36))));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim37))));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim44))));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim45))));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim46))));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim47))));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim55))));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim56))));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim57))));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim66))));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim67))));
            poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H7), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim77))));

            if (activity.enablePermutations)
            {
                poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H0), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim00)), Qualify(activity, Just(activity.anim01))));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim20))));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim30))));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim40))));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim50))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim60))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H0), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim70))));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim20)), Qualify(activity, Just(activity.anim21))));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim30)), Qualify(activity, Just(activity.anim31))));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim40)), Qualify(activity, Just(activity.anim41))));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim50)), Qualify(activity, Just(activity.anim51))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim60)), Qualify(activity, Just(activity.anim61))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H1), AnalogAnimatedBehavior.Maybe(Qualify(activity, Just(activity.anim70)), Qualify(activity, Just(activity.anim71))));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim32))));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim42))));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim52))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim62))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H2), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim72))));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim43))));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim53))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim63))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H3), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim73))));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim54))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim64))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H4), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim74))));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim65))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H5), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim75))));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H6), SingleAnimatedBehavior.Of(Qualify(activity, Just(activity.anim76))));
            }
            else
            {
                var combos = poses
                    .Where(pair => !pair.Key.IsSymmetrical())
                    .ToDictionary(pair => Permutation.LeftRight(pair.Key.Right, pair.Key.Left), pair => pair.Value);
                foreach (var pair in combos)
                {
                    poses.Add(pair.Key, pair.Value);
                }
            }

            return new PermutationManifest(poses, activity.transitionDuration);
        }

        private static QualifiedAnimation Qualify(ComboGestureActivity activity, AnimationClip clip)
        {
            return new QualifiedAnimation(
                clip,
                new Qualification(
                    activity.blinking.Contains(clip),
                    LimitedLipsyncHasWideOpenMouth(activity, clip)
                        ? QualifiedLimitation.Wide
                        : QualifiedLimitation.None
                )
            );
        }

        private static bool LimitedLipsyncHasWideOpenMouth(ComboGestureActivity activity, AnimationClip clip)
        {
            return activity.limitedLipsync.Contains(new ComboGestureActivity.LimitedLipsyncAnimation{clip = clip, limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth});
        }
    }
}
