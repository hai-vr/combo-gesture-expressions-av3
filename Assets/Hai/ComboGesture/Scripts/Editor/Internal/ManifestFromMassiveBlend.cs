using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public static class CgeManifestFromMassiveBlend
    {
        public static ICgeManifest FromMassiveBlend(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
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

        private static ICgeManifest OfSimple(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return CgeMassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<ICgeManifest>
                {
                    CgeSharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    CgeSharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        public static ICgeManifest FromDynamics(ICgeManifest zero, ICgeManifest one, string simpleParameterName, float transitionDuration)
        {
            return CgeMassiveBlendManifest.OfParameterBased(
                CgeMassiveBlendMode.Simple,
                new List<ICgeManifest>
                {
                    zero,
                    one,
                },
                simpleParameterName,
                transitionDuration
            );
        }

        private static ICgeManifest OfTwoDirections(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return CgeMassiveBlendManifest.OfParameterBased(
                massiveBlend.mode,
                new List<ICgeManifest>
                {
                    CgeSharedLayerUtils.FromMoodSet(massiveBlend.simpleZero, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    CgeSharedLayerUtils.FromMoodSet(massiveBlend.simpleOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                    CgeSharedLayerUtils.FromMoodSet(massiveBlend.simpleMinusOne, fallbackWhenAnyClipIsNull, universalAnalogSupport),
                },
                massiveBlend.simpleParameterName,
                massiveBlend.transitionDuration
            );
        }

        private static ICgeManifest OfComplexBlendTreeBased(ComboGestureMassiveBlend massiveBlend, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport)
        {
            return CgeMassiveBlendManifest.OfComplex(
                massiveBlend.mode,
                massiveBlend.blendTreeMoods
                    .Select(mood => CgeSharedLayerUtils.FromMoodSet(mood, fallbackWhenAnyClipIsNull, universalAnalogSupport))
                    .ToList(),
                (BlendTree)massiveBlend.blendTree,
                massiveBlend.transitionDuration
            );
        }
    }
}
