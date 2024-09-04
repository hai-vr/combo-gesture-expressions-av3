using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeManifestFromActivity
    {
        private readonly ComboGestureActivity _activity;
        private readonly AnimationClip _defaultClip;
        private readonly bool _universalAnalogSupport;
        private readonly bool _ignoreAnalogFist;

        private CgeManifestFromActivity(ComboGestureActivity activity, AnimationClip defaultClip, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            _activity = activity;
            _defaultClip = defaultClip;
            _universalAnalogSupport = universalAnalogSupport;
            _ignoreAnalogFist = ignoreAnalogFist;
        }

        public static CgePermutationManifest FromNothing(AnimationClip defaultClip)
        {
            var poses = CgePermutation.All().ToDictionary(
                permutation => permutation,
                permutation => CgeSingleAnimatedBehavior.Of(new CgeQualifiedAnimation(defaultClip, new Qualification(false))));
            return new CgePermutationManifest(poses, 0f);
        }

        public static ICgeManifest FromActivity(ComboGestureActivity activity, AnimationClip defaultClip, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            return new CgeManifestFromActivity(activity, defaultClip, universalAnalogSupport, ignoreAnalogFist).Resolve();
        }

        private ICgeManifest Resolve()
        {
            var isOneHandMode = _activity.activityMode == ComboGestureActivity.CgeActivityMode.LeftHandOnly
                                || _activity.activityMode == ComboGestureActivity.CgeActivityMode.RightHandOnly;
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

        ICgeAnimatedBehavior MaybeDualAnalog()
        {
            if (_ignoreAnalogFist)
            {
                return InterpretSingle(Otherwise(_activity.anim11, _activity.anim00));
            }
            
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
                                return CgeDualAnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, dualClip), Qualify(_activity, dualClip), Qualify(_activity, dualClip));
                            }

                            return CgeDualAnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, JustAnim(_activity.anim11_L)), Qualify(_activity, JustAnim(_activity.anim11_R)), Qualify(_activity, dualClip));
                        }
                        case BlendTree baseTree:
                        {
                            return CgePuppetToDualAnalogAnimatedBehavior.Of(baseTree, Qualify(_activity, JustAnim(_activity.anim11_L)), Qualify(_activity, JustAnim(_activity.anim11_R)), Qualify(_activity, dualClip), QualifyAll(_activity, baseTree));
                        }
                        default:
                            throw new ArgumentException();
                    }

                }
                case BlendTree tree:
                    return CgePuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        ICgeAnimatedBehavior MaybeUniversalAnalog(Motion resting, Motion leftSqueezing, Motion rightSqueezing, Motion bothSqueezing)
        {
            switch (bothSqueezing)
            {
                case AnimationClip dualClip:
                {
                    return CgeUniversalAnalogAnimatedBehavior.Of(
                        Universal(resting),
                        Universal(leftSqueezing),
                        Universal(rightSqueezing),
                        Qualify(_activity, dualClip)
                    );
                }
                case BlendTree tree:
                    return CgePuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        private CgeUniversalAnalogAnimatedBehavior.CgeUniversalQualifier Universal(Motion resting)
        {
            return resting is BlendTree b
                ? CgeUniversalAnalogAnimatedBehavior.CgeUniversalQualifier.OfBlend(b, QualifyAll(_activity, b))
                : CgeUniversalAnalogAnimatedBehavior.CgeUniversalQualifier.OfQualification(Qualify(_activity, (AnimationClip)resting));
        }

        ICgeAnimatedBehavior InterpretSingle(Motion motion)
        {
            switch (motion)
            {
                case AnimationClip clip:
                    return CgeSingleAnimatedBehavior.Of(Qualify(_activity, clip));
                case BlendTree tree:
                    return CgePuppetAnimatedBehavior.Of(tree, QualifyAll(_activity, tree));
                default:
                    throw new ArgumentException();
            }
        }

        ICgeAnimatedBehavior InterpretAnalog(Motion baseMotion, Motion fistMotion, CgeHandSide handSide)
        {
            if (_ignoreAnalogFist)
            {
                return InterpretSingle(Otherwise(fistMotion, baseMotion));
            }
            
            switch (fistMotion)
            {
                case AnimationClip fistClip:
                {
                    switch (baseMotion)
                    {
                        case AnimationClip baseClip:
                            return CgeAnalogAnimatedBehavior.Maybe(Qualify(_activity, baseClip), Qualify(_activity, fistClip), handSide);
                        case BlendTree baseTree:
                            return CgePuppetToAnalogAnimatedBehavior.Of(baseTree, Qualify(_activity, fistClip), QualifyAll(_activity, baseTree), handSide);
                        default:
                            throw new ArgumentException();
                    }
                }
                case BlendTree fistTree:
                    return CgePuppetAnimatedBehavior.Of(fistTree, QualifyAll(_activity, fistTree));
                default:
                    throw new ArgumentException();
            }
        }

        private CgePermutationManifest UniversalAnalog()
        {
            var permutationToNullableMotions = PermutationToNullableMotions();
            var permutationToMotions = NormalizeComboLike(permutationToNullableMotions, _defaultClip);
            var neutral = CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H0);
            var behaviors = CgePermutation.All()
                .ToDictionary(permutation => permutation, current =>
                {
                    if (current.AreBoth(CgeHandPose.H0))
                    {
                        return InterpretSingle(permutationToMotions[current]);
                    }

                    if (current.HasAnyPose(CgeHandPose.H0))
                    {
                        return InterpretAnalog(
                            permutationToMotions[neutral],
                            permutationToMotions[current],
                            current.Left == CgeHandPose.H0 ? CgeHandSide.RightHand : CgeHandSide.LeftHand
                        );
                    }

                    return MaybeUniversalAnalog(
                        permutationToMotions[neutral],
                        permutationToMotions[CgePermutation.LeftRight(current.Left, CgeHandPose.H0)],
                        permutationToMotions[CgePermutation.LeftRight(CgeHandPose.H0, current.Right)],
                        permutationToMotions[current]
                    );
                });

            return new CgePermutationManifest(behaviors, _activity.transitionDuration);
        }

        private CgePermutationManifest Regular()
        {
            var poses = new Dictionary<CgePermutation, ICgeAnimatedBehavior>();
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H0), InterpretSingle(Just(_activity.anim00)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), CgeHandSide.RightHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H2), InterpretSingle(Just(_activity.anim02)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H3), InterpretSingle(Just(_activity.anim03)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H4), InterpretSingle(Just(_activity.anim04)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H5), InterpretSingle(Just(_activity.anim05)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H6), InterpretSingle(Just(_activity.anim06)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H7), InterpretSingle(Just(_activity.anim07)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H1), MaybeDualAnalog());
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H2), InterpretAnalog(Just(_activity.anim02), Just(_activity.anim12), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H3), InterpretAnalog(Just(_activity.anim03), Just(_activity.anim13), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H4), InterpretAnalog(Just(_activity.anim04), Just(_activity.anim14), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H5), InterpretAnalog(Just(_activity.anim05), Just(_activity.anim15), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H6), InterpretAnalog(Just(_activity.anim06), Just(_activity.anim16), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H7), InterpretAnalog(Just(_activity.anim07), Just(_activity.anim17), CgeHandSide.LeftHand));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H2), InterpretSingle(Just(_activity.anim22)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H3), InterpretSingle(Just(_activity.anim23)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H4), InterpretSingle(Just(_activity.anim24)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H5), InterpretSingle(Just(_activity.anim25)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H6), InterpretSingle(Just(_activity.anim26)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H7), InterpretSingle(Just(_activity.anim27)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H3), InterpretSingle(Just(_activity.anim33)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H4), InterpretSingle(Just(_activity.anim34)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H5), InterpretSingle(Just(_activity.anim35)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H6), InterpretSingle(Just(_activity.anim36)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H7), InterpretSingle(Just(_activity.anim37)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H4), InterpretSingle(Just(_activity.anim44)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H5), InterpretSingle(Just(_activity.anim45)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H6), InterpretSingle(Just(_activity.anim46)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H7), InterpretSingle(Just(_activity.anim47)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H5), InterpretSingle(Just(_activity.anim55)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H6), InterpretSingle(Just(_activity.anim56)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H7), InterpretSingle(Just(_activity.anim57)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H6), InterpretSingle(Just(_activity.anim66)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H7), InterpretSingle(Just(_activity.anim67)));
            poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H7), InterpretSingle(Just(_activity.anim77)));

            if (_activity.activityMode == ComboGestureActivity.CgeActivityMode.Permutations)
            {
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H0), InterpretAnalog(Just(_activity.anim00), Otherwise(_activity.anim10, _activity.anim01), CgeHandSide.LeftHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim20, _activity.anim02)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim30, _activity.anim03)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim40, _activity.anim04)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim50, _activity.anim05)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim60, _activity.anim06)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H0), InterpretSingle(Otherwise(_activity.anim70, _activity.anim07)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim20, _activity.anim02), Otherwise(_activity.anim21, _activity.anim12), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim30, _activity.anim03), Otherwise(_activity.anim31, _activity.anim13), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim40, _activity.anim04), Otherwise(_activity.anim41, _activity.anim14), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim50, _activity.anim05), Otherwise(_activity.anim51, _activity.anim15), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim60, _activity.anim06), Otherwise(_activity.anim61, _activity.anim16), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H1), InterpretAnalog(Otherwise(_activity.anim70, _activity.anim07), Otherwise(_activity.anim71, _activity.anim17), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H2), InterpretSingle(Otherwise(_activity.anim32, _activity.anim23)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H2), InterpretSingle(Otherwise(_activity.anim42, _activity.anim24)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H2), InterpretSingle(Otherwise(_activity.anim52, _activity.anim25)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H2), InterpretSingle(Otherwise(_activity.anim62, _activity.anim26)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H2), InterpretSingle(Otherwise(_activity.anim72, _activity.anim27)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H3), InterpretSingle(Otherwise(_activity.anim43, _activity.anim34)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H3), InterpretSingle(Otherwise(_activity.anim53, _activity.anim35)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H3), InterpretSingle(Otherwise(_activity.anim63, _activity.anim36)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H3), InterpretSingle(Otherwise(_activity.anim73, _activity.anim37)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H4), InterpretSingle(Otherwise(_activity.anim54, _activity.anim45)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H4), InterpretSingle(Otherwise(_activity.anim64, _activity.anim46)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H4), InterpretSingle(Otherwise(_activity.anim74, _activity.anim47)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H5), InterpretSingle(Otherwise(_activity.anim65, _activity.anim56)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H5), InterpretSingle(Otherwise(_activity.anim75, _activity.anim57)));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H6), InterpretSingle(Otherwise(_activity.anim76, _activity.anim67)));
            }
            else
            {
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H0), InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), CgeHandSide.LeftHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim02), Just(_activity.anim12), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim03), Just(_activity.anim13), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim04), Just(_activity.anim14), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim05), Just(_activity.anim15), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim06), Just(_activity.anim16), CgeHandSide.RightHand));
                poses.Add(CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H1), InterpretAnalog(Just(_activity.anim07), Just(_activity.anim17), CgeHandSide.RightHand));

                var combos = poses
                    .Where(pair => !pair.Key.IsSymmetrical())
                    .Where(pair => !pair.Key.HasAnyFist())
                    .ToDictionary(pair => CgePermutation.LeftRight(pair.Key.Right, pair.Key.Left), pair => pair.Value);
                foreach (var pair in combos)
                {
                    poses.Add(pair.Key, pair.Value);
                }
            }

            return new CgePermutationManifest(poses, _activity.transitionDuration);
        }

        private CgeOneHandManifest OneHand()
        {
            var poses = new Dictionary<CgeHandPose, ICgeAnimatedBehavior>();
            var handSide = _activity.activityMode == ComboGestureActivity.CgeActivityMode.LeftHandOnly ? CgeHandSide.LeftHand : CgeHandSide.RightHand;
            foreach (CgeHandPose handPose in Enum.GetValues(typeof(CgeHandPose)))
            {
                if (_universalAnalogSupport)
                {
                    poses.Add(handPose, InterpretAnalog(Just(_activity.anim00), Just(OneHandMotionOf(_activity, handPose)), handSide));
                }
                else if (handPose == CgeHandPose.H1)
                {
                    poses.Add(handPose, InterpretAnalog(Just(_activity.anim00), Just(_activity.anim01), handSide));
                }
                else
                {
                    poses.Add(handPose, InterpretSingle(Just(OneHandMotionOf(_activity, handPose))));
                }
            }

            return new CgeOneHandManifest(poses, _activity.transitionDuration, _activity.activityMode == ComboGestureActivity.CgeActivityMode.LeftHandOnly);
        }

        private static Motion OneHandMotionOf(ComboGestureActivity activity, CgeHandPose activeHand)
        {
            switch (activeHand)
            {
                case CgeHandPose.H0: return activity.anim00;
                case CgeHandPose.H1: return activity.anim01;
                case CgeHandPose.H2: return activity.anim02;
                case CgeHandPose.H3: return activity.anim03;
                case CgeHandPose.H4: return activity.anim04;
                case CgeHandPose.H5: return activity.anim05;
                case CgeHandPose.H6: return activity.anim06;
                case CgeHandPose.H7: return activity.anim07;
                default:
                    throw new ArgumentOutOfRangeException(nameof(activeHand), activeHand, null);
            }
        }

        private static CgeQualifiedAnimation Qualify(ComboGestureActivity activity, AnimationClip clip)
        {
            return new CgeQualifiedAnimation(
                clip,
                new Qualification(activity.blinking.Contains(clip))
            );
        }

        private static List<CgeQualifiedAnimation> QualifyAll(ComboGestureActivity activity, BlendTree tree)
        {
            return CgeManifestFromSingle.AllAnimationsOf(tree)
                .Select(clip => new CgeQualifiedAnimation(
                    clip,
                    new Qualification(activity.blinking.Contains(clip))
                ))
                .ToList();
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

        private Dictionary<CgePermutation, Motion> NormalizeComboLike(Dictionary<CgePermutation,Motion> permutationToNullableMotions, bool explicitMode = false)
        {
            var nonNullableFallback = NullableMotion.OfNullable(permutationToNullableMotions[CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H0)])
                                                               .Orels(_defaultClip);
            return CgePermutation.All()
                .ToDictionary(permutation => permutation, permutation =>
                {
                    if (permutation.IsSymmetrical())
                    {
                        return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                               .OrWhen(!explicitMode, () => permutationToNullableMotions[CgePermutation.LeftRight(CgeHandPose.H0, permutation.Right)])
                               .Orels(nonNullableFallback);
                    }

                    var implicitWithFist = !explicitMode && permutation.HasAnyFist();
                    if (permutation.IsOrangeSide())
                    {
                        return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                            .Or(() => permutationToNullableMotions[permutation.ToOppositeSide()])
                            .OrWhen(implicitWithFist, () => permutationToNullableMotions[CgePermutation.LeftRight(permutation.Left, CgeHandPose.H0)])
                            .OrWhen(implicitWithFist, () => permutationToNullableMotions[CgePermutation.LeftRight(CgeHandPose.H0, permutation.Left)])
                            .Orels(nonNullableFallback);
                    }

                    return NullableMotion.OfNullable(permutationToNullableMotions[permutation])
                        .OrWhen(implicitWithFist, () => permutationToNullableMotions[CgePermutation.LeftRight(CgeHandPose.H0, permutation.Right)])
                        .Orels(nonNullableFallback);
                });
        }

        private static readonly Dictionary<CgePermutation, Func<ComboGestureActivity, Motion>> PermutationLookup;

        static CgeManifestFromActivity() {
            PermutationLookup = new Dictionary<CgePermutation, Func<ComboGestureActivity, Motion>>
            {
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H0), activity => activity.anim00 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H1), activity => activity.anim01 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H2), activity => activity.anim02 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H3), activity => activity.anim03 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H4), activity => activity.anim04 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H5), activity => activity.anim05 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H6), activity => activity.anim06 },
                { CgePermutation.LeftRight(CgeHandPose.H0, CgeHandPose.H7), activity => activity.anim07 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H0), activity => activity.anim10 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H1), activity => activity.anim11 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H2), activity => activity.anim12 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H3), activity => activity.anim13 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H4), activity => activity.anim14 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H5), activity => activity.anim15 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H6), activity => activity.anim16 },
                { CgePermutation.LeftRight(CgeHandPose.H1, CgeHandPose.H7), activity => activity.anim17 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H0), activity => activity.anim20 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H1), activity => activity.anim21 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H2), activity => activity.anim22 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H3), activity => activity.anim23 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H4), activity => activity.anim24 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H5), activity => activity.anim25 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H6), activity => activity.anim26 },
                { CgePermutation.LeftRight(CgeHandPose.H2, CgeHandPose.H7), activity => activity.anim27 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H0), activity => activity.anim30 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H1), activity => activity.anim31 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H2), activity => activity.anim32 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H3), activity => activity.anim33 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H4), activity => activity.anim34 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H5), activity => activity.anim35 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H6), activity => activity.anim36 },
                { CgePermutation.LeftRight(CgeHandPose.H3, CgeHandPose.H7), activity => activity.anim37 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H0), activity => activity.anim40 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H1), activity => activity.anim41 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H2), activity => activity.anim42 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H3), activity => activity.anim43 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H4), activity => activity.anim44 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H5), activity => activity.anim45 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H6), activity => activity.anim46 },
                { CgePermutation.LeftRight(CgeHandPose.H4, CgeHandPose.H7), activity => activity.anim47 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H0), activity => activity.anim50 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H1), activity => activity.anim51 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H2), activity => activity.anim52 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H3), activity => activity.anim53 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H4), activity => activity.anim54 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H5), activity => activity.anim55 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H6), activity => activity.anim56 },
                { CgePermutation.LeftRight(CgeHandPose.H5, CgeHandPose.H7), activity => activity.anim57 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H0), activity => activity.anim60 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H1), activity => activity.anim61 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H2), activity => activity.anim62 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H3), activity => activity.anim63 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H4), activity => activity.anim64 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H5), activity => activity.anim65 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H6), activity => activity.anim66 },
                { CgePermutation.LeftRight(CgeHandPose.H6, CgeHandPose.H7), activity => activity.anim67 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H0), activity => activity.anim70 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H1), activity => activity.anim71 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H2), activity => activity.anim72 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H3), activity => activity.anim73 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H4), activity => activity.anim74 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H5), activity => activity.anim75 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H6), activity => activity.anim76 },
                { CgePermutation.LeftRight(CgeHandPose.H7, CgeHandPose.H7), activity => activity.anim77 },
            };
        }

        private Dictionary<CgePermutation, Motion> PermutationToNullableMotions()
        {
            return CgePermutation.All()
                .ToDictionary(permutation => permutation, permutation => PermutationLookup[permutation](_activity));
        }
    }
}
