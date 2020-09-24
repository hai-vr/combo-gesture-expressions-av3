using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureLimitedLipsync))]
    [CanEditMultipleObjects]
    public class ComboGestureLimitedLipsyncEditor : UnityEditor.Editor
    {
        public SerializedProperty limitation;
        public SerializedProperty amplitudeScale;
        public SerializedProperty amplitude0;
        public SerializedProperty amplitude1;
        public SerializedProperty amplitude2;
        public SerializedProperty amplitude3;
        public SerializedProperty amplitude4;
        public SerializedProperty amplitude5;
        public SerializedProperty amplitude6;
        public SerializedProperty amplitude7;
        public SerializedProperty amplitude8;
        public SerializedProperty amplitude9;
        public SerializedProperty amplitude10;
        public SerializedProperty amplitude11;
        public SerializedProperty amplitude12;
        public SerializedProperty amplitude13;
        public SerializedProperty amplitude14;
        public SerializedProperty transitionDuration;
        public SerializedProperty transition0;
        public SerializedProperty transition1;
        public SerializedProperty transition2;
        public SerializedProperty transition3;
        public SerializedProperty transition4;
        public SerializedProperty transition5;
        public SerializedProperty transition6;
        public SerializedProperty transition7;
        public SerializedProperty transition8;
        public SerializedProperty transition9;
        public SerializedProperty transition10;
        public SerializedProperty transition11;
        public SerializedProperty transition12;
        public SerializedProperty transition13;
        public SerializedProperty transition14;
        public SerializedProperty editorLegacyFoldout;

        private void OnEnable()
        {
            limitation = serializedObject.FindProperty("limitation");
            amplitudeScale = serializedObject.FindProperty("amplitudeScale");
            amplitude0 = serializedObject.FindProperty("amplitude0");
            amplitude1 = serializedObject.FindProperty("amplitude1");
            amplitude2 = serializedObject.FindProperty("amplitude2");
            amplitude3 = serializedObject.FindProperty("amplitude3");
            amplitude4 = serializedObject.FindProperty("amplitude4");
            amplitude5 = serializedObject.FindProperty("amplitude5");
            amplitude6 = serializedObject.FindProperty("amplitude6");
            amplitude7 = serializedObject.FindProperty("amplitude7");
            amplitude8 = serializedObject.FindProperty("amplitude8");
            amplitude9 = serializedObject.FindProperty("amplitude9");
            amplitude10 = serializedObject.FindProperty("amplitude10");
            amplitude11 = serializedObject.FindProperty("amplitude11");
            amplitude12 = serializedObject.FindProperty("amplitude12");
            amplitude13 = serializedObject.FindProperty("amplitude13");
            amplitude14 = serializedObject.FindProperty("amplitude14");
            transitionDuration = serializedObject.FindProperty("transitionDuration");
            transition0 = serializedObject.FindProperty("transition0");
            transition1 = serializedObject.FindProperty("transition1");
            transition2 = serializedObject.FindProperty("transition2");
            transition3 = serializedObject.FindProperty("transition3");
            transition4 = serializedObject.FindProperty("transition4");
            transition5 = serializedObject.FindProperty("transition5");
            transition6 = serializedObject.FindProperty("transition6");
            transition7 = serializedObject.FindProperty("transition7");
            transition8 = serializedObject.FindProperty("transition8");
            transition9 = serializedObject.FindProperty("transition9");
            transition10 = serializedObject.FindProperty("transition10");
            transition11 = serializedObject.FindProperty("transition11");
            transition12 = serializedObject.FindProperty("transition12");
            transition13 = serializedObject.FindProperty("transition13");
            transition14 = serializedObject.FindProperty("transition14");
            editorLegacyFoldout = serializedObject.FindProperty("editorLegacyFoldout");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!serializedObject.isEditingMultipleObjects) {
                editorLegacyFoldout.boolValue = EditorGUILayout.Foldout(editorLegacyFoldout.boolValue, "Legacy editor");
            }
            else
            {
                GUILayout.Label("Legacy editor / Multi editing", EditorStyles.boldLabel);
            }

            if (serializedObject.isEditingMultipleObjects || editorLegacyFoldout.boolValue)
            {
                LegacyEditor();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LegacyEditor()
        {
            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(limitation, new GUIContent("Category"));
            EditorGUILayout.Slider(amplitudeScale, 0f, 0.25f, "Viseme Amplitude");
            EditorGUILayout.Slider(amplitudeScale, 0f, 1f, "(scaled to 1)");
            EditorGUILayout.PropertyField(transitionDuration, new GUIContent("Transition Duration (s)"));
            GUILayout.Label("Viseme Amplitude tweaks", EditorStyles.boldLabel);
            EditorGUILayout.Slider(amplitude0, 0f, 1f, "Amplitude 0: sil");
            EditorGUILayout.Slider(amplitude1, 0f, 1f, "Amplitude 1: PP");
            EditorGUILayout.Slider(amplitude2, 0f, 1f, "Amplitude 2: FF");
            EditorGUILayout.Slider(amplitude3, 0f, 1f, "Amplitude 3: TH");
            EditorGUILayout.Slider(amplitude4, 0f, 1f, "Amplitude 4: DD");
            EditorGUILayout.Slider(amplitude5, 0f, 1f, "Amplitude 5: kk");
            EditorGUILayout.Slider(amplitude6, 0f, 1f, "Amplitude 6: CH");
            EditorGUILayout.Slider(amplitude7, 0f, 1f, "Amplitude 7: SS");
            EditorGUILayout.Slider(amplitude8, 0f, 1f, "Amplitude 8: nn");
            EditorGUILayout.Slider(amplitude9, 0f, 1f, "Amplitude 9: RR");
            EditorGUILayout.Slider(amplitude10, 0f, 1f, "Amplitude 10: aa");
            EditorGUILayout.Slider(amplitude11, 0f, 1f, "Amplitude 11: E");
            EditorGUILayout.Slider(amplitude12, 0f, 1f, "Amplitude 12: ih");
            EditorGUILayout.Slider(amplitude13, 0f, 1f, "Amplitude 13: oh");
            EditorGUILayout.Slider(amplitude14, 0f, 1f, "Amplitude 14: ou");
            GUILayout.Label("Transition duration tweaks", EditorStyles.boldLabel);
            EditorGUILayout.Slider(transition0, 0f, 1f, "Transition 0: sil");
            EditorGUILayout.Slider(transition1, 0f, 1f, "Transition 1: PP");
            EditorGUILayout.Slider(transition2, 0f, 1f, "Transition 2: FF");
            EditorGUILayout.Slider(transition3, 0f, 1f, "Transition 3: TH");
            EditorGUILayout.Slider(transition4, 0f, 1f, "Transition 4: DD");
            EditorGUILayout.Slider(transition5, 0f, 1f, "Transition 5: kk");
            EditorGUILayout.Slider(transition6, 0f, 1f, "Transition 6: CH");
            EditorGUILayout.Slider(transition7, 0f, 1f, "Transition 7: SS");
            EditorGUILayout.Slider(transition8, 0f, 1f, "Transition 8: nn");
            EditorGUILayout.Slider(transition9, 0f, 1f, "Transition 9: RR");
            EditorGUILayout.Slider(transition10, 0f, 1f, "Transition 10: aa");
            EditorGUILayout.Slider(transition11, 0f, 1f, "Transition 11: E");
            EditorGUILayout.Slider(transition12, 0f, 1f, "Transition 12: ih");
            EditorGUILayout.Slider(transition13, 0f, 1f, "Transition 13: oh");
            EditorGUILayout.Slider(transition14, 0f, 1f, "Transition 14: ou");
        }
    }
}
