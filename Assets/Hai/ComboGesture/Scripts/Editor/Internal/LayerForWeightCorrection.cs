using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForWeightCorrection
    {
        private const string WeightCorrectionLeftLayerName = "Hai_GestureWeightLeft";
        private const string WeightCorrectionRightLayerName = "Hai_GestureWeightRight";

        private readonly AssetContainer _assetContainer;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;
        private readonly bool _universalAnalogSupport;
        private readonly AnimatorController _animatorController;

        public LayerForWeightCorrection(AssetContainer assetContainer, AnimatorController animatorController, AvatarMask weightCorrectionAvatarMask, bool writeDefaults, bool universalAnalogSupport)
        {
            _assetContainer = assetContainer;
            _animatorController = animatorController;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
            _universalAnalogSupport = universalAnalogSupport;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, WeightCorrectionLeftLayerName)
                    .WithAvatarMask(_weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboLeftWeightProxy,
                "GestureLeftWeight",
                "GestureLeft"
            );
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, WeightCorrectionRightLayerName)
                    .WithAvatarMask(_weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboRightWeightProxy,
                "GestureRightWeight",
                "GestureRight"
            );
        }

        private void InitializeMachineFor(CgeAacFlLayer layer, string proxyParam, string liveParam, string handParam)
        {
            if (_universalAnalogSupport)
            {
                // The order in which the states are created matters (initial state).
                // Move CreateListeningState as the first state has some implications.
                CreateListeningState(layer, liveParam, proxyParam);
                return;
            }

            var waiting = layer.NewState("Waiting")
                .WithAnimation(CreateProxyClip(proxyParam))
                .MotionTime(layer.FloatParameter(proxyParam))
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates);

            var listening = CreateListeningState(layer, liveParam, proxyParam);

            waiting.TransitionsTo(listening).When(layer.IntParameter(handParam).IsEqualTo(1));
            listening.TransitionsTo(waiting).When(layer.IntParameter(handParam).IsNotEqualTo(1));
        }

        private CgeAacFlState CreateListeningState(CgeAacFlLayer layer, string liveParam, string proxyParam)
        {
            return layer.NewState("Listening")
                .WithAnimation(CreateProxyClip(proxyParam))
                .MotionTime(layer.FloatParameter(liveParam))
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates);
        }

        private CgeAacFlClip CreateProxyClip(string proxyParam)
        {
            // It's not a big deal, but this proxy asset is generated multiple times identically.
            return _assetContainer.ExposeCgeAac().NewClip().Animating(clip =>
            {
                clip.Animates("", typeof(Animator), proxyParam).WithSecondsUnit(keyframes => keyframes.Linear(0, 0).Linear(1f, 1f));
            });
        }

        public static void Delete(AssetContainer assetContainer, AnimatorController controller)
        {
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, WeightCorrectionLeftLayerName);
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, WeightCorrectionRightLayerName);
        }
    }
}
