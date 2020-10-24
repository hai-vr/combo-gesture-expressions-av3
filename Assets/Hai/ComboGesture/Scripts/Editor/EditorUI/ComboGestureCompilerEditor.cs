using System;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
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

        public SerializedProperty integrateLimitedLipsync;
        public SerializedProperty lipsyncForWideOpenMouth;

        public SerializedProperty exposeDisableExpressions;
        public SerializedProperty exposeDisableBlinkingOverride;
        public SerializedProperty exposeAreEyesClosed;

        public SerializedProperty expressionsAvatarMask;
        public SerializedProperty logicalAvatarMask;
        public SerializedProperty weightCorrectionAvatarMask;
        public SerializedProperty gesturePlayableLayerExpressionsAvatarMask;
        public SerializedProperty gesturePlayableLayerTechnicalAvatarMask;
        public SerializedProperty doNotGenerateControllerLayer;
        public SerializedProperty forceGenerationOfControllerLayer;
        public SerializedProperty doNotGenerateBlinkingOverrideLayer;
        public SerializedProperty doNotGenerateLipsyncOverrideLayer;
        public SerializedProperty doNotGenerateWeightCorrectionLayer;

        public SerializedProperty conflictPreventionMode;
        public SerializedProperty conflictFxLayerMode;
        public SerializedProperty weightCorrectionMode;
        public SerializedProperty blinkCorrectionMode;
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

            integrateLimitedLipsync = serializedObject.FindProperty("integrateLimitedLipsync");
            lipsyncForWideOpenMouth = serializedObject.FindProperty("lipsyncForWideOpenMouth");

            exposeDisableExpressions = serializedObject.FindProperty("exposeDisableExpressions");
            exposeDisableBlinkingOverride = serializedObject.FindProperty("exposeDisableBlinkingOverride");
            exposeAreEyesClosed = serializedObject.FindProperty("exposeAreEyesClosed");

            expressionsAvatarMask = serializedObject.FindProperty("expressionsAvatarMask");
            logicalAvatarMask = serializedObject.FindProperty("logicalAvatarMask");
            weightCorrectionAvatarMask = serializedObject.FindProperty("weightCorrectionAvatarMask");
            gesturePlayableLayerExpressionsAvatarMask = serializedObject.FindProperty("gesturePlayableLayerExpressionsAvatarMask");
            gesturePlayableLayerTechnicalAvatarMask = serializedObject.FindProperty("gesturePlayableLayerTechnicalAvatarMask");
            doNotGenerateControllerLayer = serializedObject.FindProperty("doNotGenerateControllerLayer");
            forceGenerationOfControllerLayer = serializedObject.FindProperty("forceGenerationOfControllerLayer");
            doNotGenerateBlinkingOverrideLayer = serializedObject.FindProperty("doNotGenerateBlinkingOverrideLayer");
            doNotGenerateLipsyncOverrideLayer = serializedObject.FindProperty("doNotGenerateLipsyncOverrideLayer");
            doNotGenerateWeightCorrectionLayer = serializedObject.FindProperty("doNotGenerateWeightCorrectionLayer");

            conflictPreventionMode = serializedObject.FindProperty("conflictPreventionMode");
            conflictFxLayerMode = serializedObject.FindProperty("conflictFxLayerMode");
            weightCorrectionMode = serializedObject.FindProperty("weightCorrectionMode");
            blinkCorrectionMode = serializedObject.FindProperty("blinkCorrectionMode");
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

        private bool _foldoutHelp;
        private Texture _guideIcon16;
        private Texture _guideIcon32;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var italic = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Italic};

            _foldoutHelp = EditorGUILayout.Foldout(_foldoutHelp, new GUIContent("Help", _guideIcon32));
            if (_foldoutHelp)
            {
                if (GUILayout.Button(new GUIContent("Open documentation and tutorials", _guideIcon32)))
                {
                    Application.OpenURL("https://hai-vr.github.io/combo-gesture-expressions-av3/");
                }
            }

            EditorGUILayout.LabelField("Activities", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Make backups! The FX Animator Controller will be modified directly.", italic);
            EditorGUILayout.PropertyField(animatorController, new GUIContent("FX Animator Controller"));
            EditorGUILayout.PropertyField(activityStageName, new GUIContent("Parameter Name"));

            comboLayersReorderableList.DoLayoutList();

            EditorGUILayout.Separator();

            var compiler = AsCompiler();

            bool ThereIsAnOverlap()
            {
                return comboLayers.arraySize != compiler.comboLayers.Select(mapper => mapper.stageValue).Distinct().Count();
            }

            bool ThereIsAPuppetWithNoBlendTree()
            {
                return compiler.comboLayers
                    .Where(mapper => mapper.kind == GestureComboStageKind.Puppet)
                    .Where(mapper => mapper.puppet != null)
                    .Any(mapper => !(mapper.puppet.mainTree is BlendTree));
            }

            bool ThereIsANullActivityOrPuppet()
            {
                return compiler.comboLayers.Any(mapper =>
                    mapper.kind == GestureComboStageKind.Activity && mapper.activity == null
                    || mapper.kind == GestureComboStageKind.Puppet && mapper.puppet == null
                );
            }

            if (ThereIsAnOverlap())
            {
                EditorGUILayout.HelpBox("Some Parameters Values are overlapping.", MessageType.Error);
            }
            else if (ThereIsAPuppetWithNoBlendTree())
            {
                EditorGUILayout.HelpBox("One of the puppets has no blend tree defined inside it.", MessageType.Error);
            }
            else if (ThereIsANullActivityOrPuppet())
            {
                EditorGUILayout.HelpBox("One of the activities is missing.", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Corrections", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(avatarDescriptor, new GUIContent("Avatar descriptor"));
            if (compiler.avatarDescriptor != null) {
                EditorGUILayout.PropertyField(weightCorrectionMode, new GUIContent(FakeBooleanIcon(compiler.WillUseGestureWeightCorrection()) + "GestureWeight correction"));
                EditorGUILayout.PropertyField(blinkCorrectionMode, new GUIContent(FakeBooleanIcon(compiler.WillUseBlinkBlendshapeCorrection()) + "Blink blendshapes correction"));
                if (compiler.WillUseBlinkBlendshapeCorrection())
                {
                    var blinkBlendshapes = new BlendshapesFinder(compiler.avatarDescriptor).FindBlink();
                    if (blinkBlendshapes.Any())
                    {
                        EditorGUILayout.LabelField("Found blink blendshapes:");
                        foreach (var blendShape in blinkBlendshapes)
                        {
                            EditorGUILayout.LabelField("- " + blendShape.Path + "::" + blendShape.BlendShapeName);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No blink blendshapes found");
                    }
                }

                EditorGUI.BeginDisabledGroup(compiler.doNotGenerateLipsyncOverrideLayer);
                EditorGUILayout.PropertyField(integrateLimitedLipsync, new GUIContent("Integrate limited lipsync"));
                if (compiler.integrateLimitedLipsync)
                {
                    EditorGUILayout.HelpBox(@"Limited Lipsync is a feature that will not work with the version of VRChat at the time this version of ComboGestureExpressions has been published.

At the time this version has been published, generating the layer will break your Lipsync blendshapes.", MessageType.Error);
                }
                if (compiler.integrateLimitedLipsync && !compiler.doNotGenerateLipsyncOverrideLayer)
                {
                    EditorGUILayout.PropertyField(lipsyncForWideOpenMouth, new GUIContent("Lipsync correction"));
                    var lipsyncBlendshapes = new BlendshapesFinder(compiler.avatarDescriptor).FindLipsync();
                    if (lipsyncBlendshapes.Any())
                    {
                        var firstLipsyncBlendshape = lipsyncBlendshapes.FirstOrDefault();
                        if (lipsyncBlendshapes.Any())
                        {
                            EditorGUILayout.LabelField("Found lipsync blendshapes:");
                            EditorGUILayout.LabelField("- " + firstLipsyncBlendshape.Path + "::" + firstLipsyncBlendshape.BlendShapeName + " (+ " + (lipsyncBlendshapes.Count - 1) + " more...)");
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No lipsync blendshapes found");
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Support for ears/wings/tail/other transforms", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useGesturePlayableLayer, new GUIContent("Gesture playable layer support"));
            if (useGesturePlayableLayer.boolValue)
            {
                EditorGUILayout.LabelField("Make backups! The Gesture Animator Controller will be modified directly.", italic);
                EditorGUILayout.PropertyField(gesturePlayableLayerController, new GUIContent("Gesture Animator Controller"));
                EditorGUILayout.HelpBox("Finger positions or other muscles are not supported.", MessageType.Info);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Synchronization", EditorStyles.boldLabel);

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

            bool ThereIsNoActivityNameForMultipleActivities()
            {
                return comboLayers.arraySize >= 2 && (activityStageName.stringValue == null || activityStageName.stringValue.Trim() == "");
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
                "Synchronize Animator FX and Gesture layers" :
                "Synchronize Animator FX layers"))
            {
                DoGenerate();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox(
                @"Synchronization will regenerate CGE's animator layers and generate animations.
- Only layers starting with 'Hai_Gesture' will be affected.
- The avatar descriptor will not be modified.

You should press synchronize when any of the following happens:
- this Compiler is modified,
- an Activity, a Puppet, or a LimitedLipsync is modified,
- an animation or a blend tree or avatar mask is modified,
- the order of layers in any animator controller changes,
- the avatar descriptor Eyelids or Lipsync is modified.", MessageType.Info);

            if (compiler.assetContainer != null) {
                EditorGUILayout.LabelField("Asset generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(assetContainer, new GUIContent("Asset container"));
            }

            EditorGUILayout.Space();

            editorAdvancedFoldout.boolValue = EditorGUILayout.Foldout(editorAdvancedFoldout.boolValue, "Advanced");
            if (editorAdvancedFoldout.boolValue)
            {
                EditorGUILayout.LabelField("Fine tuning", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(customEmptyClip, new GUIContent("Custom 2-frame empty animation clip (optional)"));
                EditorGUILayout.PropertyField(analogBlinkingUpperThreshold, new GUIContent("Analog fist blinking threshold", "(0: Eyes are open, 1: Eyes are closed)"));

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Internal parameters", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(exposeDisableExpressions, new GUIContent("Expose " + SharedLayerUtils.HaiGestureComboDisableExpressionsParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeDisableBlinkingOverride, new GUIContent("Expose " + SharedLayerUtils.HaiGestureComboDisableBlinkingOverrideParamName.Substring("_Hai_GestureCombo".Length)));
                EditorGUILayout.PropertyField(exposeAreEyesClosed, new GUIContent("Expose " + SharedLayerUtils.HaiGestureComboAreEyesClosedParamName.Substring("_Hai_GestureCombo".Length)));

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Layer generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(expressionsAvatarMask, new GUIContent("Add Avatar Mask to Expressions layer"));
                EditorGUILayout.PropertyField(logicalAvatarMask, new GUIContent("Add Avatar Mask to Controller&Blinking layers"));
                EditorGUILayout.PropertyField(weightCorrectionAvatarMask, new GUIContent("Add Avatar Mask to Weight Correction layer"));
                EditorGUILayout.PropertyField(gesturePlayableLayerExpressionsAvatarMask, new GUIContent("Use specific Avatar Mask on Gesture playable expressions layer"));
                EditorGUILayout.PropertyField(gesturePlayableLayerTechnicalAvatarMask, new GUIContent("Use specific Avatar Mask on Gesture playable technical layers"));
                EditorGUILayout.PropertyField(doNotGenerateControllerLayer, new GUIContent("Don't update Controller layer"));
                EditorGUI.BeginDisabledGroup(compiler.doNotGenerateControllerLayer);
                EditorGUILayout.PropertyField(forceGenerationOfControllerLayer, new GUIContent("Force generation of Controller layer"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.PropertyField(doNotGenerateBlinkingOverrideLayer, new GUIContent("Don't update Blinking layer"));
                GenBlinkingWarning(true);
                EditorGUILayout.PropertyField(doNotGenerateLipsyncOverrideLayer, new GUIContent("Don't update Lipsync layer"));
                EditorGUILayout.PropertyField(doNotGenerateWeightCorrectionLayer, new GUIContent("Don't update Weight Correction layer"));
                GenWeightCorrection(true);

                EditorGUILayout.LabelField("Animation Conflict Prevention", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(conflictPreventionMode, new GUIContent("Mode"));

                CpmValueWarning(true);

                EditorGUILayout.LabelField("Animation generation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(assetContainer, new GUIContent("Asset container"));

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
            }
            else
            {
                GenBlinkingWarning(false);
                GenWeightCorrection(false);
                CpmValueWarning(false);
                CpmRemovalWarning(false);
            }

            serializedObject.ApplyModifiedProperties();
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

        private void CpmValueWarning(bool advancedFoldoutIsOpen)
        {
            var conflictPrevention = ConflictPrevention.Of((ConflictPreventionMode) conflictPreventionMode.intValue);
            if (!conflictPrevention.ShouldGenerateExhaustiveAnimations)
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
            EditorGUI.PropertyField(
                new Rect(rect.x + 70, rect.y, rect.width - 70 - 70, EditorGUIUtility.singleLineHeight),
                kind != GestureComboStageKind.Puppet
                    ? element.FindPropertyRelative("activity")
                    : element.FindPropertyRelative("puppet"),
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
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 70 - 51, EditorGUIUtility.singleLineHeight), "Mood sets");
            EditorGUI.LabelField(new Rect(rect.x + rect.width - 70 - 51, rect.y, 50 + 51, EditorGUIUtility.singleLineHeight), "Parameter Value");
        }
    }
}
