using System;
using System.Collections.Generic;
using AnimatorAsCode.V1;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.Dynamics;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureDynamics))]
    public class ComboGestureDynamicsEditor : UnityEditor.Editor
    {
        private SerializedProperty previewAnimator;

        private ReorderableList itemReorderableList;
        private EeRenderingCommands _renderingCommands;

        private void OnEnable()
        {
            previewAnimator = serializedObject.FindProperty(nameof(ComboGestureDynamics.previewAnimator));

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            itemReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty(nameof(ComboGestureDynamics.items)),
                true, true, true, true
            );
            itemReorderableList.drawElementCallback = DrawListElement;
            itemReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dynamics");
            itemReorderableList.elementHeightCallback = HeightListElement;
            itemReorderableList.onAddCallback = list =>
            {
                ++list.serializedProperty.arraySize;
                var element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.effect)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.source)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.clip)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.bothEyesClosed)).boolValue = false;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.moodSet)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBoneSource)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.contactReceiver)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBone)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterName)).stringValue = "";
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)).floatValue = 0f;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)).boolValue = false;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterDuration)).floatValue = 1f;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.enterTransitionDuration)).floatValue = 0.1f;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.behavesLikeOnEnter)).boolValue = false;
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.upperBound)).floatValue = 1f;

                var mutatedKeyframes = new List<Keyframe>();
                new AacFlSettingKeyframes(AacFlUnit.Seconds, mutatedKeyframes)
                    .Easing(0f, 0f)
                    .Easing(0.05f, 1f)
                    .Easing(0.2f, 1f)
                    .Easing(1f, 0f);
                var curve = element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterCurve)).animationCurveValue;
                curve.keys = mutatedKeyframes.ToArray();
                element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterCurve)).animationCurveValue = curve;
            };

            _renderingCommands = new EeRenderingCommands();
        }

        private float HeightListElement(int index)
        {
            var item = ((ComboGestureDynamics)target).items[index];
            return EditorGUIUtility.singleLineHeight * 16
                   + (item.effect == ComboGestureDynamicsEffect.Clip ? EditorGUIUtility.singleLineHeight * (4 + 1) : 0);
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = itemReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var line = EditorGUIUtility.singleLineHeight;

            var lineId = 0;
            var item = ((ComboGestureDynamics)target).items[index];

            Choices(rect, item, line, lineId, element);
        }

        private void Choices(Rect rect, ComboGestureDynamicsItem item, float line, int lineId, SerializedProperty element)
        {
            EditorGUI.LabelField(Position(rect, line, ref lineId), new GUIContent(CgeLocale.CGED_DynamicExpression), EditorStyles.boldLabel);
            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.effect)), new GUIContent(CgeLocale.CGED_Effect));

            switch (item.effect)
            {
                case ComboGestureDynamicsEffect.Clip:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.clip)), new GUIContent(CgeLocale.CGED_Clip));
                    if (item.clip != null)
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.bothEyesClosed)), new GUIContent(CgeLocale.CGEE_EyesAreClosed));
                        GUI.Box(new Rect(rect.x + 10, rect.y + line * lineId, 150, line * 4), _renderingCommands.RequireRender(item.clip, Repaint).Normal);
                        lineId += 4;
                    }
                    break;
                case ComboGestureDynamicsEffect.MoodSet:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.moodSet)), new GUIContent(CgeLocale.CGED_MoodSet));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.enterTransitionDuration)), new GUIContent(CgeLocale.CGED_EnterTransitionDuration));

            lineId += 1;

            EditorGUI.LabelField(Position(rect, line, ref lineId), new GUIContent(CgeLocale.CGED_DynamicCondition), EditorStyles.boldLabel);
            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.source)), new GUIContent(CgeLocale.CGED_Source));

            switch (item.source)
            {
                case ComboGestureDynamicsSource.Contact:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.contactReceiver)), new GUIContent(CgeLocale.CGED_ContactReceiver));
                    if (item.contactReceiver != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Type", Enum.GetName(typeof(ContactReceiver.ReceiverType), item.contactReceiver.receiverType));
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.contactReceiver.parameter);
                        EditorGUI.EndDisabledGroup();
                        if (string.IsNullOrEmpty(item.contactReceiver.parameter))
                        {
                            EditorGUI.HelpBox(new Rect(rect.x + 10 * 0, rect.y + line * lineId, rect.width - 10 * 0, line * 2), CgeLocale.CGED_MissingParameterOnContact, MessageType.Error);
                            lineId += 2;
                        }
                        else
                        {
                            if (item.contactReceiver.receiverType != ContactReceiver.ReceiverType.Proximity)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)), new GUIContent(CgeLocale.CGED_ParameterType));
                            }
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)), new GUIContent(CgeLocale.CGED_Condition));

                            if (
                                item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                                item.parameterType == ComboGestureDynamicsParameterType.Float ||
                                item.parameterType == ComboGestureDynamicsParameterType.Int)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)), new GUIContent(CgeLocale.CGED_Threshold));
                            }
                            if (item.contactReceiver.receiverType == ContactReceiver.ReceiverType.OnEnter)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterCurve)), new GUIContent(CgeLocale.CGED_OnEnterCurve));
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterDuration)), new GUIContent(CgeLocale.CGED_OnEnterDuration));
                            }
                            if (item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Constant && item.parameterType == ComboGestureDynamicsParameterType.Float)
                            {
                                // TODO: Shouldn't constant receivers always be hard threshold if they're floats???
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)), new GUIContent(CgeLocale.CGED_IsHardThreshold));
                            }

                            if (item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                                // TODO: Shouldn't constant receivers not have a upper bound even if they're floats???
                                item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Constant && item.parameterType == ComboGestureDynamicsParameterType.Float
                            )
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.upperBound)), new GUIContent(CgeLocale.CGED_UpperBound));
                            }
                        }
                    }
                    break;
                case ComboGestureDynamicsSource.PhysBone:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBone)), new GUIContent(CgeLocale.CGED_PhysBone));
                    if (item.physBone != null)
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBoneSource)), new GUIContent(CgeLocale.CGED_PhysBoneSource));

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.physBone.parameter);
                        EditorGUI.EndDisabledGroup();
                        if (string.IsNullOrEmpty(item.physBone.parameter))
                        {
                            EditorGUI.HelpBox(new Rect(rect.x + 10 * 0, rect.y + line * lineId, rect.width - 10 * 0, line * 2), CgeLocale.CGED_MissingParameterOnPhysBone, MessageType.Error);
                            lineId += 2;
                        }
                        else
                        {
                            if (item.physBoneSource == ComboGestureDynamicsPhysBoneSource.IsGrabbed)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)), new GUIContent(CgeLocale.CGED_ParameterType));
                            }
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)), new GUIContent(CgeLocale.CGED_Condition));
                            var isFloaty = item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Angle ||
                                    item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Stretch ||
                                    item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Squish ||
                                    item.parameterType == ComboGestureDynamicsParameterType.Float;
                            if (isFloaty || item.parameterType == ComboGestureDynamicsParameterType.Int)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)), new GUIContent(CgeLocale.CGED_Threshold));
                            }
                            if (isFloaty)
                            {
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)), new GUIContent(CgeLocale.CGED_IsHardThreshold));
                                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.upperBound)), new GUIContent(CgeLocale.CGED_UpperBound));
                            }
                        }
                    }
                    break;
                case ComboGestureDynamicsSource.Parameter:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterName)), new GUIContent(CgeLocale.CGED_ParameterName));
                    if (!string.IsNullOrEmpty(item.parameterName))
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)), new GUIContent(CgeLocale.CGED_ParameterType));
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)), new GUIContent(CgeLocale.CGED_Condition));
                        if (item.parameterType == ComboGestureDynamicsParameterType.Float || item.parameterType == ComboGestureDynamicsParameterType.Int)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)), new GUIContent(CgeLocale.CGED_Threshold));
                        }
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.behavesLikeOnEnter)), new GUIContent(CgeLocale.CGED_BehavesLikeOnEnter));
                        if (item.behavesLikeOnEnter)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterCurve)), new GUIContent(CgeLocale.CGED_OnEnterCurve));
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.onEnterDuration)), new GUIContent(CgeLocale.CGED_OnEnterDuration));
                        }
                        if (!item.behavesLikeOnEnter && item.parameterType == ComboGestureDynamicsParameterType.Float)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)), new GUIContent(CgeLocale.CGED_IsHardThreshold));
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.upperBound)), new GUIContent(CgeLocale.CGED_UpperBound));
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Rect Position(Rect rect, float line, ref int lineId, int shift = 0)
        {
            return new Rect(rect.x + 10 * shift, rect.y + line * lineId++, rect.width - 10 * shift, line);
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
                EditorGUILayout.LabelField("一部の翻訳は正確ではありません。cge.ja.jsonを編集することができます。");
            }

            EditorGUILayout.PropertyField(previewAnimator, new GUIContent(CgeLocale.CGEE_Preview_setup));
            _renderingCommands.SelectAnimator(((ComboGestureDynamics)target).previewAnimator);
            if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Regenerate_all_previews)))
            {
                _renderingCommands.Invalidate(Repaint);
            }

            EditorGUILayout.HelpBox(CgeLocale.CGED_Higher_priority, MessageType.Info);
            itemReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
