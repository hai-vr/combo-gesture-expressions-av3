using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Components
{
    public class ExpressionEditorPreviewAvatar : ExpressionEditorPreviewable
    {
        public EePreviewAvatarCamera mainCamera;
        public List<EePreviewAvatarCamera> secondaryCameras;
        public List<EePreviewAvatarPose> poses;
        public Animator dummy;
        public bool autoHide;
        public SkinnedMeshRenderer tempCxSmr;
        public GameObject optionalOriginalAvatarGeneratedFrom;

        public override bool IsValid()
        {
            return mainCamera.camera != null && dummy != null;
        }

        public override EePreviewAvatar AsEePreviewAvatar()
        {
            return new EePreviewAvatar(
                new List<EePreviewAvatarCamera> { mainCamera }.Concat(secondaryCameras??new List<EePreviewAvatarCamera>()).ToList(),
                dummy,
                autoHide,
                gameObject.name,
                tempCxSmr,
                poses??new List<EePreviewAvatarPose>()
            );
        }

        public override GameObject AsGameObject()
        {
            return gameObject;
        }
    }

    [Serializable]
    public readonly struct EePreviewAvatar
    {
        public readonly string Name;
        public readonly List<EePreviewAvatarCamera> Cameras;
        public readonly List<EePreviewAvatarPose> Poses;
        public readonly Animator Dummy;
        public readonly bool AutoHide;
        public readonly SkinnedMeshRenderer TempCxSmr;

        public EePreviewAvatar(List<EePreviewAvatarCamera> cameras, Animator dummy, bool autoHide, string name, SkinnedMeshRenderer tempCxSmr, List<EePreviewAvatarPose> poses)
        {
            Cameras = cameras;
            Dummy = dummy;
            AutoHide = autoHide;
            Name = name;
            TempCxSmr = tempCxSmr;
            Poses = poses;
        }

        public Camera GetEffectiveCamera(int index)
        {
            return index >= Cameras.Count ? Cameras[0].camera.GetComponentInChildren<Camera>() : Cameras[index].camera.GetComponentInChildren<Camera>();
        }

        public EePreviewAvatarCamera GetEffectiveCameraConfig(int index)
        {
            return index > Cameras.Count ? Cameras[0] : Cameras[index];
        }
    }

    [Serializable]
    public struct EePreviewAvatarCamera
    {
        public string name;
        public GameObject camera;
        public bool autoAttachToBone;
        public HumanBodyBones optionalAttachToBone;
        // public Transform optionalPivot;
        public string optionalPoseName;

        public EePreviewAvatarCamera(string name, GameObject camera, bool autoAttachToBone, HumanBodyBones optionalAttachToBone, Transform optionalPivot, string optionalPoseName)
        {
            this.name = name;
            this.camera = camera;
            this.autoAttachToBone = autoAttachToBone;
            this.optionalAttachToBone = optionalAttachToBone;
            // this.optionalPivot = optionalPivot;
            this.optionalPoseName = optionalPoseName;
        }
    }

    [Serializable]
    public struct EePreviewAvatarPose
    {
        public string name;
        public Animation pose;
    }

    [Serializable]
    public enum EePreviewAvatarCameraAttachmentType
    {
        HumanoidBone, Transform
    }
}
