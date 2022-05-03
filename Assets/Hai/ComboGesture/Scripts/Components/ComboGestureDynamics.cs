using System;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureDynamics : MonoBehaviour
    {
        public Animator previewAnimator;
        public ComboGestureDynamicsItem[] items;
    }

    [Serializable]
    public struct ComboGestureDynamicsItem
    {
        public AnimationClip clip;
        public bool bothEyesClosed;
        public ComboGestureMoodSet moodSet;
        public ComboGestureDynamicsPhysBoneSource physBoneSource;
        public VRCContactReceiver contactReceiver;
        public VRCPhysBone physBone;
        public string parameterName;
        public ComboGestureDynamicsParameterType parameterType;
        public ComboGestureDynamicsCondition condition;
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

        private ComboGestureDynamicsParameterType DynamicsResolveParameterType()
        {
            return contactReceiver != null
                ? (contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity
                    ? ComboGestureDynamicsParameterType.Float
                    : parameterType)
                : physBone != null
                    ? (physBoneSource != ComboGestureDynamicsPhysBoneSource.IsGrabbed
                        ? ComboGestureDynamicsParameterType.Float
                        : parameterType)
                    : parameterType;
        }

        private string ToSuffix()
        {
            return Enum.GetName(typeof(ComboGestureDynamicsPhysBoneSource), physBoneSource);
        }
    }

    [Serializable]
    public enum ComboGestureDynamicsParameterType
    {
        Bool, Int, Float
    }

    [Serializable]
    public enum ComboGestureDynamicsPhysBoneSource
    {
        Stretch, Angle, IsGrabbed
    }

    [Serializable]
    public enum ComboGestureDynamicsCondition
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
        public ComboGestureDynamicsParameterType parameterType;
        public ComboGestureDynamicsCondition condition;
        public bool isHardThreshold;
    }
}