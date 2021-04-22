using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal
{
    public class EePreviewSetupWizard
    {
        public struct EeSetup
        {
            public readonly SetupResult Result;
            public readonly ExpressionEditorPreviewable PreviewAvatar;

            public EeSetup(SetupResult result, ExpressionEditorPreviewable previewAvatar)
            {
                Result = result;
                PreviewAvatar = previewAvatar;
            }
        }

        public enum SetupResult
        {
            ReusedExistsAndValidInScene, NoAvatarFound, CreatedNew
        }

        public EeSetup AutoSetup()
        {
            var maybeExistingPreviewSetup = MaybeFindLastActiveAndValidPreviewComponentInRoot();

            if (maybeExistingPreviewSetup != null)
            {
                return new EeSetup(SetupResult.ReusedExistsAndValidInScene, maybeExistingPreviewSetup);
            }

            var maybeExistingAvatar = MaybeFindLastActiveAvatarInHierarchy();
            if (maybeExistingAvatar == null)
            {
                return new EeSetup(SetupResult.NoAvatarFound, null);
            }

            var newPreviewSetupGo = new GameObject("EEPreviewSetup");

            var avatarCopyGo = CreateACopyOfTheAvatar(maybeExistingAvatar, newPreviewSetupGo);

            var camera = CreateCameraSystem(maybeExistingAvatar, newPreviewSetupGo, avatarCopyGo, "(Main Camera)", 0);
            var camera2 = CreateCameraSystem(maybeExistingAvatar, newPreviewSetupGo, avatarCopyGo, "Secondary Camera", 1);
            var camera3 = CreateCameraSystem(maybeExistingAvatar, newPreviewSetupGo, avatarCopyGo, "Third Camera", 2);

            newPreviewSetupGo.transform.position += Vector3.down * 4;

            var previewSetup = newPreviewSetupGo.AddComponent<ExpressionEditorPreviewAvatar>();
            previewSetup.mainCamera = new EePreviewAvatarCamera("(Main Camera)", camera.gameObject, true, HumanBodyBones.Head, null, null);
            previewSetup.secondaryCameras = new List<EePreviewAvatarCamera>
            {
                new EePreviewAvatarCamera("Secondary Camera", camera2.gameObject, true, HumanBodyBones.Head, null, null),
                new EePreviewAvatarCamera("Third Camera", camera3.gameObject, true, HumanBodyBones.Head, null, null)
            };
            previewSetup.dummy = avatarCopyGo.GetComponent<Animator>();
            previewSetup.autoHide = false;
            previewSetup.tempCxSmr = FindFirstSmr(avatarCopyGo);
            previewSetup.optionalOriginalAvatarGeneratedFrom = maybeExistingAvatar.gameObject;

            return new EeSetup(SetupResult.CreatedNew, previewSetup);
        }

        private static Camera CreateCameraSystem(Animator maybeExistingAvatar, GameObject newPreviewSetupGo, GameObject avatarCopyGo, string cameraSystemName, int variation)
        {
            var camera = CreateCamera(maybeExistingAvatar, newPreviewSetupGo, variation);
            var headBoneTransform = avatarCopyGo.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head) ?? avatarCopyGo.transform;
            CreateCameraPivot(newPreviewSetupGo, headBoneTransform, camera, cameraSystemName);
            return camera;
        }

        private static void CreateCameraPivot(GameObject newPreviewSetupGo, Transform headBoneTransform, Camera camera, string cameraSystemName)
        {
            var cameraParent = new GameObject();
            cameraParent.name = cameraSystemName;
            cameraParent.transform.SetParent(newPreviewSetupGo.transform, true);
            cameraParent.transform.position = headBoneTransform.position;
            cameraParent.transform.rotation = headBoneTransform.rotation;
            camera.transform.SetParent(cameraParent.transform, true);
        }

        private static SkinnedMeshRenderer FindFirstSmr(GameObject avatarCopyGo)
        {
#if VRC_SDK_VRCSDK3
            var ad = avatarCopyGo.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if (ad != null && ad.VisemeSkinnedMesh != null)
            {
                return ad.VisemeSkinnedMesh;
            }
#endif

            var possibleSmrs = avatarCopyGo.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (possibleSmrs.Length == 0)
            {
                return null;
            }

            var smrWithFace = possibleSmrs
                .Where(renderer => renderer.sharedMesh != null)
                .FirstOrDefault(renderer =>
                {
                    var mesh = renderer.sharedMesh;
                    for (var i = 0; i < mesh.blendShapeCount; i++)
                    {
                        if (mesh.GetBlendShapeName(i).ToLowerInvariant().StartsWith("vrc.v_")) return true;
                        if (mesh.GetBlendShapeName(i).ToLowerInvariant() == "blink") return true;
                    }

                    return false;
                });
            if (smrWithFace)
            {
                return smrWithFace;
            }

            return possibleSmrs
                .FirstOrDefault(renderer =>
                {
                    var mesh = renderer.sharedMesh;
                    return mesh != null && mesh.blendShapeCount > 0;
                });
        }

        public static ExpressionEditorPreviewable MaybeFindLastActiveAndValidPreviewComponentInRoot()
        {
            return FindActiveAndValidPreviewComponentsInRoot().LastOrDefault();
        }

        public static List<ExpressionEditorPreviewable> FindActiveAndValidPreviewComponentsInRoot()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .Select(obj => obj.GetComponent<ExpressionEditorPreviewable>())
                .Where(setup => setup != null && setup.IsValid() && setup.isActiveAndEnabled)
                .ToList();
        }

        private static Animator MaybeFindLastActiveAvatarInHierarchy()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .Where(obj => obj.GetComponent<Animator>() != null)
