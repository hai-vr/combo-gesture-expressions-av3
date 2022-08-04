using Hai.ComboGesture.Scripts.Components;
using UnityEditor;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureFaceTracking))]
    public class ComboGestureSensorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}