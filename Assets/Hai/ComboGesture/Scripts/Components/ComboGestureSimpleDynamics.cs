using System;
using UnityEngine;
using VRC.Dynamics;
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
        public bool isHardThreshold;

        public CgeDynamicsDescriptor ToDescriptor()
        {
            return new CgeDynamicsDescriptor
            {
                parameter = DynamicsResolveParameter(),
                condition = condition,
                threshold = threshold,
                isHardThreshold = isHardThreshold,
                parameterType = DynamicsResolveParameterType()
            };
        }

        private string DynamicsResolveParameter()
        {
            return contactReceiver != null
                ? contactReceiver.parameter
                : physBone != null
                    ? $"{physBone.parameter}_{ToSuffix()}"
                    : parameterName != null
                        ? parameterName
                        : throw new ArgumentException();
        }

        private ComboGestureSimpleDynamicsParameterType DynamicsResolveParameterType()
        {
            return contactReceiver != null
                ? (contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity
                    ? ComboGestureSimpleDynamicsParameterType.Float
                    : parameterType)
                : physBone != null
                    ? (physBoneSource != ComboGestureSimpleDynamicsPhysBoneSource.IsGrabbed
                        ? ComboGestureSimpleDynamicsParameterType.Float
                        : parameterType)
                    : parameterType;
        }

        private string ToSuffix()
        {
            return Enum.GetName(typeof(ComboGestureSimpleDynamicsPhysBoneSource), physBoneSource);
        }
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

    public struct CgeDynamicsRankedDescriptor
    {
        public int rank;
        public CgeDynamicsDescriptor descriptor;
    }

    public struct CgeDynamicsDescriptor
    {
        public string parameter;
        public float threshold;
        public ComboGestureSimpleDynamicsParameterType parameterType;
        public ComboGestureSimpleDynamicsCondition condition;
        public bool isHardThreshold;
    }
}