#if UNITY_EDITOR
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Internal
{
    internal class GestureCBlinkingCombiner
    {
        private readonly Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> _combinatorIntermediateToBlinking;
        private readonly string _activityStageName;
        private readonly RawGestureManifest _rgm;
        private const float WeightUpperThreshold = 0.7f;
        private const float WeightLowerThreshold = 1f - WeightUpperThreshold;
        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public GestureCBlinkingCombiner(Dictionary<IntermediateBlinkingGroup,List<BlinkingCondition>> combinatorIntermediateToBlinking, string activityStageName)
        {
            _combinatorIntermediateToBlinking = combinatorIntermediateToBlinking;
            _activityStageName = activityStageName;
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
                        var threshold = items.Key.Posing ? WeightUpperThreshold : WeightLowerThreshold;
                    
                        // TODO: Make this code maintainable
                        if (blinkingCondition.Combo.IsSymmetrical)
                        {
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureRightWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureRightWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureLeftWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureRightWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureLeftWeight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureRightWeight);
                            }
                        }
                        else
                        {
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompiler.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompiler.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureLeftWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompiler.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompiler.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureLeftWeight);
                            }
                            {
                                var transition = restingState.AddTransition(posingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompiler.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompiler.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.Greater, threshold, ComboGestureCompiler.GestureRightWeight);
                            }
                            {
                                var transition = posingState.AddTransition(restingState);
                                ShareBlinkingCondition(transition, blinkingCondition, nullableStageValue);
                                transition.AddCondition(AnimatorConditionMode.Equals, 1, ComboGestureCompiler.GestureRight);
                                transition.AddCondition(AnimatorConditionMode.NotEqual, 1, ComboGestureCompiler.GestureLeft);
                                transition.AddCondition(AnimatorConditionMode.Less, threshold, ComboGestureCompiler.GestureRightWeight);
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
            transition.AddCondition(IsEqualTo, blinkingCondition.Combo.RawValue, ComboGestureCompiler.HaiGestureComboParamName);
            if (nullableStageValue != null) transition.AddCondition(IsEqualTo, (int) nullableStageValue, _activityStageName);
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

#endif