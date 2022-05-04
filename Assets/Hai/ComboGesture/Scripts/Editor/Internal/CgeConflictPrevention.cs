using System;
using Hai.ComboGesture.Scripts.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeConflictPrevention
    {
        public bool ShouldGenerateExhaustiveAnimations { get; }
        public bool ShouldWriteDefaults { get; }

        private static readonly CgeConflictPrevention GenerateExhaustiveAnimationsWithWriteDefaults = new CgeConflictPrevention(true, true);
        private static readonly CgeConflictPrevention GenerateExhaustiveAnimationsWithoutWriteDefaults = new CgeConflictPrevention(true, false);

        private CgeConflictPrevention(bool shouldGenerateExhaustiveAnimations, bool shouldWriteDefaults)
        {
            ShouldGenerateExhaustiveAnimations = shouldGenerateExhaustiveAnimations;
            ShouldWriteDefaults = shouldWriteDefaults;
        }

        public static CgeConflictPrevention OfIntegrator(bool writeDefaults)
        {
            return new CgeConflictPrevention(false, writeDefaults);
        }

        public static CgeConflictPrevention OfFxLayer(WriteDefaultsRecommendationMode mode)
        {
            switch (mode)
            {
                case WriteDefaultsRecommendationMode.FollowVrChatRecommendationWriteDefaultsOff:
                    return GenerateExhaustiveAnimationsWithoutWriteDefaults;
                case WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn:
                    return GenerateExhaustiveAnimationsWithWriteDefaults;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public static CgeConflictPrevention OfGestureLayer(WriteDefaultsRecommendationMode compilerWriteDefaultsRecommendationModeGesture, GestureLayerTransformCapture compilerGestureLayerTransformCapture)
        {
            return new CgeConflictPrevention(
                compilerGestureLayerTransformCapture == GestureLayerTransformCapture.CaptureDefaultTransformsFromAvatar,
                compilerWriteDefaultsRecommendationModeGesture == WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn);
        }
    }
}
