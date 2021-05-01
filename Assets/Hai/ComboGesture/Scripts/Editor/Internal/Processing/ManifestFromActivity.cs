using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Processing
{
    public class ManifestFromActivity
    {
        private readonly ComboGestureActivity _activity;
        private readonly AnimationClip _defaultClip;
        private readonly bool _universalAnalogSupport;

        private ManifestFromActivity(ComboGestureActivity activity, AnimationClip defaultClip, bool universalAnalogSupport)
        {
            _activity = activity;
            _defaultClip = defaultClip;
            _universalAnalogSupport = universalAnalogSupport;
        }

        public static PermutationManifest FromNothing(AnimationClip defaultClip)
        {
            var poses = Permutation.All().ToDictionary(
                permutation => permutation,
                permutation => SingleAnimatedBehavior.Of(new QualifiedAnimation(defaultClip, new Qualification(false, QualifiedLimitation.None))));
            return new PermutationManifest(poses, 0f);
        }

        public static PermutationManifest FromActivity(ComboGestureActivity activity, AnimationClip defaultClip, bool universalAnalogSupport)
        {
            return new ManifestFromActivity(activity, defaultClip, universalAnalogSupport).Resolve();
        }

        private PermutationManifest Resolve()
        {
            var isOneHandMode = _activity.oneHandMode != ComboGestureActivity.CgeOneHandMode.Disabled;
            if (isOneHandMode) return OneHand();
            if (_universalAnalogSupport) return UniversalAnalog();
            return Regular();
        }

        Motion Just(Motion anim)
        {
            return anim ? anim : _defaultClip;
        }

        AnimationClip JustAnim(AnimationClip anim)
        {
            return anim ? anim : _defaultClip;
        }

        Motion Otherwise(Motion anim, Motion otherwise)
        {
            return anim ? anim : Just(otherwise);
        }

        IAnimatedBehavior MaybeDualAnalog()
        {
            var dualMotion = Just(_activity.anim11);
            switch (dualMotion)
            {
                case AnimationClip dualClip:
                {
                    var baseMotion = Just(_activity.anim00);
                    switch (baseMotion)
                    {
                        case AnimationClip baseClip:
                        {
                            if (_activity.anim11_L == null && _activity.anim11_R == null)
                            {
                                return DualAnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, dualClip), Qualify(_activity, dualClip), Qualify(_activity, dualClip));
                            }

                            return DualAnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, JustAnim(_activity.anim11_L)), Qualify(_activity, JustAnim(_activity.anim11_R)), Qualify(_activity, dualClip));
                        }
                        case BlendTree baseTree:
                        {
                            return PuppetToDualAnalogAnimatedBehavior.Of(baseTree, Qualify(_activity, JustAnim(_activity.anim11_L)), Qualify(_activity, JustAnim(_activity.anim11_R)), Qualify(_activity, dualClip), QualifyAll(_activity, baseTree));
                        }
                        default:
                            throw new ArgumentException();
                    }

                }
                case BlendTree tree:
                    return PuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        IAnimatedBehavior MaybeUniversalAnalog(Motion resting, Motion leftSqueezing, Motion rightSqueezing, Motion bothSqueezing)
        {
            switch (bothSqueezing)
            {
                case AnimationClip dualClip:
                {
                    return UniversalAnalogAnimatedBehavior.Of(
                        Universal(resting),
                        Universal(leftSqueezing),
                        Universal(rightSqueezing),
                        Qualify(_activity, dualClip)
                    );
                }
                case BlendTree tree:
                    return PuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        private UniversalAnalogAnimatedBehavior.UniversalQualifier Universal(Motion resting)
        {
            return resting is BlendTree b
                ? UniversalAnalogAnimatedBehavior.UniversalQualifier.OfBlend(b, QualifyAll(_activity, b))
                : UniversalAnalogAnimatedBehavior.UniversalQualifier.OfQualification(Qualify(_activity, (AnimationClip)resting));
        }

        IAnimatedBehavior InterpretSingle(Motion motion)
        {
            switch (motion)
            {
                case AnimationClip clip:
                    return SingleAnimatedBehavior.Of(Qualify(_activity, clip));
                case BlendTree tree:
                    return PuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        IAnimatedBehavior InterpretAnalog(Motion baseMotion, Motion fistMotion, HandSide handSide)
        {
            switch (fistMotion)
            {
                case AnimationClip fistClip:
                {
                    switch (baseMotion)
                    {
                        case AnimationClip baseClip:
                            return AnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, fistClip), handSide);
                        case BlendTree baseTree:
                            return PuppetToAnalogAnimatedBehavior.Of(baseTree, Qualify(_activity, fistClip), QualifyAll(_activity, baseTree), handSide);
                        default:
                            throw new ArgumentException();
                    }
                }
                case BlendTree fistTree:
                    return PuppetAnimatedBehavior.Of(fistTree, QualifyAll(_activity, fistTree));
                default:
                    throw new ArgumentException();
            }
        }

        private PermutationManifest UniversalAnalog()
        {
            var permutationToNullableMotions = PermutationToNullableMotions();
            var permutationToMotions = NormalizeComboLike(permutationToNullableMotions, _defaultClip);
            var neutral = Permutation.LeftRight(HandPose.H0, HandPose.H0);
            var behaviors = Permutation.All()
                .ToDictionary(permutation => permutation, current =>
                {
                    if (current.AreBoth(HandPose.H0))
                    {
                        return InterpretSingle(permutationToMotions[current]);
                    }

                    if (current.HasAnyPose(HandPose.H0))
                    {
                        return InterpretAnalog(
                            permutationToMotions[neutral],
                            permutationToMotions[current],
                            current.Left == HandPose.H0 ? HandSide.RightHand : HandSide.LeftHand
                        );
                    }

                    return MaybeUniversalAnalog(
                        permutationToMotions[neutral],
                        permutationToMotions[Permutation.LeftRight(current.Left, HandPose.H0)],
                        permutationToMotions[Permutation.LeftRight(HandPose.H0, current.Right)],
                        permutationToMotions[current]
                    );
                });

            return new PermutationManifest(behaviors, _activity.transitionDuration);
        }

        private PermutationManifest Regular()
        {
            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H0), InterpretSingle(Just(_activity.anim00)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H1), InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), HandSide.RightHand));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H2), InterpretSingle(Just(_activity.anim02)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H3), InterpretSingle(Just(_activity.anim03)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H4), InterpretSingle(Just(_activity.anim04)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H5), InterpretSingle(Just(_activity.anim05)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H6), InterpretSingle(Just(_activity.anim06)));
            poses.Add(Permutation.LeftRight(HandPose.H0, HandPose.H7), InterpretSingle(Just(_activity.anim07)));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H1), MaybeDualAnalog());
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H2), InterpretAnalog(Just(_activity.anim02), Just(_activity.anim12), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H3), InterpretAnalog(Just(_activity.anim03), Just(_activity.anim13), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H4), InterpretAnalog(Just(_activity.anim04), Just(_activity.anim14), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H5), InterpretAnalog(Just(_activity.anim05), Just(_activity.anim15), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H6), InterpretAnalog(Just(_activity.anim06), Just(_activity.anim16), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H7), InterpretAnalog(Just(_activity.anim07), Just(_activity.anim17), HandSide.LeftHand));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H2), InterpretSingle(Just(_activity.anim22)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H3), InterpretSingle(Just(_activity.anim23)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H4), InterpretSingle(Just(_activity.anim24)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H5), InterpretSingle(Just(_activity.anim25)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H6), InterpretSingle(Just(_activity.anim26)));
            poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H7), InterpretSingle(Just(_activity.anim27)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H3), InterpretSingle(Just(_activity.anim33)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H4), InterpretSingle(Just(_activity.anim34)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H5), InterpretSingle(Just(_activity.anim35)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H6), InterpretSingle(Just(_activity.anim36)));
            poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H7), InterpretSingle(Just(_activity.anim37)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H4), InterpretSingle(Just(_activity.anim44)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H5), InterpretSingle(Just(_activity.anim45)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H6), InterpretSingle(Just(_activity.anim46)));
            poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H7), InterpretSingle(Just(_activity.anim47)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H5), InterpretSingle(Just(_activity.anim55)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H6), InterpretSingle(Just(_activity.anim56)));
            poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H7), InterpretSingle(Just(_activity.anim57)));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H6), InterpretSingle(Just(_activity.anim66)));
            poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H7), InterpretSingle(Just(_activity.anim67)));
            poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H7), InterpretSingle(Just(_activity.anim77)));

            if (_activity.enablePermutations)
            {
                poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H0), InterpretAnalog(Just(_activity.anim00), Otherwise(_activity.anim10, _activity.anim01), HandSide.LeftHand));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H0), InterpretSingle(Otherwise(_activity.anim20, _activity.anim02)));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H0), InterpretSingle(Otherwise(_activity.anim30, _activity.anim03)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H0), InterpretSingle(Otherwise(_activity.anim40, _activity.anim04)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H0), InterpretSingle(Otherwise(_activity.anim50, _activity.anim05)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H0), InterpretSingle(Otherwise(_activity.anim60, _activity.anim06)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H0), InterpretSingle(Otherwise(_activity.anim70, _activity.anim07)));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim20, _activity.anim02), Otherwise(_activity.anim21, _activity.anim12), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim30, _activity.anim03), Otherwise(_activity.anim31, _activity.anim13), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim40, _activity.anim04), Otherwise(_activity.anim41, _activity.anim14), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim50, _activity.anim05), Otherwise(_activity.anim51, _activity.anim15), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim60, _activity.anim06), Otherwise(_activity.anim61, _activity.anim16), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H1), InterpretAnalog(Otherwise(_activity.anim70, _activity.anim07), Otherwise(_activity.anim71, _activity.anim17), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H2), InterpretSingle(Otherwise(_activity.anim32, _activity.anim23)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H2), InterpretSingle(Otherwise(_activity.anim42, _activity.anim24)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H2), InterpretSingle(Otherwise(_activity.anim52, _activity.anim25)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H2), InterpretSingle(Otherwise(_activity.anim62, _activity.anim26)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H2), InterpretSingle(Otherwise(_activity.anim72, _activity.anim27)));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H3), InterpretSingle(Otherwise(_activity.anim43, _activity.anim34)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H3), InterpretSingle(Otherwise(_activity.anim53, _activity.anim35)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H3), InterpretSingle(Otherwise(_activity.anim63, _activity.anim36)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H3), InterpretSingle(Otherwise(_activity.anim73, _activity.anim37)));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H4), InterpretSingle(Otherwise(_activity.anim54, _activity.anim45)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H4), InterpretSingle(Otherwise(_activity.anim64, _activity.anim46)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H4), InterpretSingle(Otherwise(_activity.anim74, _activity.anim47)));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H5), InterpretSingle(Otherwise(_activity.anim65, _activity.anim56)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H5), InterpretSingle(Otherwise(_activity.anim75, _activity.anim57)));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H6), InterpretSingle(Otherwise(_activity.anim76, _activity.anim67)));
            }
            else
            {
                poses.Add(Permutation.LeftRight(HandPose.H1, HandPose.H0), InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), HandSide.LeftHand));
                poses.Add(Permutation.LeftRight(HandPose.H2, HandPose.H1), InterpretAnalog(Just(_activity.anim02), Just(_activity.anim12), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H3, HandPose.H1), InterpretAnalog(Just(_activity.anim03), Just(_activity.anim13), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H4, HandPose.H1), InterpretAnalog(Just(_activity.anim04), Just(_activity.anim14), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H5, HandPose.H1), InterpretAnalog(Just(_activity.anim05), Just(_activity.anim15), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H6, HandPose.H1), InterpretAnalog(Just(_activity.anim06), Just(_activity.anim16), HandSide.RightHand));
                poses.Add(Permutation.LeftRight(HandPose.H7, HandPose.H1), InterpretAnalog(Just(_activity.anim07), Just(_activity.anim17), HandSide.RightHand));

                var combos = poses
                    .Where(pair => !pair.Key.IsSymmetrical())
                    .Where(pair => !pair.Key.HasAnyFist())
                    .ToDictionary(pair => Permutation.LeftRight(pair.Key.Right, pair.Key.Left), pair => pair.Value);
                foreach (var pair in combos)
                {
                    poses.Add(pair.Key, pair.Value);
                }
            }

            return new PermutationManifest(poses, _activity.transitionDuration);
        }

        private PermutationManifest OneHand()
        {
            var poses = new Dictionary<Permutation, IAnimatedBehavior>();
            var handSide = _activity.oneHandMode == ComboGestureActivity.CgeOneHandMode.LeftHandOnly ? HandSide.LeftHand : HandSide.RightHand;
            for (var activeHand = HandPose.H0; activeHand <= HandPose.H7; activeHand++)
            {
                for (var ignoredHand = HandPose.H0; ignoredHand <= HandPose.H7; ignoredHand++)
                {
                    var permutation = handSide == HandSide.LeftHand ? Permutation.LeftRight(activeHand, ignoredHand) : Permutation.LeftRight(ignoredHand, activeHand);
                    if (_universalAnalogSupport)
                    {
                        poses.Add(permutation, InterpretAnalog(Just(_activity.anim00), Just(OneHandMotionOf(_activity, activeHand)), handSide));
                    }
                    else if (activeHand == HandPose.H1)
                    {
                        poses.Add(permutation, InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), handSide));
                    }
                    else
                    {
                        poses.Add(permutation, InterpretSingle(Just(OneHandMotionOf(_activity, activeHand))));
                    }
                }
            }

            return new PermutationManifest(poses, _activity.transitionDuration);
        }

        private static Motion OneHandMotionOf(ComboGestureActivity activity, HandPose activeHand)
        {
            switch (activeHand)
            {
                case HandPose.H0: return activity.anim00;
                case HandPose.H1: return activity.anim01;
                case HandPose.H2: return activity.anim02;
                case HandPose.H3: return activity.anim03;
                case HandPose.H4: return activity.anim04;
                case HandPose.H5: return activity.anim05;
                case HandPose.H6: return activity.anim06;
                case HandPose.H7: return activity.anim07;
                default:
                    throw new ArgumentOutOfRangeException(nameof(activeHand), activeHand, null);
            }
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

        private struct NullableMotion
        {
            private Motion _item;

            private NullableMotion(Motion item)
            {
                _item = item == null ? null : item;
            }

            public static NullableMotion OfNullable(Motion item)
            {
                return new NullableMotion(item);
            }

            public NullableMotion Or(Func<Motion> other)
            {
                return _item == null ? OfNullable(other.Invoke()) : this;
            }

            public NullableMotion OrWhen(bool predicateResult, Func<Motion> other)
            {
                return _item == null && predicateResult ? OfNullable(other.Invoke()) : this;
            }

            public Motion Orels(Motion nonNullableItem)
            {
                return _item == null ? nonNullableItem : _item;
            }
        }

        private Dictionary<Permutation, Motion> NormalizeComboLike(Dictionary<Permutation,Motion> permutationToNullableMotions, bool explicitMode = false)
        {
            var nonNullableFallback = NullableMotion.OfNullable(permutationToNullableMotions[Permutation.LeftRight(HandPose.H0, HandPose.H0)])
                                                               .Orels(_defaultClip);
            return Permutation.All()
                .ToDictionary(permutation => permutation, permutation =>
                {
                    if (permutation.IsSymmetrical())
                    {
                        return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                               .OrWhen(!explicitMode, () => permutationToNullableMotions[Permutation.LeftRight(HandPose.H0, permutation.Right)])
                               .Orels(nonNullableFallback);
                    }

                    var implicitWithFist = !explicitMode && permutation.HasAnyFist();
                    if (permutation.IsOrangeSide())
                    {
                        return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                            .Or(() => permutationToNullableMotions[permutation.ToOppositeSide()])
                            .OrWhen(implicitWithFist, () => permutationToNullableMotions[Permutation.LeftRight(permutation.Left, HandPose.H0)])
                            .OrWhen(implicitWithFist, () => permutationToNullableMotions[Permutation.LeftRight(HandPose.H0, permutation.Left)])
                            .Orels(nonNullableFallback);
                    }

                    return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                        .OrWhen(implicitWithFist, () => permutationToNullableMotions[Permutation.LeftRight(HandPose.H0, permutation.Right)])
                        .Orels(nonNullableFallback);
                });
        }

        private static readonly Dictionary<Permutation, Func<ComboGestureActivity, Motion>> PermutationLookup;
        static ManifestFromActivity() {
            PermutationLookup = new Dictionary<Permutation, Func<ComboGestureActivity, Motion>>
            {
                { Permutation.LeftRight(HandPose.H0, HandPose.H0), activity => activity.anim00 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H1), activity => activity.anim01 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H2), activity => activity.anim02 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H3), activity => activity.anim03 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H4), activity => activity.anim04 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H5), activity => activity.anim05 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H6), activity => activity.anim06 },
                { Permutation.LeftRight(HandPose.H0, HandPose.H7), activity => activity.anim07 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H0), activity => activity.anim10 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H1), activity => activity.anim11 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H2), activity => activity.anim12 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H3), activity => activity.anim13 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H4), activity => activity.anim14 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H5), activity => activity.anim15 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H6), activity => activity.anim16 },
                { Permutation.LeftRight(HandPose.H1, HandPose.H7), activity => activity.anim17 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H0), activity => activity.anim20 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H1), activity => activity.anim21 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H2), activity => activity.anim22 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H3), activity => activity.anim23 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H4), activity => activity.anim24 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H5), activity => activity.anim25 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H6), activity => activity.anim26 },
                { Permutation.LeftRight(HandPose.H2, HandPose.H7), activity => activity.anim27 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H0), activity => activity.anim30 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H1), activity => activity.anim31 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H2), activity => activity.anim32 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H3), activity => activity.anim33 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H4), activity => activity.anim34 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H5), activity => activity.anim35 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H6), activity => activity.anim36 },
                { Permutation.LeftRight(HandPose.H3, HandPose.H7), activity => activity.anim37 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H0), activity => activity.anim40 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H1), activity => activity.anim41 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H2), activity => activity.anim42 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H3), activity => activity.anim43 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H4), activity => activity.anim44 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H5), activity => activity.anim45 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H6), activity => activity.anim46 },
                { Permutation.LeftRight(HandPose.H4, HandPose.H7), activity => activity.anim47 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H0), activity => activity.anim50 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H1), activity => activity.anim51 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H2), activity => activity.anim52 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H3), activity => activity.anim53 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H4), activity => activity.anim54 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H5), activity => activity.anim55 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H6), activity => activity.anim56 },
                { Permutation.LeftRight(HandPose.H5, HandPose.H7), activity => activity.anim57 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H0), activity => activity.anim60 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H1), activity => activity.anim61 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H2), activity => activity.anim62 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H3), activity => activity.anim63 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H4), activity => activity.anim64 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H5), activity => activity.anim65 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H6), activity => activity.anim66 },
                { Permutation.LeftRight(HandPose.H6, HandPose.H7), activity => activity.anim67 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H0), activity => activity.anim70 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H1), activity => activity.anim71 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H2), activity => activity.anim72 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H3), activity => activity.anim73 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H4), activity => activity.anim74 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H5), activity => activity.anim75 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H6), activity => activity.anim76 },
                { Permutation.LeftRight(HandPose.H7, HandPose.H7), activity => activity.anim77 },
            };
        }

        private Dictionary<Permutation, Motion> PermutationToNullableMotions()
        {
            return Permutation.All()
                .ToDictionary(permutation => permutation, permutation => PermutationLookup[permutation](_activity));
        }
    }
}
