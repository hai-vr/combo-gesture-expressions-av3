using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForBlinkingOverrideView
    {
        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AnimationClip _emptyClip;
        private readonly List<ManifestBinding> _manifestBindings;
        private readonly bool _writeDefaultsForLogicalStates;

        public LayerForBlinkingOverrideView(string activityStageName, List<GestureComboStageMapper> comboLayers, float analogBlinkingUpperThreshold, AvatarMask logicalAvatarMask, AnimatorGenerator animatorGenerator, AnimationClip emptyClip, List<ManifestBinding> manifestBindings, bool writeDefaults)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorGenerator = animatorGenerator;
            _emptyClip = emptyClip;
            _manifestBindings = manifestBindings;
            _writeDefaultsForLogicalStates = writeDefaults;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing eyes blinking override layer", 0f);
            var machine = ReinitializeLayer();

            if (!_manifestBindings.Any(manifest => manifest.Manifest.RequiresBlinking()))
            {
                return;
            }

            var enableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Tracking, _emptyClip);
            var disableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Animation, _emptyClip);

            var requireSuspension = _activityStageName != null;
            if (requireSuspension)
            {
                var suspend = CreateSuspendState(machine, _emptyClip);

                if (_activityStageName != null)
                {
                    CreateTransitionWhenActivityIsOutOfBounds(enableBlinking, suspend);
                    CreateTransitionWhenActivityIsOutOfBounds(disableBlinking, suspend);
                }

                foreach (var layer in _comboLayers)
                {
                    var transition = suspend.AddTransition(enableBlinking);
                    SharedLayerUtils.SetupDefaultTransition(transition);
                    if (_activityStageName != null)
                    {
                        transition.AddCondition(AnimatorConditionMode.Equals, layer.stageValue, _activityStageName);
                    }
                }
            }

            var toDisable = enableBlinking.AddTransition(disableBlinking);
            SetupBlinkingTransition(toDisable);
            toDisable.AddCondition(AnimatorConditionMode.Greater, _analogBlinkingUpperThreshold, "_Hai_GestureAnimBlink");

            var toEnable = disableBlinking.AddTransition(enableBlinking);
            SetupBlinkingTransition(toEnable);
            toEnable.AddCondition(AnimatorConditionMode.Less, _analogBlinkingUpperThreshold, "_Hai_GestureAnimBlink");
        }

        private AnimatorState CreateSuspendState(AnimatorStateMachine machine, AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState("SuspendBlinking", SharedLayerUtils.GridPosition(1, 1));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = _writeDefaultsForLogicalStates;
            return enableBlinking;
        }

        private AnimatorState CreateBlinkingState(AnimatorStateMachine machine, VRC_AnimatorTrackingControl.TrackingType type,
            AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", SharedLayerUtils.GridPosition(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = _writeDefaultsForLogicalStates;
            var tracking = enableBlinking.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            tracking.trackingEyes = type;
            return enableBlinking;
        }

        private void CreateTransitionWhenActivityIsOutOfBounds(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            SharedLayerUtils.SetupDefaultTransition(transition);

            foreach (var layer in _comboLayers)
            {
                transition.AddCondition(AnimatorConditionMode.NotEqual, layer.stageValue, _activityStageName);
            }
        }


        private AnimatorStateMachine ReinitializeLayer()
        {
            return _animatorGenerator.CreateOrRemakeLayerAtSameIndex("Hai_GestureBlinking", 0f, _logicalAvatarMask).ExposeMachine();
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
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.canTransitionToSelf = false;
            transition.orderedInterruption = true;
        }
    }
}
