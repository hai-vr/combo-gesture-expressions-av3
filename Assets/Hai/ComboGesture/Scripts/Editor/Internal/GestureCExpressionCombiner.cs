using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly bool _writeDefaultsForFaceExpressions;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;

        public GestureCExpressionCombiner(AnimatorController animatorController, AnimatorStateMachine machine,
            Dictionary<IAnimatedBehavior, List<TransitionCondition>> intermediateToCombo, string activityStageName, bool writeDefaultsForFaceExpressions, bool useGestureWeightCorrection, bool useSmoothing)
        {
            _animatorController = animatorController;
            _machine = machine;
            _activityStageName = activityStageName;
            _writeDefaultsForFaceExpressions = writeDefaultsForFaceExpressions;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _intermediateToCombo = intermediateToCombo;
        }

        private const AnimatorConditionMode IsEqualTo = AnimatorConditionMode.Equals;

        public void Populate()
        {
            foreach (var entry in _intermediateToCombo)
            {
                var behavior = entry.Key;
                var transitionConditions = entry.Value;

                switch (behavior.Nature())
                {
                    case AnimatedBehaviorNature.Single:
                        ForSingle((SingleAnimatedBehavior)behavior, transitionConditions);
                        break;
                    case AnimatedBehaviorNature.Analog:
                        AnalogAnimatedBehavior intermediateAnimationGroup1 = (AnalogAnimatedBehavior)behavior;
                        ForAnalog(transitionConditions, intermediateAnimationGroup1.Squeezing.Clip, intermediateAnimationGroup1.Resting.Clip);
                        break;
                    case AnimatedBehaviorNature.PuppetToAnalog:
                        PuppetToAnalogAnimatedBehavior pta = (PuppetToAnalogAnimatedBehavior)behavior;
                        ForAnalog(transitionConditions, pta.Squeezing.Clip, pta.Resting);
                        break;
                    case AnimatedBehaviorNature.DualAnalog:
                        DualAnalogAnimatedBehavior intermediateAnimationGroup2 = (DualAnalogAnimatedBehavior)behavior;
                        ForDualAnalog(transitionConditions, intermediateAnimationGroup2.BothSqueezing.Clip, intermediateAnimationGroup2.Resting.Clip, intermediateAnimationGroup2.LeftSqueezing.Clip, intermediateAnimationGroup2.RightSqueezing.Clip);
                        break;
                    case AnimatedBehaviorNature.PuppetToDualAnalog:
                        PuppetToDualAnalogAnimatedBehavior ptda = (PuppetToDualAnalogAnimatedBehavior)behavior;
                        ForDualAnalog(transitionConditions, ptda.BothSqueezing.Clip, ptda.Resting, ptda.LeftSqueezing.Clip, ptda.RightSqueezing.Clip);
                        break;
                    case AnimatedBehaviorNature.Puppet:
                        ForPuppet(transitionConditions, (PuppetAnimatedBehavior)behavior);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ForSingle(SingleAnimatedBehavior intermediateAnimationGroup, List<TransitionCondition> transitionConditions)
        {
            var state = CreateMotionState(intermediateAnimationGroup.Posing.Clip,
                ToPotentialGridPosition(transitionConditions[0]));
            foreach (var transitionCondition in transitionConditions)
            {
                CreateTransition(GetNullableStageValue(transitionCondition),
                    transitionCondition.Permutation,
                    state, transitionCondition.TransitionDuration);
            }
        }

        private static Vector3 ToPotentialGridPosition(TransitionCondition transitionCondition)
        {
            var positionInList = transitionCondition.LayerOrdinal;
            return GridPosition((int)transitionCondition.Permutation.Right, positionInList * 8 + (int)transitionCondition.Permutation.Left);
        }

        private void ForAnalog(List<TransitionCondition> transitionConditions, Motion squeezing, Motion resting)
        {
            AllOfSide(transitionConditions.Where(condition => condition.Permutation.Left == HandPose.H1).ToList(), squeezing, resting);
            AllOfSide(transitionConditions.Where(condition => condition.Permutation.Right == HandPose.H1).ToList(), squeezing, resting);
        }

        private void AllOfSide(List<TransitionCondition> transitionConditions, Motion squeezing, Motion resting)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    blendState = CreateSidedBlendState(squeezing,
                        ToPotentialGridPosition(transitionCondition), resting,
                        Vector3.zero, transitionCondition.Permutation);
                }

                CreateTransition(nullableTransition, transitionCondition.Permutation, blendState, transitionCondition.TransitionDuration);
            }
        }

        private void ForDualAnalog(List<TransitionCondition> transitionConditions, Motion bothSqueezing, Motion resting, Motion leftSqueezingClip, Motion rightSqueezingClip)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    blendState = CreateDualBlendState(bothSqueezing,
                        resting, ToPotentialGridPosition(transitionCondition),
                        leftSqueezingClip, rightSqueezingClip);
                }

                CreateTransition(nullableTransition, transitionCondition.Permutation, blendState,
                    transitionCondition.TransitionDuration);
            }
        }

        private void ForPuppet(List<TransitionCondition> transitionConditions, PuppetAnimatedBehavior intermediateAnimationGroup)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    var positionInList = transitionCondition.LayerOrdinal;
                    blendState = CreatePuppetState(intermediateAnimationGroup.Tree, transitionCondition.Permutation == null ? GridPosition(-2, positionInList) : ToPotentialGridPosition(transitionCondition));
                }

                CreateTransition(nullableTransition, transitionCondition.Permutation, blendState, transitionCondition.TransitionDuration);
            }
        }

        private AnimatorState CreateMotionState(AnimationClip clip, Vector3 gridPosition)
        {
            var newState = _machine.AddState(SanitizeName(UnshimName(clip.name)), gridPosition);
            newState.motion = clip;
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
        }

        private void CreateTransition(int? stageValue, Permutation permutation, AnimatorState state, float transitionDuration)
        {
            var transition = _machine.AddAnyStateTransition(state);
            SetupComboTransition(transition, transitionDuration);
            if (permutation != null)
            {
                transition.AddCondition(IsEqualTo, (float) permutation.Left, SharedLayerUtils.GestureLeft);
                transition.AddCondition(IsEqualTo, (float) permutation.Right, SharedLayerUtils.GestureRight);
            }
            if (stageValue != null)
            {
                transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
            }
            if (permutation == null && stageValue == null)
            {
                transition.hasExitTime = true;
                transition.exitTime = 0f;
            }
        }

        private AnimatorState CreateSidedBlendState(Motion squeezing, Vector3 offset,
            Motion resting, Vector3 gridPosition, Permutation transitionConditionPermutation)
        {
            var isLeftSide = transitionConditionPermutation.Left == HandPose.H1;

            var clipName = UnshimName(squeezing.name) + " " + (isLeftSide ? "BlendLeft" : "BlendRight") + " " + UnshimName(resting.name);
            var newState = _machine.AddState(SanitizeName(clipName), offset + gridPosition);
            newState.motion = CreateBlendTree(
                resting,
                squeezing,
                isLeftSide
                    ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                    : RightParam(_useGestureWeightCorrection, _useSmoothing),
                clipName,
                _animatorController);
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
        }

        private static string LeftParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return SharedLayerUtils.HaiGestureComboLeftWeightSmoothing;

            if (useGestureWeightCorrection)
                return SharedLayerUtils.HaiGestureComboLeftWeightProxy;

            return "GestureLeftWeight";
        }

        private static string RightParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return SharedLayerUtils.HaiGestureComboRightWeightSmoothing;

            if (useGestureWeightCorrection)
                return SharedLayerUtils.HaiGestureComboRightWeightProxy;

            return "GestureRightWeight";
        }

        private AnimatorState CreateDualBlendState(Motion clip, Motion resting, Vector3 position, Motion posingLeft, Motion posingRight)
        {
            var clipName = UnshimName(clip.name) + " Dual " + UnshimName(resting.name);
            var newState = _machine.AddState(SanitizeName(clipName), position);
            newState.motion = CreateDualBlendTree(resting, clip, posingLeft, posingRight, clipName, _animatorController, _useGestureWeightCorrection, _useSmoothing);
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
        }

        private AnimatorState CreatePuppetState(BlendTree tree, Vector3 position)
        {
            var clipName = UnshimName(tree.name) + " Puppet";
            var newState = _machine.AddState(SanitizeName(clipName), position);
            newState.motion = tree;
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
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
                    {new ChildMotion {motion = atZero, timeScale = 1}, new ChildMotion {motion = atOne, timeScale = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(animatorController, blendTree);

            return blendTree;
        }

        private static Motion CreateDualBlendTree(Motion atZero, Motion atOne, Motion atLeft, Motion atRight, string clipName,
            AnimatorController animatorController, bool useGestureWeightCorrection, bool useSmoothing)
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
                blendParameter = LeftParam(useGestureWeightCorrection, useSmoothing),
                blendParameterY = RightParam(useGestureWeightCorrection, useSmoothing),
                blendType = BlendTreeType.FreeformDirectional2D,
                children = motions,
                hideFlags = HideFlags.HideInHierarchy
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
            transition.interruptionSource = TransitionInterruptionSource.None;
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

            return clipName.Length <= 16 ? clipName : clipName.Substring(0, 16);
        }
    }
}

