using Hai.ComboGesture.Scripts.Components;
using UnityEditor;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeEditorState
    {
        public ComboGestureActivity Activity { get; internal set; }
        internal SerializedObject SerializedObject;

        public EditorMode Mode { get; internal set; }
        public int CurrentEditorToolValue = -1;

        public bool FirstTimeSetup;
        public AutoSetupPreview.SetupResult? SetupResult;
    }

    public class CgeEditorEffector
    {
        private readonly CgeEditorState _state;

        public CgeEditorEffector(CgeEditorState state)
        {
            _state = state;
        }

        public SerializedProperty SpTransitionDuration() => _state.SerializedObject.FindProperty("transitionDuration");
        public SerializedProperty SpEditorTool() => _state.SerializedObject.FindProperty("editorTool");
        public SerializedProperty SpEditorArbitraryAnimations() => _state.SerializedObject.FindProperty("editorArbitraryAnimations");
        public SerializedProperty SpPreviewSetup() => _state.SerializedObject.FindProperty("previewSetup");
        public SerializedProperty SpEnablePermutations() => _state.SerializedObject.FindProperty("enablePermutations");

        public void SetActivity(ComboGestureActivity selectedActivity)
        {
            _state.Activity = selectedActivity;
            _state.SerializedObject = new SerializedObject(_state.Activity);

            if (SpEditorTool().intValue != _state.CurrentEditorToolValue && _state.CurrentEditorToolValue >= 0)
            {
                SpEditorTool().intValue = _state.CurrentEditorToolValue;
                ApplyModifiedProperties();
            }
        }

        public SerializedProperty SpProperty(string property)
        {
            return _state.SerializedObject.FindProperty(property);
        }

        public void SpUpdate()
        {
            _state.SerializedObject.Update();
        }

        public void ApplyModifiedProperties()
        {
            _state.SerializedObject.ApplyModifiedProperties();
        }

        public bool IsPreviewSetupValid()
        {
            return _state.Activity.previewSetup != null && _state.Activity.previewSetup.IsValid();
        }

        public void SwitchTo(EditorMode mode)
        {
            _state.Mode = mode;
        }

        public EditorMode CurrentMode()
        {
            return _state.Mode;
        }

        public ComboGestureActivity GetActivity()
        {
            return _state.Activity;
        }

        public bool IsFirstTimeSetup()
        {
            return _state.FirstTimeSetup;
        }

        public AutoSetupPreview.SetupResult? GetSetupResult()
        {
            return _state.SetupResult;
        }

        public void TryAutoSetup()
        {
            _state.SetupResult = new AutoSetupPreview(_state.Activity).AutoSetup();
        }

        public void MarkFirstTimeSetup()
        {
            _state.FirstTimeSetup = true;
        }

        public void ClearFirstTimeSetup()
        {
            _state.FirstTimeSetup = false;
        }

        public void SwitchCurrentEditorToolTo(int value)
        {
            _state.CurrentEditorToolValue = value;
        }
    }
}
