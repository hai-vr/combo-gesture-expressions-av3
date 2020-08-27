using System;
using Hai.ComboGesture.Scripts.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class ConflictPrevention
    {
        public bool ShouldGenerateAnimations { get; }
        public bool ShouldWriteDefaults { get; }

        private static readonly ConflictPrevention OnlyWriteDefaults = new ConflictPrevention(false, true);
        private static readonly ConflictPrevention GenerateAnimationsWithWriteDefaults = new ConflictPrevention(true, true);
        private static readonly ConflictPrevention GenerateAnimationsWithoutWriteDefaults = new ConflictPrevention(true, false);

        // As the purpose of this class is to describe an enumeration,
        // the boolean parameter coding convention is purposefully broken here.
        private ConflictPrevention(bool shouldGenerateAnimations, bool shouldWriteDefaults)
        {
            ShouldGenerateAnimations = shouldGenerateAnimations;
            ShouldWriteDefaults = shouldWriteDefaults;
        }

        public static ConflictPrevention Of(ConflictPreventionMode mode)
        {
            switch (mode)
            {
                case ConflictPreventionMode.UseRecommendedConfiguration:
                    return GenerateAnimationsWithWriteDefaults;
                case ConflictPreventionMode.OnlyWriteDefaults:
                    return OnlyWriteDefaults;
                case ConflictPreventionMode.GenerateAnimationsWithWriteDefaults:
                    return GenerateAnimationsWithWriteDefaults;
                case ConflictPreventionMode.GenerateAnimationsWithoutWriteDefaults:
                    return GenerateAnimationsWithoutWriteDefaults;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
