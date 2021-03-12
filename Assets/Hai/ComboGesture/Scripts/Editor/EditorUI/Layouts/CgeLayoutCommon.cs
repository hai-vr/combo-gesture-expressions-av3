using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ComboGesture.Scripts.Editor.Internal.Processing;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutCommon
    {
        private readonly Action _repaintCallback;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgeMemoryQuery _memoryQuery;
        private readonly CgeActivityPreviewQueryAggregator _activityPreviewQueryAggregator;

        public const int PictureWidth = 120;
        public const int PictureHeight = 80;
        public const int GuiSquareWidth = 140;
        public static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight;
        public static readonly int GuiSquareHeight = (int) (SingleLineHeight * 3 + PictureHeight);
        public static Color LeftSideBg;
        public static Color RightSideBg;
        public static Color NeutralSideBg;
        public static Color LeftSymmetricalBg;
        public static Color RightSymmetricalBg;
        public static Color InconsistentBg;
        public GUIStyle MiddleAligned;
        public GUIStyle MiddleAlignedBold;
        public GUIStyle LargeFont;
        public GUIStyle NormalFont;
        public static Texture GuideIcon32;
        private bool _isSimulationOfProSkin = false;

        public CgeLayoutCommon(Action repaintCallback, CgeEditorEffector editorEffector, CgeActivityPreviewQueryAggregator activityPreviewQueryAggregator, CgeMemoryQuery memoryQuery)
        {
            _repaintCallback = repaintCallback;
            _editorEffector = editorEffector;
            _activityPreviewQueryAggregator = activityPreviewQueryAggregator;
            _memoryQuery = memoryQuery;
        }

        public void GuiInit()
        {
            if (!EditorGUIUtility.isProSkin && !_isSimulationOfProSkin)
            {
                LeftSideBg = new Color(1f, 0.81f, 0.59f);
                RightSideBg = new Color(0.7f, 0.9f, 1f);
                NeutralSideBg = new Color(0.88f, 0.88f, 0.88f);
                LeftSymmetricalBg = new Color(0.7f, 0.65f, 0.59f);
                RightSymmetricalBg = new Color(0.56f, 0.64f, 0.7f);
                InconsistentBg = new Color(1f, 0.41f, 0.54f);
            }
            else
            {
                LeftSideBg = new Color(0.62f, 0.4f, 0.12f);
                RightSideBg = new Color(0.24f, 0.48f, 0.62f);
                NeutralSideBg = new Color(0.07f, 0.07f, 0.07f);
                LeftSymmetricalBg = new Color(0.34f, 0.27f, 0.23f);
                RightSymmetricalBg = new Color(0.23f, 0.26f, 0.34f);
                InconsistentBg = new Color(0.72f, 0.09f, 0.27f);
            }
            MiddleAligned = new GUIStyle("Label") {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow};
            MiddleAlignedBold = new GUIStyle("Label") {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow, fontStyle = FontStyle.Bold};
            LargeFont = new GUIStyle("Label") {fontSize = 20, richText = true};
            NormalFont = new GUIStyle("Label") {richText = true};

            if (_isSimulationOfProSkin)
            {
                MiddleAligned.normal.textColor = new Color(0.71f, 0.71f, 0.71f);
                MiddleAlignedBold.normal.textColor = new Color(0.71f, 0.71f, 0.71f);
            }

            GuideIcon32 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Hai/ComboGesture/Icons/guide-32.png");
        }

        public static void DrawColoredBackground(Color color)
        {
            var col = GUI.color;
            try
            {
                GUI.color = color;
                GUI.Box(new Rect(0, 0, GuiSquareWidth, GuiSquareHeight), "", new GUIStyle("box") {
                    normal = new GUIStyleState
                    {
                        background = Texture2D.whiteTexture
                    }
                });
            }
            finally
            {
                GUI.color = col;
            }
        }

        public static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * GuiSquareWidth, yGrid * GuiSquareHeight, GuiSquareWidth - 3, GuiSquareHeight - 3);
        }

        public void DrawPreviewOrRefreshButton(Motion element, bool grayscale = false)
        {
            if (element is AnimationClip clip) {
                if (_memoryQuery.HasClip(clip))
                {
                    GUILayout.Box(Texture(grayscale, clip), GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingButton();
                }
                else
                {
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                    var isPreviewSetupValid = _editorEffector.IsPreviewSetupValid();
                    if (GUILayout.Button(isPreviewSetupValid ? CgeLocale.CGEE_GeneratePreview : CgeLocale.CGEE_SetupPreview, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (isPreviewSetupValid)
                        {
                            _activityPreviewQueryAggregator.GenerateMissingPreviewsPrioritizing(_repaintCallback, clip);
                        }
                        else
                        {
                            if (_editorEffector.GetCurrentlyEditing() == CurrentlyEditing.Activity)
                            {
                                _editorEffector.SwitchTo(ActivityEditorMode.OtherOptions);
                            }
                            else
                            {
                                _editorEffector.SwitchTo(PuppetEditorMode.OtherOptions);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            else if (element is BlendTree tree)
            {
                var animations = ManifestFromPuppet.AllAnimationsOf(tree);
                if (animations.Count == 0)
                {
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingButton();
                }
                else if (_memoryQuery.HasClip(animations[0]))
                {
                    GUILayout.BeginHorizontal();
                    TexturedBox(grayscale, animations, 0);
                    TexturedBox(grayscale, animations, 1);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    TexturedBox(grayscale, animations, 2);
                    TexturedBox(grayscale, animations, 3);
                    GUILayout.EndHorizontal();
                    InvisibleRankPreservingButton();
                }
                else
                {
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    InvisibleRankPreservingBox();
                    EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                    var isPreviewSetupValid = _editorEffector.IsPreviewSetupValid();
                    if (GUILayout.Button(isPreviewSetupValid ? CgeLocale.CGEE_GeneratePreview : CgeLocale.CGEE_SetupPreview, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (isPreviewSetupValid)
                        {
                            _activityPreviewQueryAggregator.GenerateMissingPreviewsPrioritizing(_repaintCallback, animations[0]);
                        }
                        else
                        {
                            _editorEffector.SwitchTo(ActivityEditorMode.OtherOptions);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                InvisibleRankPreservingBox();
                InvisibleRankPreservingBox();
                InvisibleRankPreservingBox();
                InvisibleRankPreservingBox();
                InvisibleRankPreservingButton();
            }
        }

        private void TexturedBox(bool grayscale, List<AnimationClip> animations, int index)
        {
            if (animations.Count > index && _memoryQuery.HasClip(animations[index]))
            {
                GUILayout.Box(Texture(grayscale, animations[index]), GUIStyle.none, GUILayout.Width(PictureWidth / 2), GUILayout.Height(PictureHeight / 2));
            }
            else
            {
                InvisibleRankPreservingBox();
            }
        }

        private Texture Texture(bool grayscale, AnimationClip clip)
        {
            return grayscale ? _memoryQuery.GetGrayscale(clip) : _memoryQuery.GetPicture(clip);
        }

        private static void InvisibleRankPreservingBox()
        {
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(0), GUILayout.Height(0));
        }

        public static void InvisibleRankPreservingButton()
        {
            GUILayout.Button("", GUIStyle.none, GUILayout.Width(0), GUILayout.Height(0));
        }

        public void BeginLayoutUsing(Rect position, int totalHeight, int topHeight)
        {
            var totalWidth = GuiSquareWidth * 8;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), topHeight, totalWidth, totalHeight));
        }

        public void BeginLayoutUsingWidth(Rect position, int totalHeight, int topHeight, int totalWidth)
        {
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), topHeight, totalWidth, totalHeight));
        }

        public static void EndLayout()
        {
            GUILayout.EndArea();
        }
    }
}
