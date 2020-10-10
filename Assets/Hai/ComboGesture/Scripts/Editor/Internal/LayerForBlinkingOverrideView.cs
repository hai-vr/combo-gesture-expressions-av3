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
        private const bool WriteDefaultsForLogicalStates = true;

        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly FeatureToggles _featuresToggles;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AnimationClip _emptyClip;
        private readonly bool _useGestureWeightCorrection;

        public LayerForBlinkingOverrideView(string activityStageName, List<GestureComboStageMapper> comboLayers, float analogBlinkingUpperThreshold, FeatureToggles featuresToggles, AvatarMask logicalAvatarMask, AnimatorGenerator animatorGenerator, AnimationClip emptyClip, bool useGestureWeightCorrection)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _featuresToggles = featuresToggles;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorGenerator = animatorGenerator;
            _emptyClip = emptyClip;
            _useGestureWeightCorrection = useGestureWeightCorrection;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing eyes blinking override layer", 0f);
            var machine = ReinitializeLayer();

            var activityManifests = CreateManifest();
            var combinator = IntermediateBlinkingCombinator.ForBlinking(activityManifests);
            if (!combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewMotion(true)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(true, false)) &&
                !combinator.IntermediateToBlinking.ContainsKey(IntermediateBlinkingGroup.NewBlend(false, true)))
            {
                return;
            }

            var enableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Tracking, _emptyClip);
            var disableBlinking = CreateBlinkingState(machine, VRC_AnimatorTrackingControl.TrackingType.Animation, _emptyClip);

            if (Feature(FeatureToggles.ExposeAreEyesClosed))
            {
                CreateInternalParameterDriverWhenEyesAreOpen(enableBlinking);
                CreateInternalParameterDriverWhenEyesAreClosed(disableBlinking);
            }

            var requireSuspension = _activityStageName != null || Feature(FeatureToggles.ExposeDisableBlinkingOverride);
            if (requireSuspension)
            {
                var suspend = CreateSuspendState(machine, _emptyClip);

                if (_activityStageName != null)
                {
                    CreateTransitionWhenActivityIsOutOfBounds(enableBlinking, suspend);
                    CreateTransitionWhenActivityIsOutOfBounds(disableBlinking, suspend);
                }

                if (Feature(FeatureToggles.ExposeDisableBlinkingOverride))
                {
                    CreateTransitionWhenBlinkingIsDisabled(enableBlinking, suspend);
                    CreateTransitionWhenBlinkingIsDisabled(disableBlinking, suspend);
                }

                foreach (var layer in _comboLayers)
                {
                    var transition = suspend.AddTransition(enableBlinking);
                    SharedLayerUtils.SetupDefaultTransition(transition);
                    if (_activityStageName != null)
                    {
                        transition.AddCondition(AnimatorConditionMode.Equals, layer.stageValue, _activityStageName);
                    }

                    if (Feature(FeatureToggles.ExposeDisableBlinkingOverride))
                    {
                        transition.AddCondition(AnimatorConditionMode.Equals, 0, SharedLayerUtils.HaiGestureComboDisableBlinkingOverrideParamName);
                    }
                }
            }

            var toDisable = enableBlinking.AddTransition(disableBlinking);
            SetupBlinkingTransition(toDisable);
            toDisable.AddCondition(AnimatorConditionMode.Less, _analogBlinkingUpperThreshold, "_Hai_GestureAnimBlink");

            var toEnable = disableBlinking.AddTransition(enableBlinking);
            SetupBlinkingTransition(toEnable);
            toEnable.AddCondition(AnimatorConditionMode.Greater, _analogBlinkingUpperThreshold, "_Hai_GestureAnimBlink");
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
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboAreEyesClosedParamName, value = 0}
            };
        }

        private static void CreateInternalParameterDriverWhenEyesAreClosed(AnimatorState disableBlinking)
        {
            var driver = disableBlinking.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
            {
                new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboAreEyesClosedParamName, value = 1}
            };
        }

        private static void CreateTransitionWhenBlinkingIsDisabled(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            SharedLayerUtils.SetupDefaultBlinkingTransition(transition);
            transition.AddCondition(AnimatorConditionMode.NotEqual, 0, SharedLayerUtils.HaiGestureComboDisableBlinkingOverrideParamName);
        }

        private static AnimatorState CreateSuspendState(AnimatorStateMachine machine, AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState("SuspendBlinking", SharedLayerUtils.GridPosition(1, 1));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = WriteDefaultsForLogicalStates;
            return enableBlinking;
        }

        private static AnimatorState CreateBlinkingState(AnimatorStateMachine machine, VRC_AnimatorTrackingControl.TrackingType type,
            AnimationClip emptyClip)
        {
            var enableBlinking = machine.AddState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", SharedLayerUtils.GridPosition(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3));
            enableBlinking.motion = emptyClip;
            enableBlinking.writeDefaultValues = WriteDefaultsForLogicalStates;
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

        private bool Feature(FeatureToggles feature)
        {
            return (_featuresToggles & feature) == feature;
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
            transition.interruptionSource = TransitionInterruptionSource.Source;
            transition.canTransitionToSelf = false;
            transition.orderedInterruption = true;
        }
    }
}
