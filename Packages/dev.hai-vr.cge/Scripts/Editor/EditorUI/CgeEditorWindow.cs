﻿using System;
using System.Linq;
using System.Reflection;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public enum ActivityEditorMode
    {
        SetFaceExpressions,
        PreventEyesBlinking,
        AdditionalEditors
    }

    public enum PuppetEditorMode
    {
        ManipulateTrees,
        PreventEyesBlinking
    }

    public enum AdditionalEditorsMode
    {
        CreateBlendTrees,
        ViewBlendTrees,
        CombineFaceExpressions
    }

    public class CgeEditorWindow : EditorWindow
    {
        private const int RightSpace = 30 + 120 + 220 + 170;
        private CgeLayoutCommon _common;
        private CgeEditorHandler _editorHandler;
        private CgeLayoutPreventEyesBlinking _layoutPreventEyesBlinking;
        private CgeLayoutFaceExpressionCombiner _layoutFaceExpressionCombiner;
        private CgeLayoutSetFaceExpressions _layoutSetFaceExpressions;
        private CgeLayoutManipulateTrees _layoutManipulateTrees;

        private Vector2 _scrollPos;
        private Texture _helpIcon16;
        private EeRenderingCommands _renderingCommands;
        private static Animator _veeAnimator;
        private static AnimationClip _veeLastClip;
        public CgeWindowHandler WindowHandler { get; private set; }

        private void OnFocus()
        {
            if (_veeLastClip != null)
            {
                _renderingCommands.InvalidateSome(Repaint, _veeLastClip);
            }
        }

        private void OnEnable()
        {
            _renderingCommands = new EeRenderingCommands();
            _editorHandler = new CgeEditorHandler(new CgeEditorState());
            var blendTreeHandler = new CgeBlendTreeHandler();
            _common = new CgeLayoutCommon(Repaint, _renderingCommands);
            _common.GuiInit();
            var driver = new CgeActivityEditorDriver(_editorHandler);
            _layoutPreventEyesBlinking = new CgeLayoutPreventEyesBlinking(_common, _editorHandler);
            _layoutFaceExpressionCombiner = new CgeLayoutFaceExpressionCombiner(_common, driver, _editorHandler, _renderingCommands);
            _layoutSetFaceExpressions = new CgeLayoutSetFaceExpressions(_common, driver, _layoutFaceExpressionCombiner /* FIXME it is not normal to inject the layout here */, _editorHandler, Repaint, blendTreeHandler);
            _layoutManipulateTrees = new CgeLayoutManipulateTrees(_common, _editorHandler, blendTreeHandler);

            WindowHandler = new CgeWindowHandler(this, _editorHandler);

            _helpIcon16 = ComboGestureIcons.Instance.Help16;
        }

        private void OnInspectorUpdate()
        {
            var active = Selection.activeGameObject;
            if (active == null) return;

            TryNowEditingAnActivity(active);
            TryNowEditingAPuppet(active);
        }

        private void TryNowEditingAnActivity(GameObject active)
        {
            var selectedActivity = active.GetComponent<ComboGestureActivity>();
            if (selectedActivity != null && selectedActivity != _editorHandler.GetActivity())
            {
                WindowHandler.RetargetActivity(selectedActivity);
                Repaint();
            }
        }

        private void TryNowEditingAPuppet(GameObject active)
        {
            var selectedPuppet = active.GetComponent<ComboGesturePuppet>();
            if (selectedPuppet != null && selectedPuppet != _editorHandler.GetPuppet())
            {
                WindowHandler.RetargetPuppet(selectedPuppet);
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (_editorHandler.GetCurrentlyEditing() == CurrentlyEditing.Nothing)
            {
                return;
            }

            if (_editorHandler.IsFirstTimeSetup() && _editorHandler.IsPreviewSetupValid())
            {
                _editorHandler.ClearFirstTimeSetup();
            }


            // GUILayout.BeginArea(new Rect(position.width - 320 - 170, CgeLayoutCommon.SingleLineHeight * 2 + 5, 200, CgeLayoutCommon.SingleLineHeight + 2));
            // if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Regenerate_all_previews), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            // {
            //     _activityPreviewQueryAggregator.GenerateAll(Repaint);
            // }
            // GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 320, CgeLayoutCommon.SingleLineHeight * 2 + 5, 200, CgeLayoutCommon.SingleLineHeight + 2));
            if (GUILayout.Button(new GUIContent("❈ Visual Expressions Editor"), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                ShowExpressionsEditor(_editorHandler, null);
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 130, CgeLayoutCommon.SingleLineHeight * 2 + 5, 100, CgeLayoutCommon.SingleLineHeight + 2));
            if (GUILayout.Button(new GUIContent(" " + CgeLocale.CGEE_Tutorials, _helpIcon16), GUILayout.Width(100), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                Application.OpenURL(CgeLocale.DocumentationUrl());
            }
            GUILayout.EndArea();

            switch (_editorHandler.GetCurrentlyEditing())
            {
                case CurrentlyEditing.Nothing:
                    break;
                case CurrentlyEditing.Activity:
                    EditingAnActivity();
                    break;
                case CurrentlyEditing.Puppet:
                    EditingAPuppet();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void ShowExpressionsEditor(CgeEditorHandler cgeEditorHandler, AnimationClip clipNullable)
        {
            var newAnimator = cgeEditorHandler.PreviewSetup();
            var visualExpressionsEditorType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .First(type => type.Name == "VisualExpressionsEditorWindow");
            if (_veeAnimator != newAnimator && newAnimator != null)
            {
                visualExpressionsEditorType.GetMethod("OpenEditor", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] {new MenuCommand(newAnimator)});
            }
            else
            {
                visualExpressionsEditorType.GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] {});
            }

            if (clipNullable != null)
            {
                if (Selection.activeObject is AnimationClip)
                {
                    // ChangeClip will have no effect if the current selection was already a clip, so we force it here.
                    Selection.activeObject = clipNullable;
                }
                
                var instance = visualExpressionsEditorType.GetMethod("Obtain", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] {});
                visualExpressionsEditorType.GetMethod("ChangeClip", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(instance, new object[] {clipNullable});
                _veeLastClip = clipNullable;
            }

            _veeAnimator = newAnimator;
        }

        private void EditingAnActivity()
        {
            _editorHandler.SpUpdate();

            CreateActivityToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorHandler.CurrentActivityMode())
            {
                case ActivityEditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case ActivityEditorMode.AdditionalEditors:
                    switch (_editorHandler.GetAdditionalEditor())
                    {
                        case AdditionalEditorsMode.CreateBlendTrees:
                            _layoutManipulateTrees.LayoutAssetCreator(position);
                            break;
                        case AdditionalEditorsMode.ViewBlendTrees:
                            _layoutManipulateTrees.LayoutTreeViewer(position);
                            break;
                        case AdditionalEditorsMode.CombineFaceExpressions:
                            _layoutFaceExpressionCombiner.Layout(Repaint);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                // ReSharper disable once RedundantCaseLabel
                case ActivityEditorMode.SetFaceExpressions:
                default:
                    _layoutSetFaceExpressions.Layout(position);
                    break;
            }
            GUILayout.EndScrollView();

            _editorHandler.ApplyModifiedProperties();
        }

        private void CreateActivityToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, singleLineHeight * 4));

            TopBar();

            _editorHandler.SwitchTo((ActivityEditorMode) GUILayout.Toolbar((int) _editorHandler.CurrentActivityMode(), new[]
            {
                CgeLocale.CGEE_Set_face_expressions, CgeLocale.CGEE_Prevent_eyes_blinking, CgeLocale.CGEE_Additional_editors
            }));
            if (_editorHandler.CurrentActivityMode() == ActivityEditorMode.SetFaceExpressions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                EditorGUILayout.LabelField(new GUIContent(CgeLocale.CGEE_Mode), GUILayout.Width(100));
                EditorGUILayout.PropertyField(_editorHandler.SpActivityMode(), GUIContent.none, GUILayout.Width(250));
                if (_editorHandler.GetActivity().activityMode == ComboGestureActivity.CgeActivityMode.Permutations)
                {
                    _editorHandler.SpEditorTool().intValue = GUILayout.Toolbar(_editorHandler.SpEditorTool().intValue, new[] {CgeLocale.CGEE_Simplified_view, CgeLocale.CGEE_Complete_view});
                }
                GUILayout.Space(RightSpace);
                GUILayout.EndHorizontal();
                _editorHandler.SwitchCurrentEditorToolTo(_editorHandler.SpEditorTool().intValue);
            }

            else if (_editorHandler.CurrentActivityMode() == ActivityEditorMode.AdditionalEditors)
            {
                CreateAdditionalEditorsToolbar();
            }

            GUILayout.EndArea();
        }

        private void TopBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_editorHandler.SpPreviewSetup(), new GUIContent(CgeLocale.CGEE_Preview_setup));
            _renderingCommands.SelectAnimator((Animator)_editorHandler.SpPreviewSetup().objectReferenceValue); // FIXME: WTF
            if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Regenerate_all_previews), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                _renderingCommands.Invalidate(Repaint);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void EditingAPuppet()
        {
            _editorHandler.SpUpdate();

            CreatePuppetToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorHandler.CurrentPuppetMode())
            {
                case PuppetEditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case PuppetEditorMode.ManipulateTrees:
                default:
                    _layoutManipulateTrees.Layout(position);
                    break;
            }
            GUILayout.EndScrollView();

            _editorHandler.ApplyModifiedProperties();
        }

        private void CreatePuppetToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, singleLineHeight * 4));
            TopBar();

            _editorHandler.SwitchTo((PuppetEditorMode) GUILayout.Toolbar((int) _editorHandler.CurrentPuppetMode(), new[]
            {
                CgeLocale.CGEE_Manipulate_trees, CgeLocale.CGEE_Prevent_eyes_blinking
            }));
            GUILayout.EndArea();
        }

        private void CreateAdditionalEditorsToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _editorHandler.SwitchAdditionalEditorTo((AdditionalEditorsMode)GUILayout.Toolbar((int)_editorHandler.GetAdditionalEditor(), new[]
            {
                CgeLocale.CGEE_Create_blend_trees, CgeLocale.CGEE_View_blend_trees, CgeLocale.CGEE_Combine_expressions
            }, GUILayout.ExpandWidth(true)));

            GUILayout.Space(RightSpace);
            GUILayout.EndHorizontal();
        }
    }
}
