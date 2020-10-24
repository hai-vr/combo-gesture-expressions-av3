using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutOtherOptions
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgePreviewEffector _previewController;

        public CgeLayoutOtherOptions(CgeLayoutCommon common, CgeEditorEffector editorEffector, CgePreviewEffector previewController)
        {
            _common = common;
            _editorEffector = editorEffector;
            _previewController = previewController;
        }

        public void Layout(Action repaintCallback, Rect position)
        {
            _common.BeginLayoutUsingWidth(position, CgeLayoutCommon.GuiSquareHeight * 2, 0, CgeLayoutCommon.GuiSquareWidth * 4);
            if (_editorEffector.IsFirstTimeSetup() || !_editorEffector.IsPreviewSetupValid())
            {
                if (_editorEffector.HasPreviewSetupWhichIsInvalid())
                {
                    EditorGUILayout.LabelField("A preview setup was found but it is incomplete or invalid.");
                }

                EditorGUILayout.PropertyField(_editorEffector.SpPreviewSetup(), new GUIContent("Preview setup"));
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Automatically setup preview!", GUILayout.Height(50), GUILayout.Width(300)))
                {
                    DoAutoSetupPreview();
                    if (_editorEffector.GetSetupResult() != AutoSetupPreview.SetupResult.NoAvatarFound)
                    {
                        _editorEffector.SwitchTo(ActivityEditorMode.SetFaceExpressions);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(_editorEffector.SpTransitionDuration(), new GUIContent("Transition duration (s)"));
                EditorGUILayout.PropertyField(_editorEffector.SpPreviewSetup(), new GUIContent("Preview setup"));
                if (_editorEffector.GetSetupResult() != null)
                {
                    var setupResult = (AutoSetupPreview.SetupResult) _editorEffector.GetSetupResult();
                    switch (setupResult)
                    {
                        case AutoSetupPreview.SetupResult.ReusedExistsAndValidInScene:
                            EditorGUILayout.HelpBox("The scene already contains a preview setup. It has been reused here.", MessageType.Info);
                            break;
                        case AutoSetupPreview.SetupResult.NoAvatarFound:
                            EditorGUILayout.HelpBox("No active avatar descriptor was found in the root objects of the scene.", MessageType.Error);
                            break;
                        case AutoSetupPreview.SetupResult.CreatedNew:
                            EditorGUILayout.HelpBox("A new preview setup was created.", MessageType.Info);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                if (GUILayout.Button("Generate missing previews"))
                {
                    _previewController.GenerateMissingPreviews(repaintCallback);
                }

                if (GUILayout.Button("Regenerate all previews"))
                {
                    _previewController.GenerateAll(repaintCallback);
                }
                if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Activity)
                {
                    EditorGUILayout.PropertyField(_editorEffector.SpEditorArbitraryAnimations(), new GUIContent("List of arbitrary animations to &s (Drag and drop assets directly on this title)"), true);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!AnimationMode.InAnimationMode());
                if (GUILayout.Button("Stop generating previews"))
                {
                    CgePreviewProcessor.Stop_Temp();
                }
                EditorGUI.EndDisabledGroup();

                if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Activity)
                {
                    if (_editorEffector.GetActivity().editorArbitraryAnimations != null)
                    {
                        GUILayout.BeginArea(new Rect(0, CgeLayoutCommon.SingleLineHeight * 10, position.width, CgeLayoutCommon.GuiSquareHeight * 8));
                        var allClips = new HashSet<AnimationClip>(_editorEffector.GetActivity().editorArbitraryAnimations.Where(clip => clip != null)).ToList();
                        var mod = (int) Math.Max(1, position.width / CgeLayoutCommon.GuiSquareWidth);
                        for (var element = 0; element < allClips.Count; element++)
                        {
                            GUILayout.BeginArea(CgeLayoutCommon.RectAt(element % mod, element / mod));
                            DrawArbitrary(allClips[element]);
                            GUILayout.EndArea();
                        }

                        GUILayout.EndArea();
                        GUILayout.Space((allClips.Count / mod) * CgeLayoutCommon.GuiSquareHeight + CgeLayoutCommon.GuiSquareHeight + CgeLayoutCommon.SingleLineHeight * 2);
                    }
                }
            }
            CgeLayoutCommon.EndLayout();
        }

        private void DoAutoSetupPreview()
        {
            _editorEffector.TryAutoSetup();
        }

        private void DrawArbitrary(AnimationClip element)
        {
            GUILayout.BeginArea(new Rect((CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth) / 2, 0, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
            _common.DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(CgeLayoutCommon.PictureHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element, typeof(AnimationClip), true);
            EditorGUI.EndDisabledGroup();
        }
    }
}
