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
        private readonly CgeBlendTreeEffector _blendTreeEffector;
        private readonly List<Action> _queue;
        private bool isProcessing;

        public CgePreviewEffector(CgePreviewState cgePreviewState, CgeEditorEffector editorEffector, CgeBlendTreeEffector blendTreeEffector)
        {
            _cgePreviewState = cgePreviewState;
            _editorEffector = editorEffector;
            _blendTreeEffector = blendTreeEffector;
            _queue = new List<Action>();
        }

        public void GenerateMissingPreviews(Action repaintCallback)
        {
            _queue.Add(() => Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null));
            WakeQueue();
        }

        public void GenerateMissingPreviewsPrioritizing(Action repaintCallback, AnimationClip element)
        {
            _queue.Add(() => Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, element));
            WakeQueue();
        }

        public void GenerateAll(Action repaintCallback)
        {
            _queue.Add(() => Previewer(repaintCallback).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything, null));
            WakeQueue();
        }

        public void GenerateSpecific(List<AnimationPreview> animationsPreviews, Action<AnimationPreview> onClipRendered)
        {
            _queue.Add(() => new CgePreviewProcessor(_editorEffector.PreviewSetup(), animationsPreviews, onClipRendered, OnQueueTaskComplete).Capture());
            WakeQueue();
        }

        private void WakeQueue()
        {
            if (isProcessing || _queue.Count == 0)
            {
                return;
            }

            isProcessing = true;
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
                isProcessing = false;
            }
            else
            {
                ExecuteNextInQueue();
            }
        }

        private CgeActivityPreviewInternal Previewer(Action repaintCallback)
        {
            return new CgeActivityPreviewInternal(
                repaintCallback,
                _editorEffector,
                _blendTreeEffector,
                _cgePreviewState.AnimationClipToTextureDict,
                _cgePreviewState.AnimationClipToTextureDictGray,
                CgeLayoutCommon.PictureWidth,
                CgeLayoutCommon.PictureHeight,
                OnQueueTaskComplete
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
