using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureMassiveBlend : ComboGestureMoodSet
    {
        public CgeMassiveBlendMode mode;

        public float transitionDuration = 0.1f;
        public string simpleParameterName;
        public ComboGestureMoodSet simpleZero;
        public ComboGestureMoodSet simpleOne;
        public ComboGestureMoodSet simpleMinusOne;

        public Motion blendTree;
        public List<ComboGestureMoodSet> blendTreeMoods = new List<ComboGestureMoodSet>();
    }

    [Serializable]
    public enum CgeMassiveBlendMode
    {
        Simple,
        TwoDirections,
        ComplexBlendTree
    }
}
