using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    internal class CgeAacBackingAnimator
    {
        private readonly CgeAacAnimatorGenerator _generator;

        public CgeAacBackingAnimator(CgeAacAnimatorGenerator animatorGenerator)
        {
            _generator = animatorGenerator;
        }

        public CgeAacFlBoolParameter BoolParameter(string parameterName)
        {
            var result = CgeAacFlBoolParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public CgeAacFlBoolParameter TriggerParameter(string parameterName)
        {
            var result = CgeAacFlBoolParameter.Internally(parameterName);
            _generator.CreateTriggerParamsAsNeeded(result);
            return result;
        }

        public CgeAacFlFloatParameter FloatParameter(string parameterName)
        {
            var result = CgeAacFlFloatParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public CgeAacFlIntParameter IntParameter(string parameterName)
        {
            var result = CgeAacFlIntParameter.Internally(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public CgeAacFlEnumIntParameter<TEnum> EnumParameter<TEnum>(string parameterName) where TEnum : Enum
        {
            var result = CgeAacFlEnumIntParameter<TEnum>.Internally<TEnum>(parameterName);
            _generator.CreateParamsAsNeeded(result);
            return result;
        }

        public CgeAacFlBoolParameterGroup BoolParameters(params string[] parameterNames)
        {
            var result = CgeAacFlBoolParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public CgeAacFlBoolParameterGroup TriggerParameters(params string[] parameterNames)
        {
            var result = CgeAacFlBoolParameterGroup.Internally(parameterNames);
            _generator.CreateTriggerParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public CgeAacFlFloatParameterGroup FloatParameters(params string[] parameterNames)
        {
            var result = CgeAacFlFloatParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public CgeAacFlIntParameterGroup IntParameters(params string[] parameterNames)
        {
            var result = CgeAacFlIntParameterGroup.Internally(parameterNames);
            _generator.CreateParamsAsNeeded(result.ToList().ToArray());
            return result;
        }

        public CgeAacFlBoolParameterGroup BoolParameters(params CgeAacFlBoolParameter[] parameters)
        {
            var result = CgeAacFlBoolParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }

        public CgeAacFlBoolParameterGroup TriggerParameters(params CgeAacFlBoolParameter[] parameters)
        {
            var result = CgeAacFlBoolParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateTriggerParamsAsNeeded(parameters);
            return result;
        }

        public CgeAacFlFloatParameterGroup FloatParameters(params CgeAacFlFloatParameter[] parameters)
        {
            var result = CgeAacFlFloatParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }

        public CgeAacFlIntParameterGroup IntParameters(params CgeAacFlIntParameter[] parameters)
        {
            var result = CgeAacFlIntParameterGroup.Internally(parameters.Select(parameter => parameter.Name).ToArray());
            _generator.CreateParamsAsNeeded(parameters);
            return result;
        }
    }

    public class CgeAacFlStateMachine : CgeAacAnimatorNode<CgeAacFlStateMachine>
    {
        public readonly AnimatorStateMachine Machine;
        private readonly AnimationClip _emptyClip;
        private readonly CgeAacBackingAnimator _backingAnimator;
        private readonly ICgeAacDefaultsProvider _defaultsProvider;
        private readonly float _gridShiftX;
        private readonly float _gridShiftY;

        private readonly List<CgeAacAnimatorNode> _childNodes;

        internal CgeAacFlStateMachine(AnimatorStateMachine machine, AnimationClip emptyClip, CgeAacBackingAnimator backingAnimator, ICgeAacDefaultsProvider defaultsProvider, CgeAacFlStateMachine parent = null)
            : base(parent, defaultsProvider)
        {
            Machine = machine;
            _emptyClip = emptyClip;
            _backingAnimator = backingAnimator;
            _defaultsProvider = defaultsProvider;

            var grid = defaultsProvider.Grid();
            _gridShiftX = grid.x;
            _gridShiftY = grid.y;

            _childNodes = new List<CgeAacAnimatorNode>();
        }

        internal CgeAacBackingAnimator BackingAnimator()
        {
            return _backingAnimator;
        }

        public CgeAacFlStateMachine NewSubStateMachine(string name)
        {
            var lastState = LastNodePosition();
            return NewSubStateMachine(name, 0, 0).Shift(lastState, 0, 1);
        }

        public CgeAacFlStateMachine NewSubStateMachine(string name, int x, int y)
        {
            var stateMachine = Machine.AddStateMachine(name, GridPosition(x, y));
            CgeAacV0.UndoDisable(stateMachine);
            var aacMachine = new CgeAacFlStateMachine(stateMachine, _emptyClip, _backingAnimator, DefaultsProvider, this);
            _defaultsProvider.ConfigureStateMachine(stateMachine);
            _childNodes.Add(aacMachine);
            return aacMachine;
        }

        public CgeAacFlStateMachine WithEntryPosition(int x, int y)
        {
            Machine.entryPosition = GridPosition(x, y);
            return this;
        }

        public CgeAacFlStateMachine WithExitPosition(int x, int y)
        {
            Machine.exitPosition = GridPosition(x, y);
            return this;
        }

        public CgeAacFlStateMachine WithAnyStatePosition(int x, int y)
        {
            Machine.anyStatePosition = GridPosition(x, y);
            return this;
        }

        public CgeAacFlStateMachine WithParentStateMachinePosition(int x, int y)
        {
            Machine.parentStateMachinePosition = GridPosition(x, y);
            return this;
        }

        public CgeAacFlState NewState(string name)
        {
            var lastState = LastNodePosition();
            return NewState(name, 0, 0).Shift(lastState, 0, 1);
        }

        public CgeAacFlState NewState(string name, int x, int y)
        {
            var state = Machine.AddState(name, GridPosition(x, y));
            CgeAacV0.UndoDisable(state);
            DefaultsProvider.ConfigureState(state, _emptyClip);
            var aacState = new CgeAacFlState(state, this, DefaultsProvider);
            _childNodes.Add(aacState);
            return aacState;
        }

        public CgeAacFlTransition AnyTransitionsTo(CgeAacFlState destination)
        {
            return AnyTransition(destination, Machine);
        }

        public CgeAacFlEntryTransition EntryTransitionsTo(CgeAacFlState destination)
        {
            return EntryTransition(destination, Machine);
        }

        public CgeAacFlEntryTransition EntryTransitionsTo(CgeAacFlStateMachine destination)
        {
            return EntryTransition(destination, Machine);
        }

        public CgeAacFlEntryTransition TransitionsFromEntry()
        {
            return EntryTransition(this, ParentMachine.Machine);
        }

        public CgeAacFlNewTransitionContinuation TransitionsTo(CgeAacFlState destination)
        {
            var transition = ParentMachine.Machine.AddStateMachineTransition(Machine, destination.State);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlNewTransitionContinuation(transition, ParentMachine.Machine, Machine, destination.State);
        }

        public CgeAacFlNewTransitionContinuation TransitionsTo(CgeAacFlStateMachine destination)
        {
            var transition = ParentMachine.Machine.AddStateMachineTransition(Machine, destination.Machine);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlNewTransitionContinuation(transition, ParentMachine.Machine, Machine, destination.Machine);
        }

        public CgeAacFlNewTransitionContinuation Restarts()
        {
            var transition = ParentMachine.Machine.AddStateMachineTransition(Machine, Machine);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlNewTransitionContinuation(transition, ParentMachine.Machine, Machine, Machine);
        }

        public CgeAacFlNewTransitionContinuation Exits()
        {
            var transition = ParentMachine.Machine.AddStateMachineExitTransition(Machine);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlNewTransitionContinuation(transition, ParentMachine.Machine, Machine, null);
        }

        private CgeAacFlTransition AnyTransition(CgeAacFlState destination, AnimatorStateMachine animatorStateMachine)
        {
            var transition = animatorStateMachine.AddAnyStateTransition(destination.State);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlTransition(ConfigureTransition(transition), animatorStateMachine, null, destination.State);
        }

        private AnimatorStateTransition ConfigureTransition(AnimatorStateTransition transition)
        {
            DefaultsProvider.ConfigureTransition(transition);
            return transition;
        }

        private CgeAacFlEntryTransition EntryTransition(CgeAacFlState destination, AnimatorStateMachine animatorStateMachine)
        {
            var transition = animatorStateMachine.AddEntryTransition(destination.State);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlEntryTransition(transition, animatorStateMachine, null, destination.State);
        }

        private CgeAacFlEntryTransition EntryTransition(CgeAacFlStateMachine destination, AnimatorStateMachine animatorStateMachine)
        {
            var transition = animatorStateMachine.AddEntryTransition(destination.Machine);
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlEntryTransition(transition, animatorStateMachine, null, destination.Machine);
        }

        internal Vector3 LastNodePosition()
        {
            return _childNodes.LastOrDefault()?.GetPosition() ?? Vector3.right * _gridShiftX * 2;
        }

        private Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * _gridShiftX, y * _gridShiftY, 0);
        }

        internal IReadOnlyList<CgeAacAnimatorNode> GetChildNodes()
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

        public CgeAacFlStateMachine WithDefaultState(CgeAacFlState newDefaultState)
        {
            Machine.defaultState = newDefaultState.State;
            return this;
        }
    }

    public class CgeAacFlState : CgeAacAnimatorNode<CgeAacFlState>
    {
        public readonly AnimatorState State;
        private readonly AnimatorStateMachine _machine;
        private VRCAvatarParameterDriver _driver;
        private VRCAnimatorTrackingControl _tracking;
        private VRCAnimatorLocomotionControl _locomotionControl;
        private VRCAnimatorTemporaryPoseSpace _temporaryPoseSpace;

        public CgeAacFlState(AnimatorState state, CgeAacFlStateMachine parentMachine, ICgeAacDefaultsProvider defaultsProvider) : base(parentMachine, defaultsProvider)
        {
            State = state;
            _machine = parentMachine.Machine;
        }

        public CgeAacFlState WithAnimation(Motion clip)
        {
            State.motion = clip;
            return this;
        }

        public CgeAacFlState WithAnimation(CgeAacFlClip clip)
        {
            State.motion = clip.Clip;
            return this;
        }

        public CgeAacFlTransition TransitionsTo(CgeAacFlState destination)
        {
            var internalTransition = State.AddTransition(destination.State);
            CgeAacV0.UndoDisable(internalTransition);
            return new CgeAacFlTransition(ConfigureTransition(internalTransition), _machine, State, destination.State);
        }

        public CgeAacFlTransition TransitionsTo(CgeAacFlStateMachine destination)
        {
            var internalTransition = State.AddTransition(destination.Machine);
            CgeAacV0.UndoDisable(internalTransition);
            return new CgeAacFlTransition(internalTransition, _machine, State, destination.Machine);
        }

        public CgeAacFlTransition TransitionsFromAny()
        {
            var internalTransition = _machine.AddAnyStateTransition(State);
            CgeAacV0.UndoDisable(internalTransition);
            return new CgeAacFlTransition(ConfigureTransition(internalTransition), _machine, null, State);
        }

        public CgeAacFlEntryTransition TransitionsFromEntry()
        {
            var internalTransition = _machine.AddEntryTransition(State);
            CgeAacV0.UndoDisable(internalTransition);
            return new CgeAacFlEntryTransition(internalTransition, _machine, null, State);
        }

        public CgeAacFlState AutomaticallyMovesTo(CgeAacFlState destination)
        {
            var internalTransition = State.AddTransition(destination.State);
            CgeAacV0.UndoDisable(internalTransition);
            var transition = ConfigureTransition(internalTransition);
            transition.hasExitTime = true;
            return this;
        }

        public CgeAacFlState CGE_AutomaticallyMovesTo(CgeAacFlStateMachine destination)
        {
            var internalTransition = State.AddTransition(destination.Machine);
            CgeAacV0.UndoDisable(internalTransition);
            var transition = ConfigureTransition(internalTransition);
            transition.hasExitTime = true;
            return this;
        }

        public CgeAacFlTransition Exits()
        {
            var transition = State.AddExitTransition();
            CgeAacV0.UndoDisable(transition);
            return new CgeAacFlTransition(ConfigureTransition(transition), _machine, State, null);
        }

        private AnimatorStateTransition ConfigureTransition(AnimatorStateTransition transition)
        {
            DefaultsProvider.ConfigureTransition(transition);
            return transition;
        }

        public CgeAacFlState Drives(CgeAacFlIntParameter parameter, int value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = parameter.Name, value = value
            });
            return this;
        }

        public CgeAacFlState Drives(CgeAacFlFloatParameter parameter, float value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = parameter.Name, value = value
            });
            return this;
        }

        public CgeAacFlState DrivingIncreases(CgeAacFlFloatParameter parameter, float additiveValue)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = additiveValue
            });
            return this;
        }

        public CgeAacFlState DrivingDecreases(CgeAacFlFloatParameter parameter, float positiveValueToDecreaseBy)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = -positiveValueToDecreaseBy
            });
            return this;
        }

        public CgeAacFlState DrivingIncreases(CgeAacFlIntParameter parameter, int additiveValue)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = additiveValue
            });
            return this;
        }

        public CgeAacFlState DrivingDecreases(CgeAacFlIntParameter parameter, int positiveValueToDecreaseBy)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter.Name, value = -positiveValueToDecreaseBy
            });
            return this;
        }

        public CgeAacFlState DrivingRandomizesLocally(CgeAacFlFloatParameter parameter, float min, float max)
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

        public CgeAacFlState DrivingRandomizesLocally(CgeAacFlBoolParameter parameter, float chance)
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

        public CgeAacFlState DrivingRandomizesLocally(CgeAacFlIntParameter parameter, int min, int max)
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

        public CgeAacFlState Drives(CgeAacFlBoolParameter parameter, bool value)
        {
            CreateDriverBehaviorIfNotExists();
            _driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                name = parameter.Name, value = value ? 1 : 0
            });
            return this;
        }

        public CgeAacFlState Drives(CgeAacFlBoolParameterGroup parameters, bool value)
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

        public CgeAacFlState DrivingLocally()
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

        public CgeAacFlState WithWriteDefaultsSetTo(bool shouldWriteDefaults)
        {
            State.writeDefaultValues = shouldWriteDefaults;
            return this;
        }

        public CgeAacFlState PrintsToLogUsingTrackingBehaviour(string value)
        {
            CreateTrackingBehaviorIfNotExists();
            _tracking.debugString = value;

            return this;
        }

        public CgeAacFlState TrackingTracks(TrackingElement element)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, VRC_AnimatorTrackingControl.TrackingType.Tracking);

            return this;
        }

        public CgeAacFlState TrackingAnimates(TrackingElement element)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, VRC_AnimatorTrackingControl.TrackingType.Animation);

            return this;
        }

        public CgeAacFlState TrackingSets(TrackingElement element, VRC_AnimatorTrackingControl.TrackingType trackingType)
        {
            CreateTrackingBehaviorIfNotExists();
            SettingElementTo(element, trackingType);

            return this;
        }

        public CgeAacFlState LocomotionEnabled()
        {
            CreateLocomotionBehaviorIfNotExists();
            _locomotionControl.disableLocomotion = false;

            return this;
        }

        public CgeAacFlState LocomotionDisabled()
        {
            CreateLocomotionBehaviorIfNotExists();
            _locomotionControl.disableLocomotion = true;

            return this;
        }

        public CgeAacFlState PlayableEnables(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds = 0f)
        {
            return PlayableSets(blendable, blendDurationSeconds, 1.0f);
        }

        public CgeAacFlState PlayableDisables(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds = 0f)
        {
            return PlayableSets(blendable, blendDurationSeconds, 0.0f);
        }

        public CgeAacFlState PlayableSets(VRC_PlayableLayerControl.BlendableLayer blendable, float blendDurationSeconds, float weight)
        {
            var playable = State.AddStateMachineBehaviour<VRCPlayableLayerControl>();
            playable.layer = blendable;
            playable.goalWeight = weight;
            playable.blendDuration = blendDurationSeconds;

            return this;
        }

        public CgeAacFlState PoseSpaceEntered(float delaySeconds = 0f)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = true;
            _temporaryPoseSpace.fixedDelay = true;
            _temporaryPoseSpace.delayTime = delaySeconds;

            return this;
        }

        public CgeAacFlState PoseSpaceExited(float delaySeconds = 0f)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = false;
            _temporaryPoseSpace.fixedDelay = true;
            _temporaryPoseSpace.delayTime = delaySeconds;

            return this;
        }

        public CgeAacFlState PoseSpaceEnteredPercent(float delayNormalized)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = true;
            _temporaryPoseSpace.fixedDelay = false;
            _temporaryPoseSpace.delayTime = delayNormalized;

            return this;
        }

        public CgeAacFlState PoseSpaceExitedPercent(float delayNormalized)
        {
            CreateTemporaryPoseSpaceBehaviorIfNotExists();
            _temporaryPoseSpace.enterPoseSpace = false;
            _temporaryPoseSpace.fixedDelay = false;
            _temporaryPoseSpace.delayTime = delayNormalized;

            return this;
        }

        public CgeAacFlState MotionTime(CgeAacFlFloatParameter floatParam)
        {
            State.timeParameterActive = true;
            State.timeParameter = floatParam.Name;

            return this;
        }

        public CgeAacFlState WithCycleOffset(CgeAacFlFloatParameter floatParam)
        {
            State.cycleOffsetParameterActive = false;
            State.cycleOffsetParameter = floatParam.Name;

            return this;
        }

        public CgeAacFlState WithCycleOffsetSetTo(float cycleOffset)
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

        public CgeAacFlState WithSpeed(CgeAacFlFloatParameter parameter)
        {
            State.speedParameterActive = true;
            State.speedParameter = parameter.Name;

            return this;
        }

        public CgeAacFlState WithSpeedSetTo(float speed)
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

    public class CgeAacFlTransition : CgeAacFlNewTransitionContinuation
    {
        private readonly AnimatorStateTransition _transition;

        public CgeAacFlTransition(AnimatorStateTransition transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
            _transition = transition;
        }

        public CgeAacFlTransition WithSourceInterruption()
        {
            _transition.interruptionSource = TransitionInterruptionSource.Source;
            return this;
        }

        public CgeAacFlTransition WithInterruption(TransitionInterruptionSource interruptionSource)
        {
            _transition.interruptionSource = interruptionSource;
            return this;
        }

        public CgeAacFlTransition WithTransitionDurationSeconds(float transitionDuration)
        {
            _transition.duration = transitionDuration;
            return this;
        }

        public CgeAacFlTransition WithOrderedInterruption()
        {
            _transition.orderedInterruption = true;
            return this;
        }

        public CgeAacFlTransition WithNoOrderedInterruption()
        {
            _transition.orderedInterruption = false;
            return this;
        }

        public CgeAacFlTransition WithTransitionToSelf()
        {
            _transition.canTransitionToSelf = true;
            return this;
        }

        public CgeAacFlTransition WithNoTransitionToSelf()
        {
            _transition.canTransitionToSelf = false;
            return this;
        }

        public CgeAacFlTransition AfterAnimationFinishes()
        {
            _transition.hasExitTime = true;
            _transition.exitTime = 1;

            return this;
        }

        public CgeAacFlTransition AfterAnimationIsAtLeastAtPercent(float exitTimeNormalized)
        {
            _transition.hasExitTime = true;
            _transition.exitTime = exitTimeNormalized;

            return this;
        }

        public CgeAacFlTransition WithTransitionDurationPercent(float transitionDurationNormalized)
        {
            _transition.hasFixedDuration = false;
            _transition.duration = transitionDurationNormalized;

            return this;
        }
    }

    public class CgeAacFlEntryTransition : CgeAacFlNewTransitionContinuation
    {
        public CgeAacFlEntryTransition(AnimatorTransition transition, AnimatorStateMachine machine, AnimatorState sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }
    }

    public interface ICgeAacFlCondition
    {
        void ApplyTo(CgeAacFlCondition appender);
    }

    public interface ICgeAacFlOrCondition
    {
        List<CgeAacFlTransitionContinuation> ApplyTo(CgeAacFlNewTransitionContinuation firstContinuation);
    }

    public class CgeAacFlCondition
    {
        private readonly AnimatorTransitionBase _transition;

        public CgeAacFlCondition(AnimatorTransitionBase transition)
        {
            _transition = transition;
        }

        public CgeAacFlCondition Add(string parameter, AnimatorConditionMode mode, float threshold)
        {
            _transition.AddCondition(mode, threshold, parameter);
            return this;
        }
    }

    public class CgeAacFlNewTransitionContinuation
    {
        public readonly AnimatorTransitionBase Transition;
        private readonly AnimatorStateMachine _machine;
        private readonly CgeAacTransitionEndpoint _sourceNullableIfAny;
        private readonly CgeAacTransitionEndpoint _destinationNullableIfExits;

        public CgeAacFlNewTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits)
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
        /// .And(CgeAacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(CgeAacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public CgeAacFlTransitionContinuation When(ICgeAacFlCondition action)
        {
            action.ApplyTo(new CgeAacFlCondition(Transition));
            return AsContinuationWithOr();
        }

        /// <summary>
        /// Applies a series of conditions to this transition, but this series of conditions cannot include an Or operator.
        /// </summary>
        /// <param name="actionsWithoutOr"></param>
        /// <returns></returns>
        public CgeAacFlTransitionContinuation When(Action<CgeAacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            actionsWithoutOr(new CgeAacFlTransitionContinuationWithoutOr(Transition));
            return AsContinuationWithOr();
        }

        /// <summary>
        /// Applies a series of conditions, and this series may contain Or operators. However, the result can not be followed by an And operator. It can only be an Or operator.
        /// </summary>
        /// <param name="actionsWithOr"></param>
        /// <returns></returns>
        public CgeAacFlTransitionContinuationOnlyOr When(Action<CgeAacFlNewTransitionContinuation> actionsWithOr)
        {
            actionsWithOr(this);
            return AsContinuationOnlyOr();
        }

        /// <summary>
        /// Applies a series of conditions, and this series may contain Or operators. All And operators that follow will apply to all the conditions generated by this series, until the next Or operator.
        /// </summary>
        /// <param name="actionsWithOr"></param>
        /// <returns></returns>
        public CgeAacFlMultiTransitionContinuation When(ICgeAacFlOrCondition actionsWithOr)
        {
            var pendingContinuations = actionsWithOr.ApplyTo(this);
            return new CgeAacFlMultiTransitionContinuation(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits, pendingContinuations);
        }

        public CgeAacFlTransitionContinuation WhenConditions()
        {
            return AsContinuationWithOr();
        }

        private CgeAacFlTransitionContinuation AsContinuationWithOr()
        {
            return new CgeAacFlTransitionContinuation(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits);
        }

        private CgeAacFlTransitionContinuationOnlyOr AsContinuationOnlyOr()
        {
            return new CgeAacFlTransitionContinuationOnlyOr(Transition, _machine, _sourceNullableIfAny, _destinationNullableIfExits);
        }
    }

    public class CgeAacFlTransitionContinuation : CgeAacFlTransitionContinuationAbstractWithOr
    {
        public CgeAacFlTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }

        /// Adds an additional condition to the transition that requires all preceding conditions to be true.
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(CgeAacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(CgeAacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public CgeAacFlTransitionContinuation And(ICgeAacFlCondition action)
        {
            action.ApplyTo(new CgeAacFlCondition(Transition));
            return this;
        }

        /// <summary>
        /// Applies a series of conditions to this transition. The conditions cannot include an Or operator.
        /// </summary>
        /// <param name="actionsWithoutOr"></param>
        /// <returns></returns>
        public CgeAacFlTransitionContinuation And(Action<CgeAacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            actionsWithoutOr(new CgeAacFlTransitionContinuationWithoutOr(Transition));
            return this;
        }
    }

    public class CgeAacFlMultiTransitionContinuation : CgeAacFlTransitionContinuationAbstractWithOr
    {
        private readonly List<CgeAacFlTransitionContinuation> _pendingContinuations;

        public CgeAacFlMultiTransitionContinuation(AnimatorTransitionBase transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits, List<CgeAacFlTransitionContinuation> pendingContinuations) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
            _pendingContinuations = pendingContinuations;
        }

        /// Adds an additional condition to these transitions that requires all preceding conditions to be true.
        /// <example>
        /// <code>
        /// .When(_aac.BoolParameter(my.myBoolParameterName).IsTrue())
        /// .And(_aac.BoolParameter(my.myIntParameterName).IsGreaterThan(2))
        /// .And(CgeAacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(CgeAacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public CgeAacFlMultiTransitionContinuation And(ICgeAacFlCondition action)
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
        public CgeAacFlMultiTransitionContinuation And(Action<CgeAacFlTransitionContinuationWithoutOr> actionsWithoutOr)
        {
            foreach (var pendingContinuation in _pendingContinuations)
            {
                pendingContinuation.And(actionsWithoutOr);
            }

            return this;
        }
    }

    public class CgeAacFlTransitionContinuationOnlyOr : CgeAacFlTransitionContinuationAbstractWithOr
    {
        public CgeAacFlTransitionContinuationOnlyOr(AnimatorTransitionBase transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits) : base(transition, machine, sourceNullableIfAny, destinationNullableIfExits)
        {
        }
    }

    public abstract class CgeAacFlTransitionContinuationAbstractWithOr
    {
        protected readonly AnimatorTransitionBase Transition;
        private readonly AnimatorStateMachine _machine;
        private readonly CgeAacTransitionEndpoint _sourceNullableIfAny;
        private readonly CgeAacTransitionEndpoint _destinationNullableIfExits;

        public CgeAacFlTransitionContinuationAbstractWithOr(AnimatorTransitionBase transition, AnimatorStateMachine machine, CgeAacTransitionEndpoint sourceNullableIfAny, CgeAacTransitionEndpoint destinationNullableIfExits)
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
        /// .And(CgeAacAv3.ItIsLocal())
        /// .Or()
        /// .When(_aac.BoolParameters(
        ///     my.myBoolParameterName,
        ///     my.myOtherBoolParameterName
        /// ).AreTrue())
        /// .And(CgeAacAv3.ItIsRemote());
        /// </code>
        /// </example>
        public CgeAacFlNewTransitionContinuation Or()
        {
            return new CgeAacFlNewTransitionContinuation(NewTransitionFromTemplate(), _machine, _sourceNullableIfAny, _destinationNullableIfExits);
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

                    CgeAacV0.UndoDisable(newTransition);
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
                {
                    var transition = _machine.AddAnyStateTransition(state);
                    CgeAacV0.UndoDisable(transition);
                    return transition;
                }

                if (_destinationNullableIfExits.TryGetStateMachine(out stateMachine))
                {
                    var transition = _machine.AddAnyStateTransition(stateMachine);
                    CgeAacV0.UndoDisable(transition);
                    return transition;
                }

                throw new InvalidOperationException("Transition has no source nor destination.");
            }

            // source will never be a state machine if we're cloning an AnimatorStateTransition
            if (_sourceNullableIfAny.TryGetState(out var sourceState))
            {
                if (_destinationNullableIfExits == null)
                {
                    var transition = sourceState.AddExitTransition();
                    CgeAacV0.UndoDisable(transition);
                    return transition;
                }

                if (_destinationNullableIfExits.TryGetState(out state))
                {
                    var transition = sourceState.AddTransition(state);
                    CgeAacV0.UndoDisable(transition);
                    return transition;
                }

                if (_destinationNullableIfExits.TryGetStateMachine(out stateMachine))
                {
                    var transition = sourceState.AddTransition(stateMachine);
                    CgeAacV0.UndoDisable(transition);
                    return transition;
                }

                throw new InvalidOperationException("_destinationNullableIfExits is not null but does not contain an AnimatorState or AnimatorStateMachine");
            }
            throw new InvalidOperationException("_sourceNullableIfAny is not null but does not contain an AnimatorState");
        }
    }

    public class CgeAacFlTransitionContinuationWithoutOr
    {
        private readonly AnimatorTransitionBase _transition;

        public CgeAacFlTransitionContinuationWithoutOr(AnimatorTransitionBase transition)
        {
            _transition = transition;
        }

        public CgeAacFlTransitionContinuationWithoutOr And(ICgeAacFlCondition action)
        {
            action.ApplyTo(new CgeAacFlCondition(_transition));
            return this;
        }

        /// <summary>
        /// Applies a series of conditions to this transition. The conditions cannot include an Or operator.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public CgeAacFlTransitionContinuationWithoutOr AndWhenever(Action<CgeAacFlTransitionContinuationWithoutOr> action)
        {
            action(this);
            return this;
        }
    }

    public class CgeAacTransitionEndpoint
    {
        private readonly AnimatorState _state;
        private readonly AnimatorStateMachine _stateMachine;

        public CgeAacTransitionEndpoint(AnimatorState state)
        {
            _state = state;
        }

        public CgeAacTransitionEndpoint(AnimatorStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public static implicit operator CgeAacTransitionEndpoint(AnimatorState state)
        {
            return new CgeAacTransitionEndpoint(state);
        }

        public static implicit operator CgeAacTransitionEndpoint(AnimatorStateMachine stateMachine)
        {
            return new CgeAacTransitionEndpoint(stateMachine);
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
