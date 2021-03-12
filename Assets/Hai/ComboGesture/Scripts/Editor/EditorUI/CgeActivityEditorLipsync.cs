using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
using UnityEngine;
using static UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public class CgeActivityEditorLipsync
    {
        public const int LipsyncPreviewWidth = 240;
        public const int LipsyncPreviewHeight = 160;

        private readonly ComboGestureLimitedLipsync _limitedLipsync;
        private readonly Action _onClipRenderedFn;
        private readonly CgeEditorEffector _editorEffector;
        private readonly EeRenderingCommands _renderingCommands;

        private readonly List<EeRenderingSample> _visemePreviews;

        public CgeActivityEditorLipsync(ComboGestureLimitedLipsync limitedLipsync, Action onClipRenderedFn, CgeEditorEffector editorEffector, EeRenderingCommands renderingCommands)
        {
            _limitedLipsync = limitedLipsync;
            _onClipRenderedFn = onClipRenderedFn;
            _editorEffector = editorEffector;
            _renderingCommands = renderingCommands;

            _visemePreviews = Enumerable.Range(0, 15)
                .Select(i => new EeRenderingSample(
                    new AnimationClip(),
                    CgeMemoryQuery.NewPreviewTexture2D(LipsyncPreviewWidth,LipsyncPreviewHeight),
                    OnClipRendered)
                )
                .ToList();
        }

        public void Prepare(AnimationClip baseFace)
        {
            if (!_editorEffector.IsPreviewSetupValid()) return;

            for (var visemeNumber = 0; visemeNumber < _visemePreviews.Count; visemeNumber++)
            {
                _visemePreviews[visemeNumber] = new EeRenderingSample(GenerateLipsyncClip(baseFace, visemeNumber), _visemePreviews[visemeNumber].RenderTexture, OnClipRendered);
            }

            _renderingCommands.GenerateSpecific(_visemePreviews.ToList(), _editorEffector.PreviewSetup());
        }

        public void PrepareJust(AnimationClip baseFace, int visemeNumber)
        {
            if (!_editorEffector.IsPreviewSetupValid()) return;

            _visemePreviews[visemeNumber] = new EeRenderingSample(GenerateLipsyncClip(baseFace, visemeNumber), _visemePreviews[visemeNumber].RenderTexture, OnClipRendered);

            _renderingCommands.GenerateSpecific(new[] {_visemePreviews[visemeNumber]}.ToList(), _editorEffector.PreviewSetup());
        }

        private AnimationClip GenerateLipsyncClip(AnimationClip baseFace, int visemeNumber)
        {
            var generatedClip = Instantiate(baseFace);

            var amplitude = _limitedLipsync.amplitudeScale * FindVisemeAmplitudeTweak(visemeNumber);
            new VisemeAnimationMaker(
                // _editorEffector.PreviewSetup().avatarDescriptor
                null // FIXME: Disabled this code because Limited Lipsync is a disabled feature, and refactoring this is harder when decoupling VRChat
            ).OverrideAnimation(generatedClip, visemeNumber, amplitude);

            return generatedClip;
        }

        private float FindVisemeAmplitudeTweak(int visemeNumber)
        {
            switch (visemeNumber)
            {
                case 0: return _limitedLipsync.amplitude0;
                case 1: return _limitedLipsync.amplitude1;
                case 2: return _limitedLipsync.amplitude2;
                case 3: return _limitedLipsync.amplitude3;
                case 4: return _limitedLipsync.amplitude4;
                case 5: return _limitedLipsync.amplitude5;
                case 6: return _limitedLipsync.amplitude6;
                case 7: return _limitedLipsync.amplitude7;
                case 8: return _limitedLipsync.amplitude8;
                case 9: return _limitedLipsync.amplitude9;
                case 10: return _limitedLipsync.amplitude10;
                case 11: return _limitedLipsync.amplitude11;
                case 12: return _limitedLipsync.amplitude12;
                case 13: return _limitedLipsync.amplitude13;
                case 14: return _limitedLipsync.amplitude14;
                default: throw new IndexOutOfRangeException();
            }
        }

        private void OnClipRendered(EeRenderingSample obj)
        {
            _onClipRenderedFn.Invoke();
        }

        public Texture TextureForViseme(int visemeNumber)
        {
            return _visemePreviews[visemeNumber].RenderTexture;
        }
    }
}
