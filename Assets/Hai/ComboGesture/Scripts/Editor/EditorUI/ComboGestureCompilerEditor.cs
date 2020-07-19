using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureCompiler))]
    public class ComboGestureCompilerEditor : UnityEditor.Editor
    {
        public ReorderableList comboLayersReorderableList;
        public SerializedProperty comboLayers;
        public SerializedProperty animatorController;
        public SerializedProperty activityStageName;
        public SerializedProperty customEmptyClip;
        public SerializedProperty analogBlinkingUpperThreshold;
        
        public SerializedProperty exposeDisableExpressions;
        public SerializedProperty exposeDisableBlinkingOverride;
        public SerializedProperty exposeAreEyesClosed;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            activityStageName = serializedObject.FindProperty("activityStageName");
            customEmptyClip = serializedObject.FindProperty("customEmptyClip");
            analogBlinkingUpperThreshold = serializedObject.FindProperty("analogBlinkingUpperThreshold");
            
            exposeDisableExpressions = serializedObject.FindProperty("exposeDisableExpressions");
            exposeDisableBlinkingOverride = serializedObject.FindProperty("exposeDisableBlinkingOverride");
            exposeAreEyesClosed = serializedObject.FindProperty("exposeAreEyesClosed");
            
            comboLayers = serializedObject.FindProperty("comboLayers");
        
            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            comboLayersReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("comboLayers"),
                true, true, true, true
            );
            comboLayersReorderableList.drawElementCallback = ComboLayersListElement;
            comboLayersReorderableList.drawHeaderCallback = ComboLayersListHeader;
                
            _guideIcon16 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-16.png");
            _guideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");
        }
    
        private bool _foldoutAdvanced;
        private bool _foldoutHelp;
        private Texture _guideIcon16;
        private Texture _guideIcon32;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _foldoutHelp = EditorGUILayout.Foldout(_foldoutHelp, new GUIContent("Help", _guideIcon32));
            if (_foldoutHelp)
            {
                if (GUILayout.Button(new GUIContent("Open guide", _guideIcon32)))
                {
                    Application.OpenURL("https://github.com/hai-vr/combo-gesture-expressions-av3#combo-gesture-compiler");
                }
            }

            EditorGUILayout.PropertyField(animatorController, new GUIContent("FX Animator Controller to overwrite"));
            EditorGUILayout.PropertyField(activityStageName, new GUIContent("Activity Stage name"));

            comboLayersReorderableList.DoLayoutList();

            var compiler = (ComboGestureCompiler) target;
            EditorGUI.BeginDisabledGroup(
                ThereIsNoAnimatorController() ||
                ThereIsNoActivity() ||
                TheOnlyActivityIsNull() ||
                ThereIsNoActivityNameForMultipleActivities()
            );

            bool ThereIsNoAnimatorController()
            {
                return animatorController.objectReferenceValue == null;
            }

            bool ThereIsNoActivity()
            {
                return comboLayers.arraySize == 0;
            }

            bool TheOnlyActivityIsNull()
            {
                return comboLayers.arraySize == 1 && compiler.comboLayers[0].activity == null;
            }

            bool ThereIsNoActivityNameForMultipleActivities()
            {
                return comboLayers.arraySize >= 2 && (activityStageName.stringValue == null || activityStageName.stringValue.Trim() == "");
            }

            if (GUILayout.Button("Create/Overwrite Animator FX GestureCombo layers"))
            {
                new ComboGestureCompilerInternal(
                    compiler.activityStageName,
                    compiler.comboLayers,
                    compiler.animatorController,
                    compiler.customEmptyClip,
                    compiler.analogBlinkingUpperThreshold,
                    (exposeDisableExpressions.boolValue ? FeatureToggles.ExposeDisableExpressions : 0)
                    | (exposeDisableBlinkingOverride.boolValue ? FeatureToggles.ExposeDisableBlinkingOverride : 0)
                    | (exposeAreEyesClosed.boolValue ? FeatureToggles.ExposeAreEyesClosed : 0)
                ).DoOverwriteAnimatorFxLayer();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
        
            _foldoutAdvanced = EditorGUILayout.Foldout(_foldoutAdvanced, "Advanced");
            if (_foldoutAdvanced)
            {
                EditorGUILayout.LabelField("Fine tuning", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(customEmptyClip, new GUIContent("Custom 2-frame empty animation clip (optional)"));
                EditorGUILayout.PropertyField(analogBlinkingUpperThreshold, new GUIContent("Analog fist blinking threshold", "(0: Eyes are open, 1: Eyes are closed)"));
                
                EditorGUILayout.LabelField("Internal parameters", EditorStyles.boldLabel);
                if (GUILayout.Button(new GUIContent("Open advanced guide", _guideIcon16)))
                {
                    Application.OpenURL("https://github.com/hai-vr/combo-gesture-expressions-av3/blob/main/GUIDE_internal_parameters.md");
                }
                EditorGUILayout.PropertyField(exposeDisableExpressions, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboDisableExpressionsParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeDisableBlinkingOverride, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboDisableBlinkingOverrideParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeAreEyesClosed, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboAreEyesClosed.Substring("_Hai_GestureCombo".Length)));
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void ComboLayersListElement(Rect rect, int index, bool isActive, bool isFocused)
        {        
            var element = comboLayersReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), 
                element.FindPropertyRelative("activity"),
                GUIContent.none
            ); 

            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - 70, rect.y, 50, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("stageValue"),
                GUIContent.none
            );   
        }

        private static void ComboLayersListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Gesture Combo Activities");
        }
    }
}
