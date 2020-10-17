using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeWindowHandler
    {
        private readonly CgeEditorWindow _window;
        private readonly CgeEditorEffector _editorEffector;

        public CgeWindowHandler(CgeEditorWindow window, CgeEditorEffector editorEffector)
        {
            _window = window;
            _editorEffector = editorEffector;
        }

        public static CgeWindowHandler Obtain()
        {
            var editor = EditorWindow.GetWindow<CgeEditorWindow>();
            return editor.WindowHandler;
        }

        public void ShowActivity(ComboGestureActivity activity)
        {
            RetargetActivity(activity);
            _window.Show();
        }

        public void RetargetActivity(ComboGestureActivity activity)
        {
            _editorEffector.SetActivity(activity);
            if (!_editorEffector.IsPreviewSetupValid())
            {
                _editorEffector.SwitchTo(EditorMode.OtherOptions);
                _editorEffector.MarkFirstTimeSetup();
            }

            _window.titleContent = new GUIContent("CGE/" + activity.name);
        }
    }
}