#if VRC_SDK_VRCSDK3
                .Where(obj => obj.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
#endif
                .Select(obj => obj.GetComponent<Animator>())
                .LastOrDefault(avatar => avatar != null && avatar.gameObject.activeInHierarchy);
        }

        private static Camera CreateCamera(Animator maybeExistingAvatar, GameObject newPreviewSetupGo, int variation)
        {
            var newCameraGo = new GameObject("EEPreviewCamera");
            MoveCameraToFaceTheAvatar(maybeExistingAvatar, newCameraGo, variation);
            var camera = newCameraGo.AddComponent<Camera>();
            camera.fieldOfView = 10;
            camera.nearClipPlane = 0.01f;
            newCameraGo.transform.SetParent(newPreviewSetupGo.transform, true);
            return camera;
        }

        private static void MoveCameraToFaceTheAvatar(Animator maybeExistingAvatar, GameObject newCameraGo, int variation)
        {
            var faceSize = GuessFaceSize(maybeExistingAvatar) * 1.2f;
            var headTransform = FindHeadTransform(maybeExistingAvatar);
            var focusPoint = headTransform.position + headTransform.up * faceSize * 0.03f;
            newCameraGo.transform.position = focusPoint
                                             + headTransform.up * faceSize * 0.02f
                                             + headTransform.transform.forward * faceSize * (variation != 1 ? 0.65f : 0.32f)
                                             + headTransform.transform.right * faceSize * (variation != 2 ? 0.1f : 0.5f);
            newCameraGo.transform.rotation = Quaternion.LookRotation(
                (focusPoint - newCameraGo.transform.position).normalized
            );
        }

        private static float GuessFaceSize(Animator avatar)
        {
#if VRC_SDK_VRCSDK3
            var descriptor = avatar.gameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                return descriptor.ViewPosition.y;
            }
#endif
            var headTransform = avatar.GetBoneTransform(HumanBodyBones.Head);
            var feetTransform = avatar.GetBoneTransform(HumanBodyBones.RightFoot);
            return headTransform != null && feetTransform != null
                ? Mathf.Abs(headTransform.position.y - feetTransform.position.y)
                : 1f;
        }

        private static Transform FindHeadTransform(Animator avatar)
        {
            var headTransform = avatar.GetBoneTransform(HumanBodyBones.Head);
            return headTransform != null
                ? headTransform
                : avatar.transform;
        }

        private static GameObject CreateACopyOfTheAvatar(Animator avatar, GameObject newPreviewSetupGo)
        {
            var avatarCopyGo = Object.Instantiate(avatar.gameObject, newPreviewSetupGo.transform, true);
            avatarCopyGo.name = "EEPreviewDummy";
            avatarCopyGo.SetActive(true);
            if (!avatarCopyGo.GetComponent<Animator>())
            {
                avatarCopyGo.AddComponent<Animator>();
            }

#if VRC_SDK_VRCSDK3
            // We don't want that avatar to be suggested in the VRC avatar upload list
            Object.DestroyImmediate(avatarCopyGo.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>());
            Object.DestroyImmediate(avatarCopyGo.GetComponent(typeof(VRC.Core.PipelineManager)));
#endif

            avatarCopyGo.GetComponent<Animator>().runtimeAnimatorController = null;

            MutateWorkaroundSkinnedMeshesEmptyArrayAnimationModeSamplingBug(avatarCopyGo);

            return avatarCopyGo;
        }

        private static void MutateWorkaroundSkinnedMeshesEmptyArrayAnimationModeSamplingBug(GameObject avatarCopyGo)
        {
            // https://github.com/hai-vr/combo-gesture-expressions-av3/issues/253
            // SMRs that have blendshapes which have not been modified will cause issues when sampling animations for the first time,
            // causing the dummy to be permanently mangled by having unexpected blend shape values on subsequent executions.
            // The workaround is to force-set the blendshape values
            var allSmrsHavingBlendshapes = avatarCopyGo.transform.GetComponentsInChildren<SkinnedMeshRenderer>()
                .Where(renderer => renderer.sharedMesh != null)
                .Where(renderer => renderer.sharedMesh.blendShapeCount > 0)
                .ToList();
            foreach (var smr in allSmrsHavingBlendshapes)
            {
                var blendShapeCount = smr.sharedMesh.blendShapeCount;
                for (var index = 0; index < blendShapeCount; index++)
                {
                    smr.SetBlendShapeWeight(index, smr.GetBlendShapeWeight(index));
                }
            }
        }
    }
}
