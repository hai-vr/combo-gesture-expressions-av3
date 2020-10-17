using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgePreviewState
    {
        public Dictionary<AnimationClip, Texture2D> AnimationClipToTextureDict { get; } = new Dictionary<AnimationClip, Texture2D>();
        public Dictionary<AnimationClip, Texture2D> AnimationClipToTextureDictGray { get; } = new Dictionary<AnimationClip, Texture2D>();
    }

    public class CgePreviewEffector
    {
        private readonly CgePreviewState _cgePreviewState;
        private readonly CgeEditorEffector _editorEffector;

        public CgePreviewEffector(CgePreviewState cgePreviewState, CgeEditorEffector editorEffector)
        {
            _cgePreviewState = cgePreviewState;
            _editorEffector = editorEffector;
        }

        public void GenerateMissingPreviews(Action repaintCallback)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null);
        }

        public void GenerateMissingPreviewsPrioritizing(Action repaintCallback, AnimationClip element)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, element);
        }

        public void GenerateAll(Action repaintCallback)
        {
            Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything, null);
        }

        private CgeActivityPreviewInternal Previewer(Action repaintCallback)
        {
            return new CgeActivityPreviewInternal(
                repaintCallback,
                _editorEffector,
                _cgePreviewState.AnimationClipToTextureDict,
                _cgePreviewState.AnimationClipToTextureDictGray,
                CgeLayoutCommon.PictureWidth,
                CgeLayoutCommon.PictureHeight
            );
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
    }
}
