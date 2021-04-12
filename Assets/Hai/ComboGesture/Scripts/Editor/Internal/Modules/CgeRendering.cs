using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Modules
{
    public class CgeRenderingCommands
    {
        private readonly Dictionary<CgePriority, List<Action>> _priorityQueues = new Dictionary<CgePriority, List<Action>>();
        private bool _isProcessing;
        private Action _queueEmptied;

        public enum CgeDummyAutoHide
        {
            Default, DoNotHide
        }

        public enum CgePriority
        {
            High, Normal, Low
        }

        public CgeRenderingCommands()
        {
            _priorityQueues[CgePriority.High] = new List<Action>();
            _priorityQueues[CgePriority.Normal] = new List<Action>();
            _priorityQueues[CgePriority.Low] = new List<Action>();
        }

        public void SetQueueEmptiedAction(Action action)
        {
            // FIXME: bad pattern
            _queueEmptied = action;
        }

        public void GenerateSpecific(
            List<RenderingSample> animationsPreviews,
            ComboGesturePreviewSetup previewSetup,
            CgeDummyAutoHide autoHide = CgeDummyAutoHide.Default,
            CgePriority priority = CgePriority.Normal)
        {
            _priorityQueues[priority].Add(() => new CgeRenderingJob(previewSetup, animationsPreviews, true, OnQueueTaskComplete, autoHide).Capture());
            WakeQueue();
        }

        public void GenerateSpecificFastMode(
            List<RenderingSample> animationsPreviews,
            ComboGesturePreviewSetup previewSetup,
            CgeDummyAutoHide autoHide = CgeDummyAutoHide.Default,
            CgePriority priority = CgePriority.Normal)
        {
            for (int startIndex = 0; startIndex < animationsPreviews.Count; startIndex += 15)
            {
                var finalIndex = startIndex;
                _priorityQueues[priority].Add(() => new CgeRenderingJob(previewSetup, animationsPreviews.GetRange(finalIndex, Math.Min(15, animationsPreviews.Count - finalIndex)), false, OnQueueTaskComplete, autoHide).Capture());
            }
            WakeQueue();
        }

        private void WakeQueue()
        {
            if (_isProcessing || QueueIsEmpty())
            {
                return;
            }

            _isProcessing = true;
            ExecuteNextInQueue();
        }

        private void ExecuteNextInQueue()
        {
            if (QueueIsEmpty()) return;

            var queue = new[] {_priorityQueues[CgePriority.High], _priorityQueues[CgePriority.Normal], _priorityQueues[CgePriority.Low]}
                .First(list => list.Count > 0);

            var first = queue[0];
            queue.RemoveAt(0);
            first.Invoke();
        }

        private bool QueueIsEmpty()
        {
            return new[] {_priorityQueues[CgePriority.High], _priorityQueues[CgePriority.Normal], _priorityQueues[CgePriority.Low]}
                .SelectMany(list => list)
                .FirstOrDefault() == null;
        }

        private void OnQueueTaskComplete()
        {
            if (QueueIsEmpty())
            {
                _queueEmptied();
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

        private static Color ComputeGrayscaleValue(Texture2D tex, int x, int y)
        {
            var value = tex.GetPixel(x, y);

            var gsv = (value.r + value.g + value.b) / 3f;
            var actualGsv = value * 0.3f + new Color(gsv, gsv, gsv, value.a) * 0.7f;
            return actualGsv;
        }

        public static void MutateMultilevelHighlightDifferences(Texture2D mutableTexture, Texture2D shapekeys, Texture2D noShapekeys)
        {
            var width = mutableTexture.width;
            var height = mutableTexture.height;

            var major = new bool[width * height];
            var hasAnyMajorDifference = MutateGetComputeDifference(major, shapekeys, noShapekeys, 0.05f);

            var minor = new bool[width * height];
            MutateGetComputeDifference(minor, shapekeys, noShapekeys, 0.0015f);

            var boundaries = hasAnyMajorDifference ? DifferenceAsBoundaries(major, width, height) : DifferenceAsBoundaries(minor, width, height);
            if (boundaries.IsEmpty())
            {
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    mutableTexture.SetPixel(x, y, ComputeGrayscaleValue(shapekeys, x, y) * 0.2f);

                mutableTexture.Apply();
            }
            else
            {
                MutateBoundariesGrayscale(mutableTexture, shapekeys, boundaries);
            }
        }

        private static void MutateBoundariesGrayscale(Texture2D mutaTexture, Texture2D shapekeys, PreviewBoundaries boundaries)
        {
            var width = mutaTexture.width;
            var height = mutaTexture.height;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var isIn = x >= boundaries.MinX - Border && x <= boundaries.MaxX + Border && y >= boundaries.MinY - Border && y <= boundaries.MaxY + Border;
                mutaTexture.SetPixel(x, y, isIn ? shapekeys.GetPixel(x, y) : ComputeGrayscaleValue(shapekeys, x, y) * 0.5f);
            }

            mutaTexture.Apply(false);
        }

        private static bool MutateGetComputeDifference(bool[] mutated, Texture2D shapekeys, Texture2D noShapekeys, float threshold)
        {
            bool hasAnyDifference = false;

            var width = shapekeys.width;
            var height = shapekeys.height;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var a = shapekeys.GetPixel(x, y);
                var b = noShapekeys.GetPixel(x, y);
                var isDifferent = a != b && MeasureDifference(a, b) > threshold;
                if (!hasAnyDifference && isDifferent)
                {
                    hasAnyDifference = true;
                }
                mutated[x + y * width] = isDifferent;
            }

            return hasAnyDifference;
        }

        private static float MeasureDifference(Color a, Color b)
        {
            return Mathf.Abs((a.r + a.g + a.b) - (b.r + b.g + b.b)) / 3f;
        }

        public static void MutateHighlightHotspots(Texture2D mutableTexture, Texture2D blendshapeTexture, Texture2D noShapekeys)
        {
            var tex = mutableTexture;

            var width = mutableTexture.width;
            var height = mutableTexture.height;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var blendShapePixel = blendshapeTexture.GetPixel(x, y);
                    var other = noShapekeys.GetPixel(x, y);
                    if (blendShapePixel != other)
                    {
                        var difference = MeasureDifference(blendShapePixel, other) * 10;
                        tex.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, Mathf.SmoothStep(0f, 1f, difference)));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.green);
                    }
                }
            }

            tex.Apply(false);
        }

        private static PreviewBoundaries DifferenceAsBoundaries(bool[] mutableDiff, int width, int height)
        {
            var minX = -1;
            var maxX = -1;
            var minY = -1;
            var maxY = -1;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (mutableDiff[x + y * width])
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
        private readonly List<RenderingSample> _renderingSamples;
        private readonly Action _onQueueTaskComplete;
        private RenderTexture _renderTexture;
        private readonly bool _includeTransformsMode;
        private readonly CgeRenderingCommands.CgeDummyAutoHide _autoHide;

        private static bool StopGenerating { get; set; }

        public CgeRenderingJob(ComboGesturePreviewSetup previewSetup, List<RenderingSample> renderingSamples, bool includeTransformsMode, Action onQueueTaskComplete = null, CgeRenderingCommands.CgeDummyAutoHide autoHide = CgeRenderingCommands.CgeDummyAutoHide.Default)
        {
            _previewSetup = previewSetup;
            _renderingSamples = renderingSamples;
            _includeTransformsMode = includeTransformsMode;
            _autoHide = autoHide;
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

            for (var index = 0; index < _renderingSamples.Count; index++)
            {
                var currentPreview = _renderingSamples[index];
                if (_includeTransformsMode || _renderingSamples.Count <= 1)
                {
                    queue.Add(i => { SampleClip(currentPreview); });
                    queue.Add(i => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else if (index == 0)
                {
                    queue.Add(i => { SampleClip(currentPreview); });
                }
                else if (index == _renderingSamples.Count - 1)
                {
                    var previousPreview = _renderingSamples[index - 1];
                    queue.Add(i =>
                    {
                        RenderToTexture(generatedCamera, previousPreview);
                        SampleClip(currentPreview);
                    });
                    queue.Add(i => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else
                {
                    var previousPreview = _renderingSamples[index - 1];
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

        private void SampleClip(RenderingSample renderingSample)
        {
            AnimationMode.BeginSampling();
            PoseDummyUsing(renderingSample.Clip);
            AnimationMode.EndSampling();
        }

        private void RenderToTexture(Camera generatedCamera, RenderingSample renderingSample)
        {
            CaptureDummy(renderingSample.RenderTexture, generatedCamera);

            renderingSample.Callback.Invoke(renderingSample);
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

            if (_autoHide != CgeRenderingCommands.CgeDummyAutoHide.DoNotHide && _previewSetup.autoHide) {
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
