using System;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.Dynamics;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureSimpleDynamics))]
    public class ComboGestureSimpleDynamicsEditor : UnityEditor.Editor
    {
        private SerializedProperty previewAnimator;

        private ReorderableList itemReorderableList;
        private EeRenderingCommands _renderingCommands;

        private void OnEnable()
        {
            previewAnimator = serializedObject.FindProperty(nameof(ComboGestureSimpleDynamics.previewAnimator));

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            itemReorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty(nameof(ComboGestureSimpleDynamics.items)),
                true, true, true, true
            );
            itemReorderableList.drawElementCallback = DrawListElement;
            itemReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dynamics");
            itemReorderableList.elementHeightCallback = HeightListElement;
            itemReorderableList.onAddCallback = list =>
            {
                ++list.serializedProperty.arraySize;
                var element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.clip)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.bothEyesClosed)).boolValue = false;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.moodSet)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.physBoneSource)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.contactReceiver)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.physBone)).objectReferenceValue = null;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterName)).stringValue = "";
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterType)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.condition)).intValue = 0;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.threshold)).floatValue = 0f;
                element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.isHardThreshold)).boolValue = false;
            };

            _renderingCommands = new EeRenderingCommands();
        }

        private float HeightListElement(int index)
        {
            var item = ((ComboGestureSimpleDynamics)target).items[index];
            return EditorGUIUtility.singleLineHeight * 7
                   + (item.clip != null ? EditorGUIUtility.singleLineHeight * (4 + 1) : 0);
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = itemReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var line = EditorGUIUtility.singleLineHeight;

            var lineId = 0;
            var item = ((ComboGestureSimpleDynamics)target).items[index];

            ClipChoice(rect, item, line, lineId, element);
        }

        private void ClipChoice(Rect rect, ComboGestureSimpleDynamicsItem item, float line, int lineId, SerializedProperty element)
        {
            if (item.clip != null)
            {
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.clip)));
                _renderingCommands.SelectAnimator(((ComboGestureSimpleDynamics) target).previewAnimator);
                GUI.Box(new Rect(rect.x, rect.y + line * lineId, 150, line * 4), _renderingCommands.RequireRender(item.clip, Repaint).Normal);
                lineId += 4;
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.bothEyesClosed)));
                InputChoice(rect, item, line, lineId, element);
            }
            else if (item.moodSet != null)
            {
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.moodSet)));
                InputChoice(rect, item, line, lineId, element);
            }
            else
            {
                EditorGUI.LabelField(Position(rect, line, ref lineId), "Clip, Activity, or Puppet?", EditorStyles.boldLabel);
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.clip)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.moodSet)));
            }
        }

        private void InputChoice(Rect rect, ComboGestureSimpleDynamicsItem item, float line, int lineId, SerializedProperty element)
        {
            if (item.contactReceiver != null)
            {
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.contactReceiver)));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(Position(rect, line, ref lineId), "Type", Enum.GetName(typeof(ContactReceiver.ReceiverType), item.contactReceiver.receiverType));
                EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.contactReceiver.parameter);
                EditorGUI.EndDisabledGroup();
                if (item.contactReceiver.receiverType != ContactReceiver.ReceiverType.Proximity)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterType)));
                }
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.condition)));
                if (
                    item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Float ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Int)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.threshold)));
                }
                if (
                    item.contactReceiver.receiverType == ContactReceiver.ReceiverType.Proximity ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Float )
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.isHardThreshold)));
                }
            }
            else if (item.physBone != null)
            {
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.physBone)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.physBoneSource)));

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(Position(rect, line, ref lineId), "Parameter", item.physBone.parameter);
                EditorGUI.EndDisabledGroup();
                if (item.physBoneSource == ComboGestureSimpleDynamicsPhysBoneSource.IsGrabbed)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterType)));
                }
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.condition)));
                if (
                    item.physBoneSource == ComboGestureSimpleDynamicsPhysBoneSource.Angle ||
                    item.physBoneSource == ComboGestureSimpleDynamicsPhysBoneSource.Stretch ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Float ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Int)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.threshold)));
                }
                if (item.physBoneSource == ComboGestureSimpleDynamicsPhysBoneSource.Angle ||
                    item.physBoneSource == ComboGestureSimpleDynamicsPhysBoneSource.Stretch ||
                    item.parameterType == ComboGestureSimpleDynamicsParameterType.Float)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.isHardThreshold)));
                }
            }
            else if (!string.IsNullOrEmpty(item.parameterName))
            {
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterName)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterType)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.condition)));
                if (item.parameterType == ComboGestureSimpleDynamicsParameterType.Float || item.parameterType == ComboGestureSimpleDynamicsParameterType.Int)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.threshold)));
                }
                if (item.parameterType == ComboGestureSimpleDynamicsParameterType.Float)
                {
                    EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.isHardThreshold)));
                }
            }
            else
            {
                EditorGUI.LabelField(Position(rect, line, ref lineId), "Contact, PhysBone, or Parameter?", EditorStyles.boldLabel);
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.contactReceiver)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.physBone)));
                EditorGUI.PropertyField(Position(rect, line, ref lineId), element.FindPropertyRelative(nameof(ComboGestureSimpleDynamicsItem.parameterName)));
            }
        }

        private static Rect Position(Rect rect, float line, ref int lineId)
        {
            return new Rect(rect.x, rect.y + line * lineId++, rect.width, line);
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
