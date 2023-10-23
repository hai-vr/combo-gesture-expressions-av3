using System.Collections.Generic;
using System.Linq;
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.VRC;
using Hai.ComboGesture.Scripts.Components;
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
        private readonly string _eyeTrackingEnabledParameter;
        private readonly EyeTrackingParameterType _eyeTrackingParameterType;

        public CgeLayerForBlinkingOverrideView(float analogBlinkingUpperThreshold, AvatarMask logicalAvatarMask,
            AnimatorController animatorController, CgeAssetContainer assetContainer,
            List<CgeManifestBinding> manifestBindings, bool writeDefaults, string eyeTrackingEnabledParameter,
            EyeTrackingParameterType eyeTrackingParameterType)
        {
            _analogBlinkingUpperThreshold = analogBlinkingUpperThreshold;
            _logicalAvatarMask = logicalAvatarMask;
            _animatorController = animatorController;
            _assetContainer = assetContainer;
            _manifestBindings = manifestBindings;
            _writeDefaultsForLogicalStates = writeDefaults;
            _eyeTrackingEnabledParameter = eyeTrackingEnabledParameter;
            _eyeTrackingParameterType = eyeTrackingParameterType;
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

            if (!string.IsNullOrEmpty(_eyeTrackingEnabledParameter))
            {
                var blinkingIgnored = layer.NewState("BlinkingIgnoredDueToFaceTracking");
                // Modern face tracking toggles use a synced bool with the animator declaring as a float, because of blend trees.
                // - We don't know if the user is using a modern face tracking toggle.
                // - We can't introspect the animator because the user may be using a non destructive workflow.
                // - The user doesn't know the internals of the animator, so we can't ask them, because they could misunderstand it as being the synced type.
                if (_eyeTrackingParameterType == EyeTrackingParameterType.Modern)
                {
                    var param = layer.FloatParameter(_eyeTrackingEnabledParameter);
                    
                    enableBlinking.TransitionsTo(blinkingIgnored).When(param.IsGreaterThan(0.5f));
                    disableBlinking.TransitionsTo(blinkingIgnored).When(param.IsGreaterThan(0.5f));
                    
                    blinkingIgnored.TransitionsTo(enableBlinking).When(param.IsLessThan(0.5f));
                }
                else
                {
                    var param = layer.BoolParameter(_eyeTrackingEnabledParameter);
                    
                    enableBlinking.TransitionsTo(blinkingIgnored).When(param.IsTrue());
                    disableBlinking.TransitionsTo(blinkingIgnored).When(param.IsTrue());
                    
                    blinkingIgnored.TransitionsTo(enableBlinking).When(param.IsFalse());
                }
            }
        }

        private AacFlState CreateBlinkingState(AacFlLayer layer, VRC_AnimatorTrackingControl.TrackingType type)
        {
            return layer.NewState(type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? "EnableBlinking" : "DisableBlinking", type == VRC_AnimatorTrackingControl.TrackingType.Tracking ? 0 : 2, 3)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                .TrackingSets(AacAv3.Av3TrackingElement.Eyes, type);
        }

        private AacFlLayer ReinitializeLayer()
        {
            return _assetContainer.ExposeAac().CreateSupportingArbitraryControllerLayer(_animatorController, "Hai_GestureBlinking")
                .WithAvatarMask(_logicalAvatarMask)
                .WithWeight(0f);
        }
    }
}
