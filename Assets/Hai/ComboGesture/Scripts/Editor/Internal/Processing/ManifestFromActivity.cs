using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Processing
{
    public static class ManifestFromActivity
    {
        public static PermutationManifest FromNothing(AnimationClip defaultClip)
        {
            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            for (var left = HandPose.H0; left <= HandPose.H7; left++)
            {
                for (var right = HandPose.H0; right <= HandPose.H7; right++)
                {
                    poses.Add(Permutation.LeftRight(left, right), SingleAnimatedBehavior.Of(new QualifiedAnimation(defaultClip, new Qualification(false, QualifiedLimitation.None))));
                }
            }

            return new PermutationManifest(poses, 0f);
        }

        public static PermutationManifest FromActivity(ComboGestureActivity activity, AnimationClip defaultClip)
        {
            Motion Just(Motion anim)
            {
                return anim ? anim : defaultClip;
            }

            AnimationClip JustAnim(AnimationClip anim)
            {
                return anim ? anim : defaultClip;
            }

            Motion Otherwise(Motion anim, Motion otherwise)
            {
                return anim ? anim : Just(otherwise);
            }

            IAnimatedBehavior MaybeDualAnalog()
            {
                var dualMotion = Just(activity.anim11);
                switch (dualMotion)
                {
                    case AnimationClip dualClip:
                    {
                        var baseMotion = Just(activity.anim00);
                        switch (baseMotion)
                        {
                            case AnimationClip baseClip:
                            {
                                if (activity.anim11_L == null && activity.anim11_R == null)
                                {
                                    return DualAnalogAnimatedBehavior.Maybe(Qualify(activity, baseClip), Qualify(activity, dualClip), Qualify(activity, dualClip), Qualify(activity, dualClip));
                                }

                                return DualAnalogAnimatedBehavior.Maybe(Qualify(activity, baseClip), Qualify(activity, JustAnim(activity.anim11_L)), Qualify(activity, JustAnim(activity.anim11_R)), Qualify(activity, dualClip));
                            }
                            case BlendTree baseTree:
                            {
                                return PuppetToDualAnalogAnimatedBehavior.Of(baseTree, Qualify(activity, JustAnim(activity.anim11_L)), Qualify(activity, JustAnim(activity.anim11_R)), Qualify(activity, dualClip), QualifyAll(activity, baseTree));
                            }
                            default:
                                throw new ArgumentException();
                        }

                    }
                    case BlendTree tree:
                        return PuppetAnimatedBehavior.Of(tree, QualifyAll(activity, tree));
                    default:
                        throw new ArgumentException();
                }
            }

            IAnimatedBehavior InterpretSingle(Motion motion)
            {
                switch (motion)
                {
                    case AnimationClip clip:
                        return SingleAnimatedBehavior.Of(Qualify(activity, clip));
                    case BlendTree tree:
                        return PuppetAnimatedBehavior.Of(tree, QualifyAll(activity, tree));
                    default:
                        throw new ArgumentException();
                }
            }

            IAnimatedBehavior InterpretAnalog(Motion baseMotion, Motion fistMotion)
            {
                switch (fistMotion)
                {
                    case AnimationClip fistClip:
                    {
                        switch (baseMotion)
                        {
                            case AnimationClip baseClip:
                                return AnalogAnimatedBehavior.Maybe(Qualify(activity, baseClip), Qualify(activity, fistClip));
                            case BlendTree baseTree:
                                return PuppetToAnalogAnimatedBehavior.Of(baseTree, Qualify(activity, fistClip), QualifyAll(activity, baseTree));
                            default:
                                throw new ArgumentException();
                        }
                    }
                    case BlendTree fistTree:
                        return PuppetAnimatedBehavior.Of(fistTree, QualifyAll(activity, fistTree));
                    default:
                        throw new ArgumentException();
                }
            }

            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H0), InterpretSingle(Just(activity.anim00)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H1), InterpretAnalog(Just(activity.anim00), Just(activity.anim01)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H2), InterpretSingle(Just(activity.anim02)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H3), InterpretSingle(Just(activity.anim03)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H4), InterpretSingle(Just(activity.anim04)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H5), InterpretSingle(Just(activity.anim05)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H6), InterpretSingle(Just(activity.anim06)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H7), InterpretSingle(Just(activity.anim07)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H1), MaybeDualAnalog());
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H2), InterpretAnalog(Just(activity.anim02), Just(activity.anim12)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H3), InterpretAnalog(Just(activity.anim03), Just(activity.anim13)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H4), InterpretAnalog(Just(activity.anim04), Just(activity.anim14)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H5), InterpretAnalog(Just(activity.anim05), Just(activity.anim15)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H6), InterpretAnalog(Just(activity.anim06), Just(activity.anim16)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H7), InterpretAnalog(Just(activity.anim07), Just(activity.anim17)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H2), InterpretSingle(Just(activity.anim22)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H3), InterpretSingle(Just(activity.anim23)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H4), InterpretSingle(Just(activity.anim24)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H5), InterpretSingle(Just(activity.anim25)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H6), InterpretSingle(Just(activity.anim26)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H7), InterpretSingle(Just(activity.anim27)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H3), InterpretSingle(Just(activity.anim33)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H4), InterpretSingle(Just(activity.anim34)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H5), InterpretSingle(Just(activity.anim35)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H6), InterpretSingle(Just(activity.anim36)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H7), InterpretSingle(Just(activity.anim37)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H4), InterpretSingle(Just(activity.anim44)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H5), InterpretSingle(Just(activity.anim45)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H6), InterpretSingle(Just(activity.anim46)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H7), InterpretSingle(Just(activity.anim47)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H5), InterpretSingle(Just(activity.anim55)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H6), InterpretSingle(Just(activity.anim56)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H7), InterpretSingle(Just(activity.anim57)));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H6), InterpretSingle(Just(activity.anim66)));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H7), InterpretSingle(Just(activity.anim67)));
            poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H7), InterpretSingle(Just(activity.anim77)));

            if (activity.enablePermutations)
            {
                poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H0), InterpretAnalog(Just(activity.anim00), Otherwise(activity.anim10, activity.anim01)));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H0), InterpretSingle(Otherwise(activity.anim20, activity.anim02)));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H0), InterpretSingle(Otherwise(activity.anim30, activity.anim03)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H0), InterpretSingle(Otherwise(activity.anim40, activity.anim04)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H0), InterpretSingle(Otherwise(activity.anim50, activity.anim05)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H0), InterpretSingle(Otherwise(activity.anim60, activity.anim06)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H0), InterpretSingle(Otherwise(activity.anim70, activity.anim07)));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H1), InterpretAnalog(Otherwise(activity.anim20, activity.anim02), Otherwise(activity.anim21, activity.anim12)));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H1), InterpretAnalog(Otherwise(activity.anim30, activity.anim03), Otherwise(activity.anim31, activity.anim13)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H1), InterpretAnalog(Otherwise(activity.anim40, activity.anim04), Otherwise(activity.anim41, activity.anim14)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H1), InterpretAnalog(Otherwise(activity.anim50, activity.anim05), Otherwise(activity.anim51, activity.anim15)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H1), InterpretAnalog(Otherwise(activity.anim60, activity.anim06), Otherwise(activity.anim61, activity.anim16)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H1), InterpretAnalog(Otherwise(activity.anim70, activity.anim07), Otherwise(activity.anim71, activity.anim17)));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H2), InterpretSingle(Otherwise(activity.anim32, activity.anim23)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H2), InterpretSingle(Otherwise(activity.anim42, activity.anim24)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H2), InterpretSingle(Otherwise(activity.anim52, activity.anim25)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H2), InterpretSingle(Otherwise(activity.anim62, activity.anim26)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H2), InterpretSingle(Otherwise(activity.anim72, activity.anim27)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H3), InterpretSingle(Otherwise(activity.anim43, activity.anim34)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H3), InterpretSingle(Otherwise(activity.anim53, activity.anim35)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H3), InterpretSingle(Otherwise(activity.anim63, activity.anim36)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H3), InterpretSingle(Otherwise(activity.anim73, activity.anim37)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H4), InterpretSingle(Otherwise(activity.anim54, activity.anim45)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H4), InterpretSingle(Otherwise(activity.anim64, activity.anim46)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H4), InterpretSingle(Otherwise(activity.anim74, activity.anim47)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H5), InterpretSingle(Otherwise(activity.anim65, activity.anim56)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H5), InterpretSingle(Otherwise(activity.anim75, activity.anim57)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H6), InterpretSingle(Otherwise(activity.anim76, activity.anim67)));
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

        private static List<QualifiedAnimation> QualifyAll(ComboGestureActivity activity, BlendTree tree)
        {
            return ManifestFromPuppet.AllAnimationsOf(tree)
                .Select(clip => new QualifiedAnimation(
                    clip,
                    new Qualification(
                        activity.blinking.Contains(clip),
                        LimitedLipsyncHasWideOpenMouth(activity, clip)
                            ? QualifiedLimitation.Wide
                            : QualifiedLimitation.None
                    )
                ))
                .ToList();
        }

        private static bool LimitedLipsyncHasWideOpenMouth(ComboGestureActivity activity, AnimationClip clip)
        {
            return activity.limitedLipsync.Contains(new ComboGestureActivity.LimitedLipsyncAnimation{clip = clip, limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth});
        }
    }
}
