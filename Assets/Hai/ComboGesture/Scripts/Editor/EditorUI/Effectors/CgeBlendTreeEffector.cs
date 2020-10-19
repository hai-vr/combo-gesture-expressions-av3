using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Hai.ComboGesture.Scripts.Editor.Internal.Processing;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeBlendTreeEffector
    {
        public PuppetTemplate CurrentTemplate;
        public AnimationClip MiddleClip;
        public bool CenterSafety = true;
        public float Maximum = 0.97f;
        public float HorizontalFocalPoint = 0.5f;
        public float VerticalFocalPoint = 0.5f;
        public float DescaleLevel = 1f;
        public BlendTree BlendTreeBeingEdited { get; set; }

        public BlendTree CreateBlendTreeAsset()
        {
            var is2D = CurrentTemplate != PuppetTemplate.SingleAnalogFistWithHairTrigger;

            var blendTree = new BlendTree();
            blendTree.blendType = is2D ? BlendTreeType.FreeformDirectional2D : BlendTreeType.Simple1D;
            if (CurrentTemplate == PuppetTemplate.DualAnalogFist)
            {
                blendTree.blendParameter = "GestureRightWeight";
                blendTree.blendParameterY = "GestureLeftWeight";
            }
            else
            {
                blendTree.blendParameter = is2D ? "VRCFaceBlendH" : CgeBlendTreeAutoWeightCorrector.AutoGestureWeightParam;
                if (is2D) {
                    if (CurrentTemplate != PuppetTemplate.SingleAnalogFistAndTwoDirections) {
                        blendTree.blendParameterY = "VRCFaceBlendV";
                    }
                    else
                    {
                        blendTree.blendParameterY = CgeBlendTreeAutoWeightCorrector.AutoGestureWeightParam;
                    }
                }
            }

            switch (CurrentTemplate)
            {
                case PuppetTemplate.FourDirections:
                    blendTree.AddChild(null, new Vector2(0, Maximum));
                    blendTree.AddChild(null, new Vector2(Maximum, 0));
                    blendTree.AddChild(null, new Vector2(0, -Maximum));
                    blendTree.AddChild(null, new Vector2(-Maximum, 0));
                    break;
                case PuppetTemplate.EightDirections:
                    blendTree.AddChild(null, new Vector2(0, Maximum));
                    blendTree.AddChild(null, AtRevolution(1/8f));
                    blendTree.AddChild(null, new Vector2(Maximum, 0));
                    blendTree.AddChild(null, AtRevolution(3/8f));
                    blendTree.AddChild(null, new Vector2(0, -Maximum));
                    blendTree.AddChild(null, AtRevolution(5/8f));
                    blendTree.AddChild(null, new Vector2(-Maximum, 0));
                    blendTree.AddChild(null, AtRevolution(7/8f));
                    break;
                case PuppetTemplate.SixDirectionsPointingForward:
                    blendTree.AddChild(null, new Vector2(0, Maximum));
                    blendTree.AddChild(null, AtRevolution(1/6f));
                    blendTree.AddChild(null, AtRevolution(2/6f));
                    blendTree.AddChild(null, new Vector2(0, -Maximum));
                    blendTree.AddChild(null, AtRevolution(4/6f));
                    blendTree.AddChild(null, AtRevolution(5/6f));
                    break;
                case PuppetTemplate.SixDirectionsPointingSideways:
                    blendTree.AddChild(null, new Vector2(Maximum, 0));
                    blendTree.AddChild(null, AtRevolution(1/12f + 2/6f));
                    blendTree.AddChild(null, AtRevolution(1/12f + 3/6f));
                    blendTree.AddChild(null, new Vector2(-Maximum, 0));
                    blendTree.AddChild(null, AtRevolution(1/12f + 5/6f));
                    blendTree.AddChild(null, AtRevolution(1/12f + 0/6f));
                    break;
                case PuppetTemplate.SingleAnalogFistWithHairTrigger:
                    blendTree.useAutomaticThresholds = false;
                    blendTree.AddChild(MiddleClip, 0f);
                    blendTree.AddChild(null, 0.8f);
                    blendTree.AddChild(null, 1.0f);
                    break;
                case PuppetTemplate.SingleAnalogFistAndTwoDirections:
                    blendTree.AddChild(null, new Vector2(-Maximum, 1));
                    blendTree.AddChild(null, new Vector2(0, 1));
                    blendTree.AddChild(null, new Vector2(Maximum, 1));
                    blendTree.AddChild(MiddleClip, new Vector2(-1, 0));
                    blendTree.AddChild(MiddleClip, new Vector2(0, 0));
                    blendTree.AddChild(MiddleClip, new Vector2(1, 0));
                    break;
                case PuppetTemplate.DualAnalogFist:
                    blendTree.AddChild(null, new Vector2(1, 1));
                    blendTree.AddChild(null, new Vector2(1, 0));
                    blendTree.AddChild(null, new Vector2(0, 1));
                    blendTree.AddChild(MiddleClip, new Vector2(0, 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (is2D
                && CurrentTemplate != PuppetTemplate.SingleAnalogFistAndTwoDirections
                && CurrentTemplate != PuppetTemplate.DualAnalogFist)
            {
                blendTree.AddChild(MiddleClip, new Vector2(0, 0));
                if (CenterSafety)
                {
                    blendTree.AddChild(MiddleClip, new Vector2(0, 0.1f));
                    blendTree.AddChild(MiddleClip, new Vector2(0.1f, 0));
                    blendTree.AddChild(MiddleClip, new Vector2(0, -0.1f));
                    blendTree.AddChild(MiddleClip, new Vector2(-0.1f, 0));
                }
            }

            return blendTree;
        }

        private Vector2 AtRevolution(float revolutionAmount)
        {
            return new Vector2(
                (float) (Math.Sin(revolutionAmount * Math.PI * 2) * Maximum),
                (float) (Math.Cos(revolutionAmount * Math.PI * 2) * Maximum));
        }

        public List<AnimationClip> AllAnimationsOfSelected()
        {
            return BlendTreeBeingEdited != null ? ManifestFromPuppet.AllAnimationsOf(BlendTreeBeingEdited) : new List<AnimationClip>();
        }
    }

    public enum PuppetTemplate
    {
        FourDirections,
        EightDirections,
        SixDirectionsPointingForward,
        SixDirectionsPointingSideways,
        SingleAnalogFistWithHairTrigger,
        SingleAnalogFistAndTwoDirections,
        DualAnalogFist
    }
}
