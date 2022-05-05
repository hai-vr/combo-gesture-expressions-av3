using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Random = UnityEngine.Random;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class CgeExpressionCombiner
    {
        private readonly CgeAssetContainer _assetContainer;
        private CgeAacFlLayer _layer;
        private readonly List<IComposedBehaviour> _composedBehaviours;
        private readonly string _activityStageName;
        private readonly bool _writeDefaultsForFaceExpressions;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;
        private readonly CgeAacFlState _defaultState;
        private readonly string _mmdCompatibilityToggleParameter;
        private readonly int _layerIndex;

        public CgeExpressionCombiner(CgeAssetContainer assetContainer, CgeAacFlLayer layer,
            List<IComposedBehaviour> composedBehaviours, string activityStageName, bool writeDefaultsForFaceExpressions, bool useGestureWeightCorrection, bool useSmoothing, CgeAacFlState defaultState,
            string mmdCompatibilityToggleParameter,
            int layerIndex)
        {
            _assetContainer = assetContainer;
            _layer = layer;
            _activityStageName = activityStageName;
            _writeDefaultsForFaceExpressions = writeDefaultsForFaceExpressions;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _defaultState = defaultState;
            _mmdCompatibilityToggleParameter = mmdCompatibilityToggleParameter;
            _layerIndex = layerIndex;
            _composedBehaviours = composedBehaviours;
        }

        public void Populate()
        {
            EditorUtility.DisplayProgressBar("ComboGestureExpressions", "Creating sub-state machines", 0f);
            var intern = _layer.NewSubStateMachine("Internal");
            intern.Restarts();
            intern.WithEntryPosition(-1, -1);
            intern.WithExitPosition(1, _composedBehaviours.Count + 1);

            _defaultState.CGE_AutomaticallyMovesTo(intern);

            // This must be the first layer for MMD compatiblity to take over everything else
            if (!string.IsNullOrEmpty(_mmdCompatibilityToggleParameter))
            {
                var mmdOn = intern.NewState("MMD Compatibility ON").At(0, -3);

                var onLayerControl = mmdOn.State.AddStateMachineBehaviour<VRCAnimatorLayerControl>();
                onLayerControl.blendDuration = 0;
                onLayerControl.goalWeight = 0;
                onLayerControl.layer = _layerIndex;

                intern.EntryTransitionsTo(mmdOn)
                    .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsTrue())
                    .And(_layer.Av3().InStation.IsTrue());

                var mmdOff = intern.NewState("MMD Compatibility OFF");

                var offLayerControl = mmdOff.State.AddStateMachineBehaviour<VRCAnimatorLayerControl>();
                offLayerControl.blendDuration = 0;
                offLayerControl.goalWeight = 1;
                offLayerControl.layer = _layerIndex;

                mmdOn.TransitionsTo(mmdOff)
                    .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsFalse())
                    .Or().When(_layer.Av3().InStation.IsFalse());

                mmdOff.Exits().AfterAnimationFinishes();
            }

            var ssms = _composedBehaviours
                .Select((behaviour, i) =>
                {
                    var name = behaviour.IsAvatarDynamics
                        ? behaviour.IsActivityBound
                        ? $"Dynamics {behaviour.DynamicsDescriptor.rank} Act {behaviour.StageValue} #{i}"
                        : $"Dynamics {behaviour.DynamicsDescriptor.rank} #{i}"
                        : $"Activity {behaviour.StageValue}";
                    return intern.NewSubStateMachine(name).At(0, i);
                })
                .ToArray();

            for (var index = 0; index < ssms.Length; index++)
            {
                EditorUtility.DisplayProgressBar("ComboGestureExpressions", $"Creating sub-state machines {index + 1} / {ssms.Length}", (float)index / ssms.Length);
                var ssm = ssms[index];
                var composed = _composedBehaviours[index];

                var dynamicsExiters = Enumerable.Range(0, index)
                    .Select(i => _composedBehaviours[i])
                    .Where(behaviour => behaviour.IsAvatarDynamics)
                    .Where(behaviour => !behaviour.IsActivityBound || behaviour.StageValue == composed.StageValue)
                    .Select(behaviour => behaviour.DynamicsDescriptor.descriptor)
                    .ToArray();

                switch (composed)
                {
                    case PermutationComposedBehaviour pcb:
                        BuildPcb(ssm, pcb, dynamicsExiters);
                        break;
                    case OneHandComposedBehaviour ocb:
                        BuildOcb(ssm, ocb, dynamicsExiters);
                        break;
                    case SingularComposedBehaviour scb:
                        BuildScb(ssm, scb, dynamicsExiters);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Short Restarts are no longer possible with Avatar Dynamics
                // ssm.Restarts().When(ResolveEntrance(composed));
                ssm.Exits();
            }

            // Order of execution matters here
            for (var index = 0; index < ssms.Length; index++)
            {
                var destSsm = ssms[index];
                var composed = _composedBehaviours[index];
                var entryTransition = intern.EntryTransitionsTo(destSsm);
                if (composed.IsAvatarDynamics || _activityStageName != null)
                {
                    entryTransition.When(continuation => ContinuateEntrance(continuation, composed));
                }
            }

            if (_activityStageName != null)
            {
                // Order of execution matters here
                var neutral = intern.NewState("Neutral");
                intern.WithDefaultState(neutral);
                intern.EntryTransitionsTo(neutral);

                if (!string.IsNullOrEmpty(_mmdCompatibilityToggleParameter))
                {
                    neutral.Exits()
                        .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsTrue())
                        .And(_layer.Av3().InStation.IsTrue());
                }
                foreach (var composed in _composedBehaviours)
                {
                    neutral.Exits()
                        .When(continuation => ContinuateEntrance(continuation, composed));
                }
            }
        }

        private void ContinuateEntrance(CgeAacFlTransitionContinuationWithoutOr continuation, IComposedBehaviour composed)
        {
            if (composed.IsAvatarDynamics)
            {
                var descriptor = composed.DynamicsDescriptor.descriptor;
                continuation.And(ResolveEntranceDescriptor(descriptor));
            }

            if (composed.IsActivityBound)
            {
                continuation.And(_layer.IntParameter(_activityStageName).IsEqualTo(composed.StageValue));
            }
        }

        private ICgeAacFlCondition ResolveEntranceDescriptor(CgeDynamicsDescriptor descriptor)
        {
            switch (descriptor.parameterType)
            {
                case ComboGestureDynamicsParameterType.Bool:
                    return _layer.BoolParameter(descriptor.parameter)
                        .IsEqualTo(descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold);
                case ComboGestureDynamicsParameterType.Int:
                    if (descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold)
                    {
                        return _layer.IntParameter(descriptor.parameter)
                            .IsGreaterThan((int) descriptor.threshold);
                    }
                    else
                    {
                        return _layer.IntParameter(descriptor.parameter)
                            .IsLessThan((int) descriptor.threshold + 1);
                    }
                case ComboGestureDynamicsParameterType.Float:
                    if (descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold)
                    {
                        return _layer.FloatParameter(descriptor.parameter)
                            .IsGreaterThan(descriptor.threshold);
                    }
                    else
                    {
                        return _layer.FloatParameter(descriptor.parameter)
                            .IsLessThan(descriptor.threshold + 0.0001f);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ICgeAacFlCondition ResolveExiter(CgeDynamicsDescriptor descriptor)
        {
            switch (descriptor.parameterType)
            {
                case ComboGestureDynamicsParameterType.Bool:
                    return _layer.BoolParameter(descriptor.parameter)
                        .IsNotEqualTo(descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold);
                case ComboGestureDynamicsParameterType.Int:
                    if (descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold)
                    {
                        return _layer.IntParameter(descriptor.parameter)
                            .IsLessThan((int) descriptor.threshold + 1);
                    }
                    else
                    {
                        return _layer.IntParameter(descriptor.parameter)
                            .IsGreaterThan((int) descriptor.threshold);
                    }
                case ComboGestureDynamicsParameterType.Float:
                    if (descriptor.condition == ComboGestureDynamicsCondition.IsAboveThreshold)
                    {
                        return _layer.FloatParameter(descriptor.parameter)
                            .IsLessThan(descriptor.threshold + 0.0001f);
                    }
                    else
                    {
                        return _layer.FloatParameter(descriptor.parameter)
                            .IsGreaterThan(descriptor.threshold);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BuildPcb(CgeAacFlStateMachine ssm, PermutationComposedBehaviour composed, CgeDynamicsDescriptor[] dynamicsExiters)
        {
            var gestureLeftParam = _layer.Av3().GestureLeft;
            var gestureRightParam = _layer.Av3().GestureRight;
            var activityStageNameParam = _activityStageName != null ? _layer.IntParameter(_activityStageName) : null;

            foreach (CgeHandPose right in Enum.GetValues(typeof(CgeHandPose)))
            {
                var rightSsm = ssm.NewSubStateMachine($"Right {right}").At((int) right, 0);
                ssm.EntryTransitionsTo(rightSsm).When(gestureRightParam.IsEqualTo((int) right));

                // Short Restarts are no longer possible with Avatar Dynamics
                // var restartCondition = rightSsm.Restarts().When(_layer.Av3().GestureRight.IsEqualTo((int) right));
                // if (_activityStageName != null)
                // {
                //     restartCondition.And(_layer.IntParameter(_activityStageName).IsEqualTo(composed.StageValue));
                // }
                rightSsm.Exits();
                rightSsm.WithEntryPosition(-1, -1);
                rightSsm.WithExitPosition(1, 8);

                foreach (CgeHandPose left in Enum.GetValues(typeof(CgeHandPose)))
                {
                    var permutation = CgePermutation.LeftRight(left, right);
                    var state = AppendToSsm(rightSsm, composed.Behaviors[permutation]).At((int)permutation.Right, (int)permutation.Left);
                    state.State.name = $"Left {left}";

                    rightSsm.EntryTransitionsTo(state)
                        .When(gestureLeftParam.IsEqualTo((int) permutation.Left));
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(gestureLeftParam.IsNotEqualTo((int) permutation.Left))
                        .Or().When(gestureRightParam.IsNotEqualTo((int) permutation.Right));
                    if (!string.IsNullOrEmpty(_mmdCompatibilityToggleParameter))
                    {
                        state.Exits()
                            .WithTransitionDurationSeconds(composed.TransitionDuration)
                            .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsTrue())
                            .And(_layer.Av3().InStation.IsTrue());
                    }
                    if (composed.IsAvatarDynamics)
                    {
                        state.Exits()
                            .WithTransitionDurationSeconds(composed.TransitionDuration)
                            .When(ResolveExiter(composed.DynamicsDescriptor.descriptor));
                    }
                    if (composed.IsActivityBound && _activityStageName != null)
                    {
                        state.Exits()
                            .WithTransitionDurationSeconds(composed.TransitionDuration)
                            // ReSharper disable once PossibleNullReferenceException
                            .When(activityStageNameParam.IsNotEqualTo(composed.StageValue));
                    }
                    foreach (var dynamics in dynamicsExiters)
                    {
                        state.Exits()
                            .WithTransitionDurationSeconds(dynamics.enterTransitionDuration)
                            .When(ResolveEntranceDescriptor(dynamics));
                    }
                }
            }

            ssm.WithEntryPosition(-1, -1);
            ssm.WithExitPosition(9, -1);
        }

        private void BuildOcb(CgeAacFlStateMachine ssm, OneHandComposedBehaviour composed, CgeDynamicsDescriptor[] dynamicsExiters)
        {
            var which = composed.IsLeftHand ? _layer.Av3().GestureLeft : _layer.Av3().GestureRight;
            foreach (var pair in composed.Behaviors)
            {
                var handPose = pair.Key;
                var behavior = pair.Value;

                var state = AppendToSsm(ssm, behavior).At(0, (int)handPose);
                state.State.name = $"{(composed.IsLeftHand ? "Left" : "Right")} {handPose}";
                ssm.EntryTransitionsTo(state)
                    .When(which.IsEqualTo((int) handPose));
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(which.IsNotEqualTo((int) handPose));
                if (!string.IsNullOrEmpty(_mmdCompatibilityToggleParameter))
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsTrue())
                        .And(_layer.Av3().InStation.IsTrue());
                }
                if (composed.IsAvatarDynamics)
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(ResolveExiter(composed.DynamicsDescriptor.descriptor));
                }
                else if (_activityStageName != null)
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(_layer.IntParameter(_activityStageName).IsNotEqualTo(composed.StageValue));
                }
                foreach (var dynamics in dynamicsExiters)
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(dynamics.enterTransitionDuration)
                        .When(ResolveEntranceDescriptor(dynamics));
                }
            }

            ssm.WithEntryPosition(-1, -1);
            ssm.WithExitPosition(1, 8);
        }

        private void BuildScb(CgeAacFlStateMachine ssm, SingularComposedBehaviour composed, CgeDynamicsDescriptor[] dynamicsExiters)
        {
            var state = AppendToSsm(ssm, composed.Behavior);
            state.State.name = $"Single";
            ssm.EntryTransitionsTo(state);
            if (!string.IsNullOrEmpty(_mmdCompatibilityToggleParameter))
            {
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(_layer.BoolParameter(_mmdCompatibilityToggleParameter).IsTrue())
                    .And(_layer.Av3().InStation.IsTrue());
            }
            if (composed.IsAvatarDynamics)
            {
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(ResolveExiter(composed.DynamicsDescriptor.descriptor));
            }
            else if (_activityStageName != null)
            {
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(_layer.IntParameter(_activityStageName).IsNotEqualTo(composed.StageValue));
            }
            foreach (var dynamics in dynamicsExiters)
            {
                state.Exits()
                    .WithTransitionDurationSeconds(dynamics.enterTransitionDuration)
                    .When(ResolveEntranceDescriptor(dynamics));
            }
        }

        private CgeAacFlState AppendToSsm(CgeAacFlStateMachine ssm, ICgeAnimatedBehavior behaviour)
        {
            switch (behaviour.Nature())
            {
                case CgeAnimatedBehaviorNature.Single:
                    var sab = (CgeSingleAnimatedBehavior)behaviour;
                    return ForSingle(ssm, sab);
                case CgeAnimatedBehaviorNature.Analog:
                    CgeAnalogAnimatedBehavior aab = (CgeAnalogAnimatedBehavior)behaviour;
                    return ForAnalog(ssm, aab.Squeezing.Clip, aab.Resting.Clip, aab.HandSide);
                case CgeAnimatedBehaviorNature.PuppetToAnalog:
                    CgePuppetToAnalogAnimatedBehavior ptaab = (CgePuppetToAnalogAnimatedBehavior)behaviour;
                    return ForAnalog(ssm, ptaab.Squeezing.Clip, ptaab.Resting, ptaab.HandSide);
                case CgeAnimatedBehaviorNature.DualAnalog:
                    CgeDualAnalogAnimatedBehavior daab = (CgeDualAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, daab.BothSqueezing.Clip, daab.Resting.Clip, daab.LeftSqueezing.Clip, daab.RightSqueezing.Clip);
                case CgeAnimatedBehaviorNature.PuppetToDualAnalog:
                    CgePuppetToDualAnalogAnimatedBehavior ptdaab = (CgePuppetToDualAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, ptdaab.BothSqueezing.Clip, ptdaab.Resting, ptdaab.LeftSqueezing.Clip, ptdaab.RightSqueezing.Clip);
                case CgeAnimatedBehaviorNature.Puppet:
                    var pab = (CgePuppetAnimatedBehavior)behaviour;
                    return ForPuppet(ssm, pab);
                case CgeAnimatedBehaviorNature.SimpleMassiveBlend:
                    CgeSimpleMassiveBlendAnimatedBehavior smbab = (CgeSimpleMassiveBlendAnimatedBehavior)behaviour;
                    return ForSimpleMassiveBlend(ssm, smbab.Zero, smbab.One, smbab.ParameterName);
                case CgeAnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                    CgeTwoDirectionsMassiveBlendAnimatedBehavior tdmb = (CgeTwoDirectionsMassiveBlendAnimatedBehavior)behaviour;
                    return ForTwoDirectionsMassiveBlend(ssm, tdmb.Zero, tdmb.One, tdmb.MinusOne, tdmb.ParameterName);
                case CgeAnimatedBehaviorNature.ComplexMassiveBlend:
                    CgeComplexMassiveBlendAnimatedBehavior cbtmbab = (CgeComplexMassiveBlendAnimatedBehavior)behaviour;
                    return ForComplexMassiveBlend(ssm, cbtmbab.Behaviors, cbtmbab.OriginalBlendTreeTemplate);
                case CgeAnimatedBehaviorNature.UniversalAnalog:
                    CgeUniversalAnalogAnimatedBehavior uaab = (CgeUniversalAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, uaab.BothSqueezing.Clip, uaab.Resting.ToMotion(), uaab.LeftSqueezing.ToMotion(), uaab.RightSqueezing.ToMotion());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Motion Derive(ICgeAnimatedBehavior behavior)
        {
            switch (behavior.Nature())
            {
                case CgeAnimatedBehaviorNature.Single:
                    return ((CgeSingleAnimatedBehavior)behavior).Posing.Clip;
                case CgeAnimatedBehaviorNature.Analog:
                    CgeAnalogAnimatedBehavior aab = (CgeAnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        aab.Resting.Clip,
                        aab.Squeezing.Clip,
                        aab.HandSide == CgeHandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing));
                case CgeAnimatedBehaviorNature.PuppetToAnalog:
                    CgePuppetToAnalogAnimatedBehavior pta = (CgePuppetToAnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        pta.Resting,
                        pta.Squeezing.Clip,
                        pta.HandSide == CgeHandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing));
                case CgeAnimatedBehaviorNature.DualAnalog:
                    CgeDualAnalogAnimatedBehavior da = (CgeDualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(da.Resting.Clip, da.BothSqueezing.Clip, da.LeftSqueezing.Clip, da.RightSqueezing.Clip, _useGestureWeightCorrection, _useSmoothing);
                case CgeAnimatedBehaviorNature.PuppetToDualAnalog:
                    CgePuppetToDualAnalogAnimatedBehavior ptda = (CgePuppetToDualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(ptda.Resting, ptda.BothSqueezing.Clip, ptda.LeftSqueezing.Clip, ptda.RightSqueezing.Clip, _useGestureWeightCorrection, _useSmoothing);
                case CgeAnimatedBehaviorNature.Puppet:
                    return ((CgePuppetAnimatedBehavior)behavior).Tree;
                case CgeAnimatedBehaviorNature.UniversalAnalog:
                    CgeUniversalAnalogAnimatedBehavior uaab = (CgeUniversalAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(uaab.Resting.ToMotion(), uaab.BothSqueezing.Clip, uaab.LeftSqueezing.ToMotion(), uaab.RightSqueezing.ToMotion(), _useGestureWeightCorrection, _useSmoothing);
                case CgeAnimatedBehaviorNature.SimpleMassiveBlend:
                case CgeAnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                case CgeAnimatedBehaviorNature.ComplexMassiveBlend:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgeAacFlState ForSimpleMassiveBlend(CgeAacFlStateMachine ssm, ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            return CreateSimpleMassiveBlendState(zeroMotion, oneMotion, parameterName, ssm);
        }

        private CgeAacFlState ForTwoDirectionsMassiveBlend(CgeAacFlStateMachine ssm, ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, ICgeAnimatedBehavior minusOne, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var minusOneMotion = Derive(minusOne);
            return CreateTwoDirectionsMassiveBlendState(zeroMotion, oneMotion, minusOneMotion, parameterName, ssm);
        }

        private CgeAacFlState ForComplexMassiveBlend(CgeAacFlStateMachine ssm, List<ICgeAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            var motions = behaviors.Select(Derive).ToList();
            return CreateComplexMassiveBlendState(motions, originalBlendTreeTemplate, ssm);
        }

        private CgeAacFlState CreateSimpleMassiveBlendState(Motion zero, Motion one, string parameterName, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(CreateBlendTree(
                    zero,
                    one,
                    parameterName))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreateTwoDirectionsMassiveBlendState(Motion zero, Motion one, Motion minusOne, string parameterName, CgeAacFlStateMachine ssm)
        {
            var blendTree = new BlendTree
            {
                name = "autoBT_" + ThisStringWillBeDiscardedLater(),
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

            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(blendTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState ForSingle(CgeAacFlStateMachine ssm, CgeSingleAnimatedBehavior intermediateAnimationGroup)
        {
            return CreateMotionState(intermediateAnimationGroup.Posing.Clip, ssm);
        }

        private CgeAacFlState CreateComplexMassiveBlendState(List<Motion> motions, BlendTree originalBlendTreeTemplate, CgeAacFlStateMachine ssm)
        {
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

            return ssm.NewState(ThisStringWillBeDiscardedLater())
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

        private CgeAacFlState ForAnalog(CgeAacFlStateMachine ssm, Motion squeezing, Motion resting, CgeHandSide handSide)
        {
            return CreateSidedBlendState(squeezing, resting, handSide == CgeHandSide.LeftHand, ssm);
        }

        private CgeAacFlState ForDualAnalog(CgeAacFlStateMachine ssm, Motion bothSqueezing, Motion resting, Motion leftSqueezingClip, Motion rightSqueezingClip)
        {
            return CreateDualBlendState(bothSqueezing,
                resting,
                leftSqueezingClip, rightSqueezingClip, ssm);
        }

        private CgeAacFlState ForPuppet(CgeAacFlStateMachine ssm, CgePuppetAnimatedBehavior intermediateAnimationGroup)
        {
            return CreatePuppetState(intermediateAnimationGroup.Tree, ssm);
        }

        private CgeAacFlState CreateMotionState(AnimationClip clip, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(clip)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreateSidedBlendState(Motion squeezing, Motion resting, bool isLeftSide, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(CreateBlendTree(
                    resting,
                    squeezing,
                    _layer.FloatParameter(isLeftSide
                        ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                        : RightParam(_useGestureWeightCorrection, _useSmoothing)).Name))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private static string LeftParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return CgeSharedLayerUtils.HaiGestureComboLeftWeightSmoothing;

            if (useGestureWeightCorrection)
                return CgeSharedLayerUtils.HaiGestureComboLeftWeightProxy;

            return "GestureLeftWeight";
        }

        private static string RightParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return CgeSharedLayerUtils.HaiGestureComboRightWeightSmoothing;

            if (useGestureWeightCorrection)
                return CgeSharedLayerUtils.HaiGestureComboRightWeightProxy;

            return "GestureRightWeight";
        }

        private CgeAacFlState CreateDualBlendState(Motion clip, Motion resting, Motion posingLeft, Motion posingRight, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(CreateDualBlendTree(resting, clip, posingLeft, posingRight, _useGestureWeightCorrection, _useSmoothing))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreatePuppetState(BlendTree tree, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(ThisStringWillBeDiscardedLater())
                .WithAnimation(tree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private Motion CreateBlendTree(Motion atZero, Motion atOne, string weight)
        {
            var blendTree = new BlendTree
            {
                name = "autoBT_" + ThisStringWillBeDiscardedLater(),
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

        private Motion CreateDualBlendTree(Motion atZero, Motion atOne, Motion atLeft, Motion atRight, bool useGestureWeightCorrection, bool useSmoothing)
        {
            ChildMotion[] motions = {
                new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                new ChildMotion {motion = atLeft, timeScale = 1, position = Vector2.right},
                new ChildMotion {motion = atRight, timeScale = 1, position = Vector2.up},
                new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right + Vector2.up},
            };

            var blendTree = new BlendTree
            {
                name = "autoBT_" + ThisStringWillBeDiscardedLater(),
                blendParameter = LeftParam(useGestureWeightCorrection, useSmoothing),
                blendParameterY = RightParam(useGestureWeightCorrection, useSmoothing),
                blendType = BlendTreeType.FreeformDirectional2D,
                children = motions,
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(blendTree);

            return blendTree;
        }

        private static string ThisStringWillBeDiscardedLater()
        {
            // The functions need a string, but this string will be overriden later.
            return "" + Random.Range(0, Int32.MaxValue);
        }

        private void RegisterBlendTreeAsAsset(BlendTree blendTree)
        {
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(blendTree);
        }
    }
}

