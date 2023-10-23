using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeWindowHandler
    {
        private readonly CgeEditorWindow _window;
        private readonly CgeEditorHandler _editorHandler;

        public CgeWindowHandler(CgeEditorWindow window, CgeEditorHandler editorHandler)
        {
            _window = window;
            _editorHandler = editorHandler;
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
            _editorHandler.NowEditingActivity(activity);

            _window.titleContent = new GUIContent("CGE/" + activity.name);
        }

        public void ShowPuppet(ComboGesturePuppet puppet)
        {
            RetargetPuppet(puppet);
            _window.Show();
        }

        public void RetargetPuppet(ComboGesturePuppet puppet)
        {
            _editorHandler.NowEditingPuppet(puppet);

            _window.titleContent = new GUIContent("CGE/" + puppet.name);
        }
    }
}
