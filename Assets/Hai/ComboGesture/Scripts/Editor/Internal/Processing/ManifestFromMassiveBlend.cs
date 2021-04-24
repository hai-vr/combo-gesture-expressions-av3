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
        public static IManifest FromMassiveBlend(ComboGestureMoodSet massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            switch (massiveBlend.massive.mode)
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

        private static IManifest OfSimple(ComboGestureMoodSet massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.massive.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.massive.simpleZero, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.massive.simpleOne, fallbackWhenAnyClipIsNull),
                },
                massiveBlend.massive.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfTwoDirections(ComboGestureMoodSet massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfParameterBased(
                massiveBlend.massive.mode,
                new List<IManifest>
                {
                    SharedLayerUtils.FromMoodSet(massiveBlend.massive.simpleZero, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.massive.simpleOne, fallbackWhenAnyClipIsNull),
                    SharedLayerUtils.FromMoodSet(massiveBlend.massive.simpleMinusOne, fallbackWhenAnyClipIsNull),
                },
                massiveBlend.massive.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static IManifest OfComplexBlendTreeBased(ComboGestureMoodSet massiveBlend, AnimationClip fallbackWhenAnyClipIsNull)
        {
            return MassiveBlendManifest.OfComplex(
                massiveBlend.massive.mode,
                massiveBlend.massive.massiveBlendTreeMoods
                    .Select(mood => SharedLayerUtils.FromMoodSet(mood, fallbackWhenAnyClipIsNull))
                    .ToList(),
                (BlendTree)massiveBlend.massive.massiveBlendTree,
                massiveBlend.transitionDuration
            );
        }
    }
}
