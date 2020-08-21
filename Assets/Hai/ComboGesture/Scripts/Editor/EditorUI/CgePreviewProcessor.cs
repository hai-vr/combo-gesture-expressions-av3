using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public struct AnimationPreview
    {
        public AnimationPreview(AnimationClip clip, Texture2D renderTexture)
        {
            Clip = clip;
            RenderTexture = renderTexture;
        }

        public AnimationClip Clip { get; }
        public Texture2D RenderTexture { get; }
    }

    public class CgePreviewProcessor
    {
        private readonly ComboGesturePreviewSetup _previewSetup;
        private readonly List<AnimationPreview> _animationPreviews;
        private readonly Action<AnimationPreview> _onClipRendered;
        private readonly Animator _dummy;
        private RenderTexture _renderTexture;
        private object _repaintFn;
        private Dictionary<AnimationClip, Texture2D> _animationClipToTextureDict;

        private static bool StopGenerating { get; set; }

        public CgePreviewProcessor(ComboGesturePreviewSetup previewSetup, List<AnimationPreview> animationPreviews, Action<AnimationPreview> onClipRendered)
        {
            _previewSetup = previewSetup;
            _animationPreviews = animationPreviews;
            _onClipRendered = onClipRendered;
            _dummy = previewSetup.previewDummy;
        }

        public void Capture()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }

            var generatedCamera = GenerateCamera();
            _dummy.gameObject.SetActive(true);

            DoGenerationProcess(generatedCamera);
        }

        private void DoGenerationProcess(Camera generatedCamera)
        {
            var queue = new List<Action<int>>();

            foreach (var animationPreview in _animationPreviews)
            {
                queue.Add(i =>
                {
                    AnimationMode.BeginSampling();
                    PoseDummyUsing(animationPreview.Clip);
                    AnimationMode.EndSampling();
                });
                queue.Add(i =>
                {
                    CaptureDummy(animationPreview.RenderTexture, generatedCamera);

                    _onClipRendered.Invoke(animationPreview);
                });
            }

            var cleanupQueue = new List<Action<int>> {i => Terminate(generatedCamera)};
            EditorApplication.delayCall += () => RunAsync(queue, cleanupQueue);
        }

        private static void RunAsync(List<Action<int>> actions, List<Action<int>> cleanupActions)
        {
            try
            {
                var action = actions[0];
                actions.RemoveAt(0);
                action.Invoke(999);
                if (actions.Count > 0 && !StopGenerating)
                {
                    EditorApplication.delayCall += () => RunAsync(actions, cleanupActions);
                }
                else
                {
                    StartCleanupActions(cleanupActions);
                }
            }
            catch (Exception)
            {
                StartCleanupActions(cleanupActions);
            }
        }

        private static void StartCleanupActions(List<Action<int>> cleanupActions)
        {
            if (cleanupActions.Count > 0)
            {
                EditorApplication.delayCall += () => Cleanup(cleanupActions);
            }
            else
            {
                StopGenerating = false;
            }
        }

        private static void Cleanup(List<Action<int>> cleanupActions)
        {
            try
            {
                var action = cleanupActions[0];
                cleanupActions.RemoveAt(0);
                action.Invoke(999);
            }
            finally
            {
                if (cleanupActions.Count > 0)
                {
                    EditorApplication.delayCall += () => Cleanup(cleanupActions);
                }
                else
                {
                    if (cleanupActions.Count > 0) {
                        EditorApplication.delayCall += () => Cleanup(cleanupActions);
                    }
                }
            }
        }

        private void PoseDummyUsing(AnimationClip clip)
        {
            AnimationMode.SampleAnimationClip(_dummy.gameObject, clip, 1/60f);
        }

        private void CaptureDummy(Texture2D element, Camera generatedCamera)
        {
            _renderTexture = new RenderTexture(element.width, element.height, 0);
            RenderCamera(_renderTexture, generatedCamera);
            RenderTextureTo(_renderTexture, element);
        }

        private static void RenderCamera(RenderTexture renderTexture, Camera camera)
        {
            var originalRenderTexture = camera.targetTexture;
            var originalAspect = camera.aspect;
            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float) renderTexture.width / renderTexture.height;
                camera.Render();
            }
            finally
            {
                camera.targetTexture = originalRenderTexture;
                camera.aspect = originalAspect;
            }
        }

        private static void RenderTextureTo(RenderTexture renderTexture, Texture2D texture2D)
        {
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        }

        private void Terminate(Camera generatedCamera)
        {
            SceneView.RepaintAll();
            AnimationMode.StopAnimationMode();
            Object.DestroyImmediate(generatedCamera.gameObject);

            if (_previewSetup.autoHide) {
                _dummy.gameObject.SetActive(false);
            }
        }

        private Camera GenerateCamera()
        {
            var headBone = GetHeadBoneOrDefault();
            var generatedCamera = Object.Instantiate(_previewSetup.camera, headBone, true);
            generatedCamera.gameObject.SetActive(true);

            return generatedCamera;
        }

        private Transform GetHeadBoneOrDefault()
        {
            var possibleBone = _dummy.GetBoneTransform(HumanBodyBones.Head);
            return possibleBone == null ? _dummy.transform : possibleBone;
        }

        public static void Stop_Temp()
        {
            if (StopGenerating)
            {
                AnimationMode.StopAnimationMode();
            }
            StopGenerating = true;
        }

        public static Texture2D NewPreviewTexture2D(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.ARGB32, false);
        }
    }
}
