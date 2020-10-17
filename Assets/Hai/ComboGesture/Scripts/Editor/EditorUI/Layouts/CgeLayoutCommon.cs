using System;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class CgeLayoutCommon
    {
        private readonly Action _repaintCallback;
        private readonly CgeEditorEffector _editorEffector;
        private readonly CgePreviewEffector _previewController;

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
        public static GUIStyle MiddleAligned;
        public static GUIStyle MiddleAlignedBold;
        public static GUIStyle LargeFont;
        public static GUIStyle NormalFont;

        public CgeLayoutCommon(Action repaintCallback, CgeEditorEffector editorEffector, CgePreviewEffector previewController)
        {
            _repaintCallback = repaintCallback;
            _editorEffector = editorEffector;
            _previewController = previewController;
        }

        public void GuiInit()
        {
            LeftSideBg = new Color(1f, 0.81f, 0.59f);
            RightSideBg = new Color(0.7f, 0.9f, 1f);
            NeutralSideBg = new Color(1f, 1f, 1f);
            LeftSymmetricalBg = new Color(0.7f, 0.65f, 0.59f);
            RightSymmetricalBg = new Color(0.56f, 0.64f, 0.7f);
            InconsistentBg = new Color(1f, 0.41f, 0.54f);
            MiddleAligned = new GUIStyle {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow};
            MiddleAlignedBold = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow};
            LargeFont = new GUIStyle {fontSize = 20};
            NormalFont = new GUIStyle();
        }

        public static void DrawColoredBackground(Color color)
        {
            var col = GUI.color;
            try
            {
                GUI.color = color;
                GUI.Box(new Rect(0, 0, GuiSquareWidth, GuiSquareHeight), "");
            }
            finally
            {
                GUI.color = col;
            }
        }

        public static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * GuiSquareWidth, yGrid * GuiSquareHeight, GuiSquareWidth, GuiSquareHeight);
        }

        public void DrawPreviewOrRefreshButton(AnimationClip element, bool grayscale = false)
        {
            if (element != null) {
                var clipIsInDict = _previewController.HasClip(element);

                if (clipIsInDict)
                {
                    var texture = grayscale ? _previewController.GetGrayscale(element) : _previewController.GetPicture(element);
                    GUILayout.Box(texture, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    InvisibleRankPreservingButton();
                }
                else
                {
                    InvisibleRankPreservingBox();
                    EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                    var isPreviewSetupValid = _editorEffector.IsPreviewSetupValid();
                    if (GUILayout.Button(isPreviewSetupValid ? "Generate\npreview" : "Setup\npreview", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (isPreviewSetupValid)
                        {
                            _previewController.GenerateMissingPreviewsPrioritizing(_repaintCallback, element);
                        }
                        else
                        {
                            _editorEffector.SwitchTo(EditorMode.OtherOptions);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                InvisibleRankPreservingBox();
                InvisibleRankPreservingButton();
            }
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

        public static void EndLayout()
        {
            GUILayout.EndArea();
        }
    }
}
