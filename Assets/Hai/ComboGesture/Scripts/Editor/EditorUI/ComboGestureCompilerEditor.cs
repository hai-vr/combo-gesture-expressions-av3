using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureCompiler))]
    public class ComboGestureCompilerEditor : UnityEditor.Editor
    {
        public ReorderableList comboLayersReorderableList;
        public SerializedProperty comboLayers;
        public SerializedProperty animatorController;
        public SerializedProperty useGesturePlayableLayer;
        public SerializedProperty gesturePlayableLayerController;
        public SerializedProperty activityStageName;
        public SerializedProperty customEmptyClip;
        public SerializedProperty analogBlinkingUpperThreshold;
        public SerializedProperty parameterMode;

        public SerializedProperty lipsyncForWideOpenMouth;

        public SerializedProperty expressionsAvatarMask;
        public SerializedProperty logicalAvatarMask;
        public SerializedProperty weightCorrectionAvatarMask;
        public SerializedProperty gesturePlayableLayerExpressionsAvatarMask;
        public SerializedProperty gesturePlayableLayerTechnicalAvatarMask;
        public SerializedProperty doNotGenerateBlinkingOverrideLayer;
        public SerializedProperty doNotGenerateLipsyncOverrideLayer;
        public SerializedProperty doNotGenerateWeightCorrectionLayer;

        public SerializedProperty writeDefaultsRecommendationMode;
        public SerializedProperty writeDefaultsRecommendationModeGesture;
        public SerializedProperty gestureLayerTransformCapture;
        public SerializedProperty generatedAvatarMask;
        public SerializedProperty conflictFxLayerMode;
        public SerializedProperty weightCorrectionMode;
        public SerializedProperty ignoreParamList;
        public SerializedProperty fallbackParamList;
        public SerializedProperty folderToGenerateNeutralizedAssetsIn;

        public SerializedProperty avatarDescriptor;
        public SerializedProperty doNotFixSingleKeyframes;
        public SerializedProperty bypassMandatoryAvatarDescriptor;

        public SerializedProperty assetContainer;
        public SerializedProperty generateNewContainerEveryTime;

        public SerializedProperty editorAdvancedFoldout;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty("animatorController");
            useGesturePlayableLayer = serializedObject.FindProperty("useGesturePlayableLayer");
            gesturePlayableLayerController = serializedObject.FindProperty("gesturePlayableLayerController");
            activityStageName = serializedObject.FindProperty("activityStageName");
            customEmptyClip = serializedObject.FindProperty("customEmptyClip");
            analogBlinkingUpperThreshold = serializedObject.FindProperty("analogBlinkingUpperThreshold");
            parameterMode = serializedObject.FindProperty("parameterMode");

            lipsyncForWideOpenMouth = serializedObject.FindProperty("lipsyncForWideOpenMouth");

            expressionsAvatarMask = serializedObject.FindProperty("expressionsAvatarMask");
            logicalAvatarMask = serializedObject.FindProperty("logicalAvatarMask");
            weightCorrectionAvatarMask = serializedObject.FindProperty("weightCorrectionAvatarMask");
            gesturePlayableLayerExpressionsAvatarMask = serializedObject.FindProperty("gesturePlayableLayerExpressionsAvatarMask");
            gesturePlayableLayerTechnicalAvatarMask = serializedObject.FindProperty("gesturePlayableLayerTechnicalAvatarMask");
            doNotGenerateBlinkingOverrideLayer = serializedObject.FindProperty("doNotGenerateBlinkingOverrideLayer");
            doNotGenerateLipsyncOverrideLayer = serializedObject.FindProperty("doNotGenerateLipsyncOverrideLayer");
            doNotGenerateWeightCorrectionLayer = serializedObject.FindProperty("doNotGenerateWeightCorrectionLayer");

            writeDefaultsRecommendationMode = serializedObject.FindProperty("writeDefaultsRecommendationMode");
            writeDefaultsRecommendationModeGesture = serializedObject.FindProperty("writeDefaultsRecommendationModeGesture");
            gestureLayerTransformCapture = serializedObject.FindProperty("gestureLayerTransformCapture");
            generatedAvatarMask = serializedObject.FindProperty("generatedAvatarMask");
            conflictFxLayerMode = serializedObject.FindProperty("conflictFxLayerMode");
            weightCorrectionMode = serializedObject.FindProperty("weightCorrectionMode");
            ignoreParamList = serializedObject.FindProperty("ignoreParamList");
            fallbackParamList = serializedObject.FindProperty("fallbackParamList");
            folderToGenerateNeutralizedAssetsIn = serializedObject.FindProperty("folderToGenerateNeutralizedAssetsIn");

            comboLayers = serializedObject.FindProperty("comboLayers");

            avatarDescriptor = serializedObject.FindProperty("avatarDescriptor");
            doNotFixSingleKeyframes = serializedObject.FindProperty("doNotFixSingleKeyframes");
            bypassMandatoryAvatarDescriptor = serializedObject.FindProperty("bypassMandatoryAvatarDescriptor");

            assetContainer = serializedObject.FindProperty("assetContainer");
            generateNewContainerEveryTime = serializedObject.FindProperty("generateNewContainerEveryTime");

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            comboLayersReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("comboLayers"),
                true, true, true, true
            );
            comboLayersReorderableList.drawElementCallback = ComboLayersListElement;
            comboLayersReorderableList.drawHeaderCallback = ComboLayersListHeader;
            comboLayersReorderableList.onAddCallback = list =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);

                if (comboLayers.arraySize <= 1)
                {
                    return;
                }

                var previous = comboLayers.GetArrayElementAtIndex(comboLayers.arraySize - 2).FindPropertyRelative("stageValue").intValue;
                var newlyAddedElement = comboLayers.GetArrayElementAtIndex(comboLayers.arraySize - 1);
                newlyAddedElement.FindPropertyRelative("stageValue").intValue = previous + 1;
                newlyAddedElement.FindPropertyRelative("activity").objectReferenceValue = null;
                newlyAddedElement.FindPropertyRelative("puppet").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            };

            _guideIcon16 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-16.png");
            _guideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");

            editorAdvancedFoldout = serializedObject.FindProperty("editorAdvancedFoldout");
        }

        private Texture _guideIcon16;
        private Texture _guideIcon32;

        public override void OnInspectorGUI()
        {
            if (AsCompiler().comboLayers == null)
            {
                AsCompiler().comboLayers = new List<GestureComboStageMapper>();
            }
            serializedObject.Update();
            var italic = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Italic};

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

            if (GUILayout.Button(new GUIContent(CgeLocale.CGEC_Documentation_and_tutorials, _guideIcon32)))
            {
                Application.OpenURL(CgeLocale.DocumentationUrl());
            }

            EditorGUILayout.LabelField(CgeLocale.CGEC_Mood_sets, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(avatarDescriptor, new GUIContent(CgeLocale.CGEC_Avatar_descriptor));
            var compiler = AsCompiler();
            EditorGUI.BeginDisabledGroup(comboLayers.arraySize <= 1);
            EditorGUILayout.PropertyField(parameterMode, new GUIContent(CgeLocale.CGEC_Parameter_Mode));
            EditorGUI.EndDisabledGroup();
            if (compiler.parameterMode == ParameterMode.SingleInt)
            {
                EditorGUILayout.PropertyField(activityStageName, new GUIContent(CgeLocale.CGEC_Parameter_Name));
            }

            if (compiler.parameterMode == ParameterMode.SingleInt && comboLayers.arraySize < 8 && comboLayers.arraySize > 1)
            {
                EditorGUILayout.HelpBox(
                    CgeLocale.CGEC_HelpExpressionParameterOptimize,
                    MessageType.Info);
            }

            comboLayersReorderableList.DoLayoutList();

            EditorGUILayout.Separator();

            bool ThereIsAnOverlap()
            {
                if (compiler.parameterMode == ParameterMode.SingleInt)
                {
                    return compiler.comboLayers != null && comboLayers.arraySize != compiler.comboLayers.Select(mapper => mapper.stageValue).Distinct().Count();
                }
                else
                {
                    return compiler.comboLayers != null && comboLayers.arraySize != compiler.comboLayers.Select(mapper => mapper.booleanParameterName).Distinct().Count();
                }
            }

            bool MultipleBooleanNoDefault()
            {
                return compiler.parameterMode == ParameterMode.MultipleBools && compiler.comboLayers != null && compiler.comboLayers.TrueForAll(mapper => !string.IsNullOrEmpty(mapper.booleanParameterName));
            }

            bool ThereIsAPuppetWithNoBlendTree()
            {
                return compiler.comboLayers != null && compiler.comboLayers
                    .Where(mapper => mapper.kind == GestureComboStageKind.Puppet)
                    .Where(mapper => mapper.puppet != null)
                    .Any(mapper => !(mapper.puppet.mainTree is BlendTree));
            }

            bool ThereIsANullActivityOrPuppet()
            {
                return compiler.comboLayers != null && compiler.comboLayers.Any(mapper =>
                    mapper.kind == GestureComboStageKind.Activity && mapper.activity == null
                    || mapper.kind == GestureComboStageKind.Puppet && mapper.puppet == null
                );
            }

            bool ThereIsNoActivityNameForMultipleActivities()
            {
                return compiler.parameterMode == ParameterMode.SingleInt && comboLayers.arraySize >= 2 && (activityStageName.stringValue == null || activityStageName.stringValue.Trim() == "");
            }

            if (ThereIsAnOverlap())
            {
                if (compiler.parameterMode == ParameterMode.SingleInt)
                {
                    EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnValuesOverlap, MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNamesOverlap, MessageType.Error);
                }
            }
            else if (ThereIsAPuppetWithNoBlendTree())
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNoBlendTree, MessageType.Error);
            }
            else if (ThereIsNoActivityNameForMultipleActivities())
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNoActivityName, MessageType.Error);
            }
            else if (ThereIsANullActivityOrPuppet())
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNoActivity, MessageType.Warning);
            }
            else
            {
                // Good cases
                if (compiler.comboLayers != null && compiler.comboLayers.Count > 1)
                {
                    if (MultipleBooleanNoDefault())
                    {
                        EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_HelpWhenAllParameterNamesDefined, compiler.comboLayers.First().activity.name, compiler.comboLayers.First().booleanParameterName), MessageType.Info);
                    }

                    var defaultMapper = compiler.comboLayers.FirstOrDefault(mapper => mapper.booleanParameterName == "");
                    if (compiler.parameterMode == ParameterMode.MultipleBools && defaultMapper.kind == GestureComboStageKind.Activity && defaultMapper.activity != null)
                    {
                        EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_HintDefaultMood, defaultMapper.activity.name), MessageType.Info);
                    }
                    if (compiler.parameterMode == ParameterMode.MultipleBools && defaultMapper.kind == GestureComboStageKind.Puppet && defaultMapper.puppet != null)
                    {
                        EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_HintDefaultMood, defaultMapper.puppet.name), MessageType.Info);
                    }
                }
            }

            EditorGUILayout.LabelField(CgeLocale.CGEC_FX_Playable_Layer, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(CgeLocale.CGEC_BackupFX, italic);
            EditorGUILayout.PropertyField(animatorController, new GUIContent(CgeLocale.CGEC_FX_Animator_Controller));
            EditorGUILayout.PropertyField(writeDefaultsRecommendationMode, new GUIContent(CgeLocale.CGEC_FX_Playable_Mode));
            WriteDefaultsSection(writeDefaultsRecommendationMode);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(CgeLocale.CGEC_Gesture_Playable_Layer, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(CgeLocale.CGEC_Support_for_other_transforms, italic);
            EditorGUILayout.LabelField(CgeLocale.CGEC_MusclesUnsupported, italic);
            EditorGUILayout.PropertyField(useGesturePlayableLayer, new GUIContent(CgeLocale.CGEC_Gesture_playable_layer_support));
            if (useGesturePlayableLayer.boolValue)
            {
                EditorGUILayout.LabelField(CgeLocale.CGEC_BackupGesture, italic);
                EditorGUILayout.PropertyField(gesturePlayableLayerController, new GUIContent(CgeLocale.CGEC_Gesture_Animator_Controller));

                EditorGUILayout.PropertyField(writeDefaultsRecommendationModeGesture, new GUIContent(CgeLocale.CGEC_Gesture_Playable_Mode));
                EditorGUILayout.PropertyField(gestureLayerTransformCapture, new GUIContent(CgeLocale.CGEC_Capture_Transforms_Mode));
                WriteDefaultsSection(writeDefaultsRecommendationModeGesture);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(generatedAvatarMask, new GUIContent(CgeLocale.CGEC_Asset_container));
                EditorGUI.EndDisabledGroup();

                var missingMaskCount = CgeMaskApplicator.FindAllLayersMissingAMask(compiler.animatorController).Count();
                if (missingMaskCount > 0)
                {
                    EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_MissingFxMask, missingMaskCount), MessageType.Error);
                }

                EditorGUI.BeginDisabledGroup(compiler.avatarDescriptor == null || compiler.animatorController == null || missingMaskCount == 0);
                if (GUILayout.Button(CgeLocale.CGEC_Add_missing_masks))
                {
                    AddMissingMasks(compiler);
                }
                EditorGUI.EndDisabledGroup();

                if (compiler.generatedAvatarMask != null)
                {
                    EditorGUI.BeginDisabledGroup(compiler.avatarDescriptor == null || compiler.animatorController == null);
                    MaskRemovalUi(compiler);
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                if (compiler.animatorController != null && compiler.generatedAvatarMask != null)
                {
                    MaskRemovalUi(compiler);
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(CgeLocale.CGEC_Synchronization, EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(
                ThereIsNoAnimatorController() ||
                ThereIsNoGestureAnimatorController() ||
                ThereIsNoActivity() ||
                ThereIsAnOverlap() ||
                ThereIsAPuppetWithNoBlendTree() ||
                ThereIsANullActivityOrPuppet() ||
                ThereIsNoActivityNameForMultipleActivities() ||
                ThereIsNoAvatarDescriptor() ||
                LipsyncIsIntegratedButThereIsNoCorrection()
            );

            bool ThereIsNoAnimatorController()
            {
                return animatorController.objectReferenceValue == null;
            }

            bool ThereIsNoGestureAnimatorController()
            {
                return useGesturePlayableLayer.boolValue && gesturePlayableLayerController.objectReferenceValue == null;
            }

            bool ThereIsNoActivity()
            {
                return comboLayers.arraySize == 0;
            }

            bool ThereIsNoAvatarDescriptor()
            {
                return !compiler.bypassMandatoryAvatarDescriptor
                       && compiler.avatarDescriptor == null;
            }

            bool LipsyncIsIntegratedButThereIsNoCorrection()
            {
                return !compiler.bypassMandatoryAvatarDescriptor
                    && !compiler.doNotGenerateLipsyncOverrideLayer
                    && compiler.integrateLimitedLipsync
                    && compiler.lipsyncForWideOpenMouth == null;
            }

            if (GUILayout.Button(compiler.useGesturePlayableLayer ?
                CgeLocale.CGEC_Synchronize_Animator_FX_and_Gesture_layers :
                CgeLocale.CGEC_Synchronize_Animator_FX_layers, GUILayout.Height(40)))
            {
                DoGenerate();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox(
                CgeLocale.CGEC_SynchronizationConditionsV1, MessageType.Info);

            if (compiler.assetContainer != null) {
                EditorGUILayout.LabelField(CgeLocale.CGEC_Asset_generation, EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(assetContainer, new GUIContent(CgeLocale.CGEC_Asset_container));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(CgeLocale.CGEC_Other_tweaks, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(analogBlinkingUpperThreshold, new GUIContent(CgeLocale.CGEC_Analog_fist_blinking_threshold, CgeLocale.CGEC_AnalogFist_Popup));

            editorAdvancedFoldout.boolValue = EditorGUILayout.Foldout(editorAdvancedFoldout.boolValue, CgeLocale.CGEC_Advanced);
            if (editorAdvancedFoldout.boolValue)
            {
                EditorGUILayout.LabelField("Corrections", EditorStyles.boldLabel);
                if (compiler.avatarDescriptor != null) {
                    EditorGUILayout.PropertyField(weightCorrectionMode, new GUIContent(FakeBooleanIcon(compiler.WillUseGestureWeightCorrection()) + CgeLocale.CGEC_GestureWeight_correction));

                    EditorGUI.BeginDisabledGroup(compiler.doNotGenerateLipsyncOverrideLayer);
                    if (compiler.integrateLimitedLipsync && !compiler.doNotGenerateLipsyncOverrideLayer)
                    {
                        EditorGUILayout.PropertyField(lipsyncForWideOpenMouth, new GUIContent(CgeLocale.CGEC_Lipsync_correction));
                        var lipsyncBlendshapes = new BlendshapesFinder(compiler.avatarDescriptor).FindLipsync();
                        if (lipsyncBlendshapes.Any())
                        {
                            var firstLipsyncBlendshape = lipsyncBlendshapes.FirstOrDefault();
                            if (lipsyncBlendshapes.Any())
                            {
                                EditorGUILayout.LabelField(CgeLocale.CGEC_Found_lipsync_blendshapes);
                                EditorGUILayout.LabelField("- " + firstLipsyncBlendshape.Path + "::" + firstLipsyncBlendshape.BlendShapeName + " (+ " + (lipsyncBlendshapes.Count - 1) + " more...)");
                            }
                            else
                            {
                                EditorGUILayout.LabelField(CgeLocale.CGEC_No_lipsync_blendshapes_found);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.LabelField("Fine tuning", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(customEmptyClip, new GUIContent("Custom 2-frame empty animation clip (optional)"));

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Layer generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(expressionsAvatarMask, new GUIContent("Override Avatar Mask on Expressions layer"));
                EditorGUILayout.PropertyField(logicalAvatarMask, new GUIContent("Override Avatar Mask on Controller&Blinking layers"));
                EditorGUILayout.PropertyField(weightCorrectionAvatarMask, new GUIContent("Override Avatar Mask on Weight Correction layer"));
                EditorGUILayout.PropertyField(gesturePlayableLayerExpressionsAvatarMask, new GUIContent("Override Avatar Mask on Gesture playable expressions layer"));
                EditorGUILayout.PropertyField(gesturePlayableLayerTechnicalAvatarMask, new GUIContent("Override Avatar Mask on Gesture playable technical layers"));
                EditorGUILayout.PropertyField(doNotGenerateBlinkingOverrideLayer, new GUIContent("Don't update Blinking layer"));
                GenBlinkingWarning(true);
                EditorGUILayout.PropertyField(doNotGenerateLipsyncOverrideLayer, new GUIContent("Don't update Lipsync layer"));
                EditorGUILayout.PropertyField(doNotGenerateWeightCorrectionLayer, new GUIContent("Don't update Weight Correction layer"));
                GenWeightCorrection(true);

                EditorGUILayout.LabelField("Animation generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(assetContainer, new GUIContent(CgeLocale.CGEC_Asset_container));

                EditorGUI.BeginDisabledGroup(assetContainer.objectReferenceValue != null);
                EditorGUILayout.PropertyField(generateNewContainerEveryTime, new GUIContent("Don't keep track of newly generated containers"));
                EditorGUILayout.PropertyField(folderToGenerateNeutralizedAssetsIn, new GUIContent("Generate assets in the same folder as..."));
                if (animatorController.objectReferenceValue != null)
                {
                    EditorGUILayout.LabelField("Assets will be generated in:");
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(ResolveFolderToCreateNeutralizedAssetsIn((RuntimeAnimatorController)folderToGenerateNeutralizedAssetsIn.objectReferenceValue, (RuntimeAnimatorController)animatorController.objectReferenceValue));
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(conflictFxLayerMode, new GUIContent("FX Transforms removal"));

                CpmRemovalWarning(true);
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Fallback generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ignoreParamList, new GUIContent("Ignored properties"));
                EditorGUILayout.PropertyField(fallbackParamList, new GUIContent("Fallback values"));
                EditorGUILayout.PropertyField(doNotFixSingleKeyframes, new GUIContent("Do not fix single keyframes"));
                EditorGUILayout.PropertyField(bypassMandatoryAvatarDescriptor, new GUIContent("Bypass mandatory avatar descriptor"));

                EditorGUILayout.LabelField("Translations", EditorStyles.boldLabel);
                if (GUILayout.Button("(Debug) Print default translation file to console"))
                {
                    Debug.Log(CgeLocale.CompileDefaultLocaleJson());
                }
                if (GUILayout.Button("(Debug) Reload localization files"))
                {
                    CgeLocalization.ReloadLocalizations();
                }
            }
            else
            {
                GenBlinkingWarning(false);
                GenWeightCorrection(false);
                CpmRemovalWarning(false);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void MaskRemovalUi(ComboGestureCompiler compiler)
        {
            var isMaskUsed = compiler.animatorController != null && ((AnimatorController) compiler.animatorController).layers.Any(layer => layer.avatarMask == compiler.generatedAvatarMask);
            EditorGUI.BeginDisabledGroup(!isMaskUsed);
            if (GUILayout.Button(CgeLocale.CGEC_Remove_applied_masks))
            {
                DoRemoveAppliedMasks(compiler);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(isMaskUsed);
            if (GUILayout.Button(CgeLocale.CGEC_Unbind_Asset_container))
            {
                DoRemoveAppliedMasksAndAssetContainer(compiler);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void AddMissingMasks(ComboGestureCompiler compiler)
        {
            CreateAvatarMaskAssetIfNecessary(compiler);
            new CgeMaskApplicator(compiler.animatorController, compiler.generatedAvatarMask).AddMissingMasks();
            new CgeMaskApplicator(compiler.animatorController, compiler.generatedAvatarMask).UpdateMask();
        }

        private void DoRemoveAppliedMasks(ComboGestureCompiler compiler)
        {
            new CgeMaskApplicator(AsCompiler().animatorController, compiler.generatedAvatarMask).RemoveAppliedMask();
        }

        private void DoRemoveAppliedMasksAndAssetContainer(ComboGestureCompiler compiler)
        {
            new CgeMaskApplicator(AsCompiler().animatorController, compiler.generatedAvatarMask).RemoveAppliedMask();
            generatedAvatarMask.objectReferenceValue = null;
        }

        private void CreateAvatarMaskAssetIfNecessary(ComboGestureCompiler compiler)
        {
            if (compiler.generatedAvatarMask != null) return;

            var folderToCreateAssetIn = ResolveFolderToCreateNeutralizedAssetsIn(compiler.folderToGenerateNeutralizedAssetsIn, compiler.animatorController);
            var mask = new AvatarMask();
            AssetDatabase.CreateAsset(mask, folderToCreateAssetIn + "/GeneratedCGEMask__" + DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HHmmss") + ".asset");
            compiler.generatedAvatarMask = mask;
        }

        private static void WriteDefaultsSection(SerializedProperty recommendationMode)
        {
            if (recommendationMode.intValue == (int) WriteDefaultsRecommendationMode.UseUnsupportedWriteDefaultsOn)
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnWriteDefaultsChosenOff, MessageType.Warning);
            }
        }

        private static string FakeBooleanIcon(bool value)
        {
            return value ? "✓" : "×";
        }

        private void GenBlinkingWarning(bool advancedFoldoutIsOpen)
        {
            if (doNotGenerateBlinkingOverrideLayer.boolValue)
            {
                EditorGUILayout.HelpBox(@"Blinking Override layer should usually be generated as it depends on all the activities of the compiler.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
            }
        }

        private void GenWeightCorrection(bool advancedFoldoutIsOpen)
        {
            if (doNotGenerateWeightCorrectionLayer.boolValue)
            {
                EditorGUILayout.HelpBox(@"Weight correction layer should usually be generated, otherwise it may not be updated correctly on future updates of ComboGestureExpressions.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
            }
        }

        private void CpmRemovalWarning(bool advancedFoldoutIsOpen)
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
                case ConflictFxLayerMode.KeepOnlyTransforms:
                    EditorGUILayout.HelpBox(@"Everything will be removed except transforms.
However, the FX Playable layer is not meant to manipulate them.

This is not a normal usage of ComboGestureExpressions, and should not be used except in special cases." + (!advancedFoldoutIsOpen ? "\n\n(Advanced settings)" : ""), MessageType.Error);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoGenerate()
        {
            var compiler = AsCompiler();

            var folderToCreateAssetIn = ResolveFolderToCreateNeutralizedAssetsIn(compiler.folderToGenerateNeutralizedAssetsIn, compiler.animatorController);
            var actualContainer = CreateContainerIfNotExists(compiler, folderToCreateAssetIn);
            if (actualContainer != null && compiler.assetContainer == null && !compiler.generateNewContainerEveryTime)
            {
                compiler.assetContainer = actualContainer.ExposeContainerAsset();
            }

            if (compiler.comboLayers.Count > 1 && compiler.parameterMode == ParameterMode.MultipleBools)
            {
                var virtualInt = compiler.comboLayers.Exists(mapper => string.IsNullOrEmpty(mapper.booleanParameterName)) ? 1 : 0;
                for (var index = 0; index < compiler.comboLayers.Count; index++)
                {
                    var mapper = compiler.comboLayers[index];
                    if (string.IsNullOrEmpty(mapper.booleanParameterName))
                    {
                        mapper.internalVirtualStageValue = 0;
                    }
                    else
                    {
                        mapper.internalVirtualStageValue = virtualInt;
                        virtualInt++;
                    }

                    compiler.comboLayers[index] = mapper;
                }
            }

            if (compiler.avatarDescriptor.transform != null && (compiler.useGesturePlayableLayer || compiler.generatedAvatarMask != null))
            {
                CreateAvatarMaskAssetIfNecessary(compiler);
                new CgeMaskApplicator(compiler.animatorController, compiler.generatedAvatarMask).UpdateMask();
            }

            new ComboGestureCompilerInternal(compiler, actualContainer).DoOverwriteAnimatorFxLayer();
            if (compiler.useGesturePlayableLayer)
            {
                new ComboGestureCompilerInternal(compiler, actualContainer).DoOverwriteAnimatorGesturePlayableLayer();
            }
        }

        private static AssetContainer CreateContainerIfNotExists(ComboGestureCompiler compiler, string folderToCreateAssetIn)
        {
            return compiler.assetContainer == null ? AssetContainer.CreateNew(folderToCreateAssetIn) : AssetContainer.FromExisting(compiler.assetContainer);
        }

        private static string ResolveFolderToCreateNeutralizedAssetsIn(RuntimeAnimatorController preferredChoice, RuntimeAnimatorController defaultChoice)
        {
            var reference = preferredChoice == null ? defaultChoice : preferredChoice;

            var originalAssetPath = AssetDatabase.GetAssetPath(reference);
            var folder = originalAssetPath.Replace(Path.GetFileName(originalAssetPath), "");
            return folder;
        }

        private ComboGestureCompiler AsCompiler()
        {
            return (ComboGestureCompiler) target;
        }

        private void ComboLayersListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = comboLayersReorderableList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("kind"),
                GUIContent.none
            );

            var kind = (GestureComboStageKind) element.FindPropertyRelative("kind").intValue;
            var compiler = AsCompiler();
            var onlyOneLayer = compiler.comboLayers.Count <= 1;
            var singleInt = compiler.parameterMode == ParameterMode.SingleInt;
            var trailingWidth = onlyOneLayer ? 0 : singleInt ? 50 : 140;
            EditorGUI.PropertyField(
                new Rect(rect.x + 70, rect.y, rect.width - 70 - 20 - trailingWidth, EditorGUIUtility.singleLineHeight),
                kind != GestureComboStageKind.Puppet
                    ? element.FindPropertyRelative("activity")
                    : element.FindPropertyRelative("puppet"),
                GUIContent.none
            );

            if (!onlyOneLayer)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width - 20 - trailingWidth, rect.y, trailingWidth, EditorGUIUtility.singleLineHeight),
                    singleInt ? element.FindPropertyRelative("stageValue") : element.FindPropertyRelative("booleanParameterName"),
                    GUIContent.none
                );
            }
        }

        private void ComboLayersListHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 70 - 51, EditorGUIUtility.singleLineHeight), CgeLocale.CGEC_Mood_sets);
            if (AsCompiler().comboLayers.Count > 1)
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 70 - 51, rect.y, 50 + 51, EditorGUIUtility.singleLineHeight), AsCompiler().parameterMode == ParameterMode.SingleInt ? CgeLocale.CGEC_Parameter_Value : CgeLocale.CGEC_Parameter_Name);
            }
        }
    }
}
