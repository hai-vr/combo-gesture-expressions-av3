using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class CgeLayerForBlinkingOverrideView
    {
        private readonly float _analogBlinkingUpperThreshold;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimatorController _animatorController;
        private readonly CgeAssetContainer _assetContainer;
        private readonly List<CgeManifestBinding> _manifestBindings;
        private readonly bool _writeDefaultsForLogicalStates;

        public CgeLayerForBlinkingOverrideView(float analogBlinkingUpperThreshold, AvatarMask logicalAvatarMask, AnimatorController animatorController, CgeAssetContainer assetContainer, List<CgeManifestBinding> manifestBindings, bool writeDefaults)
        {
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

        private CgeAacFlState CreateBlinkingState(CgeAacFlLayer layer, VRC_AnimatorTrackingControl.TrackingType type)
        {
            return layer.NewState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                .TrackingSets(CgeAacFlState.TrackingElement.Eyes, type);
        }

        private CgeAacFlLayer ReinitializeLayer()
        {
            return _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureBlinking")
                .WithAvatarMask(_logicalAvatarMask)
                .CGE_WithLayerWeight(0f);
        }
    }
}
