using System;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureAkaneFacialOSCFTVendor))]
    public class ComboGestureAkaneFacialOSCFTVendorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DisplayGroupFor(
                nameof(ComboGestureAkaneFacialOSCFTVendor.GROUP_目の周りのデータ__SDKの計算方法と同様に計算した値),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Blink),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Wide),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Up),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Down),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Blink),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Wide),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Up),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Down),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Frown),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Frown),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left_Squeeze),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right_Squeeze));

            DisplayGroupFor(
                nameof(ComboGestureAkaneFacialOSCFTVendor.GROUP_視線__アプリ内で計算された値),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Left_Vertical),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Left_Horizontal),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Right_Vertical),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Right_Horizontal),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Vertical),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Gaze_Horizontal));

            DisplayGroupFor(
                nameof(ComboGestureAkaneFacialOSCFTVendor.GROUP_目__計算処理済みの値),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Blink),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Wide),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Up),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Down),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Frown),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Eye_Squeeze));

            DisplayGroupFor(
                nameof(ComboGestureAkaneFacialOSCFTVendor.GROUP_顔__トラッカで取得した生の値),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Jaw_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Jaw_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Jaw_Forward),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Jaw_Open),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Ape_Shape),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Overturn),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Overturn),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Pout),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Smile_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Smile_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Cheek_Puff_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Cheek_Puff_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Cheek_Suck),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_UpRight),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_UpLeft),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_DownRight),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_DownLeft),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Inside),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Inside),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Overlay),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_LongStep1),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_LongStep2),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Down),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Up),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Roll),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_UpLeft_Morph),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_UpRight_Morph),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_DownLeft_Morph),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_DownRight_Morph));

            DisplayGroupFor(
                nameof(ComboGestureAkaneFacialOSCFTVendor.GROUP_顔__アプリ内で計算_統合したデータ),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Jaw_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad_Smile_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad_Smile_Left),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Smile),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Sad_Smile),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Inside_Overturn),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Inside_Overturn),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Cheek_Puff),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Cheek_Suck_Puff),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Upper_Up),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Mouth_Lower_Down),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Left_Right),
                nameof(ComboGestureAkaneFacialOSCFTVendor.Tongue_Down_Up));
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayGroupFor(string groupPropertyName, params string[] constituents)
        {
            var group = serializedObject.FindProperty(groupPropertyName);
            EditorGUILayout.LabelField(groupPropertyName.Replace("GROUP_", "").Replace("_", " "), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(group, new GUIContent("Group"));
            var groupValue = (CgeVendorGroup)group.intValue;
            var isSome = groupValue == CgeVendorGroup.Some;
            EditorGUI.BeginDisabledGroup(!isSome);
            EditorGUILayout.BeginVertical("GroupBox");
            foreach (var constituent in constituents)
            {
                FieldFor(constituent, groupValue);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }
        
        private void FieldFor(string propertyName, CgeVendorGroup groupValue)
        {
            switch (groupValue)
            {
                case CgeVendorGroup.None:
                    EditorGUILayout.Toggle(propertyName, false);
                    break;
                case CgeVendorGroup.All:
                    EditorGUILayout.Toggle(propertyName, true);
                    break;
                case CgeVendorGroup.Some:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(propertyName));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(groupValue), groupValue, null);
            }
        }
    }
}