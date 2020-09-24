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
        private const bool WriteDefaultsForLogicalStates = true;

        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly FeatureToggles _featuresToggles;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AnimationClip _emptyClip;

        public LayerForLipsyncOverrideView(string activityStageName, List<GestureComboStageMapper> comboLayers, float analogBlinkingUpperThreshold, FeatureToggles featuresToggles, AvatarMask logicalAvatarMask, AnimatorGenerator animatorGenerator, AnimationClip emptyClip)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _featuresToggles = featuresToggles;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorGenerator = animatorGenerator;
            _emptyClip = emptyClip;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing lipsync override layer", 0f);
            var machine = ReinitializeLayer();

            var activityManifests = CreateManifest();
            var combinator = IntermediateBlinkingCombinator.ForLimitedLipsync(activityManifests);
            if (!combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewMotion(true)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(true, false)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(false, true)))
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

            new GestureCBlinkingCombiner(combinator.IntermediateToBlinking, _activityStageName, _analogBlinkingUpperThreshold)
                .Populate(enableBlinking, disableBlinking);

            // Huge hack to avoid duplicating generation logic...
            foreach (var enableBlinkingTransition in disableBlinking.transitions.Where(transition => transition.destinationState == enableBlinking).ToList())
            {
                var transition = machine.AddAnyStateTransition(enableBlinking);
                transition.conditions = enableBlinkingTransition.conditions.ToArray();
                transition.hasExitTime = enableBlinkingTransition.hasExitTime;
                transition.exitTime = enableBlinkingTransition.exitTime;
                transition.hasFixedDuration = enableBlinkingTransition.hasFixedDuration;
                transition.offset = enableBlinkingTransition.offset;
                transition.interruptionSource = enableBlinkingTransition.interruptionSource;
                transition.canTransitionToSelf = enableBlinkingTransition.canTransitionToSelf;
                transition.orderedInterruption = enableBlinkingTransition.orderedInterruption;
                transition.duration = enableBlinkingTransition.duration;
            }
            machine.RemoveState(disableBlinking);
        }

        // FIXME: This is duplicate code
        private List<ActivityManifest> CreateManifest()
        {
            return _comboLayers
                .Select((mapper, layerOrdinal) => new ActivityManifest(mapper.stageValue, SharedLayerUtils.FromManifest(mapper.activity, _emptyClip), layerOrdinal))
                .ToList();
        }


        private static void CreateInternalParameterDriverWhenEyesAreOpen(AnimatorState enableBlinking)
        {
            var driver = enableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboIsLipsyncLimited, value = 0}
            };
        }

        private static void CreateInternalParameterDriverWhenEyesAreClosed(AnimatorState disableBlinking)
        {
            var driver = disableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboIsLipsyncLimited, value = 1}
            };
        }

        private static void CreateTransitionWhenBlinkingIsDisabled(AnimatorStateMachine machine, AnimatorState to)
        {
            var transition = machine.AddAnyStateTransition(to);
            SharedLayerUtils.SetupDefaultBlinkingTransition(transition);
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.NotEqual, 0, SharedLayerUtils.HaiGestureComboDisableLipsyncOverrideParamName);
        }

        private static AnimatorState CreateSuspendState(AnimatorStateMachine machine, AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState("SuspendLipsync", SharedLayerUtils.GridPosition(1, 1));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = WriteDefaultsForLogicalStates;
            return enableBlinking;
        }

        private static AnimatorState CreateBlinkingState(AnimatorStateMachine machine, VRC_AnimatorTrackingControl.TrackingType type,
            AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableLipsync" : "DisableLipsync", SharedLayerUtils.GridPosition(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = WriteDefaultsForLogicalStates;
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
            return _animatorGenerator.CreateOrRemakeLayerAtSameIndex("Hai_GestureLipsync", 1f, _logicalAvatarMask).ExposeMachine();
        }

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
        }
    }
}
