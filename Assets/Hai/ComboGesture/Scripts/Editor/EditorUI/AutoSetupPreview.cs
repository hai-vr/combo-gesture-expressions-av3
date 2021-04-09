using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public class AutoSetupPreview
    {
        private readonly CgeEditorEffector _effector;

        public AutoSetupPreview(CgeEditorEffector effector)
        {
            _effector = effector;
        }

        public enum SetupResult
        {
            ReusedExistsAndValidInScene, NoAvatarFound, CreatedNew
        }

        public SetupResult AutoSetup()
        {
            var maybeExistingPreviewSetup = MaybeFindLastActiveAndValidPreviewComponentInRoot();

            if (maybeExistingPreviewSetup != null)
            {
                _effector.SetPreviewSetup(maybeExistingPreviewSetup);
                return SetupResult.ReusedExistsAndValidInScene;
            }

            var maybeExistingAvatar = MaybeFindLastActiveAvatarInHierarchy();
            if (maybeExistingAvatar == null)
            {
                return SetupResult.NoAvatarFound;
            }

            var newPreviewSetupGo = new GameObject("CGEPreviewSetup");

            var camera = CreateCamera(maybeExistingAvatar, newPreviewSetupGo);
            var avatarCopyGo = CreateACopyOfTheAvatar(maybeExistingAvatar, newPreviewSetupGo);

            newPreviewSetupGo.transform.position += Vector3.down * 4;

            var comboGesturePreviewSetup = newPreviewSetupGo.AddComponent<ComboGesturePreviewSetup>();
            comboGesturePreviewSetup.camera = camera;
            comboGesturePreviewSetup.previewDummy = avatarCopyGo.GetComponent<Animator>();
            comboGesturePreviewSetup.avatarDescriptor = avatarCopyGo.GetComponent<VRCAvatarDescriptor>();
            comboGesturePreviewSetup.autoHide = true;

            _effector.SetPreviewSetup(comboGesturePreviewSetup);

            return SetupResult.CreatedNew;
        }

        public static ComboGesturePreviewSetup MaybeFindLastActiveAndValidPreviewComponentInRoot()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .Select(obj => obj.GetComponent<ComboGesturePreviewSetup>())
                .LastOrDefault(setup => setup != null && setup.IsValid() && setup.gameObject.activeInHierarchy);
        }

        private static VRCAvatarDescriptor MaybeFindLastActiveAvatarInHierarchy()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .Select(obj => obj.GetComponent<VRCAvatarDescriptor>())
                .LastOrDefault(avatar => avatar != null && avatar.gameObject.activeInHierarchy);
        }

        private static Camera CreateCamera(VRCAvatarDescriptor maybeExistingAvatar, GameObject newPreviewSetupGo)
        {
            var newCameraGo = new GameObject("CGEPreviewCamera");
            MoveCameraToFaceTheAvatar(maybeExistingAvatar, newCameraGo);
            var camera = newCameraGo.AddComponent<Camera>();
            camera.fieldOfView = 10;
            camera.nearClipPlane = 0.01f;
            newCameraGo.transform.SetParent(newPreviewSetupGo.transform, true);
            newCameraGo.SetActive(false);
            return camera;
        }

        private static void MoveCameraToFaceTheAvatar(VRCAvatarDescriptor maybeExistingAvatar, GameObject newCameraGo)
        {
            var faceSize = GuessFaceSize(maybeExistingAvatar);
            var focusOrigin = FindFocusOrigin(maybeExistingAvatar);
            newCameraGo.transform.position = focusOrigin
                                             + maybeExistingAvatar.transform.forward * faceSize * 0.65f
                                             + maybeExistingAvatar.transform.right * faceSize * 0.1f;
            newCameraGo.transform.rotation = Quaternion.LookRotation(
                (focusOrigin - newCameraGo.transform.position).normalized
            );
            newCameraGo.transform.position -= maybeExistingAvatar.transform.up * faceSize * 0.006f;
        }

        private static float GuessFaceSize(VRCAvatarDescriptor avatar)
        {
            return avatar.ViewPosition.y;
        }

        private static Vector3 FindFocusOrigin(VRCAvatarDescriptor avatar)
        {
            return avatar.ViewPosition;
        }

        private static GameObject CreateACopyOfTheAvatar(VRCAvatarDescriptor maybeExistingAvatar, GameObject newPreviewSetupGo)
        {
            var avatarCopyGo = Object.Instantiate(maybeExistingAvatar.gameObject, newPreviewSetupGo.transform, true);
            avatarCopyGo.name = "CGEPreviewDummy";
            Object.DestroyImmediate(avatarCopyGo.GetComponent(typeof(PipelineManager)));
            avatarCopyGo.SetActive(true);
            if (!avatarCopyGo.GetComponent<Animator>())
            {
                avatarCopyGo.AddComponent<Animator>();
            }

            avatarCopyGo.GetComponent<Animator>().runtimeAnimatorController = null;
            return avatarCopyGo;
        }
    }
}
