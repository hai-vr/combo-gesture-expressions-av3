using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForAnalogFistSmoothing
    {
        private const string SmoothingLeftLayerName = "Hai_GestureSmoothingLeft";
        private const string SmoothingRightLayerName = "Hai_GestureSmoothingRight";
        private const string SmoothLeftZero = "Assets/Hai/ComboGesture/Hai_ComboGesture_LWSmoothZero.anim";
        private const string SmoothLeftOne = "Assets/Hai/ComboGesture/Hai_ComboGesture_LWSmoothOne.anim";
        private const string SmoothRightZero = "Assets/Hai/ComboGesture/Hai_ComboGesture_RWSmoothZero.anim";
        private const string SmoothRightOne = "Assets/Hai/ComboGesture/Hai_ComboGesture_RWSmoothOne.anim";
        private const float DefaultSmoothingFactor = 0.7f;

        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly AnimatorController _animatorController;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;

        public LayerForAnalogFistSmoothing(AnimatorGenerator animatorGenerator, AvatarMask weightCorrectionAvatarMask, bool writeDefaults, AnimatorController animatorController)
        {
            _animatorGenerator = animatorGenerator;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _animatorController = animatorController;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(SmoothingLeftLayerName, 1f, _weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboLeftWeightProxy,
                SharedLayerUtils.HaiGestureComboLeftWeightSmoothing,
                SmoothLeftZero,
                SmoothLeftOne
            );
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(SmoothingRightLayerName, 1f, _weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboRightWeightProxy,
                SharedLayerUtils.HaiGestureComboRightWeightSmoothing,
                SmoothRightZero,
                SmoothRightOne
            );
        }

        private void InitializeMachineFor(Machinist machine, string proxyParam, string smoothingParam, string zeroClip, string oneClip)
        {
            var proxyTree = InterpolationTree(proxyParam, zeroClip, oneClip);
            RegisterBlendTreeAsAsset(_animatorController, proxyTree);

            var smoothingTree = InterpolationTree(smoothingParam, zeroClip, oneClip);
            RegisterBlendTreeAsAsset(_animatorController, smoothingTree);

            var factorTree = new BlendTree
            {
                name = "autoBT_Factor_" + proxyParam + "",
                blendParameter = SharedLayerUtils.HaiGestureComboSmoothingFactor,
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
            RegisterBlendTreeAsAsset(_animatorController, factorTree);

            machine.NewState("Interpolating", 1, 1)
                .WithAnimation(factorTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForAnimatedAnimatorParameterStates)
                .Drives(new FloatParameterist(SharedLayerUtils.HaiGestureComboSmoothingFactor), DefaultSmoothingFactor);

        }

        private static BlendTree InterpolationTree(string param, string zeroClip, string oneClip)
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
                    new ChildMotion {motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(zeroClip), timeScale = 1, threshold = 0},
                    new ChildMotion {motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(oneClip), timeScale = 1, threshold = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };
        }

        private static void RegisterBlendTreeAsAsset(AnimatorController animatorController, BlendTree blendTree)
        {
            if (AssetDatabase.GetAssetPath(animatorController) != "")
            {
                AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(animatorController));
            }
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(SmoothingLeftLayerName);
            animatorGenerator.RemoveLayerIfExists(SmoothingRightLayerName);
        }
    }
}
