using System;
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

        public SerializedProperty doNotGenerateControllerLayer;
        public SerializedProperty doNotGenerateBlinkingOverrideLayer;

        public SerializedProperty conflictPreventionMode;
        public SerializedProperty conflictFxLayerMode;
        public SerializedProperty ignoreParamList;
        public SerializedProperty fallbackParamList;

        public SerializedProperty editorAdvancedFoldout;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            activityStageName = serializedObject.FindProperty("activityStageName");
            customEmptyClip = serializedObject.FindProperty("customEmptyClip");
            analogBlinkingUpperThreshold = serializedObject.FindProperty("analogBlinkingUpperThreshold");

            exposeDisableExpressions = serializedObject.FindProperty("exposeDisableExpressions");
            exposeDisableBlinkingOverride = serializedObject.FindProperty("exposeDisableBlinkingOverride");
            exposeAreEyesClosed = serializedObject.FindProperty("exposeAreEyesClosed");

            doNotGenerateControllerLayer = serializedObject.FindProperty("doNotGenerateControllerLayer");
            doNotGenerateBlinkingOverrideLayer = serializedObject.FindProperty("doNotGenerateBlinkingOverrideLayer");

            conflictPreventionMode = serializedObject.FindProperty("conflictPreventionMode");
            conflictFxLayerMode = serializedObject.FindProperty("conflictFxLayerMode");
            ignoreParamList = serializedObject.FindProperty("ignoreParamList");
            fallbackParamList = serializedObject.FindProperty("fallbackParamList");

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

            editorAdvancedFoldout = serializedObject.FindProperty("editorAdvancedFoldout");
        }

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

            var compiler = AsCompiler();
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
                return compiler.comboLayers.Count == 1 && compiler.comboLayers[0].activity == null;
            }

            bool ThereIsNoActivityNameForMultipleActivities()
            {
                return comboLayers.arraySize >= 2 && (activityStageName.stringValue == null || activityStageName.stringValue.Trim() == "");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Synchronize Animator FX GestureCombo layers"))
            {
                DoGenerate();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            editorAdvancedFoldout.boolValue = EditorGUILayout.Foldout(editorAdvancedFoldout.boolValue, "Advanced");
            if (editorAdvancedFoldout.boolValue)
            {
                EditorGUILayout.LabelField("Fine tuning", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(customEmptyClip, new GUIContent("Custom 2-frame empty animation clip (optional)"));
                EditorGUILayout.PropertyField(analogBlinkingUpperThreshold, new GUIContent("Analog fist blinking threshold", "(0: Eyes are open, 1: Eyes are closed)"));

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Internal parameters", EditorStyles.boldLabel);
                if (GUILayout.Button(new GUIContent("Open advanced guide", _guideIcon16)))
                {
                    Application.OpenURL("https://github.com/hai-vr/combo-gesture-expressions-av3/blob/main/GUIDE_internal_parameters.md");
                }
                EditorGUILayout.PropertyField(exposeDisableExpressions, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboDisableExpressionsParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeDisableBlinkingOverride, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboDisableBlinkingOverrideParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeAreEyesClosed, new GUIContent("Expose " + ComboGestureCompilerInternal.HaiGestureComboAreEyesClosed.Substring("_Hai_GestureCombo".Length)));

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Layer generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(doNotGenerateControllerLayer, new GUIContent("Don't generate Controller layer"));
                GenControllerWarning(true);
                EditorGUILayout.PropertyField(doNotGenerateBlinkingOverrideLayer, new GUIContent("Don't generate Blinking layer"));
                GenBlinkingWarning(true);

                EditorGUILayout.LabelField("Animation Conflict Prevention", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(conflictPreventionMode, new GUIContent("Mode"));

                CpmValueWarning(true);

                var conflictPrevention = ConflictPrevention.Of((ConflictPreventionMode) conflictPreventionMode.intValue);
                EditorGUI.BeginDisabledGroup(!conflictPrevention.ShouldGenerateAnimations);
                EditorGUILayout.PropertyField(conflictFxLayerMode, new GUIContent("Transforms removal"));
                EditorGUI.EndDisabledGroup();

                CpmRemovalWarning(true);
                EditorGUILayout.Separator();

                EditorGUI.BeginDisabledGroup(!conflictPrevention.ShouldGenerateAnimations);
                EditorGUILayout.LabelField("Fallback generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ignoreParamList, new GUIContent("Ignored properties"));
                EditorGUILayout.PropertyField(fallbackParamList, new GUIContent("Fallback values"));
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                GenControllerWarning(false);
                GenBlinkingWarning(false);
                CpmValueWarning(false);
                CpmRemovalWarning(false);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenControllerWarning(bool advancedFoldoutIsOpen)
        {
            if (doNotGenerateControllerLayer.boolValue)
            {
                EditorGUILayout.HelpBox(@"Controller layer should usually be generated, otherwise it may not be updated correctly on future updates of ComboGestureExpressions.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
            }
        }

        private void GenBlinkingWarning(bool advancedFoldoutIsOpen)
        {
            if (doNotGenerateBlinkingOverrideLayer.boolValue)
            {
                EditorGUILayout.HelpBox(@"Blinking Override layer should usually be generated as it depends on all the activities of the compiler.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
            }
        }

        private void CpmValueWarning(bool advancedFoldoutIsOpen)
        {
            var conflictPrevention = ConflictPrevention.Of((ConflictPreventionMode) conflictPreventionMode.intValue);
            if (!conflictPrevention.ShouldGenerateAnimations)
            {
                    EditorGUILayout.HelpBox(@"Using ""Only Write Defaults"" Mode will cause face expressions to conflict if your FX layer does not use ""Write Defaults"" on all layers. 
If in doubt, ""Use Recommended Configuration"" Mode instead." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
            }
            else
            {
                if (advancedFoldoutIsOpen) {
                    EditorGUILayout.HelpBox(@"Animations will be generated in a way that will prevent conflicts between face expressions.
Whenever an animation is modified, you will need to click Synchronize again.", MessageType.Info);
                }
                if (!conflictPrevention.ShouldWriteDefaults) {
                    EditorGUILayout.HelpBox(@"Animations will be generated in a way that will prevent conflicts between face expressions.

However, the states will have ""Write Defaults"" to OFF.
Using ""Generate Animations With Write Defaults"" is generally more compatible.

If you never use ""Write Defaults"" in your FX animator, this should not matter." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Warning);
                }
            }
        }

        private void CpmRemovalWarning(bool advancedFoldoutIsOpen)
        {
            var conflictPrevention = ConflictPrevention.Of((ConflictPreventionMode) conflictPreventionMode.intValue);
            if (conflictPrevention.ShouldGenerateAnimations)
            {
                switch ((ConflictFxLayerMode) conflictFxLayerMode.intValue)
                {
                    case ConflictFxLayerMode.RemoveTransformsAndMuscles:
                        if (advancedFoldoutIsOpen)
                        {
                            EditorGUILayout.HelpBox(@"Transforms and muscles will be removed.
This is the default behavior.", MessageType.Info);
                        }
                        break;
                    case ConflictFxLayerMode.KeepBoth:
                            EditorGUILayout.HelpBox(@"Transforms and muscles will not be removed, but the FX Playable layer is not meant to manipulate them.
Not removing them might cause conflicts with other animations." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Warning);
                        break;
                    case ConflictFxLayerMode.KeepOnlyTransformsAndMuscles:
                        EditorGUILayout.HelpBox(@"Everything will be removed except transforms and muscle animations.
However, the FX Playable layer is not meant to manipulate them.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DoGenerate()
        {
            var compiler = AsCompiler();
            new ComboGestureCompilerInternal(
                compiler.activityStageName,
                compiler.comboLayers,
                compiler.animatorController,
                compiler.customEmptyClip,
                compiler.analogBlinkingUpperThreshold,
                (exposeDisableExpressions.boolValue ? FeatureToggles.ExposeDisableExpressions : 0)
                | (exposeDisableBlinkingOverride.boolValue ? FeatureToggles.ExposeDisableBlinkingOverride : 0)
                | (exposeAreEyesClosed.boolValue ? FeatureToggles.ExposeAreEyesClosed : 0)
                | (doNotGenerateControllerLayer.boolValue ? FeatureToggles.DoNotGenerateControllerLayer : 0)
                | (doNotGenerateBlinkingOverrideLayer.boolValue ? FeatureToggles.DoNotGenerateBlinkingOverrideLayer : 0)
                ,
                compiler.conflictPreventionMode,
                compiler.conflictFxLayerMode,
                compiler.ignoreParamList,
                compiler.fallbackParamList
            ).DoOverwriteAnimatorFxLayer();
        }

        private ComboGestureCompiler AsCompiler()
        {
            return (ComboGestureCompiler) target;
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
