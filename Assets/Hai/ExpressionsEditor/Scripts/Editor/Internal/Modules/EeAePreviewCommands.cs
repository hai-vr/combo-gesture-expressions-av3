namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EePreviewCommands
    {
        private readonly EeAnimationEditorState _state;
        private readonly EePreviewHandler _previewHandler;

        public EePreviewCommands(EeAnimationEditorState state, EePreviewHandler previewHandler)
        {
            _state = state;
            _previewHandler = previewHandler;
        }

        public void RequestExplorerBlendshapes()
        {
            if (!_state.InternalDummyOptional().HasValue) return;

            _state.Altered(_previewHandler).ExplorerBlendshapesRequested();
        }

        public void SetForcePreviewGeneration(EeAnimationEditorScenePreviewMode scenePreviewMode)
        {
            _state.Altered(_previewHandler).PreviewGenerationForced(scenePreviewMode);
        }

        public void ToggleMaintainPreview()
        {
            _state.Altered(_previewHandler).MaintainPreviewToggled();
        }
    }
}
