using System;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public enum EditorMode
    {
        SetFaceExpressions,
        PreventEyesBlinking,
        MakeLipsyncMovementsSubtle,
        AdditionalEditors,
        OtherOptions
    }

    public class CgeEditorWindow : EditorWindow
    {
        private readonly CgeActivityEditorDriver _driver;
        private readonly CgeLayoutCommon _common;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeLayoutPreventEyesBlinking _layoutPreventEyesBlinking;
        private readonly CgeLayoutMakeLipsyncMovementsSubtle _layoutMakeLipsyncMovementsSubtle;
        private readonly CgeLayoutFaceExpressionCombiner _layoutFaceExpressionCombiner;
        private readonly CgeLayoutOtherOptions _layoutOtherOptions;

        private Vector2 _scrollPos;
        public CgeWindowHandler WindowHandler { get; }

        public CgeEditorWindow()
        {
            _editorEffector = new CgeEditorEffector(new CgeEditorState());
            var previewController = new CgePreviewEffector(new CgePreviewState(), _editorEffector);
            _common = new CgeLayoutCommon(Repaint, _editorEffector, previewController);
            _driver = new CgeActivityEditorDriver(_editorEffector);
            _layoutPreventEyesBlinking = new CgeLayoutPreventEyesBlinking(_common, _editorEffector);
            _layoutMakeLipsyncMovementsSubtle = new CgeLayoutMakeLipsyncMovementsSubtle(_common, _driver, _editorEffector);
            _layoutFaceExpressionCombiner = new CgeLayoutFaceExpressionCombiner(_driver, _editorEffector, previewController);
            _layoutOtherOptions = new CgeLayoutOtherOptions(_common, _editorEffector, previewController);

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

            var selectedActivity = active.GetComponent<ComboGestureActivity>();
            if (selectedActivity != null && selectedActivity != _editorEffector.GetActivity())
            {
                WindowHandler.RetargetActivity(selectedActivity);
                Repaint();
            }

            var selectedLimitedLipsync = active.GetComponent<ComboGestureLimitedLipsync>();
            if (selectedLimitedLipsync != null && !_layoutMakeLipsyncMovementsSubtle.IsLimitedLipsyncSameAs(selectedLimitedLipsync))
            {
                _layoutMakeLipsyncMovementsSubtle.SetLipsync(selectedLimitedLipsync, Repaint);
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (_editorEffector.GetActivity() == null)
            {
                return;
            }

            if (_editorEffector.IsFirstTimeSetup() && _editorEffector.IsPreviewSetupValid())
            {
                _editorEffector.ClearFirstTimeSetup();
            }

            _editorEffector.SpUpdate();
            _layoutMakeLipsyncMovementsSubtle.TryUpdate();

            CreateToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorEffector.CurrentMode())
            {
                case EditorMode.PreventEyesBlinking:
                    _layoutPreventEyesBlinking.Layout(position);
                    break;
                case EditorMode.MakeLipsyncMovementsSubtle:
                    _layoutMakeLipsyncMovementsSubtle.Layout(position, Repaint);
                    break;
                case EditorMode.AdditionalEditors:
                    _layoutFaceExpressionCombiner.Layout(Repaint);
                    break;
                case EditorMode.OtherOptions:
                    _layoutOtherOptions.Layout(Repaint, position);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case EditorMode.SetFaceExpressions:
                default:
                    if (_editorEffector.GetActivity().enablePermutations)
                    {
                        LayoutPermutationEditor();
                    }
                    else
                    {
                        LayoutActivityEditor();
                    }
                    break;
            }
            GUILayout.EndScrollView();

            _editorEffector.ApplyModifiedProperties();
            _layoutMakeLipsyncMovementsSubtle.ApplyModifiedProperties();
        }

        private void CreateToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorEffector.SwitchTo((EditorMode) GUILayout.Toolbar((int) _editorEffector.CurrentMode(), new[]
            {
                "Set face expressions", "Prevent eyes blinking", "Make lipsync movements subtle", "Additional editors", "Other options"
            }));
            if (_editorEffector.CurrentMode() == EditorMode.SetFaceExpressions)
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

            if (_editorEffector.CurrentMode() == EditorMode.MakeLipsyncMovementsSubtle)
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

            GUILayout.EndArea();
        }

        private void LayoutActivityEditor()
        {
            switch (_editorEffector.SpEditorTool().intValue)
            {
                case 1:
                    BeginLayoutUsing(CgeLayoutCommon.GuiSquareHeight * 2);
                    LayoutSinglesDoublesMatrixProjection();
                    CgeLayoutCommon.EndLayout();
                    break;
                case 2:
                    BeginLayoutUsing(CgeLayoutCommon.GuiSquareHeight * 5);
                    LayoutFistMatrixProjection();
                    CgeLayoutCommon.EndLayout();
                    break;
                case 3:
                    BeginLayoutUsing(CgeLayoutCommon.GuiSquareHeight * 8);
                    LayoutComboMatrixProjection();
                    CgeLayoutCommon.EndLayout();
                    break;
                case 4:

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(GUILayout.Width(800));
                    GUILayout.Label("<b>Permutations</b>", CgeLayoutCommon.LargeFont);
                    EditorGUILayout.LabelField("Permutations is an experimental feature. It allows animations to depend on which hand side is doing the gesture.");
                    EditorGUILayout.LabelField("It is significantly harder to create and use an Activity with permutations.");
                    EditorGUILayout.LabelField("Consider using multiple Activities instead before deciding to use permutations.");
                    EditorGUILayout.LabelField("When a permutation is not defined, the other side will be used.");
                    GUILayout.Space(15);
                    GUILayout.Label("Do you really want to use permutations?", CgeLayoutCommon.LargeFont);
                    if (GUILayout.Button("Enable permutations for this Activity", GUILayout.Width(300)))
                    {
                        _editorEffector.SpEnablePermutations().boolValue = true;
                        _editorEffector.SpEditorTool().intValue = 1;
                    }
                    EditorGUILayout.LabelField("Permutations can be disabled later. Permutations are saved even after disabling permutations.");
                    EditorGUILayout.LabelField("Compiling an Activity with permutations disabled will not take any saved permutation into account.");
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                default:
                    BeginLayoutUsing(CgeLayoutCommon.GuiSquareHeight * 8);
                    LayoutFullMatrixProjection();
                    CgeLayoutCommon.EndLayout();
                    break;
            }
        }

        private void LayoutPermutationEditor()
        {
            switch (_editorEffector.SpEditorTool().intValue)
            {
                case 0:
                    BeginPermutationLayoutUsing();
                    LayoutPermutationMatrixProjection(true);
                    break;
                default:
                    BeginPermutationLayoutUsing();
                    LayoutPermutationMatrixProjection();
                    break;
            }
            CgeLayoutCommon.EndLayout();
        }

        private void BeginLayoutUsing(int totalHeight)
        {
            var totalWidth = CgeLayoutCommon.GuiSquareWidth * 8;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), 0, totalWidth, totalHeight));
        }

        private void BeginPermutationLayoutUsing()
        {
            var totalHeight = CgeLayoutCommon.GuiSquareHeight * 9;
            var totalWidth = CgeLayoutCommon.GuiSquareWidth * 9;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), 0, totalWidth, totalHeight));
        }

        private void LayoutFistMatrixProjection()
        {
            for (var side = 0; side < 8; side++)
            {
                if (side == 1) continue;

                GUILayout.BeginArea(RectAt(side, 0));
                DrawInner("anim0" + side);
                GUILayout.EndArea();

                GUILayout.BeginArea(RectAt(side, 1));
                if (side == 0)
                {
                    DrawInner("anim01");
                }
                else
                {
                    DrawInner("anim1" + side);
                }
                GUILayout.EndArea();
            }

            GUILayout.BeginArea(RectAt(1, 0));
            DrawTransitionEdit();
            GUILayout.EndArea();

            GUILayout.BeginArea(RectAt(0, 3));
            DrawInner("anim00");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(1, 3));
            DrawInner("anim00");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(2, 3));
            DrawInner("anim00");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(0, 4));
            DrawInner("anim11_L");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(1, 4));
            DrawInner("anim11");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(2, 4));
            DrawInner("anim11_R");
            GUILayout.EndArea();
        }

        private void LayoutSinglesDoublesMatrixProjection()
        {
            for (var side = 0; side < 8; side++)
            {
                if (side == 1) continue;

                GUILayout.BeginArea(RectAt(side, 0));
                DrawInner("anim0" + side);
                GUILayout.EndArea();

                if (side != 0) {
                    GUILayout.BeginArea(RectAt(side, 1));
                    DrawInner("anim" + side + "" + side);
                    GUILayout.EndArea();
                }
            }

            GUILayout.BeginArea(RectAt(1, 0));
            DrawTransitionEdit();
            GUILayout.EndArea();
        }

        private void LayoutComboMatrixProjection()
        {
            for (var sideA = 0; sideA < 8; sideA++)
            {
                for (var sideB = 0; sideB < 8; sideB++)
                {
                    if (sideA == 0 && sideB == 0 || sideA != sideB && sideA != 1 && sideB != 1)
                    {
                        int left, right;
                        if (sideA <= sideB)
                        {
                            left = sideA;
                            right = sideB;
                        }
                        else
                        {
                            left = sideB;
                            right = sideA;
                        }
                        GUILayout.BeginArea(RectAt(sideB, sideA));
                        DrawInner("anim" + left + "" + right);
                        GUILayout.EndArea();
                    }
                }
            }

            GUILayout.BeginArea(RectAt(1, 0));
            DrawTransitionEdit();
            GUILayout.EndArea();
        }

        private void LayoutPermutationMatrixProjection(bool partial = false)
        {
            for (var sideA = 0; sideA < 8; sideA++)
            {
                for (var sideB = 0; sideB < 8; sideB++)
                {
                    GUILayout.BeginArea(RectAt(sideB, sideA));
                    DrawInner("anim" + sideA + "" + sideB, "anim" + sideB + "" + sideA, partial);
                    GUILayout.EndArea();
                }
            }
            for (var side = 1; side < 8; side++)
            {
                GUILayout.BeginArea(RectAt(8, side));
                DrawInner("anim0" + side, "anim" + side + "0", partial);
                GUILayout.EndArea();

                GUILayout.BeginArea(RectAt(side, 8));
                DrawInner("anim" + side + "0", "anim0" + side, partial);
                GUILayout.EndArea();
            }

            GUILayout.BeginArea(RectAt(0, 8));
            CgeLayoutCommon.DrawColoredBackground(CgeLayoutCommon.LeftSideBg);
            DrawInner("anim11_L");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(8, 0));
            CgeLayoutCommon.DrawColoredBackground(CgeLayoutCommon.RightSideBg);
            DrawInner("anim11_R");
            GUILayout.EndArea();

            GUILayout.BeginArea(RectAt(8, 8));
            DrawTransitionEdit();
            GUILayout.Space(singleLineHeight);
            if (GUILayout.Button("Disable permutations", GUILayout.ExpandWidth(true), GUILayout.Height(singleLineHeight * 2)))
            {
                _editorEffector.SpEnablePermutations().boolValue = false;
                _editorEffector.SpEditorTool().intValue = 4;
            }
            GUILayout.EndArea();
        }

        private void LayoutFullMatrixProjection()
        {
            for (var left = 0; left < 8; left++)
            {
                for (var right = left; right < 8; right++)
                {
                    GUILayout.BeginArea(RectAt(right, left));
                    DrawInner("anim" + left + "" + right);
                    GUILayout.EndArea();
                }
            }

            GUILayout.BeginArea(RectAt(0, 1));
            DrawTransitionEdit();
            GUILayout.EndArea();

            GUILayout.BeginArea(RectAt(0, 4));
            DrawInner("anim11_L");
            GUILayout.EndArea();

            GUILayout.BeginArea(RectAt(2, 4));
            DrawInner("anim11_R");
            GUILayout.EndArea();
        }

        private static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * CgeLayoutCommon.GuiSquareWidth, yGrid * CgeLayoutCommon.GuiSquareHeight, CgeLayoutCommon.GuiSquareWidth, CgeLayoutCommon.GuiSquareHeight);
        }

        private void DrawInner(string propertyPath, string oppositePath = null, bool partial = false)
        {
            var usePermutations = oppositePath != null;
            var property = _editorEffector.SpProperty(propertyPath);
            var oppositeProperty = usePermutations ? _editorEffector.SpProperty(oppositePath) : null;
            var isLeftHand = String.Compare(propertyPath, oppositePath, StringComparison.Ordinal) > 0;
            if (usePermutations)
            {
                if (propertyPath == oppositePath)
                {
                    CgeLayoutCommon.DrawColoredBackground(CgeLayoutCommon.NeutralSideBg);
                }
                else if (property.objectReferenceValue == null && oppositeProperty.objectReferenceValue == null || isLeftHand && property.objectReferenceValue == null && oppositeProperty.objectReferenceValue != null)
                {
                    if (isLeftHand && partial)
                    {
                        return;
                    }

                    CgeLayoutCommon.DrawColoredBackground(isLeftHand ? CgeLayoutCommon.LeftSymmetricalBg : CgeLayoutCommon.RightSymmetricalBg);
                }
                else if (oppositeProperty.objectReferenceValue == property.objectReferenceValue || isLeftHand && oppositeProperty.objectReferenceValue == null || !isLeftHand && property.objectReferenceValue == null)
                {
                    CgeLayoutCommon.DrawColoredBackground(CgeLayoutCommon.InconsistentBg);
                }
                else
                {
                    CgeLayoutCommon.DrawColoredBackground(isLeftHand ? CgeLayoutCommon.LeftSideBg : CgeLayoutCommon.RightSideBg);
                }
            }

            var translatableProperty = (usePermutations ? "p_" : "") + propertyPath;
            GUILayout.Label(_driver.ShortTranslation(translatableProperty), _driver.IsSymmetrical(translatableProperty) ? CgeLayoutCommon.MiddleAlignedBold : CgeLayoutCommon.MiddleAligned);

            // ReSharper disable once PossibleLossOfFraction
            var element = property.objectReferenceValue != null ? (Motion) property.objectReferenceValue : null;
            if (element != null)
            {
                GUILayout.BeginArea(new Rect((CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth) / 2, singleLineHeight, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
                _common.DrawPreviewOrRefreshButton(element);
                GUILayout.EndArea();
            }
            else if (usePermutations)
            {
                if (oppositeProperty.objectReferenceValue != null && propertyPath != oppositePath && isLeftHand)
                {
                    // FIXME: Rank preservation
                    // LayoutCommon.DrawColoredBackground(LayoutCommon.RightSideBg);
                    DrawInnerReversal(oppositePath);
                }
            }
            else
            {
                GUILayout.BeginArea(new Rect((CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth) / 2, singleLineHeight, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
                GUILayout.EndArea();
            }


            if (_driver.IsAPropertyThatCanBeCombined(propertyPath, usePermutations) && !(element is BlendTree))
            {
                var rect = element is AnimationClip
                    ? new Rect(CgeLayoutCommon.GuiSquareWidth - 2 * singleLineHeight, CgeLayoutCommon.PictureHeight - singleLineHeight * 0.5f, singleLineHeight * 2, singleLineHeight * 1.5f)
                    : new Rect(CgeLayoutCommon.GuiSquareWidth - 100, CgeLayoutCommon.PictureHeight - singleLineHeight * 0.5f, 100, singleLineHeight * 1.5f);

                var areSourcesCompatible = _driver.AreCombinationSourcesDefinedAndCompatible(propertyPath);
                EditorGUI.BeginDisabledGroup(!areSourcesCompatible);
                GUILayout.BeginArea(rect);
                if (GUILayout.Button((element != null ? "+" : "+ Combine")))
                {
                    var merge = _driver.ProvideCombinationPropertySources(propertyPath);
                    OpenMergeWindowFor(merge.Left, merge.Right, propertyPath, usePermutations);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (element is BlendTree)
            {
                var rect = new Rect(CgeLayoutCommon.GuiSquareWidth - 20, CgeLayoutCommon.PictureHeight - singleLineHeight * 0.5f, 20, singleLineHeight * 1.5f);

                EditorGUI.BeginDisabledGroup(false);
                GUILayout.BeginArea(rect);
                if (GUILayout.Button("?"))
                {
                    OpenBlendTreeAt(propertyPath);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                BeginInvisibleRankPreservingArea();
                CgeLayoutCommon.InvisibleRankPreservingButton();
                EndInvisibleRankPreservingArea();
            }

            if (usePermutations && propertyPath != oppositePath && property.objectReferenceValue == oppositeProperty.objectReferenceValue && property.objectReferenceValue != null)
            {
                EditorGUI.BeginDisabledGroup(false);
                GUILayout.BeginArea(new Rect(10, CgeLayoutCommon.PictureHeight - singleLineHeight * 1.75f, CgeLayoutCommon.GuiSquareWidth - 10, singleLineHeight * 1.5f));
                if (GUILayout.Button("↗↗ Simplify"))
                {
                    Simplify(isLeftHand ? propertyPath : oppositePath);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (usePermutations && (isLeftHand && property.objectReferenceValue != null && oppositeProperty.objectReferenceValue == null || !isLeftHand && property.objectReferenceValue == null && oppositeProperty.objectReferenceValue != null))
            {
                EditorGUI.BeginDisabledGroup(false);
                GUILayout.BeginArea(new Rect(10, CgeLayoutCommon.PictureHeight - singleLineHeight * 1.75f, CgeLayoutCommon.GuiSquareWidth - 10, singleLineHeight * 1.5f));
                if (GUILayout.Button("↗↙ Swap to Fix"))
                {
                    Swap(propertyPath, oppositePath);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (usePermutations && isLeftHand && property.objectReferenceValue != null)
            {
                EditorGUI.BeginDisabledGroup(false);
                GUILayout.BeginArea(new Rect(CgeLayoutCommon.GuiSquareWidth - 2 * singleLineHeight, CgeLayoutCommon.PictureHeight - singleLineHeight * 1.75f, 2 * singleLineHeight, singleLineHeight * 1.5f));
                if (GUILayout.Button("↗↙"))
                {
                    Swap(propertyPath, oppositePath);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (element == null && _driver.IsAutoSettable(propertyPath))
            {
                var propertyPathToCopyFrom = _driver.GetAutoSettableSource(propertyPath);
                var animationToBeCopied = _editorEffector.SpProperty(propertyPathToCopyFrom).objectReferenceValue;

                EditorGUI.BeginDisabledGroup(animationToBeCopied == null);
                GUILayout.BeginArea(new Rect(CgeLayoutCommon.GuiSquareWidth - 100, CgeLayoutCommon.PictureHeight - singleLineHeight * 1.75f, 100, singleLineHeight * 1.5f));
                if (GUILayout.Button("Auto-set"))
                {
                    AutoSet(propertyPath, propertyPathToCopyFrom);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (element == null && _driver.AreCombinationSourcesIdentical(propertyPath))
            {
                var propertyPathToCopyFrom = _driver.ProvideCombinationPropertySources(propertyPath).Left;
                var animationToBeCopied = _editorEffector.SpProperty(propertyPathToCopyFrom).objectReferenceValue;

                EditorGUI.BeginDisabledGroup(animationToBeCopied == null);
                GUILayout.BeginArea(new Rect(CgeLayoutCommon.GuiSquareWidth - 100, CgeLayoutCommon.PictureHeight - singleLineHeight * 1.75f, 100, singleLineHeight * 1.5f));
                if (GUILayout.Button("Auto-set"))
                {
                    AutoSet(propertyPath, propertyPathToCopyFrom);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                BeginInvisibleRankPreservingArea();
                CgeLayoutCommon.InvisibleRankPreservingButton();
                EndInvisibleRankPreservingArea();
            }

            GUILayout.Space(CgeLayoutCommon.PictureHeight);
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }

        private void DrawInnerReversal(string propertyPath)
        {
            var property = _editorEffector.SpProperty(propertyPath);

            var edge = CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth;
            GUILayout.BeginArea(new Rect(edge, singleLineHeight, CgeLayoutCommon.PictureWidth - edge, CgeLayoutCommon.PictureHeight - singleLineHeight));
            var element = (Motion)property.objectReferenceValue;
            _common.DrawPreviewOrRefreshButton(element, true);
            GUILayout.EndArea();
        }

        private void DrawTransitionEdit()
        {
            // ReSharper disable once PossibleLossOfFraction
            GUILayout.BeginArea(new Rect((CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth) / 2, singleLineHeight, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
            GUILayout.Label("Transition duration");
            GUILayout.Label("(in seconds)");
            EditorGUILayout.Slider(_editorEffector.SpTransitionDuration(), 0f, 1f, GUIContent.none);
            GUILayout.EndArea();
            GUILayout.Space(CgeLayoutCommon.PictureHeight);
        }

        private static void BeginInvisibleRankPreservingArea()
        {
            GUILayout.BeginArea(new Rect(-1000, -1000, 0, 0));
        }

        private static void EndInvisibleRankPreservingArea()
        {
            GUILayout.EndArea();
        }

        private void OpenMergeWindowFor(string left, string right, string propertyPath, bool usePermutations)
        {
            var leftAnim = _editorEffector.SpProperty(left).objectReferenceValue;
            var rightAnim = _editorEffector.SpProperty(right).objectReferenceValue;

            var areBothAnimations = leftAnim is AnimationClip && rightAnim is AnimationClip;
            if (!areBothAnimations) return;

            _layoutFaceExpressionCombiner.DoSetCombiner((AnimationClip) leftAnim, (AnimationClip) rightAnim, propertyPath, usePermutations, Repaint);
        }

        private void OpenBlendTreeAt(string propertyPath)
        {
            var blendTree = (BlendTree)_editorEffector.SpProperty(propertyPath).objectReferenceValue;


        }

        private void AutoSet(string propertyPath, string propertyPathToCopyFrom)
        {
            _editorEffector.SpProperty(propertyPath).objectReferenceValue = _editorEffector.SpProperty(propertyPathToCopyFrom).objectReferenceValue;
            _editorEffector.ApplyModifiedProperties();
        }

        private void Swap(string propertyPath, string oppositePath)
        {
            var aProp = _editorEffector.SpProperty(propertyPath);
            var bProp = _editorEffector.SpProperty(oppositePath);
            var a = aProp.objectReferenceValue;
            var b = bProp.objectReferenceValue;

            aProp.objectReferenceValue = b;
            bProp.objectReferenceValue = a;
            _editorEffector.ApplyModifiedProperties();
        }

        private void Simplify(string pathToClear)
        {
            _editorEffector.SpProperty(pathToClear).objectReferenceValue = null;
            _editorEffector.ApplyModifiedProperties();
        }
    }
}
