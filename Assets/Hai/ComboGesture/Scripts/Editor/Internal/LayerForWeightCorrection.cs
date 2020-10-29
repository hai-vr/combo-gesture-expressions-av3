using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForWeightCorrection
    {
        private const string WeightCorrectionLeftLayerName = "Hai_GestureWeightLeft";
        private const string WeightCorrectionRightLayerName = "Hai_GestureWeightRight";
        private const string LeftProxyClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_LWProxy.anim";
        private const string RightProxyClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_RWProxy.anim";

        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;

        public LayerForWeightCorrection(AnimatorGenerator animatorGenerator, AvatarMask weightCorrectionAvatarMask, bool writeDefaults)
        {
            _animatorGenerator = animatorGenerator;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(WeightCorrectionLeftLayerName, 1f, _weightCorrectionAvatarMask).ExposeMachine(),
                SharedLayerUtils.HaiGestureComboLeftWeightProxy,
                "GestureLeftWeight",
                "GestureLeft",
                LeftProxyClipPath
            );
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(WeightCorrectionRightLayerName, 1f, _weightCorrectionAvatarMask).ExposeMachine(),
                SharedLayerUtils.HaiGestureComboRightWeightProxy,
                "GestureRightWeight",
                "GestureRight",
                RightProxyClipPath
            );
        }

        private void InitializeMachineFor(AnimatorStateMachine machine, string proxyParam, string liveParam, string handParam, string clipPath)
        {
            var waiting = machine.AddState("Waiting", SharedLayerUtils.GridPosition(1, 1));
            waiting.timeParameter = proxyParam;
            waiting.timeParameterActive = true;
            waiting.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            waiting.speed = 1;
            waiting.writeDefaultValues = _writeDefaultsForAnimatedAnimatorParameterStates;

            var listening = machine.AddState("Listening", SharedLayerUtils.GridPosition(1, 2));
            listening.timeParameter = liveParam;
            listening.timeParameterActive = true;
            listening.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            listening.speed = 1;
            listening.writeDefaultValues = _writeDefaultsForAnimatedAnimatorParameterStates;

            var toListening = waiting.AddTransition(listening);
            SetupTransition(toListening);
            toListening.AddCondition(AnimatorConditionMode.Equals, 1, handParam);

            var toWaiting = listening.AddTransition(waiting);
            SetupTransition(toWaiting);
            toWaiting.AddCondition(AnimatorConditionMode.NotEqual, 1, handParam);
        }

        private static void SetupTransition(AnimatorStateTransition toListening)
        {
            toListening.hasExitTime = false;
            toListening.exitTime = 0f;
            toListening.hasFixedDuration = true;
            toListening.duration = 0;
            toListening.offset = 0;
            toListening.interruptionSource = TransitionInterruptionSource.None;
            toListening.orderedInterruption = true;
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(WeightCorrectionLeftLayerName);
            animatorGenerator.RemoveLayerIfExists(WeightCorrectionRightLayerName);
        }
    }
}
