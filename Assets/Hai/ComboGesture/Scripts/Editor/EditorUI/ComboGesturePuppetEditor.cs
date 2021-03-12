using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGesturePuppet))]
    [CanEditMultipleObjects]
    public class ComboGesturePuppetEditor : UnityEditor.Editor
    {
        public SerializedProperty mainTree;
        public SerializedProperty intent;

        public SerializedProperty transitionDuration;

        public SerializedProperty previewSetup;
        public SerializedProperty editorLegacyFoldout;

        public ReorderableList blinkingReorderableList;
        public ReorderableList limitedLipsyncReorderableList;

        private void OnEnable()
        {
            transitionDuration = serializedObject.FindProperty("transitionDuration");
            mainTree = serializedObject.FindProperty("mainTree");
            intent = serializedObject.FindProperty("intent");
            previewSetup = serializedObject.FindProperty("previewSetup");
            editorLegacyFoldout = serializedObject.FindProperty("editorLegacyFoldout");

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            blinkingReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("blinking"),
                true, true, true, true
            );
            blinkingReorderableList.drawElementCallback = BlinkingListElement;
            blinkingReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Closed eyes Animations (to disable blinking)");

            limitedLipsyncReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("limitedLipsync"),
                true, true, true, true
            );
            limitedLipsyncReorderableList.drawElementCallback = LipsyncListElement;
            limitedLipsyncReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Limited Lipsync Animations (to reduce speaking mouth movements)");
        }

        private void BlinkingListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = blinkingReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                GUIContent.none
            );
        }

        private void LipsyncListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = limitedLipsyncReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width - 200, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("clip"),
                GUIContent.none
            );

            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - 200, rect.y, 180, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("limitation"),
                GUIContent.none
            );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (mainTree.objectReferenceValue != null && mainTree.objectReferenceValue is AnimationClip)
            {
                mainTree.objectReferenceValue = null;
            }

            if (GUILayout.Button("Switch language (English / 日本語)"))
            {
                CgeLocalization.CycleLocale();
            }

            if (CgeLocalization.IsEnglishLocaleActive())
            {
                EditorGUILayout.LabelField("");
            }
            else
            {
                EditorGUILayout.LabelField("一部の翻訳は正確ではありません。cge.jp.jsonを編集することができます。");
            }

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Open_editor), GUILayout.Height(40)))
            {
                CgeWindowHandler.Obtain().ShowPuppet((ComboGesturePuppet)serializedObject.targetObject);
            }

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Editor window is not available in multi-editing.", MessageType.Info);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Separator();

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
            EditorGUILayout.PropertyField(mainTree, new GUIContent("Main BlendTree"));
            EditorGUILayout.PropertyField(intent, new GUIContent("Intent"));

            EditorGUILayout.Separator();

            blinkingReorderableList.DoLayoutList();

            limitedLipsyncReorderableList.DoLayoutList();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(transitionDuration, new GUIContent("Transition duration (s)"));

            if (serializedObject.isEditingMultipleObjects) {
                EditorGUILayout.PropertyField(previewSetup, new GUIContent("Preview setup"));
            }
        }
    }
}
