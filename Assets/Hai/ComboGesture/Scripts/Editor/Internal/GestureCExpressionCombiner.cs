using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
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
        private readonly Dictionary<IAnimatedBehavior, List<TransitionCondition>> _intermediateToCombo;
        private readonly string _activityStageName;
        private readonly bool _shouldWriteDefaults;
        private readonly bool _useGestureWeightCorrection;

        public GestureCExpressionCombiner(AnimatorController animatorController, AnimatorStateMachine machine,
            Dictionary<IAnimatedBehavior, List<TransitionCondition>> intermediateToCombo, string activityStageName, bool shouldWriteDefaults, bool useGestureWeightCorrection)
        {
            _animatorController = animatorController;
            _machine = machine;
            _activityStageName = activityStageName;
            _shouldWriteDefaults = shouldWriteDefaults;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _intermediateToCombo = intermediateToCombo;
        }

        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public void Populate()
        {
            foreach (var entry in _intermediateToCombo)
            {
                var intermediateAnimationGroup = entry.Key;
                var transitionConditions = entry.Value;

                switch (intermediateAnimationGroup.Nature())
                {
                    case AnimatedBehaviorNature.Single:
                        ForMotion((SingleAnimatedBehavior)intermediateAnimationGroup, transitionConditions);
                        break;
                    case AnimatedBehaviorNature.Analog:
                        ForBlend(transitionConditions, (AnalogAnimatedBehavior)intermediateAnimationGroup);
                        break;
                    case AnimatedBehaviorNature.DualAnalog:
                        ForBlend(transitionConditions, (DualAnalogAnimatedBehavior)intermediateAnimationGroup);
                        break;
                    case AnimatedBehaviorNature.Puppet:
                        ForPuppet(transitionConditions, (PuppetAnimatedBehavior)intermediateAnimationGroup);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ForMotion(SingleAnimatedBehavior intermediateAnimationGroup,
            List<TransitionCondition> transitionConditions)
        {
            var state = CreateMotionState(intermediateAnimationGroup.Posing.Clip,
                ToPotentialGridPosition(transitionConditions[0]));
            foreach (var transitionCondition in transitionConditions)
            {
                CreateMotionTransition(GetNullableStageValue(transitionCondition), state,
                    transitionCondition.Permutation,
                    transitionCondition.TransitionDuration);
            }
        }

        private static Vector3 ToPotentialGridPosition(TransitionCondition transitionCondition)
        {
            var positionInList = transitionCondition.LayerOrdinal;
            return GridPosition((int)transitionCondition.Permutation.Right, positionInList * 8 + (int)transitionCondition.Permutation.Left);
        }

        private void ForBlend(List<TransitionCondition> transitionConditions,
            AnalogAnimatedBehavior intermediateAnimationGroup)
        {
            AnimatorState leftBlendState = null;
            AnimatorState rightBlendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (leftBlendState == null)
                {
                    leftBlendState = CreateSidedBlendState(intermediateAnimationGroup.Squeezing.Clip,
                        ToPotentialGridPosition(transitionCondition), intermediateAnimationGroup.Resting.Clip,
                        Vector3.zero, ComboNature.BlendLeft);
                }

                if (rightBlendState == null)
                {
                    rightBlendState = CreateSidedBlendState(intermediateAnimationGroup.Squeezing.Clip,
                        ToPotentialGridPosition(transitionCondition), intermediateAnimationGroup.Resting.Clip,
                        Vector3.zero, ComboNature.BlendRight);
                }

                CreateSidedTransition(nullableTransition,
                    leftBlendState, ComboNature.BlendLeft, transitionCondition.TransitionDuration);
                CreateSidedTransition(nullableTransition,
                    rightBlendState, ComboNature.BlendRight, transitionCondition.TransitionDuration);
            }
        }

        private void ForBlend(List<TransitionCondition> transitionConditions,
            DualAnalogAnimatedBehavior intermediateAnimationGroup)
        {
            AnimatorState dualBlendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (dualBlendState == null)
                {
                    dualBlendState = CreateDualBlendState(intermediateAnimationGroup.BothSqueezing.Clip,
                        intermediateAnimationGroup.Resting.Clip, ToPotentialGridPosition(transitionCondition),
                        intermediateAnimationGroup.LeftSqueezing.Clip, intermediateAnimationGroup.RightSqueezing.Clip);
                }

                CreateDualTransition(nullableTransition, transitionCondition.Permutation, dualBlendState,
                    transitionCondition.TransitionDuration);
            }
        }

        private void ForPuppet(List<TransitionCondition> transitionConditions,
            PuppetAnimatedBehavior intermediateAnimationGroup)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    var positionInList = transitionCondition.LayerOrdinal;
                    blendState = CreatePuppetState(intermediateAnimationGroup.Tree, GridPosition(-4, positionInList * 8));
                }

                CreatePuppetTransition(nullableTransition, blendState, transitionCondition.TransitionDuration);
            }
        }

        private void CreateMotionTransition(int? stageValue, AnimatorState state, Permutation comboRawValue,
            float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, (float)(comboRawValue.Left), SharedLayerUtils.GestureLeft);
            transition.AddCondition(IsEqualTo, (float)(comboRawValue.Right), SharedLayerUtils.GestureRight);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private AnimatorState CreateMotionState(AnimationClip clip, Vector3 gridPosition)
        {
            var newState = _machine.AddState(SanitizeName(UnshimName(clip.name)), gridPosition);
            newState.motion = clip;
            newState.writeDefaultValues = _shouldWriteDefaults;
            return newState;
        }

        private void CreateSidedTransition(int? stageValue, AnimatorState state, ComboNature clipNature, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, 1, clipNature == ComboNature.BlendLeft ? SharedLayerUtils.GestureLeft : SharedLayerUtils.GestureRight);
            transition.AddCondition(AnimatorConditionMode.NotEqual, 1,
                clipNature == ComboNature.BlendRight ? SharedLayerUtils.GestureLeft : SharedLayerUtils.GestureRight);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private void CreateDualTransition(int? stageValue, Permutation comboRawValue, AnimatorState state, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            transition.AddCondition(IsEqualTo, (float)(comboRawValue.Left), SharedLayerUtils.GestureLeft);
            transition.AddCondition(IsEqualTo, (float)(comboRawValue.Right), SharedLayerUtils.GestureRight);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private void CreatePuppetTransition(int? stageValue, AnimatorState state, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            if (stageValue != null) transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
        }

        private AnimatorState CreateSidedBlendState(AnimationClip clip, Vector3 offset,
            AnimationClip resting, Vector3 gridPosition, ComboNature clipNature)
        {
            var clipName = UnshimName(clip.name) + " " + clipNature + " " + UnshimName(resting.name);
            var newState = _machine.AddState(SanitizeName(clipName),
                offset + gridPosition + new Vector3(0, clipNature == ComboNature.BlendLeft ? -20 : 20, 0));
            newState.motion = CreateBlendTree(
                resting,
                clip,
                clipNature == ComboNature.BlendLeft
                    ? (_useGestureWeightCorrection ? SharedLayerUtils.HaiGestureComboLeftWeightProxy : "GestureLeftWeight")
                    : (_useGestureWeightCorrection ? SharedLayerUtils.HaiGestureComboRightWeightProxy : "GestureRightWeight"),
                clipName,
                _animatorController);
            newState.writeDefaultValues = _shouldWriteDefaults;
            return newState;
        }

        private AnimatorState CreateDualBlendState(AnimationClip clip, AnimationClip resting, Vector3 position, AnimationClip posingLeft, AnimationClip posingRight)
        {
            var clipName = UnshimName(clip.name) + " Dual " + UnshimName(resting.name);
            var newState = _machine.AddState(SanitizeName(clipName), position);
            newState.motion = CreateDualBlendTree(resting, clip, posingLeft, posingRight, clipName, _animatorController, _useGestureWeightCorrection);
            newState.writeDefaultValues = _shouldWriteDefaults;
            return newState;
        }

        private AnimatorState CreatePuppetState(BlendTree tree, Vector3 position)
        {
            var clipName = UnshimName(tree.name) + " Puppet";
            var newState = _machine.AddState(SanitizeName(clipName), position);
            newState.motion = tree;
            newState.writeDefaultValues = _shouldWriteDefaults;
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

        private static Motion CreateDualBlendTree(Motion atZero, Motion atOne, Motion atLeft, Motion atRight, string clipName,
            AnimatorController animatorController, bool useGestureWeightCorrection)
        {
            ChildMotion[] motions;
            if (atOne == atLeft && atOne == atRight)
            {
                motions = new[]
                {
                    new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.up},
                };
            }
            else
            {
                motions = new[]
                {
                    new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                    new ChildMotion {motion = atLeft, timeScale = 1, position = Vector2.right},
                    new ChildMotion {motion = atRight, timeScale = 1, position = Vector2.up},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right + Vector2.up},
                };
            }


            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = useGestureWeightCorrection ? SharedLayerUtils.HaiGestureComboLeftWeightProxy : "GestureLeftWeight",
                blendParameterY = useGestureWeightCorrection ? SharedLayerUtils.HaiGestureComboRightWeightProxy : "GestureRightWeight",
                blendType = BlendTreeType.FreeformDirectional2D,
                children = motions
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
            switch (transitionCondition)
            {
                case TransitionCondition.ActivityBoundTransitionCondition abc:
                    return abc.StageValue;
                case TransitionCondition.PuppetBoundTransitionCondition pbc:
                    return pbc.StageValue;
                default:
                    return null;
            }
        }

        private static Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * 200, y * 70, 0);
        }

        private static string UnshimName(string shimmedName)
        {
            return shimmedName
                .Replace("zAutogeneratedExp_", "")
                .Replace("zAutogeneratedPup_", "")
                .Replace("_DO_NOT_EDIT", "");
        }

        private static string SanitizeName(string clipName)
        {
            clipName = clipName
                .Replace("Hai_ComboGesture_EmptyClip", "empty")
                .Replace("cge_", "")
                .Replace("__combined__", "+");

            return clipName.Length <= 20 ? clipName : clipName.Substring(0, 20);
        }
    }
}

