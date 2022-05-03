using System;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureSimpleDynamics : MonoBehaviour
    {
        public Animator previewAnimator;
        public ComboGestureSimpleDynamicsItem[] items;
    }

    [Serializable]
    public struct ComboGestureSimpleDynamicsItem
    {
        public AnimationClip clip;
        public bool bothEyesClosed;
        public ComboGestureMoodSet moodSet;
        public ComboGestureSimpleDynamicsPhysBoneSource physBoneSource;
        public VRCContactReceiver contactReceiver;
        public VRCPhysBone physBone;
        public string parameterName;
        public ComboGestureSimpleDynamicsParameterType parameterType;
        public ComboGestureSimpleDynamicsCondition condition;
        public float threshold;
    }

    [Serializable]
    public enum ComboGestureSimpleDynamicsParameterType
    {
        Bool, Int, Float
    }

    [Serializable]
    public enum ComboGestureSimpleDynamicsPhysBoneSource
    {
        Stretch, Angle, IsGrabbed
    }

    [Serializable]
    public enum ComboGestureSimpleDynamicsCondition
    {
        IsAboveThreshold, IsBelowOrEqualThreshold
    }
}