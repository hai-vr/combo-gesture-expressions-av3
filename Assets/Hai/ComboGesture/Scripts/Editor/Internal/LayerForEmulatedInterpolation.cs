using System.Globalization;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Reused;
using UnityEditor;
using UnityEngine;
using static Hai.ComboGesture.Scripts.Editor.Internal.Reused.Av3Parameterists;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForEmulatedInterpolation
    {
        internal const string EmulatedInterpolationLeftLayerName = "Hai_GestureEmulatedLeft";
        internal const string EmulatedInterpolationRightLayerName = "Hai_GestureEmulatedRight";
        private const string LeftEmulatedClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_LWEmulated.anim";
        private const string RightEmulatedClipPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_RWEmulated.anim";

        private readonly AnimatorGenerator _animatorGenerator;
        private readonly AvatarMask _weightCorrectionAvatarMask;
        private readonly bool _writeDefaultsForLogicalStates;
        private readonly bool _writeDefaultsForAnimatedAnimatorParameterStates;

        public LayerForEmulatedInterpolation(AnimatorGenerator animatorGenerator, AvatarMask weightCorrectionAvatarMask, bool writeDefaults)
        {
            _animatorGenerator = animatorGenerator;
            _weightCorrectionAvatarMask = weightCorrectionAvatarMask;
            _writeDefaultsForLogicalStates = writeDefaults;
            _writeDefaultsForAnimatedAnimatorParameterStates = writeDefaults;
        }

        internal void Create()
        {
            EditorUtility.DisplayProgressBar("GestureCombo", "Creating weight correction layer", 0f);
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(EmulatedInterpolationLeftLayerName, 1f, _weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboLeftWeightDelta,
                SharedLayerUtils.HaiGestureComboLeftWeightEmulatedInterpolation,
                LeftEmulatedClipPath,
                SharedLayerUtils.HaiGestureComboLeftWeightProxy);
            InitializeMachineFor(
                _animatorGenerator.CreateOrRemakeLayerAtSameIndex(EmulatedInterpolationRightLayerName, 1f, _weightCorrectionAvatarMask),
                SharedLayerUtils.HaiGestureComboRightWeightDelta,
                SharedLayerUtils.HaiGestureComboRightWeightEmulatedInterpolation,
                RightEmulatedClipPath,
                SharedLayerUtils.HaiGestureComboRightWeightProxy);
        }

        private void InitializeMachineFor(Machinist machine, string deltaParam, string smoothingParam, string clipPath, string copyParam)
        {
            var nothing = machine.NewState("Nothing", 3, 0)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

            var localCopy = machine.NewState("Local Copy", -2, 0)
                .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates);

            var localCopyState = localCopy.Expose();
            localCopyState.timeParameter = copyParam;
            localCopyState.timeParameterActive = true;
            localCopyState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            localCopyState.speed = 1;
            localCopyState.writeDefaultValues = _writeDefaultsForAnimatedAnimatorParameterStates;

            var deltaParameterist = new FloatParameterist(deltaParam);
            var smoothingParameterist = new FloatParameterist(smoothingParam);

            // Transitions must be created in this order
            machine.AnyTransitionsTo(localCopy).WithNoTransitionToSelf().Whenever(ItIsLocal());

            var progression = Enumerable.Range(0, 20).Select(i => 1f / (i * i * i + 2)).ToList();
            for (var index = 0; index < progression.Count; index++)
            {
                var positiveValue = progression[index];
                var state = machine.NewState(string.Format(CultureInfo.InvariantCulture, "+{0:0.000000}", positiveValue), 3, -progression.Count + index)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                    .DrivingIncreases(smoothingParameterist, positiveValue);
                machine.AnyTransitionsTo(state).Whenever(ItIsRemote()).And(deltaParameterist).IsGreaterThan(positiveValue);
            }
            // Two for loops: Transitions should be created in a very specific order
            for (var index = 0; index < progression.Count; index++)
            {
                var positiveValue = progression[index];
                var state = machine.NewState(string.Format(CultureInfo.InvariantCulture, "-{0:0.000000}", positiveValue), 3, progression.Count - index)
                    .WithWriteDefaultsSetTo(_writeDefaultsForLogicalStates)
                    .DrivingDecreases(smoothingParameterist, positiveValue);
                machine.AnyTransitionsTo(state).Whenever(ItIsRemote()).And(deltaParameterist).IsLesserThan(-positiveValue);
            }

            machine.AnyTransitionsTo(nothing).WithNoTransitionToSelf().Whenever(ItIsRemote());
        }

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(EmulatedInterpolationLeftLayerName);
            animatorGenerator.RemoveLayerIfExists(EmulatedInterpolationRightLayerName);
        }
    }
}
