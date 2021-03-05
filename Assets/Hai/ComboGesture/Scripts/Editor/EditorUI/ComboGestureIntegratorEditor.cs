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
        private Texture _guideIcon32;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            _guideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button(new GUIContent(CgeLocale.CGEI_Documentation, _guideIcon32)))
            {
                Application.OpenURL(CgeLocale.IntegratorDocumentationUrl());
            }

            var italic = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Italic};

            EditorGUILayout.LabelField(CgeLocale.CGEI_BackupAnimator, italic);
            EditorGUILayout.PropertyField(animatorController, new GUIContent(CgeLocale.CGEI_Animator_Controller));

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(ThereIsNoAnimatorController());

            bool ThereIsNoAnimatorController()
            {
                return animatorController.objectReferenceValue == null;
            }

            if (GUILayout.Button(CgeLocale.CGEI_Synchronize_Animator_layers))
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
