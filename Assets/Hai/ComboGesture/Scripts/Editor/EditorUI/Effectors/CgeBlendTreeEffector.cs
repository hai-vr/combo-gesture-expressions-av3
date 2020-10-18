using System;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeBlendTreeState
    {
    }

    public class CgeBlendTreeEffector
    {
        private readonly CgeBlendTreeState _state;

        public PuppetTemplate CurrentTemplate;
        public AnimationClip MiddleClip;
        public bool CenterSafety = true;
        public float Maximum = 0.97f;
        public float HorizontalFocalPoint = 0.5f;
        public float VerticalFocalPoint = 0.5f;
        public float DescaleLevel = 1f;
        public BlendTree BlendTreeBeingEdited { get; set; }

        public CgeBlendTreeEffector(CgeBlendTreeState state)
        {
            _state = state;
        }

        public BlendTree CreateBlendTreeAsset()
        {
            var is2D = CurrentTemplate != PuppetTemplate.SingleAnalogWithHairTrigger;

            var blendTree = new BlendTree();
            blendTree.blendType = is2D ? BlendTreeType.FreeformDirectional2D : BlendTreeType.Simple1D;
            blendTree.blendParameter = is2D ? "VRCFaceBlendH" : "_AutoGestureWeight";
            if (is2D) {
                blendTree.blendParameterY = "VRCFaceBlendV";
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
                case PuppetTemplate.SingleAnalogWithHairTrigger:
                    blendTree.useAutomaticThresholds = false;
                    blendTree.AddChild(MiddleClip, 0f);
                    blendTree.AddChild(null, 0.8f);
                    blendTree.AddChild(null, 1.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (is2D)
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
    }

    public enum PuppetTemplate
    {
        FourDirections,
        EightDirections,
        SixDirectionsPointingForward,
        SixDirectionsPointingSideways,
        SingleAnalogWithHairTrigger
    }
}
