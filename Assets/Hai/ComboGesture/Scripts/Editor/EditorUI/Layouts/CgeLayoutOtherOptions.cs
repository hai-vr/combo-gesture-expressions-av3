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
                    if (_editorEffector.PreviewSetup() != null)
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

                EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                if (GUILayout.Button(CgeLocale.CGEE_Generate_missing_previews))
                {
                    _activityPreviewQueryAggregator.GenerateMissingPreviews(repaintCallback);
                }

                if (GUILayout.Button(CgeLocale.CGEE_Regenerate_all_previews))
                {
                    _activityPreviewQueryAggregator.GenerateAll(repaintCallback);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!AnimationMode.InAnimationMode());
                EditorGUI.EndDisabledGroup();
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
