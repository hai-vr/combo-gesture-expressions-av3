using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
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
                        var sab = (SingleAnimatedBehavior)behavior;
                        ForSingle(sab, transitionConditions);
                        break;
                    case AnimatedBehaviorNature.Analog:
                        AnalogAnimatedBehavior aab = (AnalogAnimatedBehavior)behavior;
                        ForAnalog(transitionConditions, aab.Squeezing.Clip, aab.Resting.Clip, aab.HandSide);
                        break;
                    case AnimatedBehaviorNature.PuppetToAnalog:
                        PuppetToAnalogAnimatedBehavior ptaab = (PuppetToAnalogAnimatedBehavior)behavior;
                        ForAnalog(transitionConditions, ptaab.Squeezing.Clip, ptaab.Resting, ptaab.HandSide);
                        break;
                    case AnimatedBehaviorNature.DualAnalog:
                        DualAnalogAnimatedBehavior daab = (DualAnalogAnimatedBehavior)behavior;
                        ForDualAnalog(transitionConditions, daab.BothSqueezing.Clip, daab.Resting.Clip, daab.LeftSqueezing.Clip, daab.RightSqueezing.Clip);
                        break;
                    case AnimatedBehaviorNature.PuppetToDualAnalog:
                        PuppetToDualAnalogAnimatedBehavior ptdaab = (PuppetToDualAnalogAnimatedBehavior)behavior;
                        ForDualAnalog(transitionConditions, ptdaab.BothSqueezing.Clip, ptdaab.Resting, ptdaab.LeftSqueezing.Clip, ptdaab.RightSqueezing.Clip);
                        break;
                    case AnimatedBehaviorNature.Puppet:
                        var pab = (PuppetAnimatedBehavior)behavior;
                        ForPuppet(transitionConditions, pab);
                        break;
                    case AnimatedBehaviorNature.SimpleMassiveBlend:
                        SimpleMassiveBlendAnimatedBehavior smbab = (SimpleMassiveBlendAnimatedBehavior)behavior;
                        ForSimpleMassiveBlend(transitionConditions, smbab.Zero, smbab.One, smbab.ParameterName);
                        break;
                    case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                        TwoDirectionsMassiveBlendAnimatedBehavior tdmb = (TwoDirectionsMassiveBlendAnimatedBehavior)behavior;
                        ForTwoDirectionsMassiveBlend(transitionConditions, tdmb.Zero, tdmb.One, tdmb.MinusOne, tdmb.ParameterName);
                        break;
                    case AnimatedBehaviorNature.ComplexMassiveBlend:
                        ComplexMassiveBlendAnimatedBehavior cbtmbab = (ComplexMassiveBlendAnimatedBehavior)behavior;
                        ForComplexMassiveBlend(transitionConditions, cbtmbab.Behaviors, cbtmbab.OriginalBlendTreeTemplate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private Motion Derive(IAnimatedBehavior behavior)
        {
            switch (behavior.Nature())
            {
                case AnimatedBehaviorNature.Single:
                    return ((SingleAnimatedBehavior)behavior).Posing.Clip;
                case AnimatedBehaviorNature.Analog:
                    AnalogAnimatedBehavior aab = (AnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        aab.Resting.Clip,
                        aab.Squeezing.Clip,
                        aab.HandSide == HandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing),
                        SanitizeName(UnshimName(aab.Resting.Clip.name) + " MB " + UnshimName((aab.Squeezing.Clip.name))),
                        _animatorController);
                case AnimatedBehaviorNature.PuppetToAnalog:
                    PuppetToAnalogAnimatedBehavior pta = (PuppetToAnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        pta.Resting,
                        pta.Squeezing.Clip,
                        pta.HandSide == HandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing),
                        SanitizeName(UnshimName(pta.Resting.name) + " MB " + UnshimName((pta.Squeezing.Clip.name))),
                        _animatorController);
                case AnimatedBehaviorNature.DualAnalog:
                    DualAnalogAnimatedBehavior da = (DualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(da.Resting.Clip, da.BothSqueezing.Clip, da.LeftSqueezing.Clip, da.LeftSqueezing.Clip, SanitizeName(UnshimName(da.BothSqueezing.Clip.name)), _animatorController, _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.PuppetToDualAnalog:
                    PuppetToDualAnalogAnimatedBehavior ptda = (PuppetToDualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(ptda.Resting, ptda.BothSqueezing.Clip, ptda.LeftSqueezing.Clip, ptda.LeftSqueezing.Clip, SanitizeName(UnshimName(ptda.BothSqueezing.Clip.name)), _animatorController, _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.Puppet:
                    return ((PuppetAnimatedBehavior)behavior).Tree;
                case AnimatedBehaviorNature.SimpleMassiveBlend:
                case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                case AnimatedBehaviorNature.ComplexMassiveBlend:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ForSimpleMassiveBlend(List<TransitionCondition> transitionConditions, IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var state = CreateSimpleMassiveBlendState(zeroMotion, oneMotion, parameterName, ToPotentialGridPosition(transitionConditions[0]));
            foreach (var transitionCondition in transitionConditions)
            {
                CreateTransition(GetNullableStageValue(transitionCondition),
                    transitionCondition.Permutation,
                    state, transitionCondition.TransitionDuration);
            }
        }

        private void ForTwoDirectionsMassiveBlend(List<TransitionCondition> transitionConditions, IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var minusOneMotion = Derive(minusOne);
            var state = CreateTwoDirectionsMassiveBlendState(zeroMotion, oneMotion, minusOneMotion, parameterName, ToPotentialGridPosition(transitionConditions[0]));
            foreach (var transitionCondition in transitionConditions)
            {
                CreateTransition(GetNullableStageValue(transitionCondition),
                    transitionCondition.Permutation,
                    state, transitionCondition.TransitionDuration);
            }
        }

        private void ForComplexMassiveBlend(List<TransitionCondition> transitionConditions, List<IAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            foreach (var transitionCondition in transitionConditions)
            {
                var motions = behaviors.Select(Derive).ToList();
                var state = CreateComplexMassiveBlendState(motions, originalBlendTreeTemplate, ToPotentialGridPosition(transitionCondition));

                CreateTransition(GetNullableStageValue(transitionCondition),
                    transitionCondition.Permutation,
                    state, transitionCondition.TransitionDuration);
            }
        }

        private AnimatorState CreateSimpleMassiveBlendState(Motion zero, Motion one, string parameterName, Vector3 gridPosition)
        {
            var clipName = UnshimName(zero.name) + " massive " + UnshimName(one.name);
            var newState = _machine.AddState(SanitizeName(clipName), gridPosition);
            newState.motion = CreateBlendTree(
                zero,
                one,
                parameterName,
                clipName,
                _animatorController);
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
        }

        private AnimatorState CreateTwoDirectionsMassiveBlendState(Motion zero, Motion one, Motion minusOne, string parameterName, Vector3 gridPosition)
        {
            var clipName = UnshimName(zero.name) + " - " + UnshimName(one.name) + " - " + UnshimName(minusOne.name);
            var newState = _machine.AddState(SanitizeName(clipName), gridPosition);
            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = parameterName,
                blendType = BlendTreeType.Simple1D,
                minThreshold = -1,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                    { new ChildMotion {motion = minusOne, timeScale = 1}, new ChildMotion {motion = zero, timeScale = 1}, new ChildMotion {motion = one, timeScale = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(_animatorController, blendTree);

            newState.motion = blendTree;
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
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

        private AnimatorState CreateComplexMassiveBlendState(List<Motion> motions, BlendTree originalBlendTreeTemplate, Vector3 gridPosition)
        {
            var clipName = UnshimName(originalBlendTreeTemplate.name) + " complex";
            var newState = _machine.AddState(SanitizeName(clipName), gridPosition);

            var newBlendTree = CopyTreeIdentically(originalBlendTreeTemplate);
            newBlendTree.children = newBlendTree.children
                .Select((childMotion, index) =>
                {
                    var copy = childMotion;
                    copy.motion = motions[index];
                    return copy;
                })
                .ToArray();

            RegisterBlendTreeAsAsset(_animatorController, newBlendTree);

            newState.motion = newBlendTree;
            newState.writeDefaultValues = _writeDefaultsForFaceExpressions;
            return newState;
        }

        private BlendTree CopyTreeIdentically(BlendTree originalTree)
        {
            var newTree = new BlendTree();

            // Object.Instantiate(...) is triggering some weird issues about assertions failures.
            // Copy the blend tree manually
            newTree.name = "autoBT_" + originalTree.name;
            newTree.blendType = originalTree.blendType;
            newTree.blendParameter = originalTree.blendParameter;
            newTree.blendParameterY = originalTree.blendParameterY;
            newTree.minThreshold = originalTree.minThreshold;
            newTree.maxThreshold = originalTree.maxThreshold;
            newTree.useAutomaticThresholds = originalTree.useAutomaticThresholds;

            var copyOfChildren = originalTree.children;
            while (newTree.children.Length > 0) {
                newTree.RemoveChild(0);
            }

            newTree.children = copyOfChildren
                .Select(childMotion => new ChildMotion
                {
                    motion = childMotion.motion,
                    threshold = childMotion.threshold,
                    position = childMotion.position,
                    timeScale = childMotion.timeScale,
                    cycleOffset = childMotion.cycleOffset,
                    directBlendParameter = childMotion.directBlendParameter,
                    mirror = childMotion.mirror
                })
                .ToArray();

            return newTree;
        }

        private static Vector3 ToPotentialGridPosition(TransitionCondition transitionCondition)
        {
            var positionInList = transitionCondition.LayerOrdinal;
            return GridPosition((int)transitionCondition.Permutation.Right, positionInList * 8 + (int)transitionCondition.Permutation.Left);
        }

        private void ForAnalog(List<TransitionCondition> transitionConditions, Motion squeezing, Motion resting, HandSide handSide)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    Vector3 offset = ToPotentialGridPosition(transitionCondition);

                    blendState = CreateSidedBlendState(squeezing, offset, resting, Vector3.zero, handSide == HandSide.LeftHand);
                }

                CreateTransition(nullableTransition, transitionCondition.Permutation, blendState, transitionCondition.TransitionDuration);
            }
        }

        private void ForDualAnalogOneHanded(List<TransitionCondition> transitionConditions, Motion squeezing, Motion resting, bool isLeftActive)
        {
            AnimatorState blendState = null;
            foreach (var transitionCondition in transitionConditions)
            {
                var nullableTransition = GetNullableStageValue(transitionCondition);
                if (blendState == null)
                {
                    blendState = CreateSidedBlendState(squeezing,
                        ToPotentialGridPosition(transitionCondition),
                        resting,
                        Vector3.zero,
                        isLeftActive);
                }

                CreateTransition(nullableTransition, transitionCondition.Permutation, blendState,
                    transitionCondition.TransitionDuration);
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
            if (_activityStageName != null && stageValue != null)
            {
                transition.AddCondition(IsEqualTo, (int) stageValue, _activityStageName);
            }
            if (permutation == null && stageValue == null)
            {
                transition.hasExitTime = true;
                transition.exitTime = 0f;
            }
        }

        private AnimatorState CreateSidedBlendState(Motion squeezing, Vector3 offset, Motion resting, Vector3 gridPosition, bool isLeftSide)
        {
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

        public static Motion CreateBlendTree(Motion atZero, Motion atOne, string weight, string clipName,
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

