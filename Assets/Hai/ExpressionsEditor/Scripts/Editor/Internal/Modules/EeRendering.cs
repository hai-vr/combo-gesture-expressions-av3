using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EeRenderingCommands
    {
        private readonly Dictionary<EePriority, List<Action>> _priorityQueues = new Dictionary<EePriority, List<Action>>();
        private bool _isProcessing;
        private Action _queueEmptied;

        public enum EeDummyAutoHide
        {
            Default, DoNotHide
        }

        public enum EePriority
        {
            High, Normal, Low
        }

        public EeRenderingCommands()
        {
            _priorityQueues[EePriority.High] = new List<Action>();
            _priorityQueues[EePriority.Normal] = new List<Action>();
            _priorityQueues[EePriority.Low] = new List<Action>();
        }

        public void SetQueueEmptiedAction(Action action)
        {
            // FIXME: bad pattern
            _queueEmptied = action;
        }

        public void GenerateSpecific(
            List<EeRenderingSample> animationsPreviews,
            EePreviewAvatar previewSetup,
            EeDummyAutoHide autoHide = EeDummyAutoHide.Default,
            EePriority priority = EePriority.Normal)
        {
            _priorityQueues[priority].Add(() => new EeRenderingJob(previewSetup, 0, animationsPreviews, true, OnQueueTaskComplete, autoHide).Capture());
            WakeQueue();
        }

        public Action GenerateSpecificFastMode(
            List<EeRenderingSample> animationsPreviews,
            EePreviewAvatar previewSetup,
            int cameraIndex,
            EeDummyAutoHide autoHide = EeDummyAutoHide.Default,
            EePriority priority = EePriority.Normal)
        {
            List<EeRenderingJob> jobs = new List<EeRenderingJob>();
            for (int startIndex = 0; startIndex < animationsPreviews.Count; startIndex += 15)
            {
                var finalIndex = startIndex;
                var job = new EeRenderingJob(previewSetup, cameraIndex, animationsPreviews.GetRange(finalIndex, Math.Min(15, animationsPreviews.Count - finalIndex)), false, OnQueueTaskComplete, autoHide);
                jobs.Add(job);
                _priorityQueues[priority].Add(() => job.Capture());
            }
            WakeQueue();

            // FIXME: Obscure drop callback
            return () =>
            {
                foreach (var job in jobs)
                {
                    job.Drop();
                }
            };
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

            var queue = new[] {_priorityQueues[EePriority.High], _priorityQueues[EePriority.Normal], _priorityQueues[EePriority.Low]}
                .First(list => list.Count > 0);

            var first = queue[0];
            queue.RemoveAt(0);
            first.Invoke();
        }

        private bool QueueIsEmpty()
        {
            return new[] {_priorityQueues[EePriority.High], _priorityQueues[EePriority.Normal], _priorityQueues[EePriority.Low]}
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

    public static class EeRenderingSupport
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

        // ReSharper disable once UnusedMember.Global
        public static void MutateHighlightHotspots2(Texture2D mutableTexture, Texture2D blendshapeTexture, Texture2D noShapekeys)
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
                    var bg = (x / 10 + y / 10) % 2 == 0 ? Color.white : new Color(0.95f, 0.95f, 0.95f, 0.35f);
                    if (blendShapePixel != other)
                    {
                        var difference = MeasureDifference(blendShapePixel, other) * 10;
                        var target = new Color(blendShapePixel.r, blendShapePixel.g, blendShapePixel.b, 1f);
                        tex.SetPixel(x, y, Color.Lerp(bg, target, Mathf.SmoothStep(0.7f, 1f, difference)));
                    }
                    else
                    {
                        tex.SetPixel(x, y, bg);
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

    internal class EeRenderingJob
    {
        private readonly EePreviewAvatar _previewSetup;
        private readonly List<EeRenderingSample> _renderingSamples;
        private readonly Action _onQueueTaskComplete;
        private RenderTexture _renderTexture;
        private readonly bool _includeTransformsMode;
        private readonly EeRenderingCommands.EeDummyAutoHide _autoHide;
        private readonly int _cameraIndex;

        private bool StopGenerating { get; set; }

        public EeRenderingJob(EePreviewAvatar previewSetup, int cameraIndex, List<EeRenderingSample> renderingSamples, bool includeTransformsMode, Action onQueueTaskComplete = null, EeRenderingCommands.EeDummyAutoHide autoHide = EeRenderingCommands.EeDummyAutoHide.Default)
        {
            _previewSetup = previewSetup;
            _cameraIndex = cameraIndex;
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
            _previewSetup.Dummy.gameObject.SetActive(true);

            DoGenerationProcess(generatedCamera);
        }

        public void Drop()
        {
            StopGenerating = true;
        }

        private void DoGenerationProcess(Camera generatedCamera)
        {
            var queue = new Queue<Action>();

            for (var index = 0; index < _renderingSamples.Count; index++)
            {
                var currentPreview = _renderingSamples[index];
                if (_includeTransformsMode || _renderingSamples.Count <= 1)
                {
                    queue.Enqueue(() => { SampleClip(currentPreview); });
                    queue.Enqueue(() => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else if (index == 0)
                {
                    queue.Enqueue(() => { SampleClip(currentPreview); });
                }
                else if (index == _renderingSamples.Count - 1)
                {
                    var previousPreview = _renderingSamples[index - 1];
                    queue.Enqueue(() =>
                    {
                        RenderToTexture(generatedCamera, previousPreview);
                        SampleClip(currentPreview);
                    });
                    queue.Enqueue(() => { RenderToTexture(generatedCamera, currentPreview); });
                }
                else
                {
                    var previousPreview = _renderingSamples[index - 1];
                    queue.Enqueue(() =>
                    {
                        RenderToTexture(generatedCamera, previousPreview);
                        SampleClip(currentPreview);
                    });
                }
            }

            var cleanupQueue = new Queue<Action>();
            cleanupQueue.Enqueue(() => Terminate(generatedCamera));
            cleanupQueue.Enqueue(() => _onQueueTaskComplete.Invoke());
            EeRenderingProcessorUnityCycle.AddToQueue(() => RunAsync(queue, cleanupQueue));
        }

        private void SampleClip(EeRenderingSample eeRenderingSample)
        {
            WorkaroundAnimatedMaterialPropertiesIgnoredOnThirdSampling();

            AnimationMode.BeginSampling();
            PoseDummyUsing(eeRenderingSample.Clip);
            AnimationMode.EndSampling();
        }

        private static void WorkaroundAnimatedMaterialPropertiesIgnoredOnThirdSampling()
        {
            // https://github.com/hai-vr/combo-gesture-expressions-av3/issues/268
            // When in animation mode, for some reason the 3rd sampling will fail to apply any animated material properties
            // Waiting a for a certain number of frames will not help.
            // The workaround is to sample nothing in the same frame (why does that work?!)
            AnimationMode.BeginSampling();
            AnimationMode.EndSampling();
        }

        private void RenderToTexture(Camera generatedCamera, EeRenderingSample eeRenderingSample)
        {
            CaptureDummy(eeRenderingSample.RenderTexture, generatedCamera);

            eeRenderingSample.Callback.Invoke(eeRenderingSample);
        }

        private void RunAsync(Queue<Action> actions, Queue<Action> cleanupActions)
        {
            try
            {
                actions.Dequeue().Invoke();
                if (actions.Count > 0 && !StopGenerating)
                {
                    EeRenderingProcessorUnityCycle.AddToQueue(() => RunAsync(actions, cleanupActions));
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

        private void StartCleanupActions(Queue<Action> cleanupActions)
        {
            if (cleanupActions.Count > 0)
            {
                EeRenderingProcessorUnityCycle.AddToQueue(() => Cleanup(cleanupActions));
            }
        }

        private void Cleanup(Queue<Action> cleanupActions)
        {
            try
            {
                cleanupActions.Dequeue().Invoke();
            }
            finally
            {
                if (cleanupActions.Count > 0)
                {
                    EeRenderingProcessorUnityCycle.AddToQueue(() => Cleanup(cleanupActions));
                }
            }
        }

        private void PoseDummyUsing(AnimationClip clip)
        {
            AnimationMode.SampleAnimationClip(_previewSetup.Dummy.gameObject, clip, 1/60f);
        }

        private void CaptureDummy(Texture2D element, Camera generatedCamera)
        {
            _renderTexture = new RenderTexture(element.width, element.height, 24);
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

            if (_autoHide != EeRenderingCommands.EeDummyAutoHide.DoNotHide && _previewSetup.AutoHide) {
                _previewSetup.Dummy.gameObject.SetActive(false);
            }
        }

        private Camera GenerateCamera()
        {
            var parentNullable = MaybeResolveAttachedParent(_previewSetup.GetEffectiveCameraConfig(_cameraIndex));
            var effectiveCamera = _previewSetup.GetEffectiveCamera(_cameraIndex);
            var generatedCamera = Object.Instantiate(effectiveCamera, parentNullable == null ? effectiveCamera.transform : parentNullable, true);
            generatedCamera.gameObject.SetActive(true);

            return generatedCamera;
        }

        private Transform MaybeResolveAttachedParent(EePreviewAvatarCamera config)
        {
            if (!config.autoAttachToBone) return null;

            var possibleBone = _previewSetup.Dummy.GetBoneTransform(config.optionalAttachToBone);
            return possibleBone == null ? _previewSetup.Dummy.transform : possibleBone;
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

    internal static class EeRenderingProcessorUnityCycle
    {
        private static readonly Queue<Action> InternalActionQueue = new Queue<Action>();

        private static bool _isInternalActionQueueRunning;
        private static bool _isEditorUpdateHooked;

        public static void AddToQueue(Action action)
        {
            if (!_isInternalActionQueueRunning)
            {
                if (!_isEditorUpdateHooked)
                {
                    EditorApplication.update += OnEditorUpdate;
                    _isEditorUpdateHooked = true;
                }
                _isInternalActionQueueRunning = true;
            }

            InternalActionQueue.Enqueue(action);
        }

        private static void OnEditorUpdate()
        {
            if (!_isInternalActionQueueRunning) return;

            // Animation sampling will not affect the blendshapes when the window is not focused.
            if (!IsUnityEditorWindowFocused()) return;

            if (InternalActionQueue.Count == 0)
            {
                _isInternalActionQueueRunning = false;
                return;
            }

            var action = InternalActionQueue.Dequeue();

            action.Invoke();
        }

        private static bool IsUnityEditorWindowFocused()
        {
            return UnityEditorInternal.InternalEditorUtility.isApplicationActive;
        }
    }
}
