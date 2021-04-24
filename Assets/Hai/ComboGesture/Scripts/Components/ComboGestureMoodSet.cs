using System;
using System.Collections.Generic;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public abstract class ComboGestureMoodSet : MonoBehaviour
    {
        // MoodSetSpecifics
        public CgeCombosMoodSetSpecifics combos;
        public CgePermutationsMoodSetSpecifics permutations;
        public CgePuppetMoodSetSpecifics puppet;
        public CgeMassiveMoodSetSpecifics massive;
        public CgeOneHandSpecifics oneHand;
        public CgePriorityHandSpecifics priorityHand;

        // Shared
        public float transitionDuration = 0.1f;
        public List<AnimationClip> blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> limitedLipsync;
        public ComboGestureActivity.CgeOneHandMode oneHandMode;
        public bool enablePermutations;
    }

    [Serializable]
    public enum CgeMoodSetSpecialization
    {
        Combos,
        Permutations,
        OneHand,
        PriorityHand,
        Puppet,
        MassiveBlend
    }

    [Serializable]
    public struct CgeCombosMoodSetSpecifics
    {
        public Motion anim00;
        public Motion anim01;
        public Motion anim02;
        public Motion anim03;
        public Motion anim04;
        public Motion anim05;
        public Motion anim06;
        public Motion anim07;
        public Motion anim11;
        public Motion anim12;
        public Motion anim13;
        public Motion anim14;
        public Motion anim15;
        public Motion anim16;
        public Motion anim17;
        public Motion anim22;
        public Motion anim23;
        public Motion anim24;
        public Motion anim25;
        public Motion anim26;
        public Motion anim27;
        public Motion anim33;
        public Motion anim34;
        public Motion anim35;
        public Motion anim36;
        public Motion anim37;
        public Motion anim44;
        public Motion anim45;
        public Motion anim46;
        public Motion anim47;
        public Motion anim55;
        public Motion anim56;
        public Motion anim57;
        public Motion anim66;
        public Motion anim67;
        public Motion anim77;
        public AnimationClip anim11_L;
        public AnimationClip anim11_R;
    }

    [Serializable]
    public struct CgePermutationsMoodSetSpecifics
    {
        public Motion anim10;
        public Motion anim20;
        public Motion anim21;
        public Motion anim30;
        public Motion anim31;
        public Motion anim32;
        public Motion anim40;
        public Motion anim41;
        public Motion anim42;
        public Motion anim43;
        public Motion anim50;
        public Motion anim51;
        public Motion anim52;
        public Motion anim53;
        public Motion anim54;
        public Motion anim60;
        public Motion anim61;
        public Motion anim62;
        public Motion anim63;
        public Motion anim64;
        public Motion anim65;
        public Motion anim70;
        public Motion anim71;
        public Motion anim72;
        public Motion anim73;
        public Motion anim74;
        public Motion anim75;
        public Motion anim76;
    }

    [Serializable]
    public struct CgePuppetMoodSetSpecifics
    {
        public Motion mainTree;
    }

    [Serializable]
    public struct CgeOneHandSpecifics
    {
        public ComboGestureActivity.CgeHandSide side;
    }

    [Serializable]
    public struct CgePriorityHandSpecifics
    {
        public ComboGestureActivity.CgeHandSide side;
        public Motion recessive00;
        public Motion recessive01;
        public Motion recessive02;
        public Motion recessive03;
        public Motion recessive04;
        public Motion recessive05;
        public Motion recessive06;
        public Motion recessive07;
    }

    [Serializable]
    public struct CgeMassiveMoodSetSpecifics
    {
        public CgeMassiveBlendMode mode;
        public string simpleParameterName;
        public ComboGestureMoodSet simpleZero;
        public ComboGestureMoodSet simpleOne;
        public ComboGestureMoodSet simpleMinusOne;

        public Motion massiveBlendTree;
        public ComboGestureMoodSet[] massiveBlendTreeMoods;
    }

    [Serializable]
    public enum CgeMassiveBlendMode
    {
        Simple,
        TwoDirections,
        ComplexBlendTree
    }
}
