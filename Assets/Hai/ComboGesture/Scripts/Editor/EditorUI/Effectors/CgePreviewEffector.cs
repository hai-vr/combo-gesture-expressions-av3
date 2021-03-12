using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
using UnityEngine;

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

        public void OnMainRendered(EeHookOnMainRendered rendered, Action repaint)
        {
            _cgeMemoization.AssignRegular(rendered.Clip, UnityEngine.Object.Instantiate(rendered.OutputTexture));
            _cgeMemoization.AssignGrayscale(rendered.Clip, CgeActivityPreviewInternal.GrayscaleCopyOf(rendered.OutputTexture));
            repaint.Invoke();
        }
    }

}
