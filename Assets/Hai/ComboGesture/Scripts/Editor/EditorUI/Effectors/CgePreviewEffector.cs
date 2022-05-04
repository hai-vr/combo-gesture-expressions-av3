using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class ComboGestureViewerGenerator
    {
        private GameObject _animatedRoot;
        private Camera _camera;
        private Material _shader;

        public ComboGestureViewerGenerator()
        {
            _shader = new Material(Shader.Find("Hai/HaiCgeGrayscale"));
        }

        public void Begin(GameObject animatedRoot)
        {
            _animatedRoot = animatedRoot;

            _camera = new GameObject().AddComponent<Camera>();

            var sceneCamera = SceneView.lastActiveSceneView.camera;
            _camera.transform.position = sceneCamera.transform.position;
            _camera.transform.rotation = sceneCamera.transform.rotation;
            var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
            _camera.fieldOfView = whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView;
            _camera.orthographic = sceneCamera.orthographic;
            _camera.nearClipPlane = sceneCamera.nearClipPlane;
            _camera.farClipPlane = sceneCamera.farClipPlane;
            _camera.orthographicSize = sceneCamera.orthographicSize;
        }

        public void ParentCameraTo(Transform newParent)
        {
            _camera.transform.parent = newParent;
        }

        public void Terminate()
        {
            Object.DestroyImmediate(_camera.gameObject);
        }

        public void Render(AnimationClip clip, EeRenderingCommands.EeRenderResult renderResult, float normalizedTime)
        {
            var initPos = _animatedRoot.transform.position;
            var initRot = _animatedRoot.transform.rotation;
            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_animatedRoot.gameObject, clip, normalizedTime * clip.length);
                AnimationMode.EndSampling();
                // This is a workaround for an issue where for some reason, the animator moves to the origin
                // after sampling despite the animation having no RootT/RootQ properties.
                _animatedRoot.transform.position = initPos;
                _animatedRoot.transform.rotation = initRot;

                var renderTexture = RenderTexture.GetTemporary(renderResult.Normal.width, renderResult.Normal.height, 24);
                renderTexture.wrapMode = TextureWrapMode.Clamp;

                RenderCamera(renderTexture, _camera);
                RenderTextureTo(renderTexture, renderResult.Normal);
                RenderTexture.ReleaseTemporary(renderTexture);

                var destination = RenderTexture.GetTemporary(renderResult.Grayscale.width, renderResult.Grayscale.height, 24);
                var tempMat = Object.Instantiate(_shader);
                Graphics.Blit(renderResult.Normal, destination, tempMat);
                Object.DestroyImmediate(tempMat);
                RenderTextureTo(destination, renderResult.Grayscale);
                RenderTexture.ReleaseTemporary(destination);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                _animatedRoot.transform.position = initPos;
                _animatedRoot.transform.rotation = initRot;
            }
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
    }

    public class EeRenderingCommands
    {
        private Queue<AnimationClip> _queue = new Queue<AnimationClip>();
        private HumanBodyBones _bone = HumanBodyBones.Head;
        private float _normalizedTime;
        private AnimationClip _basePose;

        private List<AnimationClip> _invalidation = new List<AnimationClip>();
        private Dictionary<AnimationClip, EeRenderResult> _clipToRender = new Dictionary<AnimationClip, EeRenderResult>();
        private bool _scheduled;
        private Animator _animator;
        private Action _repaint;

        private bool TryRender(GameObject root)
        {
            if (_queue.Count == 0) return false;

            var originalAvatarGo = root;
            GameObject copy = null;
            var wasActive = originalAvatarGo.activeSelf;
            try
            {
                copy = Object.Instantiate(originalAvatarGo);
                copy.SetActive(true);
                originalAvatarGo.SetActive(false);
                Render(copy);
            }
            finally
            {
                if (wasActive) originalAvatarGo.SetActive(true);
                if (copy != null) Object.DestroyImmediate(copy);
            }

            return true;
        }

        private void Render(GameObject copy)
        {
            var viewer = new ComboGestureViewerGenerator();
            try
            {
                viewer.Begin(copy);
                var animator = copy.GetComponent<Animator>();
                if (animator.isHuman && _bone != HumanBodyBones.LastBone)
                {
                    var head = animator.GetBoneTransform(_bone);
                    viewer.ParentCameraTo(head);
                }
                else
                {
                    viewer.ParentCameraTo(animator.transform);
                }

                while (_queue.Count > 0)
                {
                    var clip = _queue.Dequeue();
                    if (_basePose != null)
                    {
                        var modifiedClip = Object.Instantiate(clip);
                        var missingBindings = AnimationUtility.GetCurveBindings(_basePose)
                            .Where(binding => AnimationUtility.GetEditorCurve(clip, binding) == null)
                            .ToArray();
                        foreach (var missingBinding in missingBindings)
                        {
                            AnimationUtility.SetEditorCurve(modifiedClip, missingBinding, AnimationUtility.GetEditorCurve(_basePose, missingBinding));
                        }
                        viewer.Render(modifiedClip, _clipToRender[clip], _normalizedTime);
                    }
                    else
                    {
                        viewer.Render(clip, _clipToRender[clip], _normalizedTime);
                    }

                    // This is a workaround for an issue where the muscles will not update
                    // across multiple samplings of the animator on the same frame.
                    // This issue is mainly visible when the update speed (number of animation
                    // clips updated per frame) is greater than 1.
                    // By disabling and enabling the animator copy, this allows us to resample it.
                    copy.SetActive(false);
                    copy.SetActive(true);
                }
            }
            finally
            {
                viewer.Terminate();
            }
        }

        public EeRenderResult RequireRender(AnimationClip clip, Action repaintCallback, bool isBig = false)
        {
            if (_clipToRender.ContainsKey(clip)
                && _clipToRender[clip].Normal != null) // Can happen when the texture is destroyed (Unity invalid object)
            {
                if (!_queue.Contains(clip) && _invalidation.Contains(clip))
                {
                    _invalidation.RemoveAll(inList => inList == clip);
                    _queue.Enqueue(clip);
                    ScheduleRendering(repaintCallback);
                }
                return _clipToRender[clip];
            }

            var width = isBig ? CgeActivityEditorCombiner.CombinerPreviewCenterWidth : CgeActivityEditorCombiner.CombinerPreviewWidth;
            var height = isBig ? CgeActivityEditorCombiner.CombinerPreviewCenterHeight : CgeActivityEditorCombiner.CombinerPreviewHeight;
            var render = new EeRenderResult
            {
                Normal = new Texture2D(width, height, TextureFormat.RGB24, true),
                Grayscale = new Texture2D(width, height, TextureFormat.RGB24, true)
            };
            _clipToRender[clip] = render; // TODO: Dimensions

            _queue.Enqueue(clip);
            ScheduleRendering(repaintCallback);

            return render;
        }

        public void SelectAnimator(Animator animator)
        {
            if (_animator == null && animator != null)
            {
                Invalidate(() => {});
            }
            _animator = animator;
        }

        private void ScheduleRendering(Action repaintCallback)
        {
            if (!_scheduled)
            {
                EditorApplication.delayCall += DoRender;
                _scheduled = true;
                _repaint = repaintCallback;
            }
        }

        private void DoRender()
        {
            _scheduled = false;
            if (_animator != null)
            {
                TryRender(_animator.gameObject);
            }
            _repaint.Invoke();
        }

        public struct EeRenderResult
        {
            public Texture2D Normal;
            public Texture2D Grayscale;
        }

        public void Invalidate(Action repaintCallback)
        {
            _invalidation.AddRange(_clipToRender.Keys);
            ScheduleRendering(repaintCallback);
        }

        public static Texture2D NewPreviewTexture2D(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.ARGB32, false);
        }

        public void InvalidateSome(Action repaintCallback, params AnimationClip[] clips)
        {
            _invalidation.AddRange(clips);
            ScheduleRendering(repaintCallback);
        }
    }
}