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
        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public GestureCBlinkingCombiner(Dictionary<IntermediateBlinkingGroup,List<BlinkingCondition>> combinatorIntermediateToBlinking, string activityStageName, float analogBlinkingUpperThreshold)
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
                if (items.Key.Nature == IntermediateNature.Motion)
                {
                    foreach (var blinkingCondition in items.Value)
                    {
                        var nullableStageValue = GetNullableStageValue(blinkingCondition);

                        var transition = restingState.AddTransition(posingState);
                        ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                    }
                }
                else
                {
                    foreach (var blinkingCondition in items.Value)
                    {
                        var nullableStageValue = GetNullableStageValue(blinkingCondition);
                        var threshold = items.Key.Posing ? _weightUpperThreshold : _weightLowerThreshold;

                        // TODO: Make this code maintainable
                        if (blinkingCondition.Combo.IsSymmetrical)
                        {
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                        }
                        else
                        {
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureLeftWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompilerInternal.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompilerInternal.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompilerInternal.GestureRightWeight);
                            }
                        }
                    }
                }
            }
        }

        private void ShareBlinkingCondition(AnimatorStateTransition transition, BlinkingCondition blinkingCondition,
            int? nullableStageValue)
        {
            SetupBlinkingTransition(transition);
            transition.AddCondition(IsEqualTo, blinkingCondition.Combo.RawValue, ComboGestureCompilerInternal.HaiGestureComboParamName);
            if (_activityStageName != null && nullableStageValue != null)
            {
                transition.AddCondition(IsEqualTo, (int) nullableStageValue, _activityStageName);
            }

            // transition.AddCondition(IsEqualTo, 0, GestureComboCompiler.HaiGestureComboDisableBlinkingOverrideParamName);
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
}

