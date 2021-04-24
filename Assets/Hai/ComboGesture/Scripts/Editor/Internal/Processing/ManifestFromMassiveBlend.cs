using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Processing
{
    public static class ManifestFromMassiveBlend
    {
        public static IManifest FromMassiveBlend(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            switch (massiveBlend.mode)
            {
                case CgeMassiveBlendMode.Simple:
                    return OfSimple(massiveBlend, fallbackWhenAnyClipIsNull, universalAnalogSupport);
                case CgeMassiveBlendMode.TwoDirections:
                    return OfTwoDirections(massiveBlend, fallbackWhenAnyClipIsNull, universalAnalogSupport);
                case CgeMassiveBlendMode.ComplexBlendTree:
                    return OfComplexBlendTreeBased(massiveBlend, fallbackWhenAnyClipIsNull, universalAnalogSupport);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IManifest OfSimple(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfTwoDirections(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleMinusOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfComplexBlendTreeBased(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return MassiveBlendManifest.OfComplex(
                massiveBlend.mode,
                massiveBlend.blendTreeMoods
                    .Select(mood => SharedLayerUtils.FromMoodSet(mood, fallbackWhenAnyClipIsNull, universalAnalogSupport))
                    .ToList(),
                (BlendTree)massiveBlend.blendTree,
                massiveBlend.transitionDuration
            );
        }
    }
}
