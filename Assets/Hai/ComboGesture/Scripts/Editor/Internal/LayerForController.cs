using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForController
    {
        private const string ControllerLayerName = "Hai_GestureCtrl";
        private const bool WriteDefaultsForLogicalStates = true;

        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AvatarMask _logicalAvatarMask;
        private readonly AnimationClip _emptyClip;

        public LayerForController(AnimatorGenerator animatorGenerator, AvatarMask logicalAvatarMask, AnimationClip emptyClip)
        {
            _animatorGenerator = animatorGenerator;
            _logicalAvatarMask = logicalAvatarMask;
            _emptyClip = emptyClip;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Clearing combo controller layer", 0f);
            var machine = ReinitializeLayer();

            EditorUtility.DisplayProgressBar("GestureCombo", "Creating combo controller layer", 0f);

            for (var left = 0; left < 8; left++)
            {
                for (var right = left; right < 8; right++)
                {
                    var state = machine.AddState(left + "" + right, SharedLayerUtils.GridPosition(right, left));
                    state.writeDefaultValues = WriteDefaultsForLogicalStates;
                    state.motion = _emptyClip;

                    var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
                    {
                        new VRC_AvatarParameterDriver.Parameter {name = SharedLayerUtils.HaiGestureComboParamName, value = left * 10 + right}
                    };

                    {
                        var normal = machine.AddAnyStateTransition(state);
                        SharedLayerUtils.SetupImmediateTransition(normal);
                        normal.AddCondition(AnimatorConditionMode.Equals, left, "GestureLeft");
                        normal.AddCondition(AnimatorConditionMode.Equals, right, "GestureRight");
                    }
                    if (left != right)
                    {
                        var reverse = machine.AddAnyStateTransition(state);
                        SharedLayerUtils.SetupImmediateTransition(reverse);
                        reverse.AddCondition(AnimatorConditionMode.Equals, right, "GestureLeft");
                        reverse.AddCondition(AnimatorConditionMode.Equals, left, "GestureRight");
                    }
                }
            }
        }

        private AnimatorStateMachine ReinitializeLayer()
        {
            return _animatorGenerator.CreateOrRemakeLayerAtSameIndex(ControllerLayerName, 0f, _logicalAvatarMask).ExposeMachine();
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(ControllerLayerName);
        }
    }
}
