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
        public static IManifest FromMassiveBlend(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            switch (massiveBlend.mode)
            {
                case CgeMassiveBlendMode.Simple:
                    return OfSimple(massiveBlend, fallbackWhenAnyClipIsNull);
                case CgeMassiveBlendMode.TwoDirections:
                    return OfTwoDirections(massiveBlend, fallbackWhenAnyClipIsNull);
                case CgeMassiveBlendMode.ComplexBlendTree:
                    return OfComplexBlendTreeBased(massiveBlend, fallbackWhenAnyClipIsNull);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IManifest OfSimple(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfTwoDirections(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.simpleMinusOne, fallbackWhenAnyClipIsNull),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfComplexBlendTreeBased(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfComplex(
                massiveBlend.mode,
                massiveBlend.blendTreeMoods
                    .Select(mood => SharedLayerUtils.FromMoodSet(mood, fallbackWhenAnyClipIsNull))
                    .ToList(),
                (BlendTree)massiveBlend.blendTree,
                massiveBlend.transitionDuration
            );
        }
    }
}
