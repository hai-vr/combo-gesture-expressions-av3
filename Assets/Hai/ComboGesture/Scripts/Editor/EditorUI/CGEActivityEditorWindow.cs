using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;
using static Hai.ComboGesture.Scripts.Editor.EditorUI.CgeActivityEditorCombiner;
using static UnityEditor.EditorGUIUtility;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    enum EditorMode
    {
        SetFaceExpressions,
        PreventEyesBlinking,
        MakeLipsyncMovementsSubtle,
        CombineFaceExpressions,
        OtherOptions
    }

    public class CgeEditorWindow : EditorWindow
    {
        private const int PictureWidth = 120;
        private const int PictureHeight = 80;
        private const int GuiSquareWidth = 140;

        private static readonly int GuiSquareHeight = (int) (singleLineHeight * 3 + PictureHeight);

        private Dictionary<AnimationClip, Texture2D> _animationClipToTextureDict;
        private Dictionary<AnimationClip, Texture2D> _animationClipToTextureDictGray;
        private RenderTexture _renderTexture;
        public ComboGestureActivity activity;
        public ComboGestureLimitedLipsync limitedLipsync;
        public SerializedObject serializedObject;
        public SerializedObject serializedLimitedLipsync;

        private SerializedProperty _transitionDuration;
        private SerializedProperty _editorTool;
        private SerializedProperty _editorArbitraryAnimations;
        private SerializedProperty _previewSetup;
        private SerializedProperty _enablePermutations;
        private int _currentEditorToolValue = -1;
        private EditorMode _editorMode;
        private bool _firstTimeSetup;
        private AutoSetupPreview.SetupResult? _setupResult;

        private CgeActivityEditorDriver _driver;
        private CgeActivityEditorCombiner _combiner;
        private bool _complexCombiner;
        private CgeActivityEditorLipsync _lipsync;
        private string _combinerTarget;
        private string _combinerCandidateFileName;
        private bool _combinerIsLikelyEyesClosed;
        private int _editorLipsyncTool;

        private static GUIStyle _middleAligned;
        private static GUIStyle _middleAlignedBold;
        private static GUIStyle _largeFont;
        private static GUIStyle _normalFont;

        private Vector2 scrollPos;
        private int _limitedLipsyncPreviewIndex;
        private static readonly Color LeftSideBg;
        private static readonly Color RightSideBg;
        private static readonly Color NeutralSideBg;
        private static readonly Color SymmetricalBg;
        private bool _combinerIsAPermutation;

        static CgeEditorWindow()
        {
            LeftSideBg = new Color(1f, 0.81f, 0.59f);
            RightSideBg = new Color(0.7f, 0.9f, 1f);
            NeutralSideBg = new Color(1f, 1f, 1f);
            SymmetricalBg = new Color(0.7f, 0.7f, 0.7f);
        }

        private void OnEnable()
        {
            _animationClipToTextureDict = new Dictionary<AnimationClip, Texture2D>();
            _animationClipToTextureDictGray = new Dictionary<AnimationClip, Texture2D>();
            _driver = new CgeActivityEditorDriver();
        }

        private void OnInspectorUpdate()
        {
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null)
            {
                var selectedActivity = activeGameObject.GetComponent<ComboGestureActivity>();
                if (selectedActivity != null && selectedActivity != activity)
                {
                    activity = selectedActivity;
                    serializedObject = null;
                    titleContent.text = "CGE/" + activity.name;
                    Repaint();
                }
                var selectedLimitedLipsync = activeGameObject.GetComponent<ComboGestureLimitedLipsync>();
                if (selectedLimitedLipsync != null && selectedLimitedLipsync != limitedLipsync)
                {
                    limitedLipsync = selectedLimitedLipsync;
                    serializedLimitedLipsync = null;
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            _middleAligned = new GUIStyle {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow};
            _middleAlignedBold = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow};
            _largeFont = new GUIStyle {fontSize = 20};
            _normalFont = new GUIStyle {};

            if (activity != null && serializedObject == null)
            {
                serializedObject = new SerializedObject(activity);
                _transitionDuration = serializedObject.FindProperty("transitionDuration");
                _editorTool = serializedObject.FindProperty("editorTool");
                _editorArbitraryAnimations = serializedObject.FindProperty("editorArbitraryAnimations");
                _previewSetup = serializedObject.FindProperty("previewSetup");
                _enablePermutations = serializedObject.FindProperty("enablePermutations");

                if (_editorTool.intValue != _currentEditorToolValue && _currentEditorToolValue >= 0)
                {
                    _editorTool.intValue = _currentEditorToolValue;
                    serializedObject.ApplyModifiedProperties();
                }

                if (!IsPreviewSetupValid())
                {
                    _editorMode = EditorMode.OtherOptions;
                    _firstTimeSetup = true;
                }
            }

            if (serializedObject == null)
            {
                return;
            }

            if (limitedLipsync == null && serializedLimitedLipsync != null)
            {
                serializedLimitedLipsync = null;
            }
            if (limitedLipsync != null && (serializedLimitedLipsync == null || serializedLimitedLipsync.targetObject != limitedLipsync))
            {
                serializedLimitedLipsync = new SerializedObject(limitedLipsync);
                _lipsync = new CgeActivityEditorLipsync(activity, limitedLipsync, Repaint);
            }

            if (_firstTimeSetup && IsPreviewSetupValid())
            {
                _firstTimeSetup = false;
            }

            serializedObject.Update();
            serializedLimitedLipsync?.Update();

            CreateToolbarArea();

            GUILayout.Space(singleLineHeight * 4);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - singleLineHeight * 4));
            switch (_editorMode)
            {
                case EditorMode.PreventEyesBlinking:
                    LayoutPreventEyesBlinking();
                    break;
                case EditorMode.MakeLipsyncMovementsSubtle:
                    LayoutMakeLipsyncMovementsSubtle();
                    break;
                case EditorMode.CombineFaceExpressions:
                    LayoutFaceExpressionCombiner();
                    break;
                case EditorMode.OtherOptions:
                    LayoutOtherOptions();
                    break;
                default:
                    if (activity.enablePermutations)
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

            serializedObject.ApplyModifiedProperties();

            serializedLimitedLipsync?.ApplyModifiedProperties();
        }

        private void CreateToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorMode = (EditorMode) GUILayout.Toolbar((int) _editorMode, new[]
            {
                "Set face expressions", "Prevent eyes blinking", "Make lipsync movements subtle", "Combine face expressions", "Other options"
            });
            if (_editorMode == EditorMode.SetFaceExpressions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (!activity.enablePermutations)
                {
                    _editorTool.intValue = GUILayout.Toolbar(_editorTool.intValue, new[]
                    {
                        "All combos", "Singles", "Analog Fist", "Combos", "Permutations"
                    }, GUILayout.ExpandWidth(true));
                }
                else
                {
                    _editorTool.intValue = GUILayout.Toolbar(_editorTool.intValue, new[] {"Combos", "Permutations"});
                }
                GUILayout.Space(30);
                GUILayout.EndHorizontal();
                _currentEditorToolValue = _editorTool.intValue;
            }

            if (_editorMode == EditorMode.MakeLipsyncMovementsSubtle)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                _editorLipsyncTool = GUILayout.Toolbar(_editorLipsyncTool, new[]
                {
                    "Select wide open mouth", "Edit lipsync settings"
                }, GUILayout.ExpandWidth(true));

                GUILayout.Space(30);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void LayoutActivityEditor()
        {
            switch (_editorTool.intValue)
            {
                case 1:
                    BeginLayoutUsing(GuiSquareHeight * 2);
                    LayoutSinglesDoublesMatrixProjection();
                    break;
                case 2:
                    BeginLayoutUsing(GuiSquareHeight * 5);
                    LayoutFistMatrixProjection();
                    break;
                case 3:
                    BeginLayoutUsing(GuiSquareHeight * 8);
                    LayoutComboMatrixProjection();
                    break;
                case 4:

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(GUILayout.Width(800));
                    GUILayout.Label("<b>Permutations</b>", _largeFont);
                    EditorGUILayout.LabelField("Permutations is an experimental feature. It allows animations to depend on which hand side is doing the gesture.");
                    EditorGUILayout.LabelField("It is significantly harder to create and use an activity with permutations.");
                    EditorGUILayout.LabelField("Consider using multiple Activities instead before deciding to use permutations.");
                    EditorGUILayout.LabelField("When a permutation is not defined, the other side will be used.");
                    GUILayout.Space(15);
                    GUILayout.Label("Do you really want to use permutations?", _largeFont);
                    if (GUILayout.Button("Enable permutations for this activity", GUILayout.Width(300)))
                    {
                        _enablePermutations.boolValue = true;
                        _editorTool.intValue = 1;
                    }
                    EditorGUILayout.LabelField("Permutations can be disabled later. Permutations are saved even after disabling permutations.");
                    EditorGUILayout.LabelField("Compiling an activity with permutations disabled will not take any saved permutation into account.");
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                default:
                    BeginLayoutUsing(GuiSquareHeight * 8);
                    LayoutFullMatrixProjection();
                    break;
            }
            EndLayout();
        }

        private void LayoutPermutationEditor()
        {
            switch (_editorTool.intValue)
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
            EndLayout();
        }

        private void BeginLayoutUsing(int totalHeight)
        {
            var totalWidth = GuiSquareWidth * 8;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), 0, totalWidth, totalHeight));
        }

        private void BeginPermutationLayoutUsing()
        {
            var totalHeight = GuiSquareHeight * 9;
            var totalWidth = GuiSquareWidth * 9;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), 0, totalWidth, totalHeight));
        }

        private void BeginLayoutUsing(int totalHeight, int topHeight)
        {
            var totalWidth = GuiSquareWidth * 8;
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(totalWidth), GUILayout.Height(totalHeight));
            GUILayout.BeginArea(new Rect(Math.Max((position.width - totalWidth) / 2, 0), topHeight, totalWidth, totalHeight));
        }

        private static void EndLayout()
        {
            GUILayout.EndArea();
        }

        private bool IsPreviewSetupValid()
        {
            return activity.previewSetup != null && activity.previewSetup.IsValid();
        }

        private void LayoutOtherOptions()
        {
            if (_firstTimeSetup || !IsPreviewSetupValid())
            {
                if (activity.previewSetup != null && !activity.previewSetup.IsValid())
                {
                    EditorGUILayout.LabelField("A preview setup was found but it is incomplete or invalid.");
                }

                EditorGUILayout.PropertyField(_previewSetup, new GUIContent("Preview setup"));
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Automatically setup preview!", GUILayout.Height(50), GUILayout.Width(300)))
                {
                    DoAutoSetupPreview();
                    if (_setupResult != AutoSetupPreview.SetupResult.NoAvatarFound)
                    {
                        _editorMode = 0;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(_transitionDuration, new GUIContent("Transition duration (s)"));
                EditorGUILayout.PropertyField(_previewSetup, new GUIContent("Preview setup"));
                if (_setupResult != null)
                {
                    var setupResult = (AutoSetupPreview.SetupResult) _setupResult;
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
                    GenerateMissingPreviews();
                }

                if (GUILayout.Button("Regenerate all previews"))
                {
                    new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, _animationClipToTextureDictGray, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything, null);
                }
                EditorGUILayout.PropertyField(_editorArbitraryAnimations, new GUIContent("List of arbitrary animations to &s (Drag and drop assets directly on this title)"), true);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!AnimationMode.InAnimationMode());
                if (GUILayout.Button("Stop generating previews"))
                {
                    CgePreviewProcessor.Stop_Temp();
                }
                EditorGUI.EndDisabledGroup();

                if (activity.editorArbitraryAnimations != null) {
                    GUILayout.BeginArea(new Rect(0, singleLineHeight * 10, position.width, GuiSquareHeight * 8));
                    var allClips = new HashSet<AnimationClip>(activity.editorArbitraryAnimations.Where(clip => clip != null)).ToList();
                    var mod = (int)Math.Max(1, position.width / GuiSquareWidth);
                    for (var element = 0; element < allClips.Count; element++)
                    {
                        GUILayout.BeginArea(RectAt(element % mod, element / mod));
                        DrawArbitrary(allClips[element]);
                        GUILayout.EndArea();
                    }
                    GUILayout.EndArea();
                    GUILayout.Space((allClips.Count / mod) * GuiSquareHeight + GuiSquareHeight + singleLineHeight * 2);
                }
            }
        }

        private void GenerateMissingPreviews()
        {
            new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, _animationClipToTextureDictGray, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null);
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
            DrawColoredBackground(LeftSideBg);
            DrawInner("anim11_L");
            GUILayout.EndArea();
            GUILayout.BeginArea(RectAt(8, 0));
            DrawColoredBackground(RightSideBg);
            DrawInner("anim11_R");
            GUILayout.EndArea();

            GUILayout.BeginArea(RectAt(8, 8));
            DrawTransitionEdit();
            GUILayout.Space(singleLineHeight);
            if (GUILayout.Button("Disable permutations", GUILayout.ExpandWidth(true), GUILayout.Height(singleLineHeight * 2)))
            {
                _enablePermutations.boolValue = false;
                _editorTool.intValue = 4;
            }
            GUILayout.EndArea();
        }

        private static Color SelectColorBasedOnSide(int sideA, int sideB)
        {
            Color color;
            if (sideA > sideB)
            {
                color = SymmetricalBg;
            }
            else if (sideA < sideB)
            {
                color = SymmetricalBg;
            }
            else
            {
                color = NeutralSideBg;
            }

            return color;
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

        private void LayoutPreventEyesBlinking()
        {
            GUILayout.Label("Select face expressions with <b>both eyes closed</b>.", _largeFont);
            GUILayout.BeginArea(new Rect(0, singleLineHeight * 3, position.width, GuiSquareHeight * 8));
            var allClips = activity.AllDistinctAnimations();
            var mod = Math.Max(3, Math.Min(8, (int)Math.Sqrt(allClips.Count)));
            for (var element = 0; element < allClips.Count; element++)
            {
                GUILayout.BeginArea(RectAt(element % mod, element / mod));
                DrawBlinkingSwitch(allClips[element]);
                GUILayout.EndArea();
            }
            GUILayout.EndArea();
            GUILayout.Box(
                "",
                GUIStyle.none,
                GUILayout.Width(GuiSquareHeight + GuiSquareHeight * mod + singleLineHeight * 2),
                GUILayout.Height(GuiSquareHeight + GuiSquareHeight * (allClips.Count / mod) + singleLineHeight * 2)
            );
        }

        private void LayoutMakeLipsyncMovementsSubtle()
        {
            EditorGUILayout.HelpBox(@"Limited Lipsync is a feature that will not work with the version of VRChat at the time this version of ComboGestureExpressions has been published.

At the time this version has been published, generating the layer will break your Lipsync blendshapes.", MessageType.Error);
            var helpBoxHeightReverse = 60;
            if (_editorLipsyncTool == 1)
            {
                BeginLayoutUsing(GuiSquareHeight * 8, helpBoxHeightReverse);
                LayoutLimitedLipsyncEditor();
                EndLayout();
            }
            else
            {
                GUILayout.Label("Select face expressions with a <b>wide open mouth</b>.", _largeFont);
                GUILayout.BeginArea(new Rect(0, singleLineHeight * 3 + helpBoxHeightReverse, position.width, GuiSquareHeight * 8));
                var allClips = new HashSet<AnimationClip>(activity.AllDistinctAnimations()).ToList();
                var mod = Math.Max(3, Math.Min(8, (int)Math.Sqrt(allClips.Count)));
                for (var element = 0; element < allClips.Count; element++)
                {
                    GUILayout.BeginArea(RectAt(element % mod, element / mod));
                    DrawLipsyncSwitch(allClips[element]);
                    GUILayout.EndArea();
                }
                GUILayout.EndArea();
                GUILayout.Box(
                    "",
                    GUIStyle.none,
                    GUILayout.Width(GuiSquareHeight + GuiSquareHeight * mod + singleLineHeight * 2),
                    GUILayout.Height(GuiSquareHeight + GuiSquareHeight * (allClips.Count / mod) + singleLineHeight * 2 + helpBoxHeightReverse)
                );
            }
        }

        private void LayoutLimitedLipsyncEditor()
        {
            Rect RectAt(int xGrid, int yGrid)
            {
                return new Rect(xGrid * GuiSquareWidth * 2, yGrid * GuiSquareHeight * 2, GuiSquareWidth * 2, GuiSquareHeight * 2);
            }

            if (limitedLipsync == null) {
                EditorGUILayout.LabelField("Select a ComboGestureLimitedLipsync component in the scene or choose one:");
                limitedLipsync = (ComboGestureLimitedLipsync) EditorGUILayout.ObjectField(null, typeof(ComboGestureLimitedLipsync), true);
                return;
            }

            void DrawLipsync(int visemeNumber, bool previewable)
            {
                GUILayout.Label(_driver.ShortTranslation("viseme" + visemeNumber), _middleAligned);

                GUILayout.BeginArea(new Rect((GuiSquareWidth * 2 - PictureWidth * 2) / 2, singleLineHeight, PictureWidth * 2, PictureHeight * 2 + singleLineHeight * 4));
                GUILayout.Box(_lipsync.TextureForViseme(visemeNumber), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                EditorGUI.BeginDisabledGroup(!previewable || AnimationMode.InAnimationMode());
                if (GUILayout.Button("Regenerate preview"))
                {
                    RegenerateLipsyncPreview(visemeNumber);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Amp Mul", GUILayout.Width(80));
                EditorGUILayout.Slider(serializedLimitedLipsync.FindProperty("amplitude" + visemeNumber), 0f, 1f, GUIContent.none, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Duration Mul", GUILayout.Width(80));
                EditorGUILayout.Slider(serializedLimitedLipsync.FindProperty("transition" + visemeNumber), 0f, 1f, GUIContent.none, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUILayout.Space(PictureHeight);
            }

            GUILayout.BeginArea(RectAt(0, 0));
            EditorGUILayout.LabelField("Limited Lipsync Component", EditorStyles.boldLabel);
            limitedLipsync = (ComboGestureLimitedLipsync) EditorGUILayout.ObjectField(limitedLipsync, typeof(ComboGestureLimitedLipsync), true);

            EditorGUILayout.PropertyField(serializedLimitedLipsync.FindProperty("limitation"), new GUIContent("Category"));
            EditorGUILayout.Slider(serializedLimitedLipsync.FindProperty("amplitudeScale"), 0f, 0.25f, "Viseme Amplitude");
            EditorGUILayout.Slider(serializedLimitedLipsync.FindProperty("amplitudeScale"), 0f, 1f, "(scaled to 1)");
            EditorGUILayout.PropertyField(serializedLimitedLipsync.FindProperty("transitionDuration"), new GUIContent("Transition Duration (s)"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            var previewables = ListAllPreviewableNames();
            var previewIsPossible = previewables.Any();
            if (previewIsPossible) {
                if (_limitedLipsyncPreviewIndex >= previewables.Length)
                {
                    _limitedLipsyncPreviewIndex = 0;
                }
                _limitedLipsyncPreviewIndex = EditorGUILayout.Popup(
                    _limitedLipsyncPreviewIndex,
                    previewables
                );
                var avatarHasVisemeBlendShapes = IsPreviewSetupValid() && activity.previewSetup.avatarDescriptor.VisemeSkinnedMesh;
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
                    _editorMode = EditorMode.MakeLipsyncMovementsSubtle;
                    _editorLipsyncTool = 0;
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
            return activity.limitedLipsync
                .Where(animation => animation.clip != null)
                .Select(animation => animation.clip.name)
                .ToArray();
        }

        private AnimationClip[] ListAllPreviewableClips()
        {
            return activity.limitedLipsync
                .Where(animation => animation.clip != null)
                .Select(animation => animation.clip)
                .ToArray();
        }

        private void RegenerateLipsyncPreviews()
        {
            _lipsync.Prepare(ListAllPreviewableClips()[_limitedLipsyncPreviewIndex]);
        }

        private void RegenerateLipsyncPreview(int visemeNumber)
        {
            _lipsync.PrepareJust(ListAllPreviewableClips()[_limitedLipsyncPreviewIndex], visemeNumber);
        }

        private void LayoutFaceExpressionCombiner()
        {
            if (_combiner == null) return;

            var decider = _combiner.GetDecider();

            GUILayout.BeginHorizontal(GUILayout.Width(CombinerPreviewWidth * 3));

            GUILayout.BeginVertical(GUILayout.MaxWidth(CombinerPreviewWidth));
            GUILayout.Box(_combiner.LeftTexture(), GUILayout.Width(CombinerPreviewWidth), GUILayout.Height(CombinerPreviewHeight));
            LayoutIntersectionDecider(decider.intersection, Side.Left);
            LayoutSideDecider(decider.left, Side.Left);
            GUILayout.Space(singleLineHeight * 2);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MaxWidth(CombinerPreviewWidth), GUILayout.Width(CombinerPreviewWidth));
            GUILayout.Space(20);
            GUILayout.Box(_combiner.CombinedTexture(), GUILayout.Width(CombinerPreviewWidth), GUILayout.Height(CombinerPreviewHeight));
            _combinerCandidateFileName = GUILayout.TextField(_combinerCandidateFileName, GUILayout.MaxWidth(CombinerPreviewWidth));
            if (GUILayout.Button("Save and assign to " + _driver.ShortTranslation((_combinerIsAPermutation ? "p_" : "") + _combinerTarget), GUILayout.MaxWidth(CombinerPreviewWidth)))
            {
                var savedClip = _combiner.SaveTo(_combinerCandidateFileName);
                serializedObject.FindProperty(_combinerTarget).objectReferenceValue = savedClip;
                serializedObject.ApplyModifiedProperties();

                GenerateMissingPreviews();
                if (_combinerIsLikelyEyesClosed)
                {
                    activity.blinking.Add(savedClip);
                }
                _editorMode = EditorMode.SetFaceExpressions;
            }
            GUILayout.Space(singleLineHeight * 2);
            _complexCombiner = EditorGUILayout.Toggle("Show hidden", _complexCombiner);
            GUILayout.Space(singleLineHeight * 2);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MaxWidth(CombinerPreviewWidth));
            GUILayout.Box(_combiner.RightTexture(), GUILayout.Width(CombinerPreviewWidth), GUILayout.Height(CombinerPreviewHeight));
            LayoutIntersectionDecider(decider.intersection, Side.Right);
            LayoutSideDecider(decider.right, Side.Right);
            GUILayout.Space(singleLineHeight * 2);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void LayoutSideDecider(List<SideDecider> sideDeciders, Side side)
        {
            var duplicate = sideDeciders.ToList(); // collection is being modified while iterating and I don't have time to fix this
            foreach (var sideDecider in duplicate)
            {
                var value = sideDecider.Choice;
                GUILayout.BeginHorizontal();

                if (side == Side.Left) {
                    EditorGUI.BeginDisabledGroup(!value);
                    GUILayout.Label(ToFormattedName(sideDecider.Key, value), _normalFont);
                    EditorGUI.EndDisabledGroup();
                }

                if (GUILayout.Button((value ? "" : "(") + sideDecider.SampleValue + (value ? "" : ")"), GUILayout.Width(50 - (sideDecider.SampleValue == 0 ? 20 : 0))))
                {
                    _combiner.UpdateSide(side, sideDecider.Key, sideDecider.SampleValue, !value);
                }

                if (side == Side.Right) {
                    EditorGUI.BeginDisabledGroup(!value);
                    GUILayout.Label(ToFormattedName(sideDecider.Key, value), _normalFont);
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.EndHorizontal();
            }
        }

        private void LayoutIntersectionDecider(List<IntersectionDecider> intersectionDeciders, Side side)
        {
            var duplicate = intersectionDeciders.ToList(); // collection is being modified while iterating and I don't have time to fix this
            foreach (var intersectionDecider in duplicate)
            {
                var currentChoice = intersectionDecider.Choice;
                var valueAsBool = side == Side.Left && currentChoice == IntersectionChoice.UseLeft ||
                        side == Side.Right && currentChoice == IntersectionChoice.UseRight;

                var formattedName = ToFormattedName(
                    intersectionDecider.Key,
                    valueAsBool
                );

                if (_complexCombiner || (side == Side.Left && intersectionDecider.SampleLeftValue != 0 || side == Side.Right && intersectionDecider.SampleRightValue != 0))
                {
                    GUILayout.BeginHorizontal();
                    if (side == Side.Left)
                    {
                        EditorGUI.BeginDisabledGroup(!valueAsBool);
                        GUILayout.Label(formattedName, _normalFont);
                        EditorGUI.EndDisabledGroup();
                    }
                    var sampleValue = side == Side.Left ? intersectionDecider.SampleLeftValue : intersectionDecider.SampleRightValue;
                    bool useButton;
                    if (intersectionDecider.SampleLeftValue != intersectionDecider.SampleRightValue)
                    {
                        string message;
                        if (intersectionDecider.Choice != IntersectionChoice.UseNone)
                        {
                            message = (side == Side.Left && valueAsBool ? "← " : "") + sampleValue + (side == Side.Right && valueAsBool ? " →" : "");
                        }
                        else
                        {
                            message = "(" + sampleValue + ")";
                        }
                        useButton = GUILayout.Button(message, GUILayout.Width(80 - (sampleValue == 0 ? 50 : 0)));
                    }
                    else
                    {
                        string message;
                        if (intersectionDecider.Choice != IntersectionChoice.UseNone)
                        {
                            message = "" + sampleValue;
                        }
                        else
                        {
                            message = "(" + sampleValue + ")";
                        }
                        useButton = GUILayout.Button(message, GUILayout.Width(50 - (intersectionDecider.SampleLeftValue == 0 ? 20 : 0)));
                    }
                    if (useButton)
                    {
                        IntersectionChoice newChoice;
                        if (_complexCombiner || intersectionDecider.SampleLeftValue != 0 && intersectionDecider.SampleRightValue != 0)
                        {
                            if (side == Side.Left)
                            {
                                newChoice = currentChoice != IntersectionChoice.UseLeft ? IntersectionChoice.UseLeft : IntersectionChoice.UseNone;
                            }
                            else
                            {
                                newChoice = currentChoice != IntersectionChoice.UseRight ? IntersectionChoice.UseRight : IntersectionChoice.UseNone;
                            }
                        }
                        else
                        {
                            if (side == Side.Left)
                            {
                                newChoice = currentChoice != IntersectionChoice.UseLeft ? IntersectionChoice.UseLeft : IntersectionChoice.UseRight;
                            }
                            else
                            {
                                newChoice = currentChoice != IntersectionChoice.UseRight ? IntersectionChoice.UseRight : IntersectionChoice.UseLeft;
                            }
                        }
                        _combiner.UpdateIntersection(intersectionDecider, newChoice);
                    }
                    if (side == Side.Right) {
                        EditorGUI.BeginDisabledGroup(!valueAsBool);
                        GUILayout.Label(formattedName, _normalFont);
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private static string ToFormattedName(CurveKey key, bool value)
        {
            var niceName = key.PropertyName.Replace("blendShape.", "::") + " @ " + key.Path;
            if (value)
            {
                return "<b>" + niceName + "</b>";
            }

            return niceName;
        }

        private static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * GuiSquareWidth, yGrid * GuiSquareHeight, GuiSquareWidth, GuiSquareHeight);
        }

        private void DrawInner(string propertyPath)
        {
            DrawInner(propertyPath, null);
        }

        private void DrawInner(string propertyPath, string oppositePath, bool partial = false)
        {
            var usePermutations = oppositePath != null;
            var property = serializedObject.FindProperty(propertyPath);
            var oppositeProperty = usePermutations ? serializedObject.FindProperty(oppositePath) : null;
            var isLeftHand = String.Compare(propertyPath, oppositePath, StringComparison.Ordinal) > 0;
            if (usePermutations)
            {
                if (propertyPath == oppositePath)
                {
                    DrawColoredBackground(NeutralSideBg);
                }
                else if ((isLeftHand && property.objectReferenceValue != null || !isLeftHand && oppositeProperty.objectReferenceValue != null) && property.objectReferenceValue != oppositeProperty.objectReferenceValue)
                {
                    DrawColoredBackground(isLeftHand ? LeftSideBg : RightSideBg);
                }
                else
                {
                    if (isLeftHand && partial)
                    {
                        // FIXME Rank preservation
                        BeginInvisibleRankPreservingArea();
                        EndInvisibleRankPreservingArea();
                        BeginInvisibleRankPreservingArea();
                        InvisibleRankPreservingButton();
                        EndInvisibleRankPreservingArea();
                        return;
                    }
                    DrawColoredBackground(SymmetricalBg);
                }
            }

            var translatableProperty = (usePermutations ? "p_" : "") + propertyPath;
            GUILayout.Label(_driver.ShortTranslation(translatableProperty), _driver.IsSymmetrical(translatableProperty) ? _middleAlignedBold : _middleAligned);

            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, singleLineHeight, PictureWidth, PictureHeight));
            var element = property.objectReferenceValue != null ? (AnimationClip) property.objectReferenceValue : null;
            if (element != null)
            {
                DrawPreviewOrRefreshButton(element);
            }
            else if (usePermutations)
            {
                if (oppositeProperty.objectReferenceValue != null && propertyPath != oppositePath && isLeftHand)
                {
                    // FIXME: Rank preservation
                    // DrawColoredBackground(RightSideBg);
                    DrawInnerReversal(oppositePath);
                }
            }

            GUILayout.EndArea();

            if (_driver.IsAPropertyThatCanBeCombined(propertyPath, usePermutations))
            {
                var rect = element != null
                    ? new Rect(GuiSquareWidth - 2 * singleLineHeight, PictureHeight - singleLineHeight * 0.5f, singleLineHeight * 2, singleLineHeight * 1.5f)
                    : new Rect(GuiSquareWidth - 100, PictureHeight - singleLineHeight * 0.5f, 100, singleLineHeight * 1.5f);

                var areSourcesCompatible = _driver.AreCombinationSourcesDefinedAndCompatible(serializedObject, propertyPath);
                EditorGUI.BeginDisabledGroup(!areSourcesCompatible);
                GUILayout.BeginArea(rect);
                if (GUILayout.Button(element != null ? "+": "+ Combine"))
                {
                    var merge = _driver.ProvideCombinationPropertySources(propertyPath);
                    OpenMergeWindowFor(merge.Left, merge.Right, propertyPath, usePermutations);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                BeginInvisibleRankPreservingArea();
                InvisibleRankPreservingButton();
                EndInvisibleRankPreservingArea();
            }

            if (element == null && _driver.IsAutoSettable(propertyPath))
            {
                var propertyPathToCopyFrom = _driver.GetAutoSettableSource(propertyPath);
                var animationToBeCopied = serializedObject.FindProperty(propertyPathToCopyFrom).objectReferenceValue;

                EditorGUI.BeginDisabledGroup(animationToBeCopied == null);
                GUILayout.BeginArea(new Rect(GuiSquareWidth - 100, PictureHeight - singleLineHeight * 1.75f, 100, singleLineHeight * 1.5f));
                if (GUILayout.Button("Auto-set"))
                {
                    AutoSet(propertyPath, propertyPathToCopyFrom);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }
            else if (element == null && _driver.AreCombinationSourcesIdentical(serializedObject, propertyPath))
            {
                var propertyPathToCopyFrom = _driver.ProvideCombinationPropertySources(propertyPath).Left;
                var animationToBeCopied = serializedObject.FindProperty(propertyPathToCopyFrom).objectReferenceValue;

                EditorGUI.BeginDisabledGroup(animationToBeCopied == null);
                GUILayout.BeginArea(new Rect(GuiSquareWidth - 100, PictureHeight - singleLineHeight * 1.75f, 100, singleLineHeight * 1.5f));
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
                InvisibleRankPreservingButton();
                EndInvisibleRankPreservingArea();
            }

            GUILayout.Space(PictureHeight);
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }

        private void DrawInnerReversal(string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);

            var edge = (GuiSquareWidth - PictureWidth) / 2;
            GUILayout.BeginArea(new Rect(edge, singleLineHeight / 2, PictureWidth - edge * 2, PictureHeight - singleLineHeight));
            var element = property.objectReferenceValue != null ? (AnimationClip) property.objectReferenceValue : null;
            DrawPreviewOrRefreshButton(element, true);
            GUILayout.EndArea();
        }

        private void DrawTransitionEdit()
        {
            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, singleLineHeight, PictureWidth, PictureHeight));
            GUILayout.Label("Transition duration");
            GUILayout.Label("(in seconds)");
            EditorGUILayout.Slider(_transitionDuration, 0f, 1f, GUIContent.none);
            GUILayout.EndArea();
            GUILayout.Space(PictureHeight);
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
            var leftAnim = serializedObject.FindProperty(left).objectReferenceValue;
            var rightAnim = serializedObject.FindProperty(right).objectReferenceValue;

            var areBothAnimations = leftAnim is AnimationClip && rightAnim is AnimationClip;
            if (!areBothAnimations) return;

            _combiner = new CgeActivityEditorCombiner(activity, (AnimationClip) leftAnim, (AnimationClip) rightAnim, Repaint);
            _combiner.Prepare();

            _combinerTarget = propertyPath;
            _combinerIsAPermutation = usePermutations;
            _combinerCandidateFileName = "cge_" + leftAnim.name + "__combined__" + rightAnim.name;

            _combinerIsLikelyEyesClosed = activity.blinking.Contains(leftAnim) || activity.blinking.Contains(rightAnim);

            _editorMode = EditorMode.CombineFaceExpressions;
        }

        private void AutoSet(string propertyPath, string propertyPathToCopyFrom)
        {
            serializedObject.FindProperty(propertyPath).objectReferenceValue = serializedObject.FindProperty(propertyPathToCopyFrom).objectReferenceValue;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBlinkingSwitch(AnimationClip element)
        {
            var isRegisteredAsBlinking = activity.blinking.Contains(element);

            if (isRegisteredAsBlinking)
            {
                DrawColoredBackground(new Color(0.44f, 0.65f, 1f));
            }
            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, 0, PictureWidth, PictureHeight));
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(PictureHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element, typeof(AnimationClip), true);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(isRegisteredAsBlinking ? "Blinking" : ""))
            {
                if (isRegisteredAsBlinking)
                {
                    activity.blinking.Remove(element);
                }
                else
                {
                    activity.blinking.Add(element);
                }
            }
        }

        private void DrawLipsyncSwitch(AnimationClip element)
        {
            var isRegisteredAsLipsync = activity.limitedLipsync.Exists(animation => animation.clip == element);

            if (isRegisteredAsLipsync) {
                var col = GUI.color;
                try
                {
                    GUI.color = new Color(0.44f, 0.65f, 1f);
                    GUI.Box(new Rect(0, 0, GuiSquareWidth, GuiSquareHeight), "");
                }
                finally
                {
                    GUI.color = col;
                }
            }
            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, 0, PictureWidth, PictureHeight));
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(PictureHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element, typeof(AnimationClip), true);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(isRegisteredAsLipsync ? "Limited Lipsync" : ""))
            {
                if (isRegisteredAsLipsync)
                {
                    activity.limitedLipsync.RemoveAll(animation => animation.clip == element);
                }
                else
                {
                    activity.limitedLipsync.Add(new ComboGestureActivity.LimitedLipsyncAnimation
                    {
                        clip = element,
                        limitation = ComboGestureActivity.LipsyncLimitation.WideOpenMouth
                    });
                }
            }
        }

        private void DrawArbitrary(AnimationClip element)
        {
            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, 0, PictureWidth, PictureHeight));
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(PictureHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element, typeof(AnimationClip), true);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawPreviewOrRefreshButton(AnimationClip element, bool grayscale = false)
        {
            if (element != null) {
                var clipIsInDict = _animationClipToTextureDict.ContainsKey(element);

                if (clipIsInDict)
                {
                    var texture = grayscale ? _animationClipToTextureDictGray[element] : _animationClipToTextureDict[element];
                    GUILayout.Box(texture, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    InvisibleRankPreservingButton();
                }
                else
                {
                    InvisibleRankPreservingBox();
                    EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                    if (GUILayout.Button(IsPreviewSetupValid() ? "Generate\npreview" : "Setup\npreview", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (IsPreviewSetupValid())
                        {
                            new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, _animationClipToTextureDictGray, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, element);
                        }
                        else
                        {
                            _editorMode = EditorMode.OtherOptions;
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

        private static void InvisibleRankPreservingButton()
        {
            GUILayout.Button("", GUIStyle.none, GUILayout.Width(0), GUILayout.Height(0));
        }

        private void DoAutoSetupPreview()
        {
            _setupResult = new AutoSetupPreview(activity).AutoSetup();
        }

        private static void DrawColoredBackground(Color color)
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
    }
}
