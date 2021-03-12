using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Layouts
{
    public class LipsyncState
    {
        public ComboGestureLimitedLipsync LimitedLipsync { get; internal set; }
        public SerializedObject SerializedLimitedLipsync;
        public CgeActivityEditorLipsync Lipsync;
        public int EditorLipsyncTool;
        public int LimitedLipsyncPreviewIndex;
    }

    public class CgeLayoutMakeLipsyncMovementsSubtle
    {
        private readonly CgeLayoutCommon _common;
        private readonly CgeActivityEditorDriver _driver;
        private readonly CgeEditorEffector _editorEffector;
        private readonly EeRenderingCommands _renderingCommands;
        private readonly LipsyncState _lipsyncState = new LipsyncState();

        public CgeLayoutMakeLipsyncMovementsSubtle(CgeLayoutCommon common, CgeActivityEditorDriver driver, CgeEditorEffector editorEffector, EeRenderingCommands renderingCommands)
        {
            _common = common;
            _driver = driver;
            _editorEffector = editorEffector;
            _renderingCommands = renderingCommands;
        }

        public void Layout(Rect position, Action repaintCallback)
        {
            EditorGUILayout.HelpBox(@"Limited Lipsync is a feature that will not work with the version of VRChat at the time this version of ComboGestureExpressions has been published.

At the time this version has been published, generating the layer will break your Lipsync blendshapes.", MessageType.Error);
            var helpBoxHeightReverse = 60;
            if (_lipsyncState.EditorLipsyncTool == 1)
            {
                _common.BeginLayoutUsing(position, CgeLayoutCommon.GuiSquareHeight * 8, helpBoxHeightReverse);
                LayoutLimitedLipsyncEditor(repaintCallback);
                CgeLayoutCommon.EndLayout();
            }
            else
            {
                GUILayout.Label("Select face expressions with a <b>wide open mouth</b>.", _common.LargeFont);
                GUILayout.BeginArea(new Rect(0, CgeLayoutCommon.SingleLineHeight * 3 + helpBoxHeightReverse, position.width, CgeLayoutCommon.GuiSquareHeight * 8));
                var allClips = new HashSet<AnimationClip>(_editorEffector.AllDistinctAnimations()).ToList();
                var mod = Math.Max(3, Math.Min(8, (int)Math.Sqrt(allClips.Count)));
                for (var element = 0; element < allClips.Count; element++)
                {
                    GUILayout.BeginArea(CgeLayoutCommon.RectAt(element % mod, element / mod));
                    DrawLipsyncSwitch(allClips[element]);
                    GUILayout.EndArea();
                }
                GUILayout.EndArea();
                GUILayout.Box(
                    "",
                    GUIStyle.none,
                    GUILayout.Width(CgeLayoutCommon.GuiSquareHeight + CgeLayoutCommon.GuiSquareHeight * mod + CgeLayoutCommon.SingleLineHeight * 2),
                    GUILayout.Height(CgeLayoutCommon.GuiSquareHeight + CgeLayoutCommon.GuiSquareHeight * (allClips.Count / mod) + CgeLayoutCommon.SingleLineHeight * 2 + helpBoxHeightReverse)
                );
            }
        }
        private void DrawLipsyncSwitch(AnimationClip element)
        {
            var isRegisteredAsLipsync = _editorEffector.MutableLimitedLipsync().Exists(animation => animation.clip == element);

            if (isRegisteredAsLipsync) {
                var col = GUI.color;
                try
                {
                    GUI.color = new Color(0.44f, 0.65f, 1f);
                    GUI.Box(new Rect(0, 0, CgeLayoutCommon.GuiSquareWidth, CgeLayoutCommon.GuiSquareHeight), "");
                }
                finally
                {
                    GUI.color = col;
                }
            }
            GUILayout.BeginArea(new Rect((CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth) / 2, 0, CgeLayoutCommon.PictureWidth, CgeLayoutCommon.PictureHeight));
            _common.DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(CgeLayoutCommon.PictureHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element, typeof(AnimationClip), true);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(isRegisteredAsLipsync ? "Limited Lipsync" : ""))
            {
                if (isRegisteredAsLipsync)
                {
                    _editorEffector.MutableLimitedLipsync().RemoveAll(animation => animation.clip == element);
                }
                else
                {
                    _editorEffector.MutableLimitedLipsync().Add(new ComboGestureActivity.LimitedLipsyncAnimation
                    {
                        clip = element,
                        limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth
                    });
                }
            }
        }

        private void LayoutLimitedLipsyncEditor(Action repaintCallback)
        {
            Rect RectAt(int xGrid, int yGrid)
            {
                return new Rect(xGrid * CgeLayoutCommon.GuiSquareWidth * 2, yGrid * CgeLayoutCommon.GuiSquareHeight * 2, CgeLayoutCommon.GuiSquareWidth * 2, CgeLayoutCommon.GuiSquareHeight * 2);
            }

            if (_lipsyncState.LimitedLipsync == null) {
                EditorGUILayout.LabelField("Select a ComboGestureLimitedLipsync component in the scene or choose one:");
                var newLipsync = (ComboGestureLimitedLipsync) EditorGUILayout.ObjectField(null, typeof(ComboGestureLimitedLipsync), true);
                if (newLipsync != null)
                {
                    SetLipsync(newLipsync, repaintCallback);
                }
                return;
            }

            void DrawLipsync(int visemeNumber, bool previewable)
            {
                GUILayout.Label(_driver.ShortTranslation("viseme" + visemeNumber), _common.MiddleAligned);

                GUILayout.BeginArea(new Rect(CgeLayoutCommon.GuiSquareWidth - CgeLayoutCommon.PictureWidth, CgeLayoutCommon.SingleLineHeight, CgeLayoutCommon.PictureWidth * 2, CgeLayoutCommon.PictureHeight * 2 + CgeLayoutCommon.SingleLineHeight * 4));
                GUILayout.Box(_lipsyncState.Lipsync.TextureForViseme(visemeNumber), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                EditorGUI.BeginDisabledGroup(!previewable || AnimationMode.InAnimationMode());
                if (GUILayout.Button("Regenerate preview"))
                {
                    RegenerateLipsyncPreview(visemeNumber);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Amp Mul", GUILayout.Width(80));
                EditorGUILayout.Slider(_lipsyncState.SerializedLimitedLipsync.FindProperty("amplitude" + visemeNumber), 0f, 1f, GUIContent.none, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Duration Mul", GUILayout.Width(80));
                EditorGUILayout.Slider(_lipsyncState.SerializedLimitedLipsync.FindProperty("transition" + visemeNumber), 0f, 1f, GUIContent.none, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUILayout.Space(CgeLayoutCommon.PictureHeight);
            }

            GUILayout.BeginArea(RectAt(0, 0));
            EditorGUILayout.LabelField("Limited Lipsync Component", EditorStyles.boldLabel);
            var otherNewLipsync = (ComboGestureLimitedLipsync) EditorGUILayout.ObjectField(_lipsyncState.LimitedLipsync, typeof(ComboGestureLimitedLipsync), true);
            if (!IsLimitedLipsyncSameAs(otherNewLipsync))
            {
                SetLipsync(otherNewLipsync, repaintCallback);
            }

            EditorGUILayout.PropertyField(_lipsyncState.SerializedLimitedLipsync.FindProperty("limitation"), new GUIContent("Category"));
            EditorGUILayout.Slider(_lipsyncState.SerializedLimitedLipsync.FindProperty("amplitudeScale"), 0f, 0.25f, "Viseme Amplitude");
            EditorGUILayout.Slider(_lipsyncState.SerializedLimitedLipsync.FindProperty("amplitudeScale"), 0f, 1f, "(scaled to 1)");
            EditorGUILayout.PropertyField(_lipsyncState.SerializedLimitedLipsync.FindProperty("transitionDuration"), new GUIContent("Transition Duration (s)"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            var previewables = ListAllPreviewableNames();
            var previewIsPossible = previewables.Any();
            if (previewIsPossible) {
                if (_lipsyncState.LimitedLipsyncPreviewIndex >= previewables.Length)
                {
                    _lipsyncState.LimitedLipsyncPreviewIndex = 0;
                }
                _lipsyncState.LimitedLipsyncPreviewIndex = EditorGUILayout.Popup(
                    _lipsyncState.LimitedLipsyncPreviewIndex,
                    previewables
                );
                var avatarHasVisemeBlendShapes = _editorEffector.IsPreviewSetupValid() && _editorEffector.PreviewSetup().TempCxSmr;
                if (!avatarHasVisemeBlendShapes)
                {
                    EditorGUILayout.HelpBox("The avatar has no lipsync face mesh.", MessageType.Error);
                }
                EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                if (GUILayout.Button("Regenerate all previews"))
                {
                    RegenerateLipsyncPreviews();
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (GUILayout.Button("Select an animation..."))
                {
                    _editorEffector.SwitchTo(ActivityEditorMode.MakeLipsyncMovementsSubtle);
                    _lipsyncState.EditorLipsyncTool = 0;
                }
            }

            GUILayout.EndArea();

            for (var viseme = 0; viseme < 15; viseme++)
            {
                var gridIndex = viseme + 1;
                GUILayout.BeginArea(RectAt(gridIndex % 4, gridIndex / 4));
                DrawLipsync(viseme, previewIsPossible);
                GUILayout.EndArea();
            }
        }

        private string[] ListAllPreviewableNames()
        {
            return _editorEffector.MutableLimitedLipsync()
                .Where(animation => animation.clip != null)
                .Select(animation => animation.clip.name)
                .ToArray();
        }

        private AnimationClip[] ListAllPreviewableClips()
        {
            return _editorEffector.MutableLimitedLipsync()
                .Where(animation => animation.clip != null)
                .Select(animation => animation.clip)
                .ToArray();
        }

        private void RegenerateLipsyncPreviews()
        {
            _lipsyncState.Lipsync.Prepare(ListAllPreviewableClips()[_lipsyncState.LimitedLipsyncPreviewIndex]);
        }

        private void RegenerateLipsyncPreview(int visemeNumber)
        {
            _lipsyncState.Lipsync.PrepareJust(ListAllPreviewableClips()[_lipsyncState.LimitedLipsyncPreviewIndex], visemeNumber);
        }

        public void TryUpdate()
        {
            _lipsyncState.SerializedLimitedLipsync?.Update();
        }

        public void SetLipsync(ComboGestureLimitedLipsync limitedLipsync, Action repaintCallback)
        {
            _lipsyncState.LimitedLipsync = limitedLipsync;
            _lipsyncState.SerializedLimitedLipsync = new SerializedObject(limitedLipsync);
            _lipsyncState.Lipsync = new CgeActivityEditorLipsync(limitedLipsync, repaintCallback, _editorEffector, _renderingCommands);
        }

        public bool IsLimitedLipsyncSameAs(ComboGestureLimitedLipsync selectedLimitedLipsync)
        {
            return _lipsyncState.LimitedLipsync == selectedLimitedLipsync;
        }

        public void ApplyModifiedProperties()
        {
            _lipsyncState.SerializedLimitedLipsync?.ApplyModifiedProperties();
        }

        public void SetEditorLipsync(int editorLipsyncTool)
        {
            _lipsyncState.EditorLipsyncTool = editorLipsyncTool;
        }

        public int GetEditorLipsync()
        {
            return _lipsyncState.EditorLipsyncTool;
        }
    }
}
