using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePreviewSetup : MonoBehaviour
    {
        public Camera camera;
        public Animator previewDummy;
        public bool autoHide;
        public VRCAvatarDescriptor avatarDescriptor;

        public bool IsValid()
        {
            return camera != null && previewDummy != null && avatarDescriptor != null;
        }
    }
}
