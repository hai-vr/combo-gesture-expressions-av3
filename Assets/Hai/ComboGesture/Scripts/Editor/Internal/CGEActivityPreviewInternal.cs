using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeActivityPreviewInternal
    {
        private static bool StopGenerating { get; set; }

        private readonly Action _repaintFn;
        private readonly ComboGestureActivity _activity;
        private readonly Dictionary<AnimationClip, Texture2D> _animationClipToTextureDict;
        private readonly int _pictureWidth;
        private readonly int _pictureHeight;
        private readonly AnimationClip[] _editorArbitraryAnimations;
        private RenderTexture _renderTexture;

        public CgeActivityPreviewInternal(Action repaintFn, ComboGestureActivity activity, Dictionary<AnimationClip, Texture2D> animationClipToTextureDict, int pictureWidth, int pictureHeight, AnimationClip[] editorArbitraryAnimations)
        {
            _repaintFn = repaintFn;
            _activity = activity;
            _animationClipToTextureDict = animationClipToTextureDict;
            _pictureWidth = pictureWidth;
            _pictureHeight = pictureHeight;
            _editorArbitraryAnimations = editorArbitraryAnimations ?? new AnimationClip[]{};
        }

        public enum ProcessMode
        {
            RecalculateEverything, CalculateMissing
        }

        public void Process(ProcessMode processMode, AnimationClip prioritize)
        {
            if (AnimationMode.InAnimationMode())
            {
                return;
            }

            var clipToTextureDictionary = GatherAnimations(processMode);

            SetupCaptureScaffolding();
            var generatedCamera = GenerateCamera();

            var actions = new List<Action<int>>();

            DoGenerationProcess(prioritize, clipToTextureDictionary, actions, generatedCamera);
        }

        private Camera GenerateCamera()
        {
            var headBone = GetHeadBoneOrDefault();
            var generatedCamera = Object.Instantiate(_activity.previewSetup.camera, headBone, true);
            generatedCamera.gameObject.SetActive(true);
            return generatedCamera;
        }

        private void DoGenerationProcess(AnimationClip prioritize, Dictionary<AnimationClip, Texture2D> clipToTextureDictionary, List<Action<int>> actions, Camera generatedCamera)
        {
            GetDummy().gameObject.SetActive(true);

            if (prioritize != null)
            {
                Generate(actions, generatedCamera, prioritize, clipToTextureDictionary[prioritize]);
            }

            foreach (var clipToTexture in clipToTextureDictionary)
            {
                Generate(actions, generatedCamera, clipToTexture.Key, clipToTexture.Value);
            }

            var cleanupActions = new List<Action<int>>();
            cleanupActions.Add(i => Terminate(generatedCamera));
            EditorApplication.delayCall += () => Reevaluate(actions, cleanupActions);
        }

        private void Generate(List<Action<int>> actions, Camera generatedCamera, AnimationClip clip, Texture2D texture)
        {
            actions.Add(i =>
            {
                AnimationMode.BeginSampling();
                PoseDummyUsing(clip);
                AnimationMode.EndSampling();
            });
            actions.Add(i =>
            {
                CaptureDummy(texture, generatedCamera);
                _animationClipToTextureDict[clip] = texture;
                _repaintFn.Invoke();
            });
        }

        private void Terminate(Camera generatedCamera)
        {
            SceneView.RepaintAll();
            AnimationMode.StopAnimationMode();
            Object.DestroyImmediate(generatedCamera.gameObject);
            GetDummy().gameObject.SetActive(false);
        }

        private static void Reevaluate(List<Action<int>> actions, List<Action<int>> cleanupActions)
        {
            try
            {
                var action = actions[0];
                actions.RemoveAt(0);
                action.Invoke(999);
                if (actions.Count > 0 && !StopGenerating)
                {
                    EditorApplication.delayCall += () => Reevaluate(actions, cleanupActions);
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

        private Transform GetHeadBoneOrDefault()
        {
            var possibleBone = GetDummy().GetBoneTransform(HumanBodyBones.Head);
            return possibleBone == null ? GetDummy().transform : possibleBone;
        }

        private Animator GetDummy()
        {
            return _activity.previewSetup.previewDummy;
        }

        private static void SetupCaptureScaffolding()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }
        }

        private void PoseDummyUsing(AnimationClip clip)
        {
            AnimationMode.SampleAnimationClip(GetDummy().gameObject, clip, 1/60f);
        }

        private void CaptureDummy(Texture2D element, Camera generatedCamera)
        {
            _renderTexture = new RenderTexture(_pictureWidth, _pictureHeight, 0);
            RenderCamera(_renderTexture, generatedCamera);
            RenderTextureTo(_renderTexture, element);
        }

        private Dictionary<AnimationClip, Texture2D> GatherAnimations(ProcessMode processMode)
        {
            var allAvailableAnimations = new HashSet<AnimationClip>(_editorArbitraryAnimations);
            allAvailableAnimations.UnionWith(_activity.OrderedAnimations());

            var enumerable = allAvailableAnimations.Where(clip => clip != null);
            if (processMode == ProcessMode.CalculateMissing)
            {
                enumerable = enumerable.Where(clip => !_animationClipToTextureDict.ContainsKey(clip));
            }

            return new HashSet<AnimationClip>(enumerable.ToList())
                    .ToDictionary(clip => clip, clip => NewTexture2D());
        }

        private Texture2D NewTexture2D()
        {
            return new Texture2D(_pictureWidth, _pictureHeight, TextureFormat.ARGB32, false);
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

        public static void Stop_Temp()
        {
            if (StopGenerating) {
                AnimationMode.StopAnimationMode();
            }
            StopGenerating = true;
        }
    }
}
