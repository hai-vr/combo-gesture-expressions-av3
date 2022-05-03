﻿using System;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
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
            };

            _renderingCommands = new EeRenderingCommands();
        }

        private float HeightListElement(int index)
        {
            var item = ((ComboGestureDynamics)target).items[index];
            return EditorGUIUtility.singleLineHeight * 13
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
            EditorGUI.LabelField(Position(rect, line, ref lineId), "Dynamic Expression", EditorStyles.boldLabel);
            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.effect)));

            switch (item.effect)
            {
                case ComboGestureDynamicsEffect.Clip:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.clip)));
                    if (item.clip != null)
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.bothEyesClosed)));
                        GUI.Box(new Rect(rect.x + 10, rect.y + line * lineId, 150, line * 4), _renderingCommands.RequireRender(item.clip, Repaint).Normal);
                        lineId += 4;
                    }
                    break;
                case ComboGestureDynamicsEffect.MoodSet:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.moodSet)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lineId += 1;

            EditorGUI.LabelField(Position(rect, line, ref lineId), "Dynamic Source", EditorStyles.boldLabel);
            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.source)));

            switch (item.source)
            {
                case ComboGestureDynamicsSource.Contact:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.contactReceiver)));
                    if (item.contactReceiver != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Type", Enum.GetName(typeof(ContactReceiver.ReceiverType), item.contactReceiver.receiverType));
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.contactReceiver.parameter);
                        EditorGUI.EndDisabledGroup();
                        if (item.contactReceiver.receiverType != ContactReceiver.ReceiverType.Proximity)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)));
                        }
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)));
                        if (
                            item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                            item.parameterType == ComboGestureDynamicsParameterType.Float ||
                            item.parameterType == ComboGestureDynamicsParameterType.Int)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)));
                        }
                        if (
                            item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                            item.parameterType == ComboGestureDynamicsParameterType.Float )
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)));
                        }
                    }
                    break;
                case ComboGestureDynamicsSource.PhysBone:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBone)));
                    if (item.physBone != null)
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.physBoneSource)));

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.physBone.parameter);
                        EditorGUI.EndDisabledGroup();
                        if (item.physBoneSource == ComboGestureDynamicsPhysBoneSource.IsGrabbed)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)));
                        }
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)));
                        if (
                            item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Angle ||
                            item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Stretch ||
                            item.parameterType == ComboGestureDynamicsParameterType.Float ||
                            item.parameterType == ComboGestureDynamicsParameterType.Int)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)));
                        }
                        if (item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Angle ||
                            item.physBoneSource == ComboGestureDynamicsPhysBoneSource.Stretch ||
                            item.parameterType == ComboGestureDynamicsParameterType.Float)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)));
                        }
                    }
                    break;
                case ComboGestureDynamicsSource.Parameter:
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterName)));
                    if (!string.IsNullOrEmpty(item.parameterName))
                    {
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.parameterType)));
                        EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.condition)));
                        if (item.parameterType == ComboGestureDynamicsParameterType.Float || item.parameterType == ComboGestureDynamicsParameterType.Int)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.threshold)));
                        }
                        if (item.parameterType == ComboGestureDynamicsParameterType.Float)
                        {
                            EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureDynamicsItem.isHardThreshold)));
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
                EditorGUILayout.LabelField("一部の翻訳は正確ではありません。cge.jp.jsonを編集することができます。");
            }

            EditorGUILayout.PropertyField(previewAnimator);
            itemReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}