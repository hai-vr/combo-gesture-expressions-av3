using System;
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
        MakeLipsyncMovementsSubtle,
        AdditionalEditors,
        OtherOptions
    }

    public enum PuppetEditorMode
    {
        ManipulateTrees,
        PreventEyesBlinking,
        MakeLipsyncMovementsSubtle,
        OtherOptions
    }

    public enum AdditionalEditorsMode
    {
        CreateBlendTrees,
        ViewBlendTrees,
        CombineFaceExpressions
    }

    public class CgeEditorWindow : EditorWindow
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeBlendTreeEffector _blendTreeEffector;
        private readonly CgeLayoutPreventEyesBlinking _layoutPreventEyesBlinking;
        private readonly CgeLayoutMakeLipsyncMovementsSubtle _layoutMakeLipsyncMovementsSubtle;
        private readonly CgeLayoutFaceExpressionCombiner _layoutFaceExpressionCombiner;
        private readonly CgeLayoutOtherOptions _layoutOtherOptions;
        private readonly CgeLayoutSetFaceExpressions _layoutSetFaceExpressions;
        private readonly CgeLayoutManipulateTrees _layoutManipulateTrees;

        private Vector2 _scrollPos;
        public CgeWindowHandler WindowHandler { get; }

        public CgeEditorWindow()
        {
            _editorEffector = new CgeEditorEffector(new CgeEditorState());
            _blendTreeEffector = new CgeBlendTreeEffector(new CgeBlendTreeState());
            var previewController = new CgePreviewEffector(new CgePreviewState(), _editorEffector, _blendTreeEffector);
            _common = new CgeLayoutCommon(Repaint, _editorEffector, previewController);
            var driver = new CgeActivityEditorDriver(_editorEffector);
            _layoutPreventEyesBlinking = new CgeLayoutPreventEyesBlinking(_common, _editorEffector);
            _layoutMakeLipsyncMovementsSubtle = new CgeLayoutMakeLipsyncMovementsSubtle(_common, driver, _editorEffector);
            _layoutFaceExpressionCombiner = new CgeLayoutFaceExpressionCombiner(driver, _editorEffector, previewController);
            _layoutOtherOptions = new CgeLayoutOtherOptions(_common, _editorEffector, previewController);
            _layoutSetFaceExpressions = new CgeLayoutSetFaceExpressions(_common, driver, _layoutFaceExpressionCombiner /* FIXME it is not normal to inject the layout here */, _editorEffector, Repaint, _blendTreeEffector);
            _layoutManipulateTrees = new CgeLayoutManipulateTrees(_common, driver, _editorEffector, Repaint, _blendTreeEffector);

            WindowHandler = new CgeWindowHandler(this, _editorEffector);
        }

        private void OnEnable()
        {
            _common.GuiInit();
        }

        private void OnInspectorUpdate()
        {
            var active = Selection.activeGameObject;
            if (active == null) return;

            TryNowEditingAnActivity(active);
            TryNowEditingAPuppet(active);
            TryAlsoEditingLipsync(active);
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

        private void TryAlsoEditingLipsync(GameObject active)
        {
            var selectedLimitedLipsync = active.GetComponent<ComboGestureLimitedLipsync>();
            if (selectedLimitedLipsync != null && !_layoutMakeLipsyncMovementsSubtle.IsLimitedLipsyncSameAs(selectedLimitedLipsync))
            {
                _layoutMakeLipsyncMovementsSubtle.SetLipsync(selectedLimitedLipsync, Repaint);
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

        private void EditingAnActivity()
        {
            _editorEffector.SpUpdate();
            _layoutMakeLipsyncMovementsSubtle.TryUpdate();

            CreateActivityToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorEffector.CurrentActivityMode())
            {
                case ActivityEditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case ActivityEditorMode.MakeLipsyncMovementsSubtle:
                    _layoutMakeLipsyncMovementsSubtle.Layout(position, Repaint);
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
                case ActivityEditorMode.OtherOptions:
                    _layoutOtherOptions.Layout(Repaint, position);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case ActivityEditorMode.SetFaceExpressions:
                default:
                    _layoutSetFaceExpressions.Layout(position);
                    break;
            }
            GUILayout.EndScrollView();

            _editorEffector.ApplyModifiedProperties();
            _layoutMakeLipsyncMovementsSubtle.ApplyModifiedProperties();
        }

        private void CreateActivityToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorEffector.SwitchTo((ActivityEditorMode) GUILayout.Toolbar((int) _editorEffector.CurrentActivityMode(), new[]
            {
                "Set face expressions", "Prevent eyes blinking", "Make lipsync movements subtle", "Additional editors", "Other options"
            }));
            if (_editorEffector.CurrentActivityMode() == ActivityEditorMode.SetFaceExpressions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (!_editorEffector.GetActivity().enablePermutations)
                {
                    _editorEffector.SpEditorTool().intValue = GUILayout.Toolbar(_editorEffector.SpEditorTool().intValue, new[]
                    {
                        "All combos", "Singles", "Analog Fist", "Combos", "Permutations"
                    }, GUILayout.ExpandWidth(true));
                }
                else
                {
                    _editorEffector.SpEditorTool().intValue = GUILayout.Toolbar(_editorEffector.SpEditorTool().intValue, new[] {"Combos", "Permutations"});
                }
                GUILayout.Space(30);
                GUILayout.EndHorizontal();
                _editorEffector.SwitchCurrentEditorToolTo(_editorEffector.SpEditorTool().intValue);
            }

            if (_editorEffector.CurrentActivityMode() == ActivityEditorMode.MakeLipsyncMovementsSubtle)
            {
                CreateLipsyncToolbar();
            }
            else if (_editorEffector.CurrentActivityMode() == ActivityEditorMode.AdditionalEditors)
            {
                CreateAdditionalEditorsToolbar();
            }

            GUILayout.EndArea();
        }

        private void EditingAPuppet()
        {
            _editorEffector.SpUpdate();
            _layoutMakeLipsyncMovementsSubtle.TryUpdate();

            CreatePuppetToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorEffector.CurrentPuppetMode())
            {
                case PuppetEditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case PuppetEditorMode.MakeLipsyncMovementsSubtle:
                    _layoutMakeLipsyncMovementsSubtle.Layout(position, Repaint);
                    break;
                case PuppetEditorMode.OtherOptions:
                    _layoutOtherOptions.Layout(Repaint, position);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case PuppetEditorMode.ManipulateTrees:
                default:
                    _layoutManipulateTrees.Layout(position);
                    break;
            }
            GUILayout.EndScrollView();

            _editorEffector.ApplyModifiedProperties();
            _layoutMakeLipsyncMovementsSubtle.ApplyModifiedProperties();
        }

        private void CreatePuppetToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorEffector.SwitchTo((PuppetEditorMode) GUILayout.Toolbar((int) _editorEffector.CurrentPuppetMode(), new[]
            {
                "Manipulate trees", "Prevent eyes blinking", "Make lipsync movements subtle", "Other options"
            }));
            if (_editorEffector.CurrentPuppetMode() == PuppetEditorMode.MakeLipsyncMovementsSubtle)
            {
                CreateLipsyncToolbar();
            }

            GUILayout.EndArea();
        }

        private void CreateLipsyncToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _layoutMakeLipsyncMovementsSubtle.SetEditorLipsync(GUILayout.Toolbar(_layoutMakeLipsyncMovementsSubtle.GetEditorLipsync(), new[]
            {
                "Select wide open mouth", "Edit lipsync settings"
            }, GUILayout.ExpandWidth(true)));

            GUILayout.Space(30);
            GUILayout.EndHorizontal();
        }

        private void CreateAdditionalEditorsToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _editorEffector.SwitchAdditionalEditorTo((AdditionalEditorsMode)GUILayout.Toolbar((int)_editorEffector.GetAdditionalEditor(), new[]
            {
                "Create blend trees", "View blend trees", "Combine face expressions"
            }, GUILayout.ExpandWidth(true)));

            GUILayout.Space(30);
            GUILayout.EndHorizontal();
        }
    }
}
