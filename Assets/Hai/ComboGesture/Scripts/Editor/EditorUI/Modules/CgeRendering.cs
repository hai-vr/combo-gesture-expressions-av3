using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Modules
{
    public class CgeRenderingCommands
    {
        private readonly List<Action> _queue = new List<Action>();
        private bool _isProcessing;

        public void GenerateSpecific(List<AnimationPreview> animationsPreviews, Action<AnimationPreview> onClipRendered, ComboGesturePreviewSetup previewSetup)
        {
            _queue.Add(() => new CgeRenderingJob(previewSetup, animationsPreviews, onClipRendered, true, OnQueueTaskComplete).Capture());
            WakeQueue();
        }

        public void GenerateSpecificFastMode(List<AnimationPreview> animationsPreviews, Action<AnimationPreview> onClipRendered, ComboGesturePreviewSetup previewSetup)
        {
            _queue.Add(() => new CgeRenderingJob(previewSetup, animationsPreviews, onClipRendered, false, OnQueueTaskComplete).Capture());
            WakeQueue();
        }

        private void WakeQueue()
        {
            if (_isProcessing || _queue.Count == 0)
            {
                return;
            }

            _isProcessing = true;
            ExecuteNextInQueue();
        }

        private void ExecuteNextInQueue()
        {
            var first = _queue[0];
            _queue.RemoveAt(0);
            first.Invoke();
        }

        private void OnQueueTaskComplete()
        {
            if (_queue.Count == 0)
            {
                _isProcessing = false;
            }
            else
            {
                ExecuteNextInQueue();
            }
        }
    }

    public static class CgeRenderingSupport
    {
        private const int Border = 4;

        public static void MutateHighlightDifferences(Texture2D mutableTexture, Texture2D noShapekeys)
        {
            var tex = mutableTexture;
            var boundaries = DifferenceAsBoundaries(tex, noShapekeys);

            var width = mutableTexture.width;
            var height = mutableTexture.height;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = tex.GetPixel(x, y);

                    var gsv = (value.r + value.g + value.b) / 3f;
                    var actualGsv = value * 0.3f + new Color(gsv, gsv, gsv, value.a) * 0.7f;

                    if (boundaries.MinX == -1)
                    {
                        tex.SetPixel(x, y, actualGsv * 0.2f);
                    }
                    else
                    {
                        var isIn = x >= boundaries.MinX - Border && x <= boundaries.MaxX + Border && y >= boundaries.MinY - Border && y <= boundaries.MaxY + Border;
                        tex.SetPixel(x, y, isIn ? value : actualGsv * 0.5f);
                    }
                }
            }

            tex.Apply(false);
        }

        private static PreviewBoundaries DifferenceAsBoundaries(Texture2D a, Texture2D b)
        {
            var minX = -1;
            var maxX = -1;
            var minY = -1;
            var maxY = -1;

            for (var y = 0; y < a.height; y++)
            {
                for (var x = 0; x < a.width; x++)
                {
                    if (a.GetPixel(x, y) != b.GetPixel(x, y))
                    {
                        if (minX == -1 || x < minX)
                        {
                            minX = x;
                        }

                        if (minY == -1 || y < minY)
                        {
                            minY = y;
                        }

                        if (x > maxX)
                        {
                            maxX = x;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }

            return new PreviewBoundaries(minX, maxX, minY, maxY);
        }

        private readonly struct PreviewBoundaries
        {
            public PreviewBoundaries(int minX, int maxX, int minY, int maxY)
            {
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }

            public readonly int MinX;
            public readonly int MaxX;
            public readonly int MinY;
            public readonly int MaxY;

            public bool IsEmpty()
            {
                return MinX == -1 && MaxX == -1 && MinY == -1 && MaxY == -1;
            }
        }
    }

    internal class CgeRenderingJob
    {
        private readonly ComboGesturePreviewSetup _previewSetup;
        private readonly List<AnimationPreview> _animationPreviews;
        private readonly Action<AnimationPreview> _onClipRendered;
        private readonly Action _onQueueTaskComplete;
        private RenderTexture _renderTexture;
        private readonly bool _includeTransformsMode;

        private static bool StopGenerating { get; set; }

        public CgeRenderingJob(ComboGesturePreviewSetup previewSetup, List<AnimationPreview> animationPreviews, Action<AnimationPreview> onClipRendered, bool includeTransformsMode, Action onQueueTaskComplete = null)
        {
            _previewSetup = previewSetup;
            _animationPreviews = animationPreviews;
            _onClipRendered = onClipRendered;
            _includeTransformsMode = includeTransformsMode;
            _onQueueTaskComplete = onQueueTaskComplete ?? (() => {});
        }

        public void Capture()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }

            var generatedCamera = GenerateCamera();
            _previewSetup.previewDummy.gameObject.SetActive(true);

            DoGenerationProcess(generatedCamera);
        }

        private void DoGenerationProcess(Camera generatedCamera)
        {
            var queue = new List<Action<int>>();

            for (var index = 0; index < _animationPreviews.Count; index++)
            {
                var currentPreview = _animationPreviews[index];
                if (_includeTransformsMode || _animationPreviews.Count <= 1)
                {
                    queue.Add(i => { SampleClip(currentPreview); });
                    queue.Add(i => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else if (index == 0)
                {
                    queue.Add(i => { SampleClip(currentPreview); });
                }
                else if (index == _animationPreviews.Count - 1)
                {
                    var previousPreview = _animationPreviews[index - 1];
                    queue.Add(i =>
                    {
                        RenderToTexture(generatedCamera, previousPreview);
                        SampleClip(currentPreview);
                    });
                    queue.Add(i => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else
                {
                    var previousPreview = _animationPreviews[index - 1];
                    queue.Add(i =>
                    {
                        RenderToTexture(generatedCamera, previousPreview);
                        SampleClip(currentPreview);
                    });
                }
            }

            var cleanupQueue = new List<Action<int>> {i => Terminate(generatedCamera), i => _onQueueTaskComplete.Invoke()};
            CgeRenderingProcessorUnityCycle.AddToQueue(() => RunAsync(queue, cleanupQueue));
        }

        private void SampleClip(AnimationPreview animationPreview)
        {
            AnimationMode.BeginSampling();
            PoseDummyUsing(animationPreview.Clip);
            AnimationMode.EndSampling();
        }

        private void RenderToTexture(Camera generatedCamera, AnimationPreview animationPreview)
        {
            CaptureDummy(animationPreview.RenderTexture, generatedCamera);

            _onClipRendered.Invoke(animationPreview);
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
                    CgeRenderingProcessorUnityCycle.AddToQueue(() => RunAsync(actions, cleanupActions));
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
                CgeRenderingProcessorUnityCycle.AddToQueue(() => Cleanup(cleanupActions));
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
                    CgeRenderingProcessorUnityCycle.AddToQueue(() => Cleanup(cleanupActions));
                }
                else
                {
                    StopGenerating = false;
                }
            }
        }

        private void PoseDummyUsing(AnimationClip clip)
        {
            AnimationMode.SampleAnimationClip(_previewSetup.previewDummy.gameObject, clip, 1/60f);
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
                _previewSetup.previewDummy.gameObject.SetActive(false);
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
            var possibleBone = _previewSetup.previewDummy.GetBoneTransform(HumanBodyBones.Head);
            return possibleBone == null ? _previewSetup.previewDummy.transform : possibleBone;
        }
    }
}
