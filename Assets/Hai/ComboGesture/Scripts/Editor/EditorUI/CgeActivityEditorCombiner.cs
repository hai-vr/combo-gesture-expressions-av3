using System;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    internal class CgeActivityEditorCombiner
    {
        public const int CombinerPreviewWidth = 240;
        public const int CombinerPreviewHeight = 160;

        private readonly AnimationPreview _leftPreview;
        private readonly AnimationPreview _rightPreview;
        private readonly ComboGestureActivity _activity;
        private readonly Action _onClipRenderedFn;

        public CgeActivityEditorCombiner(ComboGestureActivity activity, AnimationClip leftAnim, AnimationClip rightAnim, Action onClipRenderedFn)
        {
            _activity = activity;
            _leftPreview = new AnimationPreview(leftAnim, CgePreviewProcessor.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight));
            _rightPreview = new AnimationPreview(rightAnim, CgePreviewProcessor.NewPreviewTexture2D(CombinerPreviewWidth, CombinerPreviewHeight));
            _onClipRenderedFn = onClipRenderedFn;
        }

        public void Prepare()
        {
            CreatePreviews();
        }

        private void CreatePreviews()
        {
            if (_activity.previewSetup == null) return;

            var animationsPreviews = new[] {_leftPreview, _rightPreview}.ToList();
            new CgePreviewProcessor(_activity.previewSetup, animationsPreviews, OnClipRendered).Capture();
        }

        private void OnClipRendered(AnimationPreview obj)
        {
            _onClipRenderedFn.Invoke();
        }

        public Texture LeftTexture()
        {
            return _leftPreview.RenderTexture;
        }

        public Texture RightTexture()
        {
            return _rightPreview.RenderTexture;
        }
    }
}
