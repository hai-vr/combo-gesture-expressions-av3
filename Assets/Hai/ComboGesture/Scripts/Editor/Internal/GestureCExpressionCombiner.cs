using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class GestureCExpressionCombiner
    {
        private readonly AssetContainer _assetContainer;
        private CgeAacFlLayer _layer;
        private readonly Dictionary<IAnimatedBehavior, TransitionCondition> _intermediateToCombo;
        private readonly string _activityStageName;
        private readonly bool _writeDefaultsForFaceExpressions;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;
        private readonly CgeAacFlState _defaultState;

        public GestureCExpressionCombiner(AssetContainer assetContainer, CgeAacFlLayer layer,
            Dictionary<IAnimatedBehavior, TransitionCondition> intermediateToCombo, string activityStageName, bool writeDefaultsForFaceExpressions, bool useGestureWeightCorrection, bool useSmoothing, CgeAacFlState defaultState)
        {
            _assetContainer = assetContainer;
            _layer = layer;
            _activityStageName = activityStageName;
            _writeDefaultsForFaceExpressions = writeDefaultsForFaceExpressions;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _defaultState = defaultState;
            _intermediateToCombo = intermediateToCombo;
        }

        public void Populate()
        {
            var stageIdxs = _intermediateToCombo
                .Select(pair => pair.Value.StageValue)
                .Distinct()
                .ToArray();

            var intern = _layer.NewSubStateMachine("Internal");
            intern.Restarts();
            _defaultState.CGE_AutomaticallyMovesTo(intern);

            var ssms = stageIdxs.ToDictionary(
                stage => stage,
                stage => intern.NewSubStateMachine($"Activity {stage}")
            );

            foreach (var stageToSsm in ssms)
            {
                var mine = _intermediateToCombo
                    .Where(pair => pair.Value.StageValue == stageToSsm.Key)
                    .ToArray();
                foreach (var behaviourToCondition in mine)
                {
                    AppendToSsm(stageToSsm.Value, behaviourToCondition.Key, behaviourToCondition.Value);
                }

                stageToSsm.Value.Restarts().When(_layer.IntParameter(_activityStageName).IsEqualTo(stageToSsm.Key));
                stageToSsm.Value.Exits();
            }

            if (_activityStageName != null)
            {
                // Order of execution matters here
                foreach (var destSsm in ssms)
                {
                    intern.EntryTransitionsTo(destSsm.Value)
                        .When(_layer.IntParameter(_activityStageName).IsEqualTo(destSsm.Key));
                }

                // Order of execution matters here
                var neutral = intern.NewState("Neutral");
                intern.WithDefaultState(neutral);
                intern.EntryTransitionsTo(neutral);

                foreach (var destSsm in ssms)
                {
                    neutral.Exits()
                        .When(_layer.IntParameter(_activityStageName).IsEqualTo(destSsm.Key));
                }
            }
            else
            {
                foreach (var destSsm in ssms)
                {
                    intern.EntryTransitionsTo(destSsm.Value);
                }
            }
        }

        private void AppendToSsm(CgeAacFlStateMachine ssm, IAnimatedBehavior behaviour, TransitionCondition condition)
        {
            switch (behaviour.Nature())
            {
                case AnimatedBehaviorNature.Single:
                    var sab = (SingleAnimatedBehavior)behaviour;
                    ForSingle(ssm, condition, sab);
                    break;
                case AnimatedBehaviorNature.Analog:
                    AnalogAnimatedBehavior aab = (AnalogAnimatedBehavior)behaviour;
                    ForAnalog(ssm, condition, aab.Squeezing.Clip, aab.Resting.Clip, aab.HandSide);
                    break;
                case AnimatedBehaviorNature.PuppetToAnalog:
                    PuppetToAnalogAnimatedBehavior ptaab = (PuppetToAnalogAnimatedBehavior)behaviour;
                    ForAnalog(ssm, condition, ptaab.Squeezing.Clip, ptaab.Resting, ptaab.HandSide);
                    break;
                case AnimatedBehaviorNature.DualAnalog:
                    DualAnalogAnimatedBehavior daab = (DualAnalogAnimatedBehavior)behaviour;
                    ForDualAnalog(ssm, condition, daab.BothSqueezing.Clip, daab.Resting.Clip, daab.LeftSqueezing.Clip, daab.RightSqueezing.Clip);
                    break;
                case AnimatedBehaviorNature.PuppetToDualAnalog:
                    PuppetToDualAnalogAnimatedBehavior ptdaab = (PuppetToDualAnalogAnimatedBehavior)behaviour;
                    ForDualAnalog(ssm, condition, ptdaab.BothSqueezing.Clip, ptdaab.Resting, ptdaab.LeftSqueezing.Clip, ptdaab.RightSqueezing.Clip);
                    break;
                case AnimatedBehaviorNature.Puppet:
                    var pab = (PuppetAnimatedBehavior)behaviour;
                    ForPuppet(ssm, condition, pab);
                    break;
                case AnimatedBehaviorNature.SimpleMassiveBlend:
                    SimpleMassiveBlendAnimatedBehavior smbab = (SimpleMassiveBlendAnimatedBehavior)behaviour;
                    ForSimpleMassiveBlend(ssm, condition, smbab.Zero, smbab.One, smbab.ParameterName);
                    break;
                case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                    TwoDirectionsMassiveBlendAnimatedBehavior tdmb = (TwoDirectionsMassiveBlendAnimatedBehavior)behaviour;
                    ForTwoDirectionsMassiveBlend(ssm, condition, tdmb.Zero, tdmb.One, tdmb.MinusOne, tdmb.ParameterName);
                    break;
                case AnimatedBehaviorNature.ComplexMassiveBlend:
                    ComplexMassiveBlendAnimatedBehavior cbtmbab = (ComplexMassiveBlendAnimatedBehavior)behaviour;
                    ForComplexMassiveBlend(ssm, condition, cbtmbab.Behaviors, cbtmbab.OriginalBlendTreeTemplate);
                    break;
                case AnimatedBehaviorNature.UniversalAnalog:
                    UniversalAnalogAnimatedBehavior uaab = (UniversalAnalogAnimatedBehavior)behaviour;
                    ForDualAnalog(ssm, condition, uaab.BothSqueezing.Clip, uaab.Resting.ToMotion(), uaab.LeftSqueezing.ToMotion(), uaab.RightSqueezing.ToMotion());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                        SanitizeName(UnshimName(aab.Resting.Clip.name) + " MB " + UnshimName((aab.Squeezing.Clip.name))));
                case AnimatedBehaviorNature.PuppetToAnalog:
                    PuppetToAnalogAnimatedBehavior pta = (PuppetToAnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        pta.Resting,
                        pta.Squeezing.Clip,
                        pta.HandSide == HandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing),
                        SanitizeName(UnshimName(pta.Resting.name) + " MB " + UnshimName((pta.Squeezing.Clip.name))));
                case AnimatedBehaviorNature.DualAnalog:
                    DualAnalogAnimatedBehavior da = (DualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(da.Resting.Clip, da.BothSqueezing.Clip, da.LeftSqueezing.Clip, da.LeftSqueezing.Clip, SanitizeName(UnshimName(da.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.PuppetToDualAnalog:
                    PuppetToDualAnalogAnimatedBehavior ptda = (PuppetToDualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(ptda.Resting, ptda.BothSqueezing.Clip, ptda.LeftSqueezing.Clip, ptda.LeftSqueezing.Clip, SanitizeName(UnshimName(ptda.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.Puppet:
                    return ((PuppetAnimatedBehavior)behavior).Tree;
                case AnimatedBehaviorNature.UniversalAnalog:
                    UniversalAnalogAnimatedBehavior uaab = (UniversalAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(uaab.Resting.ToMotion(), uaab.BothSqueezing.Clip, uaab.LeftSqueezing.ToMotion(), uaab.LeftSqueezing.ToMotion(), SanitizeName(UnshimName(uaab.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.SimpleMassiveBlend:
                case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                case AnimatedBehaviorNature.ComplexMassiveBlend:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ForSimpleMassiveBlend(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var state = CreateSimpleMassiveBlendState(zeroMotion, oneMotion, parameterName, ToPotentialGridPosition(transitionCondition), ssm);
            CreateTransitions(GetNullableStageValue(transitionCondition),
                transitionCondition.PermutationNullable,
                state, transitionCondition.TransitionDuration, ssm);
        }

        private void ForTwoDirectionsMassiveBlend(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var minusOneMotion = Derive(minusOne);
            var state = CreateTwoDirectionsMassiveBlendState(zeroMotion, oneMotion, minusOneMotion, parameterName, ToPotentialGridPosition(transitionCondition), ssm);
            CreateTransitions(GetNullableStageValue(transitionCondition),
                transitionCondition.PermutationNullable,
                state, transitionCondition.TransitionDuration, ssm);
        }

        private void ForComplexMassiveBlend(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, List<IAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            var motions = behaviors.Select(Derive).ToList();
            var state = CreateComplexMassiveBlendState(motions, originalBlendTreeTemplate, ToPotentialGridPosition(transitionCondition), ssm);

            CreateTransitions(GetNullableStageValue(transitionCondition),
                transitionCondition.PermutationNullable,
                state, transitionCondition.TransitionDuration, ssm);
        }

        private CgeAacFlState CreateSimpleMassiveBlendState(Motion zero, Motion one, string parameterName, Vector3 position, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(zero.name) + " massive " + UnshimName(one.name);
            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(CreateBlendTree(
                    zero,
                    one,
                    parameterName,
                    clipName))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreateTwoDirectionsMassiveBlendState(Motion zero, Motion one, Motion minusOne, string parameterName, Vector3 position, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(zero.name) + " - " + UnshimName(one.name) + " - " + UnshimName(minusOne.name);
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

            RegisterBlendTreeAsAsset(blendTree);

            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(blendTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private void ForSingle(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, SingleAnimatedBehavior intermediateAnimationGroup)
        {
            var state = CreateMotionState(intermediateAnimationGroup.Posing.Clip,
                ToPotentialGridPosition(transitionCondition), ssm);
            CreateTransitions(GetNullableStageValue(transitionCondition),
                transitionCondition.PermutationNullable,
                state,
                transitionCondition.TransitionDuration,
                ssm);
        }

        private CgeAacFlState CreateComplexMassiveBlendState(List<Motion> motions, BlendTree originalBlendTreeTemplate, Vector3 position, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(originalBlendTreeTemplate.name) + " complex";

            var newBlendTree = CopyTreeIdentically(originalBlendTreeTemplate);
            newBlendTree.children = newBlendTree.children
                .Select((childMotion, index) =>
                {
                    var copy = childMotion;
                    copy.motion = motions[index];
                    return copy;
                })
                .ToArray();

            RegisterBlendTreeAsAsset(newBlendTree);

            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(newBlendTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
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
            return GridPosition((int)transitionCondition.PermutationNullable.Right, (int)transitionCondition.PermutationNullable.Left);
        }

        private void ForAnalog(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, Motion squeezing, Motion resting, HandSide handSide)
        {
            var nullableTransition = GetNullableStageValue(transitionCondition);
            var offset = ToPotentialGridPosition(transitionCondition);

            var blendState = CreateSidedBlendState(squeezing, offset, resting, Vector3.zero, handSide == HandSide.LeftHand, ssm);

            CreateTransitions(nullableTransition, transitionCondition.PermutationNullable, blendState, transitionCondition.TransitionDuration, ssm);
        }

        // private void ForDualAnalogOneHanded(List<TransitionCondition> transitionConditions, Motion squeezing, Motion resting, bool isLeftActive)
        // {
        //     AnimatorState blendState = null;
        //     foreach (var transitionCondition in transitionConditions)
        //     {
        //         var nullableTransition = GetNullableStageValue(transitionCondition);
        //         if (blendState == null)
        //         {
        //             blendState = CreateSidedBlendState(squeezing,
        //                 ToPotentialGridPosition(transitionCondition),
        //                 resting,
        //                 Vector3.zero,
        //                 isLeftActive);
        //         }
        //
        //         CreateTransitions(nullableTransition, transitionCondition.Permutation, blendState,
        //             transitionCondition.TransitionDuration);
        //     }
        // }

        private void ForDualAnalog(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, Motion bothSqueezing, Motion resting, Motion leftSqueezingClip, Motion rightSqueezingClip)
        {
            var nullableTransition = GetNullableStageValue(transitionCondition);
            var blendState = CreateDualBlendState(bothSqueezing,
                resting, ToPotentialGridPosition(transitionCondition),
                leftSqueezingClip, rightSqueezingClip, ssm);

            CreateTransitions(nullableTransition, transitionCondition.PermutationNullable, blendState,
                transitionCondition.TransitionDuration, ssm);
        }

        private void ForPuppet(CgeAacFlStateMachine ssm, TransitionCondition transitionCondition, PuppetAnimatedBehavior intermediateAnimationGroup)
        {
            var nullableTransition = GetNullableStageValue(transitionCondition);
            var blendState = CreatePuppetState(intermediateAnimationGroup.Tree, transitionCondition.PermutationNullable == null ? GridPosition(-2, 0) : ToPotentialGridPosition(transitionCondition), ssm);

            CreateTransitions(nullableTransition, transitionCondition.PermutationNullable, blendState, transitionCondition.TransitionDuration, ssm);
        }

        private CgeAacFlState CreateMotionState(AnimationClip clip, Vector3 gridPosition, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(SanitizeName(UnshimName(clip.name)), (int) gridPosition.x, (int) gridPosition.y)
                .WithAnimation(clip)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private void CreateTransitions(int? stageValue, Permutation permutation, CgeAacFlState state, float transitionDuration, CgeAacFlStateMachine ssm)
        {
            var entryConditions = ssm.EntryTransitionsTo(state).WhenConditions();
            Func<CgeAacFlTransition> exitTransitionGenerator = () => state.Exits()
                .WithTransitionDurationSeconds(transitionDuration);

            if (permutation != null)
            {
                entryConditions.And(_layer.Av3().GestureLeft.IsEqualTo((int) permutation.Left));
                entryConditions.And(_layer.Av3().GestureRight.IsEqualTo((int) permutation.Right));
                exitTransitionGenerator.Invoke().When(_layer.Av3().GestureLeft.IsNotEqualTo((int) permutation.Left));
                exitTransitionGenerator.Invoke().When(_layer.Av3().GestureRight.IsNotEqualTo((int) permutation.Right));
            }
            if (_activityStageName != null && stageValue != null)
            {
                exitTransitionGenerator.Invoke().When(_layer.IntParameter(_activityStageName).IsNotEqualTo((int) stageValue));
            }
        }

        private CgeAacFlState CreateSidedBlendState(Motion squeezing, Vector3 offset, Motion resting, Vector3 gridPosition, bool isLeftSide, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(squeezing.name) + " " + (isLeftSide ? "BlendLeft" : "BlendRight") + " " + UnshimName(resting.name);
            var position = offset + gridPosition;
            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(CreateBlendTree(
                    resting,
                    squeezing,
                    _layer.FloatParameter(isLeftSide
                        ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                        : RightParam(_useGestureWeightCorrection, _useSmoothing)).Name,
                    clipName))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
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

        private CgeAacFlState CreateDualBlendState(Motion clip, Motion resting, Vector3 position, Motion posingLeft, Motion posingRight, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(clip.name) + " Dual " + UnshimName(resting.name);
            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(CreateDualBlendTree(resting, clip, posingLeft, posingRight, clipName, _useGestureWeightCorrection, _useSmoothing))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreatePuppetState(BlendTree tree, Vector3 position, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(tree.name) + " Puppet";
            return ssm.NewState(SanitizeName(clipName), (int) position.x, (int) position.y)
                .WithAnimation(tree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        public Motion CreateBlendTree(Motion atZero, Motion atOne, string weight, string clipName)
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

            RegisterBlendTreeAsAsset(blendTree);

            return blendTree;
        }

        private Motion CreateDualBlendTree(Motion atZero, Motion atOne, Motion atLeft, Motion atRight, string clipName, bool useGestureWeightCorrection, bool useSmoothing)
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

            RegisterBlendTreeAsAsset(blendTree);

            return blendTree;
        }

        private void RegisterBlendTreeAsAsset(BlendTree blendTree)
        {
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(blendTree);
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
            return new Vector3(x, y, 0);
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

