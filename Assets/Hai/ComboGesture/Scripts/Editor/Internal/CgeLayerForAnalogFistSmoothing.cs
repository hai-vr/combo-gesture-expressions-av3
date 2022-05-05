using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class CgeLayerForAnalogFistSmoothing
    {
        private const string SmoothingLeftLayerName = "Hai_GestureSmoothingLeft";
        private const string SmoothingRightLayerName = "Hai_GestureSmoothingRight";
        private const float DefaultSmoothingFactor = 0.7f;

        private readonly CgeAssetContainer _assetContainer;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly AnimatorController _animatorController;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;

        public CgeLayerForAnalogFistSmoothing(CgeAssetContainer assetContainer, AvatarMask weightCorrectionAvatarMask, bool writeDefaults, AnimatorController animatorController)
        {
            _assetContainer = assetContainer;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _animatorController = animatorController;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("ComboGestureExpressions", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, SmoothingLeftLayerName).WithAvatarMask(_weightCorrectionAvatarMask),
                CgeSharedLayerUtils.HaiGestureComboLeftWeightProxy,
                CgeSharedLayerUtils.HaiGestureComboLeftWeightSmoothing,
                "GestureLeftWeight",
                "GestureLeft"
            );
            InitializeMachineFor(
                _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_animatorController, SmoothingRightLayerName).WithAvatarMask(_weightCorrectionAvatarMask),
                CgeSharedLayerUtils.HaiGestureComboRightWeightProxy,
                CgeSharedLayerUtils.HaiGestureComboRightWeightSmoothing,
                "GestureRightWeight",
                "GestureRight"
            );
        }

        private void InitializeMachineFor(CgeAacFlLayer layer, string proxyParam, string smoothingParam, string liveParam, string handParam)
        {
            var smoothingFactor = layer.FloatParameter(CgeSharedLayerUtils.HaiGestureComboSmoothingFactor);
            var factorTree = CreateFactorTreeFull(layer, proxyParam, smoothingParam);

            layer.OverrideValue(smoothingFactor, DefaultSmoothingFactor);
            var waiting = layer.NewState("Waiting", 1, 1)
                .WithAnimation(factorTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates)
                .MotionTime(layer.FloatParameter(proxyParam))
                .Drives(smoothingFactor, DefaultSmoothingFactor);

            var listening = layer.NewState("Listening")
                .WithAnimation(factorTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates)
                .MotionTime(layer.FloatParameter(liveParam))
                .Drives(smoothingFactor, DefaultSmoothingFactor);

            waiting.TransitionsTo(listening).When(layer.IntParameter(handParam).IsEqualTo(1));
            listening.TransitionsTo(waiting).When(layer.IntParameter(handParam).IsNotEqualTo(1));
        }

        private BlendTree CreateFactorTreeFull(CgeAacFlLayer layer, string proxyParam, string smoothingParam)
        {
            var zeroClip = SmoothingClip(layer, proxyParam, smoothingParam, 0f);
            var oneClip = SmoothingClip(layer, proxyParam, smoothingParam, 1f);
            return CreateFactorTree(layer, proxyParam, smoothingParam, zeroClip, oneClip);
        }

        private BlendTree CreateFactorTree(CgeAacFlLayer layer, string proxyParam, string smoothingParam, AnimationClip zeroClip, AnimationClip oneClip)
        {
            var proxyTree = InterpolationTree(layer.FloatParameter(proxyParam).Name, zeroClip, oneClip);
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(proxyTree);

            var smoothingTree = InterpolationTree(layer.FloatParameter(smoothingParam).Name, zeroClip, oneClip);
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(smoothingTree);

            var smoothingFactor = layer.FloatParameter(CgeSharedLayerUtils.HaiGestureComboSmoothingFactor);
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
                    new ChildMotion {motion = smoothingTree, timeScale = 1, threshold = 1}
                },
                hideFlags = HideFlags.HideInHierarchy
            };
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(factorTree);

            return factorTree;
        }

        private AnimationClip SmoothingClip(CgeAacFlLayer layer, string proxyParam, string smoothingParam, float desiredValue)
        {
            return _assetContainer.ExposeCgeAac().NewClip().Animating(clip =>
            {
                clip.AnimatesAnimator(layer.FloatParameter(smoothingParam)).WithOneFrame(desiredValue);
                clip.Animates("", typeof(Animator), proxyParam).WithSecondsUnit(keyframes => keyframes.Linear(0, 0).Linear(1f, 1f));
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

        public static void Delete(CgeAssetContainer assetContainer, AnimatorController controller)
        {
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, SmoothingLeftLayerName);
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(controller, SmoothingRightLayerName);
        }
    }
}
