using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForBlinkingOverrideView
    {
        private readonly string _activityStageName;
        private readonly List<GestureComboStageMapper> _comboLayers;
        private readonly float _analogBlinkingUpperThreshold;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorController _animatorController;
        private readonly AssetContainer _assetContainer;
        private readonly List<ManifestBinding> _manifestBindings;
        private readonly bool _writeDefaultsForLogicalStates;

        public LayerForBlinkingOverrideView(string activityStageName, List<GestureComboStageMapper> comboLayers, float analogBlinkingUpperThreshold, AvatarMask logicalAvatarMask, AnimatorController animatorController, AssetContainer assetContainer, List<ManifestBinding> manifestBindings, bool writeDefaults)
        {
            _activityStageName = activityStageName;
            _comboLayers = comboLayers;
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorController = animatorController;
            _assetContainer = assetContainer;
            _manifestBindings = manifestBindings;
            _writeDefaultsForLogicalStates = writeDefaults;
        }

        public void Create()
        {
            EditorUtility.DisplayProgressBar("ComboGestureExpressions", "Clearing eyes blinking override layer", 0f);
            var layer = ReinitializeLayer();

            if (!_manifestBindings.Any(manifest => manifest.Manifest.RequiresBlinking()))
            {
                return;
            }

            var enableBlinking = CreateBlinkingState(layer, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            var disableBlinking = CreateBlinkingState(layer, VRC_AnimatorTrackingControl.TrackingType.Animation);

            enableBlinking.TransitionsTo(disableBlinking)
                .When(layer.FloatParameter("_Hai_GestureAnimBlink").IsGreaterThan(_analogBlinkingUpperThreshold));
            disableBlinking.TransitionsTo(enableBlinking)
                .When(layer.FloatParameter("_Hai_GestureAnimBlink").IsLessThan(_analogBlinkingUpperThreshold));
        }

        private CgeAacFlState CreateSuspendState(CgeAacFlLayer machine)
        {
            return machine.NewState("SuspendBlinking", 1, 1)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);
        }

        private CgeAacFlState CreateBlinkingState(CgeAacFlLayer layer, VRC_AnimatorTrackingControl.TrackingType type)
        {
            return layer.NewState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                .TrackingSets(CgeAacFlState.TrackingElement.Eyes, type);
        }

        private void CreateTransitionWhenActivityIsOutOfBounds(CgeAacFlLayer layer, CgeAacFlState from, CgeAacFlState to)
        {
            var conditions = from.TransitionsTo(to).WhenConditions();

            foreach (var comboLayer in _comboLayers)
            {
                conditions.And(layer.IntParameter(_activityStageName).IsNotEqualTo(comboLayer.internalVirtualStageValue));
            }
        }

        private CgeAacFlLayer ReinitializeLayer()
        {
            return _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureBlinking")
                .WithAvatarMask(_logicalAvatarMask)
                .CGE_WithLayerWeight(0f);
        }
    }
}
