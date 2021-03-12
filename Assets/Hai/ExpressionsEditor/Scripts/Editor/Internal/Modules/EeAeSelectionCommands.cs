using Hai.ExpressionsEditor.Scripts.Components;
using Hai.ExpressionsEditor.Scripts.Editor.EditorUI.EditorWindows;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EeSelectionCommands
    {
        private readonly EeAnimationEditorState _state;
        private readonly EePreviewHandler _previewHandler;

        public EeSelectionCommands(EeAnimationEditorState state, EePreviewHandler previewHandler)
        {
            _state = state;
            _previewHandler = previewHandler;
        }

        public void SelectDummy(ExpressionEditorPreviewable dummy)
        {
            if (!dummy.IsValid()) return;

            _state.Altered(_previewHandler).DummySelected(dummy);
        }

        public void SelectCurrentClip(AnimationClip activeNonNull)
        {
            if (_state.CurrentClip == activeNonNull) return;

            _state.Altered(_previewHandler).NewClipSelected(activeNonNull);

            // FIXME: Backward dependency, this should be a listener
            EeAnimationEditorWindow.Obtain().OnNewClipSelected(_state.CurrentClip);
        }

        public void ForgetDummy()
        {
            _state.Altered(_previewHandler).DummyForgotten();
        }

        public void SelectCamera(int selectedPreviewCamera)
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue || selectedPreviewCamera >= dummy.Value.Cameras.Count) return;

            _state.Altered(_previewHandler).CameraSelected(selectedPreviewCamera);
        }
    }
}
