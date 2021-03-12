using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ExpressionsEditor.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutOtherOptions
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeActivityPreviewQueryAggregator _activityPreviewQueryAggregator;

        public CgeLayoutOtherOptions(CgeLayoutCommon common, CgeEditorEffector editorEffector, CgeActivityPreviewQueryAggregator activityPreviewQueryAggregator)
        {
            _common = common;
            _editorEffector = editorEffector;
            _activityPreviewQueryAggregator = activityPreviewQueryAggregator;
        }

        public void Layout(Action repaintCallback, Rect position)
        {
            _common.BeginLayoutUsingWidth(position, CgeLayoutCommon.GuiSquareHeight * 2, 0, CgeLayoutCommon.GuiSquareWidth * 4);
            if (_editorEffector.IsFirstTimeSetup() || !_editorEffector.IsPreviewSetupValid())
            {
                if (_editorEffector.HasPreviewSetupWhichIsInvalid())
                {
                    EditorGUILayout.LabelField(CgeLocale.CGEE_IncompletePreviewSetup);
                }

                EditorGUILayout.PropertyField(_editorEffector.SpPreviewSetup(), new GUIContent(CgeLocale.CGEE_Preview_setup));
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(CgeLocale.CGEE_Automatically_setup_preview, GUILayout.Height(50), GUILayout.Width(300)))
                {
                    DoAutoSetupPreview();
                    if (_editorEffector.GetSetupResult() != EePreviewSetupWizard.SetupResult.NoAvatarFound)
                    {
                        _editorEffector.SwitchTo(ActivityEditorMode.SetFaceExpressions);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(_editorEffector.SpTransitionDuration(), new GUIContent(CgeLocale.CGEE_Transition_duration));
                EditorGUILayout.PropertyField(_editorEffector.SpPreviewSetup(), new GUIContent(CgeLocale.CGEE_Preview_setup));
                if (_editorEffector.GetSetupResult() != null)
                {
                    var setupResult = (EePreviewSetupWizard.SetupResult) _editorEffector.GetSetupResult();
                    switch (setupResult)
                    {
                        case EePreviewSetupWizard.SetupResult.ReusedExistsAndValidInScene:
                            EditorGUILayout.HelpBox(CgeLocale.CGEE_AutoSetupReused, MessageType.Info);
                            break;
                        case EePreviewSetupWizard.SetupResult.NoAvatarFound:
                            EditorGUILayout.HelpBox(CgeLocale.CGEE_AutoSetupNoActiveAvatarDescriptor, MessageType.Error);
                            break;
                        case EePreviewSetupWizard.SetupResult.CreatedNew:
                            EditorGUILayout.HelpBox(CgeLocale.CGEE_AutoSetupCreated, MessageType.Info);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                if (GUILayout.Button(CgeLocale.CGEE_Generate_missing_previews))
                {
                    _activityPreviewQueryAggregator.GenerateMissingPreviews(repaintCallback);
                }

                if (GUILayout.Button(CgeLocale.CGEE_Regenerate_all_previews))
                {
                    _activityPreviewQueryAggregator.GenerateAll(repaintCallback);
                }
                if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Activity)
                {
                    EditorGUILayout.PropertyField(_editorEffector.SpEditorArbitraryAnimations(), new GUIContent(CgeLocale.CGEE_List_of_arbitrary_animations), true);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!AnimationMode.InAnimationMode());
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
