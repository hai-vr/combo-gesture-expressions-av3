using System;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(ComboGestureVRCFaceTrackingFTVendor))]
    public class ComboGestureVRCFaceTrackingFTVendorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ComboGestureFTVendor.expressionParameters)));
            EditorGUILayout.LabelField("VRCFaceTracking vendor", EditorStyles.boldLabel);
            EditorGUILayout.TextField("https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters");
            EditorGUILayout.HelpBox(@"This is NOT an endorsement.

It is INHERENTLY DANGEROUS to run code that someone else has written. It is your responsibility to exercise caution when running projects.", MessageType.Warning);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ComboGestureFTVendor.debugShowInfluences)));
            
            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Eye_Tracking_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.EyesX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.EyesY),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeLid),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeLid),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CombinedEyeLid),
                nameof(ComboGestureVRCFaceTrackingFTVendor.EyesWiden),
                nameof(ComboGestureVRCFaceTrackingFTVendor.EyesDilation),
                nameof(ComboGestureVRCFaceTrackingFTVendor.EyesSqueeze),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeY),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeY),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeWiden),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeWiden),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeSqueeze),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeSqueeze),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeLidExpanded),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeLidExpanded),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CombinedEyeLidExpanded),
                nameof(ComboGestureVRCFaceTrackingFTVendor.LeftEyeLidExpandedSqueeze),
                nameof(ComboGestureVRCFaceTrackingFTVendor.RightEyeLidExpandedSqueeze),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CombinedEyeLidExpandedSqueeze));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Lip_Tracking_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawForward),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpen),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthApeShape),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthSmileRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthSmileLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthSadRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthSadLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CheekPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CheekPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.CheekSuck),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerOverlay),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueLongStep1),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueLongStep2),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueDown),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueUp),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueRoll),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueUpLeftMorph),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueUpRightMorph),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueDownLeftMorph),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueDownRightMorph));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_General_Combined_Lip_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpper),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLower),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileSadRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileSadLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileSad),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueY),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueX),
                nameof(ComboGestureVRCFaceTrackingFTVendor.TongueSteps),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffSuckRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffSuckLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffSuck));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Jaw_Open_Combined_Lip_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenPuff),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenSuck),
                nameof(ComboGestureVRCFaceTrackingFTVendor.JawOpenForward));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Upper_Up_Right_Combined_Lip_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRightUpperInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRightPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRightApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRightPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpRightOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Upper_Up_Left_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeftUpperInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeftPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeftApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeftPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpLeftOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Upper_Up_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpUpperInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpPuff),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthUpperUpOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Lower_Down_Right_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRightLowerInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRightPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRightApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRightPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownRightOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Lower_Down_Left_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeftLowerInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeftPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeftApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeftPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLeftOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Mouth_Lower_Down_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownLowerInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownInside),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownPuff),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownPuffLeft),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownPuffRight),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownPout),
                nameof(ComboGestureVRCFaceTrackingFTVendor.MouthLowerDownOverlay));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Smile_Right_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightOverlay),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileRightPout));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Smile_Left_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftOverlay),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLeftPout));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Smile_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileApe),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmileOverlay),
                nameof(ComboGestureVRCFaceTrackingFTVendor.SmilePout));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Cheek_Puff_Right_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffRightUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffRightLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffRightOverturn));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Cheek_Puff_Left_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffLeftUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffLeftLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffLeftOverturn));

            DisplayGroupFor(nameof(ComboGestureVRCFaceTrackingFTVendor.GROUP_Cheek_Puff_Combined_Parameters),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffUpperOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffLowerOverturn),
                nameof(ComboGestureVRCFaceTrackingFTVendor.PuffOverturn));
                
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayGroupFor(string groupPropertyName, params string[] constituents)
        {
            var vendor = ((ComboGestureFTVendor)target);
            var expressionParametersNullable = vendor.expressionParameters;
            // var group = serializedObject.FindProperty(groupPropertyName);
            var map = ((ComboGestureVRCFaceTrackingFTVendor)target).ExposeMap();
            EditorGUILayout.LabelField(groupPropertyName.Replace("GROUP_", "").Replace("_", " "), EditorStyles.boldLabel);
            // EditorGUILayout.PropertyField(group, new GUIContent("Group"));
            // var groupValue = (CgeVendorGroup)group.intValue;
            // var isSome = groupValue == CgeVendorGroup.Some;
            // EditorGUI.BeginDisabledGroup(!isSome);
            EditorGUILayout.BeginVertical("GroupBox");
            foreach (var constituent in constituents)
            {
                // FieldFor(constituent, groupValue);
                EditorGUILayout.BeginHorizontal();
                var sp = serializedObject.FindProperty(constituent);
                EditorGUILayout.PropertyField(sp, new GUIContent(constituent));
                if (!serializedObject.isEditingMultipleObjects && expressionParametersNullable != null)
                {
                    var isOn = sp.boolValue;
                    var contains = expressionParametersNullable.parameters.Any(parameter => parameter.name == constituent);
                    if (isOn && !contains)
                    {
                        if (GUILayout.Button("Update params"))
                        {
                            var newArray = expressionParametersNullable.parameters.Concat(new[]
                            {
                                new VRCExpressionParameters.Parameter
                                {
                                    name = constituent,
                                    defaultValue = 0f,
                                    saved = false,
                                    valueType = VRCExpressionParameters.ValueType.Float
                                }
                            }).ToArray();
                            expressionParametersNullable.parameters = newArray;
                            EditorUtility.SetDirty(expressionParametersNullable);
                        }
                    }
                    if (!isOn && contains)
                    {
                        if (GUILayout.Button("Update params"))
                        {
                            var newArray = expressionParametersNullable.parameters
                                .Where(parameter => parameter.name != constituent)
                                .ToArray();
                            expressionParametersNullable.parameters = newArray;
                            EditorUtility.SetDirty(expressionParametersNullable);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (vendor.debugShowInfluences == ComboGestureFTVendor.CgeDebugInfluence.All || vendor.debugShowInfluences == ComboGestureFTVendor.CgeDebugInfluence.OnlyActive && sp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(string.Join(", ", map[constituent].Select(actuator => actuator.element + $"[{actuator.actuator.neutral}:{actuator.actuator.actuated}]").ToArray()));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            // EditorGUI.EndDisabledGroup();
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