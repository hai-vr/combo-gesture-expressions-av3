using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase;
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

        public SerializedProperty expressionsAvatarMask;
        public SerializedProperty logicalAvatarMask;
        public SerializedProperty weightCorrectionAvatarMask;
        public SerializedProperty gesturePlayableLayerExpressionsAvatarMask;
        public SerializedProperty gesturePlayableLayerTechnicalAvatarMask;
        public SerializedProperty doNotGenerateBlinkingOverrideLayer;
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

        public SerializedProperty useViveAdvancedControlsForNonFistAnalog;
        public SerializedProperty dynamics;
        private SerializedProperty doNotForceBlinkBlendshapes;
        private SerializedProperty mmdCompatibilityToggleParameter;
        private SerializedProperty eyeTrackingEnabledParameter;
        private SerializedProperty eyeTrackingParameterType;

        private void OnEnable()
        {
            animatorController = serializedObject.FindProperty(nameof(ComboGestureCompiler.animatorController));
            useGesturePlayableLayer = serializedObject.FindProperty(nameof(ComboGestureCompiler.useGesturePlayableLayer));
            gesturePlayableLayerController = serializedObject.FindProperty(nameof(ComboGestureCompiler.gesturePlayableLayerController));
            activityStageName = serializedObject.FindProperty(nameof(ComboGestureCompiler.activityStageName));
            customEmptyClip = serializedObject.FindProperty(nameof(ComboGestureCompiler.customEmptyClip));
            analogBlinkingUpperThreshold = serializedObject.FindProperty(nameof(ComboGestureCompiler.analogBlinkingUpperThreshold));
            parameterMode = serializedObject.FindProperty(nameof(ComboGestureCompiler.parameterMode));

            expressionsAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.expressionsAvatarMask));
            logicalAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.logicalAvatarMask));
            weightCorrectionAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.weightCorrectionAvatarMask));
            gesturePlayableLayerExpressionsAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.gesturePlayableLayerExpressionsAvatarMask));
            gesturePlayableLayerTechnicalAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.gesturePlayableLayerTechnicalAvatarMask));
            doNotGenerateBlinkingOverrideLayer = serializedObject.FindProperty(nameof(ComboGestureCompiler.doNotGenerateBlinkingOverrideLayer));
            doNotGenerateWeightCorrectionLayer = serializedObject.FindProperty(nameof(ComboGestureCompiler.doNotGenerateWeightCorrectionLayer));

            writeDefaultsRecommendationMode = serializedObject.FindProperty(nameof(ComboGestureCompiler.writeDefaultsRecommendationMode));
            writeDefaultsRecommendationModeGesture = serializedObject.FindProperty(nameof(ComboGestureCompiler.writeDefaultsRecommendationModeGesture));
            gestureLayerTransformCapture = serializedObject.FindProperty(nameof(ComboGestureCompiler.gestureLayerTransformCapture));
            generatedAvatarMask = serializedObject.FindProperty(nameof(ComboGestureCompiler.generatedAvatarMask));
            conflictFxLayerMode = serializedObject.FindProperty(nameof(ComboGestureCompiler.conflictFxLayerMode));
            weightCorrectionMode = serializedObject.FindProperty(nameof(ComboGestureCompiler.weightCorrectionMode));
            ignoreParamList = serializedObject.FindProperty(nameof(ComboGestureCompiler.ignoreParamList));
            fallbackParamList = serializedObject.FindProperty(nameof(ComboGestureCompiler.fallbackParamList));
            folderToGenerateNeutralizedAssetsIn = serializedObject.FindProperty(nameof(ComboGestureCompiler.folderToGenerateNeutralizedAssetsIn));

            comboLayers = serializedObject.FindProperty(nameof(ComboGestureCompiler.comboLayers));

            avatarDescriptor = serializedObject.FindProperty(nameof(ComboGestureCompiler.avatarDescriptor));
            doNotFixSingleKeyframes = serializedObject.FindProperty(nameof(ComboGestureCompiler.doNotFixSingleKeyframes));
            bypassMandatoryAvatarDescriptor = serializedObject.FindProperty(nameof(ComboGestureCompiler.bypassMandatoryAvatarDescriptor));

            assetContainer = serializedObject.FindProperty(nameof(ComboGestureCompiler.assetContainer));
            generateNewContainerEveryTime = serializedObject.FindProperty(nameof(ComboGestureCompiler.generateNewContainerEveryTime));

            useViveAdvancedControlsForNonFistAnalog = serializedObject.FindProperty(nameof(ComboGestureCompiler.useViveAdvancedControlsForNonFistAnalog));
            dynamics = serializedObject.FindProperty(nameof(ComboGestureCompiler.dynamics));
            doNotForceBlinkBlendshapes = serializedObject.FindProperty(nameof(ComboGestureCompiler.doNotForceBlinkBlendshapes));
            mmdCompatibilityToggleParameter = serializedObject.FindProperty(nameof(ComboGestureCompiler.mmdCompatibilityToggleParameter));
            eyeTrackingEnabledParameter = serializedObject.FindProperty(nameof(ComboGestureCompiler.eyeTrackingEnabledParameter));
            eyeTrackingParameterType = serializedObject.FindProperty(nameof(ComboGestureCompiler.eyeTrackingParameterType));

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            comboLayersReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty(nameof(ComboGestureCompiler.comboLayers)),
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

                var previous = comboLayers.GetArrayElementAtIndex(comboLayers.arraySize - 2).FindPropertyRelative(nameof(GestureComboStageMapper.stageValue)).intValue;
                var newlyAddedElement = comboLayers.GetArrayElementAtIndex(comboLayers.arraySize - 1);
                newlyAddedElement.FindPropertyRelative(nameof(GestureComboStageMapper.stageValue)).intValue = previous + 1;
                newlyAddedElement.FindPropertyRelative(nameof(GestureComboStageMapper.activity)).objectReferenceValue = null;
                newlyAddedElement.FindPropertyRelative(nameof(GestureComboStageMapper.puppet)).objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            };
            comboLayersReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 2.5f;

            _guideIcon16 = ComboGestureIcons.Instance.Guide16;
            _guideIcon32 = ComboGestureIcons.Instance.Guide32;

            editorAdvancedFoldout = serializedObject.FindProperty(nameof(ComboGestureCompiler.editorAdvancedFoldout));
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

            EditorGUILayout.LabelField(CgeLocale.CGEC_Avatar_Dynamics, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dynamics, new GUIContent(CgeLocale.CGEC_MainDynamics));

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

            bool ThereIsAMassiveBlendWithIncorrectConfiguration()
            {
                return compiler.comboLayers != null && compiler.comboLayers
                    .Where(mapper => mapper.kind == GestureComboStageKind.Massive)
                    .Where(mapper => mapper.massiveBlend != null)
                    .Any(mapper =>
                    {
                        var massiveBlend = mapper.massiveBlend;
                        switch (massiveBlend.mode)
                        {
                            case CgeMassiveBlendMode.Simple:
                                return massiveBlend.simpleZero == null || massiveBlend.simpleOne == null;
                            case CgeMassiveBlendMode.TwoDirections:
                                return massiveBlend.simpleZero == null || massiveBlend.simpleOne == null || massiveBlend.simpleMinusOne == null;
                            case CgeMassiveBlendMode.ComplexBlendTree:
                                return massiveBlend.blendTreeMoods.Count == 0
                                       || massiveBlend.blendTree == null
                                       || !(massiveBlend.blendTree is BlendTree)
                                       || ((BlendTree) massiveBlend.blendTree).children.Length != massiveBlend.blendTreeMoods.Count;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
            }

            bool ThereIsANullMapper()
            {
                return compiler.comboLayers != null && compiler.comboLayers.Any(mapper =>
                    mapper.kind == GestureComboStageKind.Activity && mapper.activity == null
                    || mapper.kind == GestureComboStageKind.Puppet && mapper.puppet == null
                    || mapper.kind == GestureComboStageKind.Massive && mapper.massiveBlend == null
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
            else if (ThereIsAMassiveBlendWithIncorrectConfiguration())
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNoMassiveBlend, MessageType.Error);
            }
            else if (ThereIsNoActivityNameForMultipleActivities())
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnNoActivityName, MessageType.Error);
            }
            else if (ThereIsANullMapper())
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
                        EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_HelpWhenAllParameterNamesDefined, compiler.comboLayers.First().SimpleName(), compiler.comboLayers.First().booleanParameterName), MessageType.Info);
                    }

                    var defaultMapper = compiler.comboLayers.FirstOrDefault(mapper => mapper.booleanParameterName == "");
                    if (compiler.parameterMode == ParameterMode.MultipleBools &&
                        (defaultMapper.kind == GestureComboStageKind.Activity && defaultMapper.activity != null
                        || defaultMapper.kind == GestureComboStageKind.Puppet && defaultMapper.puppet != null
                        || defaultMapper.kind == GestureComboStageKind.Massive && defaultMapper.massiveBlend != null))
                    {
                        EditorGUILayout.HelpBox(string.Format(CgeLocale.CGEC_HintDefaultMood, defaultMapper.SimpleName()), MessageType.Info);
                    }
                }
            }

            EditorGUILayout.LabelField(CgeLocale.CGEC_FX_Playable_Layer, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(CgeLocale.CGEC_BackupFX, italic);
            EditorGUILayout.PropertyField(animatorController, new GUIContent(CgeLocale.CGEC_FX_Animator_Controller));
            EditorGUILayout.PropertyField(writeDefaultsRecommendationMode, new GUIContent(CgeLocale.CGEC_FX_Playable_Mode));
            WriteDefaultsSection(writeDefaultsRecommendationMode);

            EditorGUILayout.PropertyField(doNotForceBlinkBlendshapes, new GUIContent(CgeLocale.CGEC_DoNotForceBlinkBlendshapes));

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

            if (useViveAdvancedControlsForNonFistAnalog.boolValue)
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_ViveAdvancedControlsWarning, MessageType.Error);
                EditorGUILayout.PropertyField(useViveAdvancedControlsForNonFistAnalog, new GUIContent("Use Vive Advanced Controls Analog"));
            }

            EditorGUI.BeginDisabledGroup(
                ThereIsNoAnimatorController() ||
                ThereIsNoGestureAnimatorController() ||
                ThereIsNoActivity() ||
                ThereIsAnOverlap() ||
                ThereIsAPuppetWithNoBlendTree() ||
                ThereIsAMassiveBlendWithIncorrectConfiguration() ||
                ThereIsANullMapper() ||
                ThereIsNoActivityNameForMultipleActivities() ||
                ThereIsNoAvatarDescriptor()
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

            if (GUILayout.Button(compiler.useGesturePlayableLayer ?
                CgeLocale.CGEC_Synchronize_Animator_FX_and_Gesture_layers :
                CgeLocale.CGEC_Synchronize_Animator_FX_layers, GUILayout.Height(40)))
            {
                DoGenerateLayers();
                compiler.totalNumberOfGenerations++;
                if (compiler.totalNumberOfGenerations % 5 == 0)
                {
                    EditorUtility.DisplayDialog("ComboGestureExpressions", CgeLocale.CGEC_Slowness_warning, "OK", DialogOptOutDecisionType.ForThisSession, "CGE_SlownessWarning");
                }
            }
            if (compiler.totalNumberOfGenerations >= 5)
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_Slowness_warning, MessageType.Warning);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox(
                CgeLocale.CGEC_SynchronizationConditionsV2, MessageType.Info);

            if (compiler.assetContainer != null) {
                EditorGUILayout.LabelField(CgeLocale.CGEC_Asset_generation, EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(assetContainer, new GUIContent(CgeLocale.CGEC_Asset_container));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(CgeLocale.CGEC_Other_tweaks, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(analogBlinkingUpperThreshold, new GUIContent(CgeLocale.CGEC_Analog_fist_blinking_threshold, CgeLocale.CGEC_AnalogFist_Popup));
            EditorGUILayout.PropertyField(mmdCompatibilityToggleParameter, new GUIContent(CgeLocale.CGEC_MMD_compatibility_toggle_parameter));
            EditorGUILayout.PropertyField(eyeTrackingEnabledParameter, new GUIContent(CgeLocale.CGEC_Eye_tracking_enabled_parameter));
            EditorGUILayout.PropertyField(eyeTrackingParameterType, new GUIContent(CgeLocale.CGEC_Eye_tracking_parameter_type));
            if (eyeTrackingEnabledParameter.stringValue != "" &&
                (EyeTrackingParameterType)eyeTrackingParameterType.intValue == EyeTrackingParameterType.LegacyBool)
            {
                EditorGUILayout.HelpBox(CgeLocale.CGEC_WarnEyeTrackingParameterType, MessageType.Warning);
            }

            editorAdvancedFoldout.boolValue = EditorGUILayout.Foldout(editorAdvancedFoldout.boolValue, CgeLocale.CGEC_Advanced);
            if (editorAdvancedFoldout.boolValue)
            {
                EditorGUILayout.LabelField("Corrections", EditorStyles.boldLabel);
                if (compiler.avatarDescriptor != null) {
                    EditorGUILayout.PropertyField(weightCorrectionMode, new GUIContent(FakeBooleanIcon(compiler.WillUseGestureWeightCorrection()) + CgeLocale.CGEC_GestureWeight_correction));
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

                EditorGUILayout.LabelField("Special cases", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(useViveAdvancedControlsForNonFistAnalog, new GUIContent("Support non-Fist expression saving in Vive Advanced Controls"));

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
            else
            {
                for (var index = 0; index < compiler.comboLayers.Count; index++)
                {
                    var mapper = compiler.comboLayers[index];
                    mapper.internalVirtualStageValue = mapper.stageValue;
                    compiler.comboLayers[index] = mapper;
                }
            }

            if (compiler.avatarDescriptor.transform != null && (compiler.useGesturePlayableLayer || compiler.generatedAvatarMask != null))
            {
                CreateAvatarMaskAssetIfNecessary(compiler);
                new CgeMaskApplicator(compiler.animatorController, compiler.generatedAvatarMask).UpdateMask();
            }

            actualContainer.ExposeAac().ClearPreviousAssets();
            new ComboGestureCompilerInternal(compiler, actualContainer).DoOverwriteAnimatorFxLayer();
            if (compiler.useGesturePlayableLayer)
            {
                new ComboGestureCompilerInternal(compiler, actualContainer).DoOverwriteAnimatorGesturePlayableLayer();
            }
        }

        private void DoGenerateLayers()
        {
            try
            {
                // var pfi = ProfilerDriver.GetPreviousFrameIndex(Time.frameCount);
                // Debug.Log($"PFI: {pfi}");
                Profiler.BeginSample("CGE");
                AssetDatabase.StartAssetEditing();
                DoGenerate();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                Profiler.EndSample();
                EditorUtility.ClearProgressBar();
            }
        }

        private static CgeAssetContainer CreateContainerIfNotExists(ComboGestureCompiler compiler, string folderToCreateAssetIn)
        {
            return compiler.assetContainer == null ? CgeAssetContainer.CreateNew(folderToCreateAssetIn) : CgeAssetContainer.FromExisting(compiler.assetContainer);
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
                element.FindPropertyRelative(nameof(GestureComboStageMapper.kind)),
                GUIContent.none
            );

            var kind = (GestureComboStageKind) element.FindPropertyRelative(nameof(GestureComboStageMapper.kind)).intValue;
            var compiler = AsCompiler();
            var onlyOneLayer = compiler.comboLayers.Count <= 1;
            var singleInt = compiler.parameterMode == ParameterMode.SingleInt;
            var trailingWidth = onlyOneLayer ? 0 : singleInt ? rect.width * 0.2f : Mathf.Min(rect.width * 0.3f, 100);
            EditorGUI.PropertyField(
                new Rect(rect.x + 70, rect.y, rect.width - 70 - 20 - trailingWidth, EditorGUIUtility.singleLineHeight),
                PropertyForKind(kind, element),
                GUIContent.none
            );

            if (!onlyOneLayer)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width - 20 - trailingWidth, rect.y, trailingWidth, EditorGUIUtility.singleLineHeight),
                    singleInt ? element.FindPropertyRelative(nameof(GestureComboStageMapper.stageValue)) : element.FindPropertyRelative(nameof(GestureComboStageMapper.booleanParameterName)),
                    GUIContent.none
                );
            }

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, 110, EditorGUIUtility.singleLineHeight),
                new GUIContent(CgeLocale.CGEC_Dynamics)
            );
            EditorGUI.PropertyField(
                new Rect(rect.x + 110, rect.y + EditorGUIUtility.singleLineHeight, rect.width - 110 - 20 - trailingWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative(nameof(GestureComboStageMapper.dynamics)),
                GUIContent.none);
        }

        private static SerializedProperty PropertyForKind(GestureComboStageKind kind, SerializedProperty element)
        {
            switch (kind)
            {
                case GestureComboStageKind.Puppet:
                    return element.FindPropertyRelative(nameof(GestureComboStageMapper.puppet));
                case GestureComboStageKind.Activity:
                    return element.FindPropertyRelative(nameof(GestureComboStageMapper.activity));
                case GestureComboStageKind.Massive:
                    return element.FindPropertyRelative(nameof(GestureComboStageMapper.massiveBlend));
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
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
