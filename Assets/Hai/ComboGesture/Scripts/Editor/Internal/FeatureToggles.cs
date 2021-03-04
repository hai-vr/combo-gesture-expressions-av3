using System;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    [Flags]
    public enum FeatureToggles
    {
        None = 0,
        // ExposeDisableExpressions = 1,
        // ExposeDisableBlinkingOverride = 2,
        // ExposeAreEyesClosed = 4,
        DoNotGenerateBlinkingOverrideLayer = 8,
        // DoNotGenerateControllerLayer = 16,
        DoNotGenerateLipsyncOverrideLayer = 32,
        ExposeDisableLipsyncOverride = 64,
        ExposeIsLipsyncLimited = 128,
        DoNotGenerateWeightCorrectionLayer = 256,
        // ForceGenerationOfControllerLayer = 512,
        DoNotFixSingleKeyframes = 1024
    }
}
