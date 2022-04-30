using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeMemoization
    {
        public Dictionary<AnimationClip, Texture2D> AnimationClipToTextureDict { get; } = new Dictionary<AnimationClip, Texture2D>();
        public Dictionary<AnimationClip, Texture2D> AnimationClipToTextureDictGray { get; } = new Dictionary<AnimationClip, Texture2D>();

        public void AssignRegular(AnimationClip clip, Texture2D texture)
        {
            AnimationClipToTextureDict[clip] = texture;
        }

        public void AssignGrayscale(AnimationClip clip, Texture2D texture)
        {
            AnimationClipToTextureDictGray[clip] = texture;
        }

        public bool Has(AnimationClip clip)
        {
            return AnimationClipToTextureDict.ContainsKey(clip);
        }
    }

    public class CgeMemoryQuery
    {
        private readonly CgeMemoization _cgePreviewState;

        public CgeMemoryQuery(CgeMemoization cgePreviewState)
        {
            _cgePreviewState = cgePreviewState;
        }

        public bool HasClip(AnimationClip element)
        {
            return _cgePreviewState.AnimationClipToTextureDict.ContainsKey(element);
        }

        public Texture GetPicture(AnimationClip element)
        {
            return _cgePreviewState.AnimationClipToTextureDict[element];
        }

        public Texture GetGrayscale(AnimationClip element)
        {
            return _cgePreviewState.AnimationClipToTextureDictGray[element];
        }

        public static Texture2D NewPreviewTexture2D(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.ARGB32, false);
        }
    }

    public class CgeActivityPreviewQueryAggregator
    {
        private readonly CgeMemoization _cgeMemoization;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeBlendTreeEffector _blendTreeEffector;
        private readonly EeRenderingCommands _renderingCommands;
        private bool _isProcessing;

        public CgeActivityPreviewQueryAggregator(CgeMemoization cgeMemoization, CgeEditorEffector editorEffector, CgeBlendTreeEffector blendTreeEffector, EeRenderingCommands renderingCommands)
        {
            _cgeMemoization = cgeMemoization;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
            _renderingCommands = renderingCommands;
        }

        public void GenerateMissingPreviews(Action repaintCallback)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null, _editorEffector.PreviewSetup());
        }

        public void GenerateMissingPreviewsPrioritizing(Action repaintCallback, AnimationClip element)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, element, _editorEffector.PreviewSetup());
        }

        public void GenerateAll(Action repaintCallback)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything, null, _editorEffector.PreviewSetup());
        }

        private CgeActivityPreviewInternal Previewer(Action repaintCallback)
        {
            return new CgeActivityPreviewInternal(
                repaintCallback,
                _editorEffector,
                _blendTreeEffector,
                _cgeMemoization,
                CgeLayoutCommon.PictureWidth,
                CgeLayoutCommon.PictureHeight,
                _renderingCommands
            );
        }
    }

    public class ComboGestureViewerGenerator
    {
        private GameObject _animatedRoot;
        private Camera _camera;

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

        public void Render(AnimationClip clip, Texture2D element, float normalizedTime)
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

                var renderTexture = RenderTexture.GetTemporary(element.width, element.height, 24);
                renderTexture.wrapMode = TextureWrapMode.Clamp;

                RenderCamera(renderTexture, _camera);
                RenderTextureTo(renderTexture, element);
                RenderTexture.ReleaseTemporary(renderTexture);
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
        private Queue<EeRenderingSample> _queue;
        private HumanBodyBones _bone = HumanBodyBones.Head;
        private float _normalizedTime;
        private AnimationClip _basePose;

        public EeRenderingCommands()
        {
            _queue = new Queue<EeRenderingSample>();
        }

        public void GenerateSpecific(
            List<EeRenderingSample> animationsPreviews,
            Animator previewSetup)
        {
            foreach (var sample in animationsPreviews)
            {
                _queue.Enqueue(sample);
            }
            TryRender(previewSetup.gameObject);
        }

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
                    var sample = _queue.Dequeue();
                    var clip = sample.Clip;
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
                        viewer.Render(modifiedClip, sample.RenderTexture, _normalizedTime);
                    }
                    else
                    {
                        viewer.Render(clip, sample.RenderTexture, _normalizedTime);
                    }
                    sample.Callback(sample);

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
    }

    public readonly struct EeRenderingSample
    {
        public readonly AnimationClip Clip;
        public readonly Texture2D RenderTexture;
        public readonly Action<EeRenderingSample> Callback;

        public EeRenderingSample(AnimationClip clip, Texture2D renderTexture, Action<EeRenderingSample> callback)
        {
            Clip = clip;
            RenderTexture = renderTexture;
            Callback = callback;
        }
    }
}