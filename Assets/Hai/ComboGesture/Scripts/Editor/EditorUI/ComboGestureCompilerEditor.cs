﻿using Hai.ComboGesture.Scripts.Components;
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

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            activityStageName = serializedObject.FindProperty("activityStageName");
            customEmptyClip = serializedObject.FindProperty("customEmptyClip");
            comboLayers = serializedObject.FindProperty("comboLayers");
        
            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            comboLayersReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("comboLayers"),
                true, true, true, true
            );
            comboLayersReorderableList.drawElementCallback = ComboLayersListElement;
            comboLayersReorderableList.drawHeaderCallback = ComboLayersListHeader;
        }
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(animatorController, new GUIContent("FX Animator Controller to overwrite"));
            EditorGUILayout.PropertyField(activityStageName, new GUIContent("Activity Stage name"));
            EditorGUILayout.PropertyField(customEmptyClip, new GUIContent("Custom 2-frame empty animation clip (optional)"));
            comboLayersReorderableList.DoLayoutList();
        
            EditorGUI.BeginDisabledGroup(
                animatorController.objectReferenceValue == null ||
                (activityStageName.stringValue == null || activityStageName.stringValue.Trim() == "") && comboLayers.arraySize >= 2 ||
                comboLayers.arraySize == 0
            );
            if (GUILayout.Button("Create/Overwrite Animator FX GestureCombo layers"))
            {
                var compiler = ((ComboGestureCompiler) target);
                new ComboGestureCompilerInternal(
                    compiler.activityStageName,
                    compiler.comboLayers,
                    compiler.animatorController,
                    compiler.customEmptyClip
                ).DoOverwriteAnimatorFxLayer();
            }
            EditorGUI.EndDisabledGroup();
        
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