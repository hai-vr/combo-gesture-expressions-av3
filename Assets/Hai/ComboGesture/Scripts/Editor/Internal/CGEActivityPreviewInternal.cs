using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Modules;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeActivityPreviewInternal
    {
        private readonly Action _onClipRenderedFn;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeBlendTreeEffector _blendTreeEffector;
        private readonly CgeMemoization _memoization;
        private readonly int _pictureWidth;
        private readonly int _pictureHeight;
        private readonly AnimationClip[] _editorArbitraryAnimations;
        private readonly CgeRenderingCommands _cgeRenderingCommands;

        public CgeActivityPreviewInternal(Action onClipRenderedFn,
            CgeEditorEffector editorEffector,
            CgeBlendTreeEffector blendTreeEffector,
            CgeMemoization memoization,
            int pictureWidth,
            int pictureHeight,
            CgeRenderingCommands cgeRenderingCommands)
        {
            _onClipRenderedFn = onClipRenderedFn;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
            _memoization = memoization;
            _pictureWidth = pictureWidth;
            _pictureHeight = pictureHeight;
            _editorArbitraryAnimations = _editorEffector.GetActivity()?.editorArbitraryAnimations ?? new AnimationClip[]{};
            _cgeRenderingCommands = cgeRenderingCommands;
        }

        public enum ProcessMode
        {
            RecalculateEverything, CalculateMissing
        }

        public void Process(ProcessMode processMode, AnimationClip prioritize, ComboGesturePreviewSetup previewSetup)
        {
            var clipDictionary = GatherAnimations(processMode);
            var animationPreviews = ToPrioritizedList(clipDictionary, prioritize);

            _cgeRenderingCommands.GenerateSpecific(animationPreviews, OnClipRendered, previewSetup);
        }

        private void OnClipRendered(AnimationPreview animationPreview)
        {
            _memoization.AssignRegular(animationPreview.Clip, animationPreview.RenderTexture);
            _memoization.AssignGrayscale(animationPreview.Clip, GrayscaleCopyOf(animationPreview.RenderTexture));
            _onClipRenderedFn.Invoke();
        }

        private static Texture2D GrayscaleCopyOf(Texture2D originalTexture)
        {
            var texture = UnityEngine.Object.Instantiate(originalTexture);
            var mipCount = Mathf.Min(3, texture.mipmapCount);

            for (var mip = 0; mip < mipCount; ++mip)
            {
                var cols = texture.GetPixels(mip);
                for (var i = 0; i < cols.Length; ++i)
                {
                    var value = (cols[i].r + cols[i].g + cols[i].b) / 3f;
                    cols[i] = new Color(value, value, value);
                }
                texture.SetPixels(cols, mip);
            }
            texture.Apply(false);

            return texture;
        }

        private static List<AnimationPreview> ToPrioritizedList(Dictionary<AnimationClip, Texture2D> clipDictionary, AnimationClip prioritize)
        {
            if (prioritize != null && clipDictionary.ContainsKey(prioritize))
            {
                var animationPreviews = clipDictionary.Where(pair => pair.Key != prioritize)
                    .Select(pair => new AnimationPreview(pair.Key, pair.Value))
                    .ToList();
                animationPreviews.Insert(0, new AnimationPreview(prioritize, clipDictionary[prioritize]));

                return animationPreviews;
            }

            return clipDictionary
                .Select(pair => new AnimationPreview(pair.Key, pair.Value))
                .ToList();
        }


        private Dictionary<AnimationClip, Texture2D> GatherAnimations(ProcessMode processMode)
        {
            var enumerable = _editorArbitraryAnimations
                .Union(_editorEffector.AllDistinctAnimations())
                .Union(_blendTreeEffector.AllAnimationsOfSelected())
                .Distinct()
                .Where(clip => clip != null);

            if (processMode == ProcessMode.CalculateMissing)
            {
                enumerable = enumerable.Where(clip => !_memoization.Has(clip));
            }

            return new HashSet<AnimationClip>(enumerable.ToList())
                    .ToDictionary(clip => clip, clip => CgeMemoryQuery.NewPreviewTexture2D(_pictureWidth, _pictureHeight));
        }
    }
}
