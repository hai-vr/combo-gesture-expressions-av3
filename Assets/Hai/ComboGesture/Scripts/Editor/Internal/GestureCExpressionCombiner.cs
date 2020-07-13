using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal enum ComboNature
    {
        BlendLeft, BlendRight
    }

    internal class GestureCExpressionCombiner
    {
        private readonly AnimatorController _animatorController;
        private readonly AnimatorStateMachine _machine;
        private readonly Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> _intermediateToCombo;
        private readonly string _activityStageName;

        public GestureCExpressionCombiner(AnimatorController animatorController, AnimatorStateMachine machine,
            Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> intermediateToCombo, string activityStageName)
        {
            _animatorController = animatorController;
            _machine = machine;
            _activityStageName = activityStageName;
            _intermediateToCombo = intermediateToCombo;
        }

        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public void Populate()
        {
            foreach (var entry in _intermediateToCombo)
            {
                var intermediateAnimationGroup = entry.Key;
                var transitionConditions = entry.Value;

                switch (intermediateAnimationGroup.Nature)
                {
                    case IntermediateNature.Motion:
                        ForMotion(intermediateAnimationGroup, transitionConditions);
                        break;
                    case IntermediateNature.Blend:
                        ForBlend(transitionConditions, intermediateAnimationGroup);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ForMotion(IntermediateAnimationGroup intermediateAnimationGroup,
            List<TransitionCondition> transitionConditions)
        {
            var state = CreateMotionState(intermediateAnimationGroup.Posing,
                ToPotentialGridPosition(transitionConditions[0]));
            foreach (var transitionCondition in transitionConditions)
            {
                CreateMotionTransition(GetNullableStageValue(transitionCondition), state,
                    transitionCondition.Combo.RawValue,
                    transitionCondition.TransitionDuration);
            }
        }

        private static Vector3 ToPotentialGridPosition(TransitionCondition transitionCondition)
        {
            var positionInList = transitionCondition.LayerOrdinal;
            return GridPosition(transitionCondition.Combo.Right, positionInList * 8 + transitionCondition.Combo.Left);
        }

        private void ForBlend(List<TransitionCondition> transitionConditions,
            IntermediateAnimationGroup intermediateAnimationGroup)
        {
            AnimatorState dualBlendState = null;
            AnimatorState leftBlendState = null;
            AnimatorState rightBlendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (transitionCondition.Combo.IsSymmetrical)
                {
                    if (dualBlendState == null)
                    {
                        dualBlendState = CreateDualBlendState(intermediateAnimationGroup.Posing,
                            intermediateAnimationGroup.Resting, ToPotentialGridPosition(transitionCondition));
                    }

                    CreateDualTransition(nullableTransition, transitionCondition.Combo.RawValue, dualBlendState,
                        transitionCondition.TransitionDuration);
                }
                else
                {
                    if (leftBlendState == null)
                    {
                        leftBlendState = CreateSidedBlendState(intermediateAnimationGroup.Posing,
                            ToPotentialGridPosition(transitionCondition), intermediateAnimationGroup.Resting,
                            Vector3.zero, ComboNature.BlendLeft);
                    }

                    if (rightBlendState == null)
                    {
                        rightBlendState = CreateSidedBlendState(intermediateAnimationGroup.Posing,
                            ToPotentialGridPosition(transitionCondition), intermediateAnimationGroup.Resting,
                            Vector3.zero, ComboNature.BlendRight);
                    }

                    CreateSidedTransition(nullableTransition, transitionCondition.Combo.RawValue,
                        leftBlendState, ComboNature.BlendLeft, transitionCondition.TransitionDuration);
                    CreateSidedTransition(nullableTransition, transitionCondition.Combo.RawValue,
                        rightBlendState, ComboNature.BlendRight, transitionCondition.TransitionDuration);
                }
            }
        }

        private void CreateMotionTransition(int? stageValue, AnimatorState state, int comboRawValue,
            float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, comboRawValue, ComboGestureCompilerInternal.HaiGestureComboParamName);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private AnimatorState CreateMotionState(AnimationClip clip, Vector3 gridPosition)
        {
            var newState = _machine.AddState(clip.name, gridPosition);
            newState.motion = clip;
            newState.writeDefaultValues = true;
            return newState;
        }

        private void CreateSidedTransition(int? stageValue,
            int comboRawValue, AnimatorState state, ComboNature clipNature, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, comboRawValue, ComboGestureCompilerInternal.HaiGestureComboParamName);
            transition.AddCondition(IsEqualTo, 1, clipNature == ComboNature.BlendLeft ? "GestureLeft" : "GestureRight");
            transition.AddCondition(AnimatorConditionMode.NotEqual, 1,
                clipNature == ComboNature.BlendRight ? "GestureLeft" : "GestureRight");
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private void CreateDualTransition(int? stageValue, int comboRawValue, AnimatorState state, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, comboRawValue, ComboGestureCompilerInternal.HaiGestureComboParamName);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private AnimatorState CreateSidedBlendState(AnimationClip clip, Vector3 offset,
            AnimationClip resting, Vector3 gridPosition, ComboNature clipNature)
        {
            var clipName = clip.name + " " + clipNature + " " + resting.name;
            var newState = _machine.AddState(clipName,
                offset + gridPosition + new Vector3(0, clipNature == ComboNature.BlendLeft ? -20 : 20, 0));
            newState.motion = CreateBlendTree(resting, clip,
                clipNature == ComboNature.BlendLeft ? "GestureLeftWeight" : "GestureRightWeight", clipName,
                _animatorController);
            newState.writeDefaultValues = true;
            return newState;
        }

        private AnimatorState CreateDualBlendState(AnimationClip clip, AnimationClip resting, Vector3 position)
        {
            var clipName = clip.name + " Dual " + resting.name;
            var newState = _machine.AddState(clipName, position);
            newState.motion = CreateDualBlendTree(resting, clip, clipName, _animatorController);
            newState.writeDefaultValues = true;
            return newState;
        }

        private static Motion CreateBlendTree(Motion atZero, Motion atOne, string weight, string clipName,
            AnimatorController animatorController)
        {
            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = weight,
                blendType = BlendTreeType.Simple1D,
                minThreshold = 0,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                    {new ChildMotion {motion = atZero, timeScale = 1}, new ChildMotion {motion = atOne, timeScale = 1}}
            };

            RegisterBlendTreeAsAsset(animatorController, blendTree);

            return blendTree;
        }

        private static Motion CreateDualBlendTree(Motion atZero, Motion atOne, string clipName,
            AnimatorController animatorController)
        {
            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = "GestureLeftWeight",
                blendParameterY = "GestureRightWeight",
                blendType = BlendTreeType.FreeformDirectional2D,
                children = new[]
                {
                    new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.up}
                }
            };

            RegisterBlendTreeAsAsset(animatorController, blendTree);

            return blendTree;
        }

        private static void RegisterBlendTreeAsAsset(AnimatorController animatorController, BlendTree blendTree)
        {
            if (AssetDatabase.GetAssetPath(animatorController) != "")
            {
                AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(animatorController));
            }
        }

        private static void SetupComboTransition(AnimatorStateTransition transition, float transitionDuration)
        {
            SetupSourceTransition(transition);

            transition.duration = transitionDuration;
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

        private static int? GetNullableStageValue(TransitionCondition transitionCondition)
        {
            return transitionCondition is TransitionCondition.ActivityBoundTransitionCondition condition
                ? condition.StageValue
                : (int?) null;
        }

        private static Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * 200, y * 70, 0);
        }
    }
}

