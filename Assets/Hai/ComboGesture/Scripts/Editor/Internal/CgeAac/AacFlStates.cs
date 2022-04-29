using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    internal class AacBackingAnimator
    {
        private readonly AacAnimatorGenerator _generator;

        public AacBackingAnimator(AacAnimatorGenerator animatorGenerator)
        {
            _generator = animatorGenerator;
        }

        public AacFlBoolParameter BoolParameter(string parameterName)
        {
            var result = AacFlBoolParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public AacFlBoolParameter TriggerParameter(string parameterName)
        {
            var result = AacFlBoolParameter.Internally(parameterName);
            _generator.CreateTriggerParamsAsNeeded(result);
            return result;
        }

        public AacFlFloatParameter FloatParameter(string parameterName)
        {
            var result = AacFlFloatParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public AacFlIntParameter IntParameter(string parameterName)
        {
            var result = AacFlIntParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public AacFlEnumIntParameter<TEnum> EnumParameter<TEnum>(string parameterName) where TEnum : Enum
        {
            var result = AacFlEnumIntParameter<TEnum>.Internally<TEnum>(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public AacFlBoolParameterGroup BoolParameters(params string[] parameterNames)
        {
            var result = AacFlBoolParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public AacFlBoolParameterGroup TriggerParameters(params string[] parameterNames)
        {
            var result = AacFlBoolParameterGroup.Internally(parameterNames);
            _generator.CreateTriggerParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public AacFlFloatParameterGroup FloatParameters(params string[] parameterNames)
        {
            var result = AacFlFloatParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public AacFlIntParameterGroup IntParameters(params string[] parameterNames)
        {
            var result = AacFlIntParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public AacFlBoolParameterGroup BoolParameters(params AacFlBoolParameter[] parameters)
        {
            var result = AacFlBoolParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }

        public AacFlBoolParameterGroup TriggerParameters(params AacFlBoolParameter[] parameters)
        {
            var result = AacFlBoolParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateTriggerParamsAsNeeded(parameters);
            return result;
        }

        public AacFlFloatParameterGroup FloatParameters(params AacFlFloatParameter[] parameters)
        {
            var result = AacFlFloatParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }

        public AacFlIntParameterGroup IntParameters(params AacFlIntParameter[] parameters)
        {
            var result = AacFlIntParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }
    }

    public class AacFlStateMachine : AacAnimatorNode<AacFlStateMachine>
    {
        public readonly AnimatorStateMachine Machine;
        private readonly AnimationClip _emptyClip;
        private readonly AacBackingAnimator _backingAnimator;
        private readonly IAacDefaultsProvider _defaultsProvider;
        private readonly float _gridShiftX;
        private readonly float _gridShiftY;

        private readonly List<AacAnimatorNode> _childNodes;

        internal AacFlStateMachine(AnimatorStateMachine machine, AnimationClip emptyClip, AacBackingAnimator backingAnimator, IAacDefaultsProvider defaultsProvider, AacFlStateMachine parent = null)
            : base(parent, defaultsProvider)
        {
            Machine = machine;
            _emptyClip = emptyClip;
            _backingAnimator = backingAnimator;
            _defaultsProvider = defaultsProvider;

            var grid = defaultsProvider.Grid();
            _gridShiftX = grid.x;
            _gridShiftY = grid.y;

            _childNodes = new List<AacAnimatorNode>();
        }

        internal AacBackingAnimator BackingAnimator()
        {
            return _backingAnimator;
        }

        public AacFlStateMachine NewSubStateMachine(string name)
        {
            var lastState = LastNodePosition();
            return NewSubStateMachine(name, 0, 0).Shift(lastState, 0, 1);
        }

        public AacFlStateMachine NewSubStateMachine(string name, int x, int y)
        {
            var stateMachine = Machine.AddStateMachine(name, GridPosition(x, y));
            var aacMachine = new AacFlStateMachine(stateMachine, _emptyClip, _backingAnimator, DefaultsProvider, this);
            _defaultsProvider.ConfigureStateMachine(stateMachine);
            _childNodes.Add(aacMachine);
            return aacMachine;
        }

        public AacFlStateMachine WithEntryPosition(int x, int y)
        {
            Machine.entryPosition = GridPosition(x, y);
            return this;
        }

        public AacFlStateMachine WithExitPosition(int x, int y)
        {
            Machine.exitPosition = GridPosition(x, y);
            return this;
        }

        public AacFlStateMachine WithAnyStatePosition(int x, int y)
        {
            Machine.anyStatePosition = GridPosition(x, y);
            return this;
        }

        public AacFlStateMachine WithParentStateMachinePosition(int x, int y)
        {
            Machine.parentStateMachinePosition = GridPosition(x, y);
            return this;
        }

        public AacFlState NewState(string name)
        {
            var lastState = LastNodePosition();
            return NewState(name, 0, 0).Shift(lastState, 0, 1);
        }

        public AacFlState NewState(string name, int x, int y)
        {
            var state = Machine.AddState(name, GridPosition(x, y));
            DefaultsProvider.ConfigureState(state, _emptyClip);
            var aacState = new AacFlState(state, this, DefaultsProvider);
            _childNodes.Add(aacState);
            return aacState;
        }

        public AacFlTransition AnyTransitionsTo(AacFlState destination)
        {
            return AnyTransition(destination, Machine);
        }

        public AacFlEntryTransition EntryTransitionsTo(AacFlState destination)
        {
            return EntryTransition(destination, Machine);
        }

        public AacFlEntryTransition EntryTransitionsTo(AacFlStateMachine destination)
        {
            return EntryTransition(destination, Machine);
        }

        public AacFlEntryTransition TransitionsFromEntry()
        {
            return EntryTransition(this, ParentMachine.Machine);
        }

        public AacFlNewTransitionContinuation TransitionsTo(AacFlState destination)
        {
            return new AacFlNewTransitionContinuation(ParentMachine.Machine.AddStateMachineTransition(Machine, destination.State), ParentMachine.Machine, Machine, destination.State);
        }

        public AacFlNewTransitionContinuation TransitionsTo(AacFlStateMachine destination)
        {
            return new AacFlNewTransitionContinuation(ParentMachine.Machine.AddStateMachineTransition(Machine, destination.Machine), ParentMachine.Machine, Machine, destination.Machine);
        }

        public AacFlNewTransitionContinuation Restarts()
        {
            return new AacFlNewTransitionContinuation(ParentMachine.Machine.AddStateMachineTransition(Machine, Machine), ParentMachine.Machine, Machine, Machine);
        }

        public AacFlNewTransitionContinuation Exits()
        {
            return new AacFlNewTransitionContinuation(ParentMachine.Machine.AddStateMachineExitTransition(Machine), ParentMachine.Machine, Machine, null);
        }

        private AacFlTransition AnyTransition(AacFlState destination, AnimatorStateMachine animatorStateMachine)
        {
            return new AacFlTransition(ConfigureTransition(animatorStateMachine.AddAnyStateTransition(destination.State)), animatorStateMachine, null, destination.State);
        }

        private AnimatorStateTransition ConfigureTransition(AnimatorStateTransition transition)
        {
            DefaultsProvider.ConfigureTransition(transition);
            return transition;
        }

        private AacFlEntryTransition EntryTransition(AacFlState destination, AnimatorStateMachine animatorStateMachine)
        {
            return new AacFlEntryTransition(animatorStateMachine.AddEntryTransition(destination.State), animatorStateMachine, null, destination.State);
        }

        private AacFlEntryTransition EntryTransition(AacFlStateMachine destination, AnimatorStateMachine animatorStateMachine)
        {
            return new AacFlEntryTransition(animatorStateMachine.AddEntryTransition(destination.Machine), animatorStateMachine, null, destination.Machine);
        }

        internal Vector3 LastNodePosition()
        {
            return _childNodes.LastOrDefault()?.GetPosition() ?? Vector3.right * _gridShiftX * 2;
        }

        private Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * _gridShiftX, y * _gridShiftY, 0);
        }

        internal IReadOnlyList<AacAnimatorNode> GetChildNodes()
        {
            return _childNodes;
        }

        protected internal override Vector3 GetPosition()
        {
            return ParentMachine.Machine.stateMachines.First(x => x.stateMachine == Machine).position;
        }

        protected internal override void SetPosition(Vector3 position)
        {
            var stateMachines = ParentMachine.Machine.stateMachines;
            for (var i = 0; i < stateMachines.Length; i++)
            {
                var m = stateMachines[i];
                if (m.stateMachine == Machine)
                {
                    m.position = position;
                    stateMachines[i] = m;
                    break;
                }
            }
            ParentMachine.Machine.stateMachines = stateMachines;
        }

        public AacFlStateMachine WithDefaultState(AacFlState newDefaultState)
        {
            Machine.defaultState = newDefaultState.State;
            return this;
        }
    }

    public class AacFlState : AacAnimatorNode<AacFlState>
    {
        public readonly AnimatorState State;
        private readonly AnimatorStateMachine _machine;
        private VRCAvatarParameterDriver _driver;
        private VRCAnimatorTrackingControl _tracking;
        private VRCAnimatorLocomotionControl _locomotionControl;
        private VRCAnimatorTemporaryPoseSpace _temporaryPoseSpace;

        public AacFlState(AnimatorState state, AacFlStateMachine parentMachine, IAacDefaultsProvider defaultsProvider) : base(parentMachine, defaultsProvider)
        {
            State = state;
            _machine = parentMachine.Machine;
        }

        public AacFlState WithAnimation(Motion clip)
        {
            State.motion = clip;
            return this;
        }

        public AacFlState WithAnimation(AacFlClip clip)
        {
            State.motion = clip.Clip;
            return this;
        }

        public AacFlTransition TransitionsTo(AacFlState destination)
        {
            return new AacFlTransition(ConfigureTransition(State.AddTransition(destination.State)), _machine, State, destination.State);
        }

        public AacFlTransition TransitionsTo(AacFlStateMachine destination)
        {
            return new AacFlTransition(State.AddTransition(destination.Machine), _machine, State, destination.Machine);
        }

        public AacFlTransition TransitionsFromAny()
        {
            return new AacFlTransition(ConfigureTransition(_machine.AddAnyStateTransition(State)), _machine, null, State);
        }

        public AacFlEntryTransition TransitionsFromEntry()
        {
            return new AacFlEntryTransition(_machine.AddEntryTransition(State), _machine, null, State);
        }

        public AacFlState AutomaticallyMovesTo(AacFlState destination)
        {
            var transition = ConfigureTransition(State.AddTransition(destination.State));
            transition.hasExitTime = true;
            return this;
        }

        public AacFlState CGE_AutomaticallyMovesTo(AacFlStateMachine destination)
        {
            var transition = ConfigureTransition(State.AddTransition(destination.Machine));
            transition.hasExitTime = true;
            return this;
        }

        public AacFlTransition Exits()
        {
            return new AacFlTransition(ConfigureTransition(State.AddExitTransition()), _machine, State, null);
        }

        private AnimatorStateTransition ConfigureTransition(AnimatorStateTransition transition)
        {
            DefaultsProvider.ConfigureTransition(transition);
            return transition;
        }

        public AacFlState Drives(AacFlIntParameter parameter, int value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = parameter.Name, value = value
            });
            return this;
        }

        public AacFlState Drives(AacFlFloatParameter parameter, float value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = parameter.Name, value = value
            });
            return this;
        }

        public AacFlState DrivingIncreases(AacFlFloatParameter parameter, float additiveValue)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = additiveValue
            });
            return this;
        }

        public AacFlState DrivingDecreases(AacFlFloatParameter parameter, float positiveValueToDecreaseBy)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = -positiveValueToDecreaseBy
            });
            return this;
        }

        public AacFlState DrivingIncreases(AacFlIntParameter parameter, int additiveValue)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = additiveValue
            });
            return this;
        }

        public AacFlState DrivingDecreases(AacFlIntParameter parameter, int positiveValueToDecreaseBy)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = -positiveValueToDecreaseBy
            });
            return this;
        }

        public AacFlState DrivingRandomizesLocally(AacFlFloatParameter parameter, float min, float max)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Random,
                name = parameter.Name, valueMin = min, valueMax = max
            });
            _driver.localOnly = true;
            return this;
        }

        public AacFlState DrivingRandomizesLocally(AacFlBoolParameter parameter, float chance)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Random,
                name = parameter.Name, chance = chance
            });
            _driver.localOnly = true;
            return this;
        }

        public AacFlState DrivingRandomizesLocally(AacFlIntParameter parameter, int min, int max)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Random,
                name = parameter.Name, valueMin = min, valueMax = max
            });
            _driver.localOnly = true;
            return this;
        }

        public AacFlState Drives(AacFlBoolParameter parameter, bool value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                name = parameter.Name, value = value ? 1 : 0
            });
            return this;
        }

        public AacFlState Drives(AacFlBoolParameterGroup parameters, bool value)
        {
            CreateDriverBehaviorIfNotExists();
            foreach (var parameter in parameters.ToList())
            {
                _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                {
                    name = parameter.Name, value = value ? 1 : 0
                });
            }
            return this;
        }

        public AacFlState DrivingLocally()
        {
            CreateDriverBehaviorIfNotExists();
            _driver.localOnly = true;
            return this;
        }

        private void CreateDriverBehaviorIfNotExists()
        {
            if (_driver != null) return;
            _driver = State.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            _driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>();
        }

        public AacFlState WithWriteDefaultsSetTo(bool shouldWriteDefaults)
        {
            State.writeDefaultValues = shouldWriteDefaults;
            return this;
        }

        public AacFlState PrintsToLogUsingTrackingBehaviour(string value)
        {
            CreateTrackingBehaviorIfNotExists();
            _tracking.debugString = value;

            return this;
        }

        public AacFlState TrackingTracks(TrackingElement element)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, VRC_AnimatorTrackingControl.TrackingType.Tracking);

            return this;
        }

        public AacFlState TrackingAnimates(TrackingElement element)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, VRC_AnimatorTrackingControl.TrackingType.Animation);

            return this;
        }

        public AacFlState TrackingSets(TrackingElement element, VRC_AnimatorTrackingControl.TrackingType trackingType)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, trackingType);

            return this;
        }

        public AacFlState LocomotionEnabled()
        {
            CreateLocomotionBehaviorIfNotExists();
            _locomotionControl.disableLocomotion = false;

            return this;
        }

        public AacFlState LocomotionDisabled()
        {
            CreateLocomotionBehaviorIfNotExists();
            _locomotionControl.disableLocomotion = true;

            return this;
        }

        public AacFlState PlayableEnables(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds = 0f)
        {
            return PlayableSets(blendable, blendDurationSeconds, 1.0f);
        }

        public AacFlState PlayableDisables(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds = 0f)
        {
            return PlayableSets(blendable, blendDurationSeconds, 0.0f);
        }

        public AacFlState PlayableSets(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds, float weight)
        {
            var playable = State.AddStateMachineBehaviour<VRCPlayableLayerControl>();
            playable.layer = blendable;
            playable.goalWeight = weight;
            playable.blendDuration = blendDurationSeconds;

            return this;
        }

        public AacFlState PoseSpaceEntered(float delaySeconds = 0f)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = true;
            _temporaryPoseSpace.fixedDelay = true;
            _temporaryPoseSpace.delayTime = delaySeconds;

            return this;
        }

        public AacFlState PoseSpaceExited(float delaySeconds = 0f)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = false;
            _temporaryPoseSpace.fixedDelay = true;
            _temporaryPoseSpace.delayTime = delaySeconds;

            return this;
        }

        public AacFlState PoseSpaceEnteredPercent(float delayNormalized)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = true;
            _temporaryPoseSpace.fixedDelay = false;
            _temporaryPoseSpace.delayTime = delayNormalized;

            return this;
        }

        public AacFlState PoseSpaceExitedPercent(float delayNormalized)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = false;
            _temporaryPoseSpace.fixedDelay = false;
            _temporaryPoseSpace.delayTime = delayNormalized;

            return this;
        }

        public AacFlState MotionTime(AacFlFloatParameter floatParam)
        {
            State.timeParameterActive = true;
            State.timeParameter = floatParam.Name;

            return this;
        }

        public AacFlState WithCycleOffset(AacFlFloatParameter floatParam)
        {
            State.cycleOffsetParameterActive = false;
            State.cycleOffsetParameter = floatParam.Name;

            return this;
        }

        public AacFlState WithCycleOffsetSetTo(float cycleOffset)
        {
            State.cycleOffsetParameterActive = false;
            State.cycleOffset = cycleOffset;

            return this;
        }

        private void SettingElementTo(TrackingElement element, VRC_AnimatorTrackingControl.TrackingType target)
        {
            switch (element)
            {
                case TrackingElement.Head:
                    _tracking.trackingHead = target;
                    break;
                case TrackingElement.LeftHand:
                    _tracking.trackingLeftHand = target;
                    break;
                case TrackingElement.RightHand:
                    _tracking.trackingRightHand = target;
                    break;
                case TrackingElement.Hip:
                    _tracking.trackingHip = target;
                    break;
                case TrackingElement.LeftFoot:
                    _tracking.trackingLeftFoot = target;
                    break;
                case TrackingElement.RightFoot:
                    _tracking.trackingRightFoot = target;
                    break;
                case TrackingElement.LeftFingers:
                    _tracking.trackingLeftFingers = target;
                    break;
                case TrackingElement.RightFingers:
                    _tracking.trackingRightFingers = target;
                    break;
                case TrackingElement.Eyes:
                    _tracking.trackingEyes = target;
                    break;
                case TrackingElement.Mouth:
                    _tracking.trackingMouth = target;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(element), element, null);
            }
        }

        private void CreateTrackingBehaviorIfNotExists()
        {
            if (_tracking != null) return;
            _tracking = State.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
        }

        private void CreateLocomotionBehaviorIfNotExists()
        {
            if (_locomotionControl != null) return;
            _locomotionControl = State.AddStateMachineBehaviour<VRCAnimatorLocomotionControl>();
        }

        private void CreateTemporaryPoseSpaceBehaviorIfNotExists()
        {
            if (_temporaryPoseSpace != null) return;
            _temporaryPoseSpace = State.AddStateMachineBehaviour<VRCAnimatorTemporaryPoseSpace>();
        }

        public enum TrackingElement
        {
            Head,
            LeftHand,
            RightHand,
            Hip,
            LeftFoot,
            RightFoot,
            LeftFingers,
            RightFingers,
            Eyes,
            Mouth
        }

        public AacFlState WithSpeed(AacFlFloatParameter parameter)
        {
            State.speedParameterActive = true;
            State.speedParameter = parameter.Name;

            return this;
        }

        public AacFlState WithSpeedSetTo(float speed)
        {
            State.speedParameterActive = false;
            State.speed = speed;

            return this;
        }

        protected internal override Vector3 GetPosition()
        {
            return _machine.states.First(x => x.state == State).position;
        }

        protected internal override void SetPosition(Vector3 position)
        {
            var states = _machine.states;
            for (var i = 0; i < states.Length; i++)
            {
                var m = states[i];
                if (m.state == State)
                {
                    m.position = position;
                    states[i] = m;
                    break;
                }
            }
            _machine.states = states;
        }
    }

    public class AacFlTransition : AacFlNewTransitionContinuation
    {
        private readonly AnimatorStateTransition _transition;

        public AacFlTransition(AnimatorStateTransition transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
            _transition = transition;
        }

        public AacFlTransition WithSourceInterruption()
        {
            _transition.interruptionSource = TransitionInterruptionSource.Source;
            return this;
        }

        public AacFlTransition WithInterruption(TransitionInterruptionSource interruptionSource)
        {
            _transition.interruptionSource = interruptionSource;
            return this;
        }

        public AacFlTransition WithTransitionDurationSeconds(float transitionDuration)
        {
            _transition.duration = transitionDuration;
            return this;
        }

        public AacFlTransition WithOrderedInterruption()
        {
            _transition.orderedInterruption = true;
            return this;
        }

        public AacFlTransition WithNoOrderedInterruption()
        {
            _transition.orderedInterruption = false;
            return this;
        }

        public AacFlTransition WithTransitionToSelf()
        {
            _transition.canTransitionToSelf = true;
            return this;
        }

        public AacFlTransition WithNoTransitionToSelf()
        {
            _transition.canTransitionToSelf = false;
            return this;
        }

        public AacFlTransition AfterAnimationFinishes()
        {
            _transition.hasExitTime = true;
            _transition.exitTime = 1;

            return this;
        }

        public AacFlTransition AfterAnimationIsAtLeastAtPercent(float exitTimeNormalized)
        {
            _transition.hasExitTime = true;
            _transition.exitTime = exitTimeNormalized;

            return this;
        }

        public AacFlTransition WithTransitionDurationPercent(float transitionDurationNormalized)
        {
            _transition.hasFixedDuration = false;
            _transition.duration = transitionDurationNormalized;

            return this;
        }
    }

    public class AacFlEntryTransition : AacFlNewTransitionContinuation
    {
        public AacFlEntryTransition(AnimatorTransition transition, AnimatorStateMachine machine, AnimatorState sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }
    }

    public interface IAacFlCondition
    {
        void ApplyTo(AacFlCondition appender);
    }

    public interface IAacFlOrCondition
    {
        List<AacFlTransitionContinuation> ApplyTo(AacFlNewTransitionContinuation firstContinuation);
    }

    public class AacFlCondition
    {
        private readonly AnimatorTransitionBase _transition;

        public AacFlCondition(AnimatorTransitionBase transition)
        {
            _transition = transition;
        }

        public AacFlCondition Add(string parameter, AnimatorConditionMode mode, float threshold)
        {
            _transition.AddCondition(mode, threshold, parameter);
            return this;
        }
    }

    public class AacFlNewTransitionContinuation
    {
        public readonly AnimatorTransitionBase Transition;
        private readonly AnimatorStateMachine _machine;
        private readonly AacTransitionEndpoint _sourceNullableIfAny;
        private readonly AacTransitionEndpoint _destinationNullableIfExits;

        public AacFlNewTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits)
        {
            Transition = transition;
            _machine = machine;
            _sourceNullableIfAny = sourceNullableIfAny;
            _destinationNullableIfExits = destinationNullableIfExits;
        }

        /// Adds a condition to the transition.
        ///
        /// The settings of the transition can no longer be modified after this point.
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(AacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(AacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public AacFlTransitionContinuation When(IAacFlCondition action)
        {
            action.ApplyTo(new AacFlCondition(Transition));
            return AsContinuationWithOr();
        }

        /// <summary>
        /// Applies a series of conditions to this transition, but this series of conditions cannot include an Or operator.
        /// </summary>
        /// <param name="actionsWithoutOr"></param>
        /// <returns></returns>
        public AacFlTransitionContinuation When(Action<AacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            actionsWithoutOr(new AacFlTransitionContinuationWithoutOr(Transition));
            return AsContinuationWithOr();
        }

        /// <summary>
        /// Applies a series of conditions, and this series may contain Or operators. However, the result can not be followed by an And operator. It can only be an Or operator.
        /// </summary>
        /// <param name="actionsWithOr"></param>
        /// <returns></returns>
        public AacFlTransitionContinuationOnlyOr When(Action<AacFlNewTransitionContinuation> actionsWithOr)
        {
            actionsWithOr(this);
            return AsContinuationOnlyOr();
        }

        /// <summary>
        /// Applies a series of conditions, and this series may contain Or operators. All And operators that follow will apply to all the conditions generated by this series, until the next Or operator.
        /// </summary>
        /// <param name="actionsWithOr"></param>
        /// <returns></returns>
        public AacFlMultiTransitionContinuation When(IAacFlOrCondition actionsWithOr)
        {
            var pendingContinuations = actionsWithOr.ApplyTo(this);
            return new AacFlMultiTransitionContinuation(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits, pendingContinuations);
        }

        public AacFlTransitionContinuation WhenConditions()
        {
            return AsContinuationWithOr();
        }

        private AacFlTransitionContinuation AsContinuationWithOr()
        {
            return new AacFlTransitionContinuation(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits);
        }

        private AacFlTransitionContinuationOnlyOr AsContinuationOnlyOr()
        {
            return new AacFlTransitionContinuationOnlyOr(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits);
        }
    }

    public class AacFlTransitionContinuation : AacFlTransitionContinuationAbstractWithOr
    {
        public AacFlTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }

        /// Adds an additional condition to the transition that requires all preceding conditions to be true.
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(AacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(AacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public AacFlTransitionContinuation And(IAacFlCondition action)
        {
            action.ApplyTo(new AacFlCondition(Transition));
            return this;
        }

        /// <summary>
        /// Applies a series of conditions to this transition. The conditions cannot include an Or operator.
        /// </summary>
        /// <param name="actionsWithoutOr"></param>
        /// <returns></returns>
        public AacFlTransitionContinuation And(Action<AacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            actionsWithoutOr(new AacFlTransitionContinuationWithoutOr(Transition));
            return this;
        }
    }

    public class AacFlMultiTransitionContinuation : AacFlTransitionContinuationAbstractWithOr
    {
        private readonly List<AacFlTransitionContinuation> _pendingContinuations;

        public AacFlMultiTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits, List<AacFlTransitionContinuation> pendingContinuations) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
            _pendingContinuations = pendingContinuations;
        }

        /// Adds an additional condition to these transitions that requires all preceding conditions to be true.
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(AacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(AacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public AacFlMultiTransitionContinuation And(IAacFlCondition action)
        {
            foreach (var pendingContinuation in _pendingContinuations)
            {
                pendingContinuation.And(action);
            }

            return this;
        }

        /// <summary>
        /// Applies a series of conditions to these transitions. The conditions cannot include an Or operator.
        /// </summary>
        /// <param name="actionsWithoutOr"></param>
        /// <returns></returns>
        public AacFlMultiTransitionContinuation And(Action<AacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            foreach (var pendingContinuation in _pendingContinuations)
            {
                pendingContinuation.And(actionsWithoutOr);
            }

            return this;
        }
    }

    public class AacFlTransitionContinuationOnlyOr : AacFlTransitionContinuationAbstractWithOr
    {
        public AacFlTransitionContinuationOnlyOr(AnimatorTransitionBase transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }
    }

    public abstract class AacFlTransitionContinuationAbstractWithOr
    {
        protected readonly AnimatorTransitionBase Transition;
        private readonly AnimatorStateMachine _machine;
        private readonly AacTransitionEndpoint _sourceNullableIfAny;
        private readonly AacTransitionEndpoint _destinationNullableIfExits;

        public AacFlTransitionContinuationAbstractWithOr(AnimatorTransitionBase transition, AnimatorStateMachine machine, AacTransitionEndpoint sourceNullableIfAny, AacTransitionEndpoint destinationNullableIfExits)
        {
            Transition = transition;
            _machine = machine;
            _sourceNullableIfAny = sourceNullableIfAny;
            _destinationNullableIfExits = destinationNullableIfExits;
        }

        /// <summary>
        /// Creates a new transition with identical settings but having no conditions defined yet.
        /// </summary>
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(AacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(AacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public AacFlNewTransitionContinuation Or()
        {
            return new AacFlNewTransitionContinuation(NewTransitionFromTemplate(), _machine, _sourceNullableIfAny, _destinationNullableIfExits);
        }

        private AnimatorTransitionBase NewTransitionFromTemplate()
        {
            AnimatorTransitionBase newTransition;
            if (Transition is AnimatorStateTransition templateStateTransition)
            {
                var stateTransition = NewTransition();
                stateTransition.duration = templateStateTransition.duration;
                stateTransition.offset = templateStateTransition.offset;
                stateTransition.interruptionSource = templateStateTransition.interruptionSource;
                stateTransition.orderedInterruption = templateStateTransition.orderedInterruption;
                stateTransition.exitTime = templateStateTransition.exitTime;
                stateTransition.hasExitTime = templateStateTransition.hasExitTime;
                stateTransition.hasFixedDuration = templateStateTransition.hasFixedDuration;
                stateTransition.canTransitionToSelf = templateStateTransition.canTransitionToSelf;
                newTransition = stateTransition;
            }
            else
            {
                if (_sourceNullableIfAny == null)
                {
                    if (_destinationNullableIfExits.TryGetState(out var state))
                        newTransition = _machine.AddEntryTransition(state);
                    else if (_destinationNullableIfExits.TryGetStateMachine(out var stateMachine))
                        newTransition = _machine.AddEntryTransition(stateMachine);
                    else
                        throw new InvalidOperationException("_destinationNullableIfExits is not null but does not contain an AnimatorState or AnimatorStateMachine");
                }
                // source will never be a state if we're cloning an AnimatorTransition
                else if (_sourceNullableIfAny.TryGetStateMachine(out var stateMachine))
                {
                    if (_destinationNullableIfExits == null)
                        newTransition = _machine.AddStateMachineExitTransition(stateMachine);
                    else if (_destinationNullableIfExits.TryGetState(out var destinationState))
                        newTransition = _machine.AddStateMachineTransition(stateMachine, destinationState);
                    else if (_destinationNullableIfExits.TryGetStateMachine(out var destinationStateMachine))
                        newTransition = _machine.AddStateMachineTransition(stateMachine, destinationStateMachine);
                    else
                        throw new InvalidOperationException("_destinationNullableIfExits is not null but does not contain an AnimatorState or AnimatorStateMachine");
                }
                else
                    throw new InvalidOperationException("_sourceNullableIfAny is not null but does not contain an AnimatorStateMachine");
            }
            return newTransition;
        }

        private AnimatorStateTransition NewTransition()
        {
            AnimatorState state;
            AnimatorStateMachine stateMachine;

            if (_sourceNullableIfAny == null)
            {
                if (_destinationNullableIfExits.TryGetState(out state))
                    return _machine.AddAnyStateTransition(state);
                if (_destinationNullableIfExits.TryGetStateMachine(out stateMachine))
                    return _machine.AddAnyStateTransition(stateMachine);
                throw new InvalidOperationException("Transition has no source nor destination.");
            }

            // source will never be a state machine if we're cloning an AnimatorStateTransition
            if (_sourceNullableIfAny.TryGetState(out var sourceState))
            {
                if (_destinationNullableIfExits == null)
                {
                    return sourceState.AddExitTransition();
                }

                if (_destinationNullableIfExits.TryGetState(out state))
                {
                    return sourceState.AddTransition(state);
                }

                if (_destinationNullableIfExits.TryGetStateMachine(out stateMachine))
                    return sourceState.AddTransition(stateMachine);
                throw new InvalidOperationException("_destinationNullableIfExits is not null but does not contain an AnimatorState or AnimatorStateMachine");
            }
            throw new InvalidOperationException("_sourceNullableIfAny is not null but does not contain an AnimatorState");
        }
    }

    public class AacFlTransitionContinuationWithoutOr
    {
        private readonly AnimatorTransitionBase _transition;

        public AacFlTransitionContinuationWithoutOr(AnimatorTransitionBase transition)
        {
            _transition = transition;
        }

        public AacFlTransitionContinuationWithoutOr And(IAacFlCondition action)
        {
            action.ApplyTo(new AacFlCondition(_transition));
            return this;
        }

        /// <summary>
        /// Applies a series of conditions to this transition. The conditions cannot include an Or operator.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public AacFlTransitionContinuationWithoutOr AndWhenever(Action<AacFlTransitionContinuationWithoutOr> action)
        {
            action(this);
            return this;
        }
    }

    public class AacTransitionEndpoint
    {
        private readonly AnimatorState _state;
        private readonly AnimatorStateMachine _stateMachine;

        public AacTransitionEndpoint(AnimatorState state)
        {
            _state = state;
        }

        public AacTransitionEndpoint(AnimatorStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public static implicit operator AacTransitionEndpoint(AnimatorState state)
        {
            return new AacTransitionEndpoint(state);
        }

        public static implicit operator AacTransitionEndpoint(AnimatorStateMachine stateMachine)
        {
            return new AacTransitionEndpoint(stateMachine);
        }

        public bool TryGetState(out AnimatorState state)
        {
            state = _state;
            return _state != null;
        }

        public bool TryGetStateMachine(out AnimatorStateMachine stateMachine)
        {
            stateMachine = _stateMachine;
            return _stateMachine != null;
        }
    }
}
