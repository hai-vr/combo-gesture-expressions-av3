using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForBooleansToVirtualActivity
    {
        private const string VirtualActivityLayerName = "Hai_GestureVirtualActivity";

        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly bool _writeDefaultsForLogicalStates;
        private readonly List<GestureComboStageMapper> _comboLayers;

        public LayerForBooleansToVirtualActivity(AnimatorGenerator animatorGenerator, AvatarMask logicalAvatarMask, bool writeDefaults, List<GestureComboStageMapper> comboLayers)
        {
            _animatorGenerator = animatorGenerator;
            _logicalAvatarMask = logicalAvatarMask;
            _writeDefaultsForLogicalStates = writeDefaults;
            _comboLayers = comboLayers;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing virtual activity layer", 0f);
            var machine = ReinitializeLayerAsMachinist();

            EditorUtility.DisplayProgressBar("GestureCombo", "Creating virtual activity layer", 0f);

            var init = machine.NewState("Init", 3, 3)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

            var virtualActivity = new IntParameterist(SharedLayerUtils.HaiVirtualActivity);

            Statist defaultState = null;
            foreach (var layer in _comboLayers)
            {
                var virtualStageValue = layer.internalVirtualStageValue;
                var turnedOn = machine.NewState("Enter Virtual-" + virtualStageValue, 5, 3 + virtualStageValue * 2)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                    .Drives(virtualActivity, layer.internalVirtualStageValue);
                var turnedOff = machine.NewState("Exit Virtual-" + virtualStageValue, 5, 3 + virtualStageValue * 2 + 1)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

                if (!string.IsNullOrEmpty(layer.booleanParameterName))
                {
                    turnedOff.Drives(new BoolParameterist(layer.booleanParameterName), false);
                }

                var turningOn = init.TransitionsTo(turnedOn);
                foreach (var otherLayer in _comboLayers)
                {
                    if (otherLayer.booleanParameterName != "")
                    {
                        var otherLayerBool = new BoolParameterist(otherLayer.booleanParameterName);
                        var isSameLayer = layer.booleanParameterName == otherLayer.booleanParameterName;

                        turnedOn.Drives(otherLayerBool, isSameLayer);
                        turningOn.When(otherLayerBool).Is(isSameLayer);

                        turnedOn.TransitionsTo(turnedOff)
                            .When(otherLayerBool).Is(!isSameLayer);
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

        private Machinist ReinitializeLayerAsMachinist()
        {
            return _animatorGenerator.CreateOrRemakeLayerAtSameIndex(VirtualActivityLayerName, 0f, _logicalAvatarMask);
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(VirtualActivityLayerName);
        }
    }
}
