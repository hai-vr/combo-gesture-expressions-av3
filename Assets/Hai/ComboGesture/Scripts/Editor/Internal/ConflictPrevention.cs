using System;
using Hai.ComboGesture.Scripts.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class ConflictPrevention
    {
        public bool ShouldGenerateExhaustiveAnimations { get; }
        public bool ShouldWriteDefaults { get; }

        private static readonly ConflictPrevention GenerateExhaustiveAnimationsWithWriteDefaults = new ConflictPrevention(true, true);
        private static readonly ConflictPrevention GenerateExhaustiveAnimationsWithoutWriteDefaults = new ConflictPrevention(true, false);

        // As the purpose of this class is to describe an enumeration,
        // the boolean parameter coding convention is purposefully broken here.
        private ConflictPrevention(bool shouldGenerateExhaustiveAnimations, bool shouldWriteDefaults)
        {
            ShouldGenerateExhaustiveAnimations = shouldGenerateExhaustiveAnimations;
            ShouldWriteDefaults = shouldWriteDefaults;
        }

        public static ConflictPrevention OfIntegrator(bool writeDefaults)
        {
            return new ConflictPrevention(false, writeDefaults);
        }

        public static ConflictPrevention OfFxLayer(WriteDefaultsRecommendationMode mode)
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

        public static ConflictPrevention OfGestureLayer(WriteDefaultsRecommendationMode compilerWriteDefaultsRecommendationModeGesture, GestureLayerTransformCapture compilerGestureLayerTransformCapture)
        {
            return new ConflictPrevention(
                compilerGestureLayerTransformCapture == GestureLayerTransformCapture.CaptureDefaultTransformsFromAvatar,
                compilerWriteDefaultsRecommendationModeGesture == WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn);
        }
    }
}
