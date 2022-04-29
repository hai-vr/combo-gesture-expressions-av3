using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForBooleansToVirtualActivity
    {
        private const string VirtualActivityLayerName = "Hai_GestureVirtualActivity";

        private readonly AssetContainer _assetContainer;
        private readonly AnimatorController _controller;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly bool _writeDefaultsForLogicalStates;
        private readonly List<GestureComboStageMapper> _comboLayers;

        public LayerForBooleansToVirtualActivity(AssetContainer assetContainer, AnimatorController controller, AvatarMask logicalAvatarMask, bool writeDefaults, List<GestureComboStageMapper> comboLayers)
        {
            _assetContainer = assetContainer;
            _controller = controller;
            _logicalAvatarMask = logicalAvatarMask;
            _writeDefaultsForLogicalStates = writeDefaults;
            _comboLayers = comboLayers;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing virtual activity layer", 0f);
            var layer = ReinitializeLayerAsMachinist();

            EditorUtility.DisplayProgressBar("GestureCombo", "Creating virtual activity layer", 0f);

            var init = layer.NewState("Init", 3, 3)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

            var virtualActivity = layer.IntParameter(SharedLayerUtils.HaiVirtualActivity);

            AacFlState defaultState = null;
            foreach (var comboLayer in _comboLayers)
            {
                var virtualStageValue = comboLayer.internalVirtualStageValue;
                var turnedOn = layer.NewState("Enter Virtual-" + virtualStageValue, 5, 3 + virtualStageValue * 2)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                    .Drives(virtualActivity, comboLayer.internalVirtualStageValue);
                var turnedOff = layer.NewState("Exit Virtual-" + virtualStageValue, 5, 3 + virtualStageValue * 2 + 1)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

                if (!string.IsNullOrEmpty(comboLayer.booleanParameterName))
                {
                    turnedOff.Drives(layer.BoolParameter(comboLayer.booleanParameterName), false);
                }

                var turningOn = init.TransitionsTo(turnedOn);
                foreach (var otherLayer in _comboLayers)
                {
                    if (otherLayer.booleanParameterName != "")
                    {
                        var otherLayerBool = layer.BoolParameter(otherLayer.booleanParameterName);
                        var isSameLayer = comboLayer.booleanParameterName == otherLayer.booleanParameterName;

                        turnedOn.Drives(otherLayerBool, isSameLayer);
                        turningOn.When(otherLayerBool.IsEqualTo(isSameLayer));

                        turnedOn.TransitionsTo(turnedOff)
                            .When(otherLayerBool.IsEqualTo(!isSameLayer));
                    }
                }

                turnedOff.AutomaticallyMovesTo(init);

                if (virtualStageValue == 0)
                {
                    defaultState = turnedOn;
                }
            }

            // This shouldn't be null, there should always be a virtual stage value of 0
            if (defaultState != null)
            {
                // This transition needs to be created last
                init.AutomaticallyMovesTo(defaultState);
            }
        }

        private AacFlLayer ReinitializeLayerAsMachinist()
        {
            return _assetContainer.ExposeAac().CreateSupportingArbitraryControllerLayer(_controller, VirtualActivityLayerName)
                .CGE_WithLayerWeight(0f)
                .WithAvatarMask(_logicalAvatarMask);
        }

        public static void Delete(AssetContainer assetContainer, AnimatorController animatorController)
        {
            assetContainer.ExposeAac().CGE_RemoveSupportingArbitraryControllerLayer(animatorController, VirtualActivityLayerName);
        }
    }
}
