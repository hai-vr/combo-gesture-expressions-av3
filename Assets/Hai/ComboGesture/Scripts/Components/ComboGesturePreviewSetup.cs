using System.Collections.Generic;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGesturePreviewSetup : MonoBehaviour
    {
        public Camera camera;
        public Animator previewDummy;
        public bool autoHide;

        public bool IsValid()
        {
            return camera != null && previewDummy != null;
        }
    }
}
