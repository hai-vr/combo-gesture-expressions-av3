using System;
using System.Linq;
using System.Reflection;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
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
        private CgeEditorEffector _editorEffector;
        private CgeLayoutPreventEyesBlinking _layoutPreventEyesBlinking;
        private CgeLayoutFaceExpressionCombiner _layoutFaceExpressionCombiner;
        private CgeLayoutSetFaceExpressions _layoutSetFaceExpressions;
        private CgeLayoutManipulateTrees _layoutManipulateTrees;

        private Vector2 _scrollPos;
        private Texture _helpIcon16;
        private EeRenderingCommands _renderingCommands;
        public CgeWindowHandler WindowHandler { get; private set; }

        private void OnEnable()
        {
            _renderingCommands = new EeRenderingCommands();
            _editorEffector = new CgeEditorEffector(new CgeEditorState());
            var blendTreeEffector = new CgeBlendTreeEffector();
            _common = new CgeLayoutCommon(Repaint, _renderingCommands);
            var driver = new CgeActivityEditorDriver(_editorEffector);
            _layoutPreventEyesBlinking = new CgeLayoutPreventEyesBlinking(_common, _editorEffector);
            _layoutFaceExpressionCombiner = new CgeLayoutFaceExpressionCombiner(_common, driver, _editorEffector, _renderingCommands);
            _layoutSetFaceExpressions = new CgeLayoutSetFaceExpressions(_common, driver, _layoutFaceExpressionCombiner /* FIXME it is not normal to inject the layout here */, _editorEffector, Repaint, blendTreeEffector);
            _layoutManipulateTrees = new CgeLayoutManipulateTrees(_common, _editorEffector, blendTreeEffector);

            WindowHandler = new CgeWindowHandler(this, _editorEffector);

            _common.GuiInit();
            _helpIcon16 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/help-16.png");
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
            if (selectedActivity != null && selectedActivity != _editorEffector.GetActivity())
            {
                WindowHandler.RetargetActivity(selectedActivity);
                Repaint();
            }
        }

        private void TryNowEditingAPuppet(GameObject active)
        {
            var selectedPuppet = active.GetComponent<ComboGesturePuppet>();
            if (selectedPuppet != null && selectedPuppet != _editorEffector.GetPuppet())
            {
                WindowHandler.RetargetPuppet(selectedPuppet);
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Nothing)
            {
                return;
            }

            if (_editorEffector.IsFirstTimeSetup() && _editorEffector.IsPreviewSetupValid())
            {
                _editorEffector.ClearFirstTimeSetup();
            }


            // GUILayout.BeginArea(new Rect(position.width - 320 - 170, CgeLayoutCommon.SingleLineHeight * 2 + 5, 200, CgeLayoutCommon.SingleLineHeight + 2));
            // if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Regenerate_all_previews), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            // {
            //     _activityPreviewQueryAggregator.GenerateAll(Repaint);
            // }
            // GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 320, CgeLayoutCommon.SingleLineHeight * 2 + 5, 200, CgeLayoutCommon.SingleLineHeight + 2));
            if (GUILayout.Button(new GUIContent("❈ ExpressionsEditor"), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                ShowExpressionsEditor(_editorEffector);
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 130, CgeLayoutCommon.SingleLineHeight * 2 + 5, 100, CgeLayoutCommon.SingleLineHeight + 2));
            if (GUILayout.Button(new GUIContent(" " + CgeLocale.CGEE_Tutorials, _helpIcon16), GUILayout.Width(100), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                Application.OpenURL(CgeLocale.DocumentationUrl());
            }
            GUILayout.EndArea();

            switch (_editorEffector.GetCurrentlyEditing())
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

        public static void ShowExpressionsEditor(CgeEditorEffector cgeEditorEffector)
        {
            var visualExpressionsEditorType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .First(type => type.Name == "VisualExpressionsEditorWindow");
            visualExpressionsEditorType.GetMethod("OpenEditor", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] {new MenuCommand(cgeEditorEffector.PreviewSetup())});
        }

        private void EditingAnActivity()
        {
            _editorEffector.SpUpdate();

            CreateActivityToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorEffector.CurrentActivityMode())
            {
                case ActivityEditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case ActivityEditorMode.AdditionalEditors:
                    switch (_editorEffector.GetAdditionalEditor())
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

            _editorEffector.ApplyModifiedProperties();
        }

        private void CreateActivityToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, singleLineHeight * 4));

            TopBar();

            _editorEffector.SwitchTo((ActivityEditorMode) GUILayout.Toolbar((int) _editorEffector.CurrentActivityMode(), new[]
            {
                CgeLocale.CGEE_Set_face_expressions, CgeLocale.CGEE_Prevent_eyes_blinking, CgeLocale.CGEE_Additional_editors
            }));
            if (_editorEffector.CurrentActivityMode() == ActivityEditorMode.SetFaceExpressions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                EditorGUILayout.PropertyField(_editorEffector.SpActivityMode(), GUIContent.none, GUILayout.Width(200));
                if (_editorEffector.GetActivity().activityMode == ComboGestureActivity.CgeActivityMode.Permutations)
                {
                    _editorEffector.SpEditorTool().intValue = GUILayout.Toolbar(_editorEffector.SpEditorTool().intValue, new[] {CgeLocale.CGEE_Simplified_view, CgeLocale.CGEE_Complete_view});
                }
                GUILayout.Space(RightSpace);
                GUILayout.EndHorizontal();
                _editorEffector.SwitchCurrentEditorToolTo(_editorEffector.SpEditorTool().intValue);
            }

            else if (_editorEffector.CurrentActivityMode() == ActivityEditorMode.AdditionalEditors)
            {
                CreateAdditionalEditorsToolbar();
            }

            GUILayout.EndArea();
        }

        private void TopBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_editorEffector.SpPreviewSetup(), new GUIContent(CgeLocale.CGEE_Preview_setup));
            _renderingCommands.SelectAnimator((Animator)_editorEffector.SpPreviewSetup().objectReferenceValue); // FIXME: WTF
            if (GUILayout.Button(new GUIContent(CgeLocale.CGEE_Regenerate_all_previews), GUILayout.Width(170), GUILayout.Height(CgeLayoutCommon.SingleLineHeight + 2)))
            {
                _renderingCommands.Invalidate(Repaint);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void EditingAPuppet()
        {
            _editorEffector.SpUpdate();

            CreatePuppetToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorEffector.CurrentPuppetMode())
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

            _editorEffector.ApplyModifiedProperties();
        }

        private void CreatePuppetToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, singleLineHeight * 4));
            TopBar();

            _editorEffector.SwitchTo((PuppetEditorMode) GUILayout.Toolbar((int) _editorEffector.CurrentPuppetMode(), new[]
            {
                CgeLocale.CGEE_Manipulate_trees, CgeLocale.CGEE_Prevent_eyes_blinking
            }));
            GUILayout.EndArea();
        }

        private void CreateAdditionalEditorsToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _editorEffector.SwitchAdditionalEditorTo((AdditionalEditorsMode)GUILayout.Toolbar((int)_editorEffector.GetAdditionalEditor(), new[]
            {
                CgeLocale.CGEE_Create_blend_trees, CgeLocale.CGEE_View_blend_trees, CgeLocale.CGEE_Combine_expressions
            }, GUILayout.ExpandWidth(true)));

            GUILayout.Space(RightSpace);
            GUILayout.EndHorizontal();
        }
    }
}
