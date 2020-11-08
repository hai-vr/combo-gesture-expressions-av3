using System;
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
    internal class LayerForLipsyncOverrideView
    {
        private const string LipsyncLayerName = "Hai_GestureLipsync";

        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly FeatureToggles _featuresToggles;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly VRCAvatarDescriptor _avatarDescriptor;
        private readonly ComboGestureLimitedLipsync _limitedLipsync;
        private readonly AssetContainer _assetContainer;
        private readonly AnimationClip _emptyClip;
        private readonly List<ManifestBinding> _manifestBindings;
        private readonly bool _writeDefaultsForLogicalStates;
        private readonly bool _writeDefaultsForLipsyncBlendshapes;

        public LayerForLipsyncOverrideView(string activityStageName,
            List<GestureComboStageMapper> comboLayers,
            float analogBlinkingUpperThreshold,
            FeatureToggles featuresToggles,
            AvatarMask logicalAvatarMask,
            AnimatorGenerator animatorGenerator,
            VRCAvatarDescriptor avatarDescriptor,
            ComboGestureLimitedLipsync limitedLipsync,
            AssetContainer assetContainer,
            AnimationClip emptyClip,
            List<ManifestBinding> manifestBindings,
            bool writeDefaults)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _featuresToggles = featuresToggles;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorGenerator = animatorGenerator;
            _avatarDescriptor = avatarDescriptor;
            _limitedLipsync = limitedLipsync;
            _assetContainer = assetContainer;
            _emptyClip = emptyClip;
            _manifestBindings = manifestBindings;
            _writeDefaultsForLogicalStates = writeDefaults;
            _writeDefaultsForLipsyncBlendshapes = writeDefaults;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing lipsync override layer", 0f);
            var machine = ReinitializeLayer();

            if (!_manifestBindings.Any(manifest => manifest.Manifest.RequiresLimitedLipsync()))
            {
                return;
            }

            var enableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Tracking, _emptyClip);
            var disableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Animation, _emptyClip);

            if (Feature(FeatureToggles.ExposeIsLipsyncLimited))
            {
                CreateInternalParameterDriverWhenEyesAreOpen(enableBlinking);
                CreateInternalParameterDriverWhenEyesAreClosed(disableBlinking);
            }

            var requireSuspension = _activityStageName != null || Feature(FeatureToggles.ExposeDisableLipsyncOverride);
            if (requireSuspension)
            {
                var suspend = CreateSuspendState(machine, _emptyClip);

                if (_activityStageName != null)
                {
                    CreateTransitionWhenActivityIsOutOfBounds(machine, suspend);
                }

                if (Feature(FeatureToggles.ExposeDisableLipsyncOverride))
                {
                    CreateTransitionWhenBlinkingIsDisabled(machine, suspend);
                }
            }

            var toDisable = enableBlinking.AddTransition(disableBlinking);
            SetupBlinkingTransition(toDisable);
            toDisable.AddCondition(AnimatorConditionMode.Greater, _analogBlinkingUpperThreshold, "_Hai_GestureAnimLSWide");

            var toEnable = disableBlinking.AddTransition(enableBlinking);
            SetupBlinkingTransition(toEnable);
            toEnable.AddCondition(AnimatorConditionMode.Less, _analogBlinkingUpperThreshold, "_Hai_GestureAnimLSWide");

            // Huge hack to avoid duplicating generation logic...
            foreach (var enableBlinkingTransition in disableBlinking.transitions.Where(transition => transition.destinationState == enableBlinking).ToList())
            {
                var transition = machine.AddAnyStateTransition(enableBlinking);
                transition.conditions = enableBlinkingTransition.conditions.ToArray();
                transition.hasExitTime = enableBlinkingTransition.hasExitTime;
                transition.exitTime = enableBlinkingTransition.exitTime;
                transition.hasFixedDuration = enableBlinkingTransition.hasFixedDuration;
                transition.offset = enableBlinkingTransition.offset;
                transition.interruptionSource = TransitionInterruptionSource.None;
                transition.orderedInterruption = enableBlinkingTransition.orderedInterruption;
                transition.duration = enableBlinkingTransition.duration;
                transition.canTransitionToSelf = false;
            }
            machine.RemoveState(disableBlinking);

            _assetContainer.RemoveAssetsStartingWith("zAutogeneratedLipsync_", typeof(AnimationClip));
            var visemeClips = Enumerable.Range(0, 15)
                .Select(visemeNumber =>
                {
                    var finalAmplitude = _limitedLipsync.amplitudeScale * FindVisemeAmplitudeTweak(visemeNumber);
                    var clip = new AnimationClip {name = "zAutogeneratedLipsync_ " + visemeNumber};
                    new VisemeAnimationMaker(_avatarDescriptor).OverrideAnimation(clip, visemeNumber, finalAmplitude);
                    return clip;
                })
                .ToList();
            foreach (var visemeClip in visemeClips)
            {
                _assetContainer.AddAnimation(visemeClip);
            }
            AssetContainer.GlobalSave();
            for (var visemeNumber = 0; visemeNumber < visemeClips.Count; visemeNumber++)
            {
                var state = machine.AddState("A0 - Viseme " + visemeNumber, SharedLayerUtils.GridPosition(4, 2 + visemeNumber));
                state.motion = visemeClips[visemeNumber];
                state.writeDefaultValues = _writeDefaultsForLipsyncBlendshapes;

                var tracking = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                tracking.trackingMouth = VRC_AnimatorTrackingControl.TrackingType.Animation;

                var transition = machine.AddAnyStateTransition(state);
                SharedLayerUtils.SetupDefaultTransition(transition);
                transition.canTransitionToSelf = false;
                transition.duration = _limitedLipsync.transitionDuration * FindVisemeTransitionTweak(visemeNumber);
                transition.AddCondition(AnimatorConditionMode.Equals, visemeNumber, "Viseme");
            }
        }

        private float FindVisemeAmplitudeTweak(int visemeNumber)
        {
            switch (visemeNumber)
            {
                case 0: return _limitedLipsync.amplitude0;
                case 1: return _limitedLipsync.amplitude1;
                case 2: return _limitedLipsync.amplitude2;
                case 3: return _limitedLipsync.amplitude3;
                case 4: return _limitedLipsync.amplitude4;
                case 5: return _limitedLipsync.amplitude5;
                case 6: return _limitedLipsync.amplitude6;
                case 7: return _limitedLipsync.amplitude7;
                case 8: return _limitedLipsync.amplitude8;
                case 9: return _limitedLipsync.amplitude9;
                case 10: return _limitedLipsync.amplitude10;
                case 11: return _limitedLipsync.amplitude11;
                case 12: return _limitedLipsync.amplitude12;
                case 13: return _limitedLipsync.amplitude13;
                case 14: return _limitedLipsync.amplitude14;
                default: throw new IndexOutOfRangeException();
            }
        }

        private float FindVisemeTransitionTweak(int visemeNumber)
        {
            switch (visemeNumber)
            {
                case 0: return _limitedLipsync.transition0;
                case 1: return _limitedLipsync.transition1;
                case 2: return _limitedLipsync.transition2;
                case 3: return _limitedLipsync.transition3;
                case 4: return _limitedLipsync.transition4;
                case 5: return _limitedLipsync.transition5;
                case 6: return _limitedLipsync.transition6;
                case 7: return _limitedLipsync.transition7;
                case 8: return _limitedLipsync.transition8;
                case 9: return _limitedLipsync.transition9;
                case 10: return _limitedLipsync.transition10;
                case 11: return _limitedLipsync.transition11;
                case 12: return _limitedLipsync.transition12;
                case 13: return _limitedLipsync.transition13;
                case 14: return _limitedLipsync.transition14;
                default: throw new IndexOutOfRangeException();
            }
        }

        private static void CreateInternalParameterDriverWhenEyesAreOpen(AnimatorState enableBlinking)
        {
            var driver = enableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboIsLipsyncLimitedParamName, value = 0}
            };
        }

        private static void CreateInternalParameterDriverWhenEyesAreClosed(AnimatorState disableBlinking)
        {
            var driver = disableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboIsLipsyncLimitedParamName, value = 1}
            };
        }

        private static void CreateTransitionWhenBlinkingIsDisabled(AnimatorStateMachine machine, AnimatorState to)
        {
            var transition = machine.AddAnyStateTransition(to);
            SharedLayerUtils.SetupDefaultBlinkingTransition(transition);
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.NotEqual, 0, SharedLayerUtils.HaiGestureComboDisableLipsyncOverrideParamName);
        }

        private AnimatorState CreateSuspendState(AnimatorStateMachine machine, AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState("SuspendLipsync", SharedLayerUtils.GridPosition(1, 1));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = _writeDefaultsForLogicalStates;
            return enableBlinking;
        }

        private AnimatorState CreateBlinkingState(AnimatorStateMachine machine, VRC_AnimatorTrackingControl.TrackingType type,
            AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableLipsync" : "DisableLipsync", SharedLayerUtils.GridPosition(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = _writeDefaultsForLogicalStates;
            var tracking = enableBlinking.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            tracking.trackingMouth = type;
            return enableBlinking;
        }

        private void CreateTransitionWhenActivityIsOutOfBounds(AnimatorStateMachine machine, AnimatorState to)
        {
            var transition = machine.AddAnyStateTransition(to);
            SharedLayerUtils.SetupDefaultTransition(transition);
            transition.canTransitionToSelf = false;

            foreach (var layer in _comboLayers)
            {
                transition.AddCondition(AnimatorConditionMode.NotEqual, layer.stageValue, _activityStageName);
            }
        }


        private AnimatorStateMachine ReinitializeLayer()
        {
            return _animatorGenerator.CreateOrRemakeLayerAtSameIndex(LipsyncLayerName, 1f, _logicalAvatarMask).ExposeMachine();
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(LipsyncLayerName);
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
