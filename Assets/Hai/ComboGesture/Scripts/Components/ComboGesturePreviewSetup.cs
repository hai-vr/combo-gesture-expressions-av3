using System.Collections.Generic;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePreviewSetup : ExpressionEditorPreviewable
    {
        public Camera camera;
        public Animator previewDummy;
        public bool autoHide;
        public VRCAvatarDescriptor avatarDescriptor;

        public override bool IsValid()
        {
            return false;
        }

        public override EePreviewAvatar AsEePreviewAvatar()
        {
            return new EePreviewAvatar(
                new List<EePreviewAvatarCamera> { new EePreviewAvatarCamera("(Main Camera)", camera.gameObject, true, HumanBodyBones.Head, null, null) },
                previewDummy,
                autoHide,
                gameObject.name,
                avatarDescriptor.VisemeSkinnedMesh,
                new List<EePreviewAvatarPose>()
            );
        }

        public override GameObject AsGameObject()
        {
            return gameObject;
        }
    }
}
