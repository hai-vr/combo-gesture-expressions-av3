using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeActivityPreviewInternal
    {
        private readonly ComboGestureActivity _activity;
        private readonly Dictionary<AnimationClip, Texture2D> _animationClipToTextureDict;
        private readonly AnimationClip _noAnimationClipNullObject;
        private readonly int _pictureWidth;
        private readonly int _pictureHeight;
        private readonly AnimationClip[] _editorArbitraryAnimations;
        private RenderTexture _renderTexture;

        public CgeActivityPreviewInternal(ComboGestureActivity activity, Dictionary<AnimationClip, Texture2D> animationClipToTextureDict, AnimationClip noAnimationClipNullObject, int pictureWidth, int pictureHeight, AnimationClip[] editorArbitraryAnimations)
        {
            _activity = activity;
            _animationClipToTextureDict = animationClipToTextureDict;
            _noAnimationClipNullObject = noAnimationClipNullObject;
            _pictureWidth = pictureWidth;
            _pictureHeight = pictureHeight;
            _editorArbitraryAnimations = editorArbitraryAnimations ?? new AnimationClip[]{};

        }

        public void ProcessJust(AnimationClip element)
        {
            if (AnimationMode.InAnimationMode())
            {
                CarefullyCleanupCaptureModeAndScaffolding();
                // FIXME: handle this error, and the caller should not have called me -> gray button
                return;
            }

            var clipToTextureDictionary = new Dictionary<AnimationClip, Texture2D>
            {
                {element, NewTexture2D()}
            };

            SetupCaptureScaffolding();
            var generatedCamera = GenerateCamera();
            DoGenerationProcess(clipToTextureDictionary, new List<Action<int>>(), generatedCamera, null);
        }

        public enum ProcessMode
        {
            RecalculateEverything, CalculateMissing
        }

        public void Process(ProcessMode processMode)
        {
            if (AnimationMode.InAnimationMode())
            {
                CarefullyCleanupCaptureModeAndScaffolding();
                // FIXME: handle this error, and the caller should not have called me -> gray button
                return;
            }

            var clipToTextureDictionary = GatherAnimations(processMode);
            var defaultFace = NewTexture2D();

            SetupCaptureScaffolding();
            var generatedCamera = GenerateCamera();

            var actions = new List<Action<int>>();
            // CaptureDummy(defaultFace, generatedCamera);

            actions.Add(i =>
            {
                CaptureDummy(defaultFace, generatedCamera);
            });

            DoGenerationProcess(clipToTextureDictionary, actions, generatedCamera, defaultFace);
        }

        private Camera GenerateCamera()
        {
            var headBone = GetHeadBoneOrDefault();
            var generatedCamera = Object.Instantiate(_activity.previewSetup.camera, headBone, true);
            generatedCamera.gameObject.SetActive(true);
            return generatedCamera;
        }

        private void DoGenerationProcess(Dictionary<AnimationClip, Texture2D> clipToTextureDictionary, List<Action<int>> actions, Camera generatedCamera, Texture2D defaultFace)
        {
            GetDummy().gameObject.SetActive(true);

            var enumerator = clipToTextureDictionary.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var cur = enumerator.Current;
                actions.Add(i =>
                {
                    AnimationMode.BeginSampling();
                    PoseDummyUsing(cur.Key);
                    AnimationMode.EndSampling();
                });
                var hasNext = true;
                while (hasNext)
                {
                    var curCopy = cur;
                    hasNext = enumerator.MoveNext();
                    KeyValuePair<AnimationClip, Texture2D>? nextCopy = null;
                    if (hasNext)
                    {
                        nextCopy = enumerator.Current;
                    }

                    var hasNextCopy = hasNext;
                    actions.Add(i =>
                    {
                        CaptureDummy(curCopy.Value, generatedCamera);
                        if (hasNextCopy)
                        {
                            AnimationMode.BeginSampling();
                            PoseDummyUsing(nextCopy.Value.Key);
                            AnimationMode.EndSampling();
                        }
                    });
                    if (hasNext)
                    {
                        cur = (KeyValuePair<AnimationClip, Texture2D>) nextCopy;
                    }
                }
            }

            foreach (var clipToTexture in clipToTextureDictionary)
            {
                actions.Add(i =>
                {
                    AnimationMode.BeginSampling();
                    PoseDummyUsing(clipToTexture.Key);
                    AnimationMode.EndSampling();
                });
                actions.Add(i => { CaptureDummy(clipToTexture.Value, generatedCamera); });
            }

            var cleanupActions = new List<Action<int>>();
            cleanupActions.Add(i => Terminate(generatedCamera, defaultFace, clipToTextureDictionary));
            var count = (float) actions.Count + cleanupActions.Count;
            var progressBarFn = DisplayProgressBarFn(count);
            EditorApplication.delayCall += () => Reevaluate(actions, cleanupActions, progressBarFn);
        }

        private void Terminate(Camera generatedCamera, Texture2D defaultFace, Dictionary<AnimationClip, Texture2D> clipToTextureDictionary)
        {
            CarefullyCleanupCaptureModeAndScaffolding();
            Object.DestroyImmediate(generatedCamera.gameObject);
            GetDummy().gameObject.SetActive(false);
            ReassignTextures(defaultFace, clipToTextureDictionary);
            AnimationMode.StopAnimationMode();
        }

        private static Action<int> DisplayProgressBarFn(float count)
        {
            return actualCount => EditorUtility.DisplayProgressBar("CGE Preview", "Generating previews... (Keep editor window in focus!)", (count - actualCount) / count);
        }

        private static void Reevaluate(List<Action<int>> actions, List<Action<int>> cleanupActions, Action<int> progressBarFn)
        {
            try
            {
                var action = actions[0];
                actions.RemoveAt(0);
                action.Invoke(999);
                if (actions.Count > 0)
                {
                    progressBarFn.Invoke(actions.Count + cleanupActions.Count);
                    EditorApplication.delayCall += () => Reevaluate(actions, cleanupActions, progressBarFn);
                }
                else
                {
                    StartCleanupActions(cleanupActions, progressBarFn);
                }
            }
            catch (Exception)
            {
                StartCleanupActions(cleanupActions, progressBarFn);
            }
        }

        private static void StartCleanupActions(List<Action<int>> cleanupActions, Action<int> progressBarFn)
        {
            if (cleanupActions.Count > 0)
            {
                EditorApplication.delayCall += () => Cleanup(cleanupActions, progressBarFn);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void Cleanup(List<Action<int>> cleanupActions, Action<int> progressBarFn)
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
                    progressBarFn.Invoke(cleanupActions.Count);
                    EditorApplication.delayCall += () => Cleanup(cleanupActions, progressBarFn);
                }
                else
                {
                    if (cleanupActions.Count > 0) {
                        EditorApplication.delayCall += () => Cleanup(cleanupActions, progressBarFn);
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        private void ReassignTextures(Texture2D defaultFace, Dictionary<AnimationClip, Texture2D> clipToTextureDictionary)
        {
            if (defaultFace != null)
            {
                _animationClipToTextureDict[_noAnimationClipNullObject] = defaultFace;
            }
            foreach (var clipToTexture in clipToTextureDictionary)
            {
                _animationClipToTextureDict[clipToTexture.Key] = clipToTexture.Value;
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

        private static void CarefullyCleanupCaptureModeAndScaffolding()
        {
            if (AnimationMode.InAnimationMode()) {
                try
                {
                }
                catch (Exception)
                {
                    // ignored
                }
                try
                {
                    SceneView.RepaintAll();
                }
                catch (Exception)
                {
                    // ignored
                }
                try
                {
                    AnimationMode.StopAnimationMode();
                }
                catch (Exception)
                {
                    // ignored
                }
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
    }
}
