using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureIntegrator))]
    public class ComboGestureIntegratorEditor : UnityEditor.Editor
    {
        public SerializedProperty animatorController;
        private SerializedProperty writeDefaults;

        private Texture _guideIcon32;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            writeDefaults = serializedObject.FindProperty("writeDefaults");
            _guideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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

            if (GUILayout.Button(new GUIContent(CgeLocale.CGEI_Documentation, _guideIcon32)))
            {
                Application.OpenURL(CgeLocale.IntegratorDocumentationUrl());
            }

            var italic = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Italic};

            EditorGUILayout.LabelField(CgeLocale.CGEI_BackupAnimator, italic);
            EditorGUILayout.PropertyField(animatorController, new GUIContent(CgeLocale.CGEI_Animator_Controller));
            EditorGUILayout.PropertyField(writeDefaults, new GUIContent("Write Defaults"));

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(ThereIsNoAnimatorController());

            bool ThereIsNoAnimatorController()
            {
                return animatorController.objectReferenceValue == null;
            }

            if (GUILayout.Button(CgeLocale.CGEI_Synchronize_Animator_layers, GUILayout.Height(40)))
            {
                DoGenerate();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(CgeLocale.CGEI_Info, MessageType.Info);
            EditorGUILayout.TextField("GestureLeftWeight", "_Hai_GestureLWSmoothing");
            EditorGUILayout.TextField("GestureRightWeight", "_Hai_GestureRWSmoothing");

            serializedObject.ApplyModifiedProperties();
        }

        private void DoGenerate()
        {
            var integrator = AsIntegrator();

            new ComboGestureCompilerInternal(integrator).IntegrateWeightCorrection();
        }

        private ComboGestureIntegrator AsIntegrator()
        {
            return (ComboGestureIntegrator) target;
        }
    }
}
