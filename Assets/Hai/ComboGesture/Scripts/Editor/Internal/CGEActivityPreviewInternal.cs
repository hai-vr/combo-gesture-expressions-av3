using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ExpressionsEditor.Scripts.Components;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
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
        private readonly EeRenderingCommands _eeRenderingCommands;

        public CgeActivityPreviewInternal(Action onClipRenderedFn,
            CgeEditorEffector editorEffector,
            CgeBlendTreeEffector blendTreeEffector,
            CgeMemoization memoization,
            int pictureWidth,
            int pictureHeight,
            EeRenderingCommands eeRenderingCommands)
        {
            _onClipRenderedFn = onClipRenderedFn;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
            _memoization = memoization;
            _pictureWidth = pictureWidth;
            _pictureHeight = pictureHeight;
            _editorArbitraryAnimations = _editorEffector.GetActivity()?.editorArbitraryAnimations ?? new AnimationClip[]{};
            _eeRenderingCommands = eeRenderingCommands;
        }

        public enum ProcessMode
        {
            RecalculateEverything, CalculateMissing
        }

        public void Process(ProcessMode processMode, AnimationClip prioritize, EePreviewAvatar previewSetup)
        {
            var clipDictionary = GatherAnimations(processMode);
            var animationPreviews = ToPrioritizedList(clipDictionary, prioritize);

            _eeRenderingCommands.GenerateSpecific(animationPreviews, previewSetup);
        }

        private void OnClipRendered(EeRenderingSample animationPreview)
        {
            _memoization.AssignRegular(animationPreview.Clip, animationPreview.RenderTexture);
            _memoization.AssignGrayscale(animationPreview.Clip, GrayscaleCopyOf(animationPreview.RenderTexture));
            _onClipRenderedFn.Invoke();
        }

        internal static Texture2D GrayscaleCopyOf(Texture2D originalTexture)
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

        private List<EeRenderingSample> ToPrioritizedList(Dictionary<AnimationClip, Texture2D> clipDictionary, AnimationClip prioritize)
        {
            if (prioritize != null && clipDictionary.ContainsKey(prioritize))
            {
                var animationPreviews = clipDictionary.Where(pair => pair.Key != prioritize)
                    .Select(pair => new EeRenderingSample(pair.Key, pair.Value, OnClipRendered))
                    .ToList();
                animationPreviews.Insert(0, new EeRenderingSample(prioritize, clipDictionary[prioritize], OnClipRendered));

                return animationPreviews;
            }

            return clipDictionary
                .Select(pair => new EeRenderingSample(pair.Key, pair.Value, OnClipRendered))
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
