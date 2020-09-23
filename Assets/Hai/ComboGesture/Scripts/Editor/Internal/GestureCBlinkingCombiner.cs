using System;
using System.Collections.Generic;
using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class GestureCBlinkingCombiner
    {
        private readonly Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> _combinatorIntermediateToBlinking;
        private readonly string _activityStageName;
        private readonly RawGestureManifest _rgm;
        private readonly float _weightUpperThreshold;
        private readonly float _weightLowerThreshold;

        private static readonly Dictionary<ValidQuadrant, TripleBlendCondition> QuadrantToTripleBlendMap = new Dictionary<ValidQuadrant, TripleBlendCondition>
        {
            {new ValidQuadrant(true, false, false, false), new TripleBlendCondition(TripleBlendType.Corner, Affinity.Negative, Affinity.Negative)},
            {new ValidQuadrant(false, false, false, true), new TripleBlendCondition(TripleBlendType.Corner, Affinity.Positive, Affinity.Positive)},
            {new ValidQuadrant(false, true, false, false), new TripleBlendCondition(TripleBlendType.Corner, Affinity.Positive, Affinity.Negative)},
            {new ValidQuadrant(false, false, true, false), new TripleBlendCondition(TripleBlendType.Corner, Affinity.Negative, Affinity.Positive)},
            {new ValidQuadrant(false, true, false, true), new TripleBlendCondition(TripleBlendType.Edge, Affinity.Positive, Affinity.None)},
            {new ValidQuadrant(false, false, true, true), new TripleBlendCondition(TripleBlendType.Edge, Affinity.None, Affinity.Positive)},
            {new ValidQuadrant(true, false, true, false), new TripleBlendCondition(TripleBlendType.Edge, Affinity.Negative, Affinity.None)},
            {new ValidQuadrant(true, true, false, false), new TripleBlendCondition(TripleBlendType.Edge, Affinity.None, Affinity.Negative)},
            {new ValidQuadrant(true, true, true, false), new TripleBlendCondition(TripleBlendType.Arrow, Affinity.Negative, Affinity.Negative)},
            {new ValidQuadrant(false, true, true, true), new TripleBlendCondition(TripleBlendType.Arrow, Affinity.Positive, Affinity.Positive)},
            {new ValidQuadrant(true, true, false, true), new TripleBlendCondition(TripleBlendType.Arrow, Affinity.Positive, Affinity.Negative)},
            {new ValidQuadrant(true, false, true, true), new TripleBlendCondition(TripleBlendType.Arrow, Affinity.Negative, Affinity.Positive)},
        };

        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public GestureCBlinkingCombiner(Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> combinatorIntermediateToBlinking, string activityStageName, float analogBlinkingUpperThreshold)
        {
            _combinatorIntermediateToBlinking = combinatorIntermediateToBlinking;
            _activityStageName = activityStageName;
            _weightUpperThreshold = analogBlinkingUpperThreshold;
            _weightLowerThreshold = 1f - _weightUpperThreshold;
        }

        public void Populate(AnimatorState enableBlinking, AnimatorState disableBlinking)
        {
            foreach (var items in _combinatorIntermediateToBlinking)
            {
                var posingState = items.Key.Posing ? disableBlinking : enableBlinking;
                var restingState = !items.Key.Posing ? disableBlinking : enableBlinking;
                switch (items.Key.Nature)
                {
                    case IntermediateNature.Motion:
                    {
                        foreach (var blinkingCondition in items.Value)
                        {
                            var nullableStageValue = GetNullableStageValue(blinkingCondition);

                            var transition = restingState.AddTransition(posingState);
                            ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                        }

                        break;
                    }
                    case IntermediateNature.Blend:
                    {
                        foreach (var blinkingCondition in items.Value)
                        {
                            var nullableStageValue = GetNullableStageValue(blinkingCondition);
                            var threshold = items.Key.Posing ? _weightUpperThreshold : _weightLowerThreshold;
                            var toPosing = items.Key.Posing ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less;
                            var toResting = items.Key.Posing ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater;

                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(toPosing, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(toResting, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(toPosing, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(toResting, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                        }

                        break;
                    }
                    case IntermediateNature.TripleBlend:
                    {
                        foreach (var blinkingCondition in items.Value)
                        {
                            var nullableStageValue = GetNullableStageValue(blinkingCondition);

                            var quadrant = new ValidQuadrant(items.Key.Resting, items.Key.PosingLeft, items.Key.PosingRight, items.Key.Posing);
                            var tripleBlendCondition = QuadrantToTripleBlendMap[quadrant];

                            switch (tripleBlendCondition.TripleBlendType)
                            {
                                case TripleBlendType.Edge:
                                {
                                    HandleEdge(restingState, posingState, blinkingCondition, nullableStageValue, tripleBlendCondition);
                                    break;
                                }
                                case TripleBlendType.Corner:
                                {
                                    HandleCorner(restingState, posingState, blinkingCondition, nullableStageValue, tripleBlendCondition);
                                    break;
                                }
                                case TripleBlendType.Arrow:
                                {
                                    HandleArrow(restingState, posingState, blinkingCondition, nullableStageValue, tripleBlendCondition);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void HandleEdge(AnimatorState restingState, AnimatorState posingState, BlinkingCondition blinkingCondition, int? nullableStageValue, TripleBlendCondition tripleBlendCondition)
        {
            var toPosing = restingState.AddTransition(posingState);
            ShareBlinkingCondition(toPosing, blinkingCondition, nullableStageValue);
            var toResting = posingState.AddTransition(restingState);
            ShareBlinkingCondition(toPosing, blinkingCondition, nullableStageValue);
            if (tripleBlendCondition.Left != Affinity.None)
            {
                if (tripleBlendCondition.Left == Affinity.Positive)
                {
                    toPosing.AddCondition(AnimatorConditionMode.Greater, _weightUpperThreshold, ComboGestureCompilerInternal.GestureLeftWeight);
                    toResting.AddCondition(AnimatorConditionMode.Less, _weightUpperThreshold, ComboGestureCompilerInternal.GestureLeftWeight);
                }
                else
                {
                    toPosing.AddCondition(AnimatorConditionMode.Less, _weightLowerThreshold, ComboGestureCompilerInternal.GestureLeftWeight);
                    toResting.AddCondition(AnimatorConditionMode.Greater, _weightLowerThreshold, ComboGestureCompilerInternal.GestureLeftWeight);
                }
            }
            else
            {
                if (tripleBlendCondition.Right == Affinity.Positive)
                {
                    toPosing.AddCondition(AnimatorConditionMode.Greater, _weightUpperThreshold, ComboGestureCompilerInternal.GestureRightWeight);
                    toResting.AddCondition(AnimatorConditionMode.Less, _weightUpperThreshold, ComboGestureCompilerInternal.GestureRightWeight);
                }
                else
                {
                    toPosing.AddCondition(AnimatorConditionMode.Less, _weightLowerThreshold, ComboGestureCompilerInternal.GestureRightWeight);
                    toResting.AddCondition(AnimatorConditionMode.Greater, _weightLowerThreshold, ComboGestureCompilerInternal.GestureRightWeight);
                }
            }
        }

        private void HandleCorner(AnimatorState restingState, AnimatorState posingState, BlinkingCondition blinkingCondition, int? nullableStageValue, TripleBlendCondition tripleBlendCondition)
        {
            {
                var toPosing = restingState.AddTransition(posingState);
                ShareBlinkingCondition(toPosing, blinkingCondition, nullableStageValue);
                toPosing.AddCondition(
                    tripleBlendCondition.Left == Affinity.Positive ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                    tripleBlendCondition.Left == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureLeftWeight
                );
                toPosing.AddCondition(
                    tripleBlendCondition.Right == Affinity.Positive ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                    tripleBlendCondition.Right == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureRightWeight
                );
            }
            {
                var toResting = posingState.AddTransition(restingState);
                ShareBlinkingCondition(toResting, blinkingCondition, nullableStageValue);
                toResting.AddCondition(
                    tripleBlendCondition.Left == Affinity.Positive ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
                    tripleBlendCondition.Left == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureLeftWeight
                );
            }
            {
                var toResting = posingState.AddTransition(restingState);
                ShareBlinkingCondition(toResting, blinkingCondition, nullableStageValue);
                toResting.AddCondition(
                    tripleBlendCondition.Right == Affinity.Positive ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
                    tripleBlendCondition.Right == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureRightWeight
                );
            }
        }

        private void HandleArrow(AnimatorState restingState, AnimatorState posingState, BlinkingCondition blinkingCondition, int? nullableStageValue, TripleBlendCondition tripleBlendCondition)
        {
            {
                var toPosing = restingState.AddTransition(posingState);
                ShareBlinkingCondition(toPosing, blinkingCondition, nullableStageValue);
                toPosing.AddCondition(
                    tripleBlendCondition.Left == Affinity.Positive ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                    tripleBlendCondition.Left == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureLeftWeight
                );
            }
            {
                var toPosing = restingState.AddTransition(posingState);
                ShareBlinkingCondition(toPosing, blinkingCondition, nullableStageValue);
                toPosing.AddCondition(
                    tripleBlendCondition.Right == Affinity.Positive ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                    tripleBlendCondition.Right == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureRightWeight
                );
            }
            {
                var toResting = posingState.AddTransition(restingState);
                ShareBlinkingCondition(toResting, blinkingCondition, nullableStageValue);
                toResting.AddCondition(
                    tripleBlendCondition.Left == Affinity.Positive ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
                    tripleBlendCondition.Left == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureLeftWeight
                );
                toResting.AddCondition(
                    tripleBlendCondition.Right == Affinity.Positive ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
                    tripleBlendCondition.Right == Affinity.Positive ? _weightUpperThreshold : _weightLowerThreshold,
                    ComboGestureCompilerInternal.GestureRightWeight
                );
            }
        }

        private void ShareBlinkingCondition(AnimatorStateTransition transition, BlinkingCondition blinkingCondition,
            int? nullableStageValue)
        {
            SetupBlinkingTransition(transition);
            transition.AddCondition(IsEqualTo, blinkingCondition.Combo.RawValue, SharedLayerUtils.HaiGestureComboParamName);
            if (_activityStageName != null && nullableStageValue != null)
            {
                transition.AddCondition(IsEqualTo, (int) nullableStageValue, _activityStageName);
            }
        }

        private static void SetupBlinkingTransition(AnimatorStateTransition transition)
        {
            SetupSourceTransition(transition);

            transition.duration = 0;
        }

        private static void SetupSourceTransition(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.offset = 0;
            transition.interruptionSource = TransitionInterruptionSource.Source;
            transition.canTransitionToSelf = false;
            transition.orderedInterruption = true;
        }

        private static int? GetNullableStageValue(BlinkingCondition blinkingCondition)
        {
            return blinkingCondition is BlinkingCondition.ActivityBoundBlinkingCondition ? ((BlinkingCondition.ActivityBoundBlinkingCondition)blinkingCondition).StageValue : (int?) null;
        }
    }

    enum TripleBlendType
        {
            Edge,
            Corner,
            Arrow
        }

        enum Affinity
        {
            None,
            Positive,
            Negative
        }

        struct ValidQuadrant
        {
            public bool Neither { get; }
            public bool Left { get; }
            public bool Right { get; }
            public bool Both { get; }

            public ValidQuadrant(bool neither, bool left, bool right, bool both)
            {
                bool NoneIsBlinking()
                {
                    return !both && !neither && !left && !right;
                }

                bool AllIsBlinking()
                {
                    return both && neither && left && right;
                }

                bool UnsupportedOnlyBlinkingAcross()
                {
                    return both && neither && !left && !right
                           || !both && !neither && left && right;
                }

                if (NoneIsBlinking() || AllIsBlinking() || UnsupportedOnlyBlinkingAcross())
                {
                    throw new ArgumentException("Unsupported quadrant");
                }

                Neither = neither;
                Left = left;
                Right = right;
                Both = both;
            }

            public bool Equals(ValidQuadrant other)
            {
                return Neither == other.Neither && Left == other.Left && Right == other.Right && Both == other.Both;
            }

            public override bool Equals(object obj)
            {
                return obj is ValidQuadrant other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Neither.GetHashCode();
                    hashCode = (hashCode * 397) ^ Left.GetHashCode();
                    hashCode = (hashCode * 397) ^ Right.GetHashCode();
                    hashCode = (hashCode * 397) ^ Both.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(ValidQuadrant left, ValidQuadrant right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ValidQuadrant left, ValidQuadrant right)
            {
                return !left.Equals(right);
            }
        }

        struct TripleBlendCondition
        {
            public TripleBlendType TripleBlendType { get; }
            public Affinity Left { get; }
            public Affinity Right { get; }

            public TripleBlendCondition(TripleBlendType tripleBlendType, Affinity left, Affinity right)
            {
                Left = left;
                Right = right;
                TripleBlendType = tripleBlendType;
            }
        }
}

