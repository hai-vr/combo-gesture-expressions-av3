using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForAnalogFistSmoothing
    {
        private const string SmoothingLeftLayerName = "Hai_GestureSmoothingLeft";
        private const string SmoothingRightLayerName = "Hai_GestureSmoothingRight";
        private const float DefaultSmoothingFactor = 0.7f;

        private readonly AssetContainer _assetContainer;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly AnimatorController _animatorController;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;

        public LayerForAnalogFistSmoothing(AssetContainer assetContainer, AvatarMask weightCorrectionAvatarMask, bool writeDefaults, AnimatorController animatorController)
        {
            _assetContainer = assetContainer;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _animatorController = animatorController;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, SmoothingLeftLayerName).WithAvatarMask(_weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboLeftWeightProxy,
                SharedLayerUtils.HaiGestureComboLeftWeightSmoothing
            );
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, SmoothingRightLayerName).WithAvatarMask(_weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboRightWeightProxy,
                SharedLayerUtils.HaiGestureComboRightWeightSmoothing
            );
        }

        private void InitializeMachineFor(CgeAacFlLayer layer, string proxyParam, string smoothingParam)
        {
            var zeroClip = SmoothingClip(layer, smoothingParam, 0f);
            var oneClip = SmoothingClip(layer, smoothingParam, 1f);
            var proxyTree = InterpolationTree(layer.FloatParameter(proxyParam).Name, zeroClip, oneClip);
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(proxyTree);

            var smoothingTree = InterpolationTree(layer.FloatParameter(smoothingParam).Name, zeroClip, oneClip);
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(smoothingTree);

            var smoothingFactor = layer.FloatParameter(SharedLayerUtils.HaiGestureComboSmoothingFactor);
            var factorTree = new BlendTree
            {
                name = "autoBT_Factor_" + proxyParam + "",
                blendParameter = smoothingFactor.Name,
                blendType = BlendTreeType.Simple1D,
                minThreshold = 0,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                {
                    new ChildMotion {motion = proxyTree, timeScale = 1, threshold = 0},
                    new ChildMotion {motion = smoothingTree, timeScale = 1, threshold = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(factorTree);

            layer.OverrideValue(smoothingFactor, DefaultSmoothingFactor);
            layer.NewState("Interpolating", 1, 1)
                .WithAnimation(factorTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates)
                .Drives(smoothingFactor, DefaultSmoothingFactor);
        }

        private AnimationClip SmoothingClip(CgeAacFlLayer layer, string smoothingParam, float desiredValue)
        {
            return _assetContainer.ExposeCgeAac().NewClip().Animating(clip =>
            {
                clip.AnimatesAnimator(layer.FloatParameter(smoothingParam)).WithOneFrame(desiredValue);
            }).Clip;
        }

        private static BlendTree InterpolationTree(string param, AnimationClip zeroClip, AnimationClip oneClip)
        {
            return new BlendTree
            {
                name = "autoBT_" + param + "_Interp",
                blendParameter = param,
                blendType = BlendTreeType.Simple1D,
                minThreshold = 0,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                {
                    new ChildMotion {motion = zeroClip, timeScale = 1, threshold = 0},
                    new ChildMotion {motion = oneClip, timeScale = 1, threshold = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };
        }

        public static void Delete(AssetContainer assetContainer, AnimatorController controller)
        {
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, SmoothingLeftLayerName);
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, SmoothingRightLayerName);
        }
    }
}
