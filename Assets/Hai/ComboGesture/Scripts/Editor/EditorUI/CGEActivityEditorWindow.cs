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
        private RenderTexture _renderTexture;
        public ComboGestureActivity activity;
        public SerializedObject serializedObject;

        private SerializedProperty _transitionDuration;
        private SerializedProperty _editorTool;
        private SerializedProperty _editorArbitraryAnimations;
        private SerializedProperty _previewSetup;
        private int _currentEditorToolValue = -1;
        private EditorMode _editorMode;
        private bool _firstTimeSetup;
        private AutoSetupPreview.SetupResult? _setupResult;

        private CgeActivityEditorDriver _driver;
        private CgeActivityEditorCombiner _combiner;
        private string _combinerTarget;
        private string _combinerCandidateFileName;
        private bool _combinerIsLikelyEyesClosed;

        private static GUIStyle _middleAligned;
        private static GUIStyle _middleAlignedBold;
        private static GUIStyle _largeFont;
        private static GUIStyle _normalFont;

        private Vector2 scrollPos;

        private void OnEnable()
        {
            _animationClipToTextureDict = new Dictionary<AnimationClip, Texture2D>();
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

                if (_editorTool.intValue != _currentEditorToolValue && _currentEditorToolValue >= 0)
                {
                    _editorTool.intValue = _currentEditorToolValue;
                    serializedObject.ApplyModifiedProperties();
                }

                if (activity.previewSetup == null)
                {
                    _editorMode = EditorMode.OtherOptions;
                    _firstTimeSetup = true;
                }
            }

            if (serializedObject == null)
            {
                return;
            }

            if (_firstTimeSetup && activity.previewSetup != null)
            {
                _firstTimeSetup = false;
            }

            serializedObject.Update();

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
                    LayoutActivityEditor();
                    break;
            }
            GUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateToolbarArea()
        {
            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorMode = (EditorMode) GUILayout.Toolbar((int) _editorMode, new[]
            {
                "Set face expressions", "Prevent eyes blinking", "Make lipsync movements subtle", "Combine face expressions", "Other options"
            });
            if (_editorMode == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                _editorTool.intValue = GUILayout.Toolbar(_editorTool.intValue, new[]
                {
                    "All gestures", "Singles", "Analog Fist", "Combos"
                }, GUILayout.ExpandWidth(true));

                GUILayout.Space(30);
                GUILayout.EndHorizontal();
                _currentEditorToolValue = _editorTool.intValue;
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
                default:
                    BeginLayoutUsing(GuiSquareHeight * 8);
                    LayoutFullMatrixProjection();
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

        private static void EndLayout()
        {
            GUILayout.EndArea();
        }

        private void LayoutOtherOptions()
        {
            if (_firstTimeSetup || activity.previewSetup == null)
            {
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
                    new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything, null);
                }
                EditorGUILayout.PropertyField(_editorArbitraryAnimations, new GUIContent("List of arbitrary animations to generate previews (Drag and drop assets directly on this title)"), true);
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
            new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null);
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
            var allClips = new HashSet<AnimationClip>(activity.OrderedAnimations().Where(clip => clip != null)).ToList();
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
            GUILayout.Label("Select face expressions with a <b>wide open mouth</b>.", _largeFont);
            GUILayout.BeginArea(new Rect(0, singleLineHeight * 3, position.width, GuiSquareHeight * 8));
            var allClips = new HashSet<AnimationClip>(activity.OrderedAnimations().Where(clip => clip != null)).ToList();
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
                GUILayout.Height(GuiSquareHeight + GuiSquareHeight * (allClips.Count / mod) + singleLineHeight * 2)
            );
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
            if (GUILayout.Button("Save and assign to " + _driver.ShortTranslation(_combinerTarget), GUILayout.MaxWidth(CombinerPreviewWidth)))
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

                if (GUILayout.Button("Use", GUILayout.Width(50)))
                {
                    _combiner.UpdateSide(side, sideDecider.Key, !value);
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
                GUILayout.BeginHorizontal();

                var currentChoice = intersectionDecider.Choice;
                var valueAsBool = side == Side.Left && currentChoice == IntersectionChoice.UseLeft ||
                        side == Side.Right && currentChoice == IntersectionChoice.UseRight;

                var formattedName = ToFormattedName(
                    intersectionDecider.Key,
                    valueAsBool
                );
                if (side == Side.Left)
                {
                    EditorGUI.BeginDisabledGroup(!valueAsBool);
                    GUILayout.Label(formattedName, _normalFont);
                    EditorGUI.EndDisabledGroup();
                }
                if (GUILayout.Button("Use", GUILayout.Width(80)))
                {
                    IntersectionChoice newChoice;
                    if (side == Side.Left)
                    {
                        newChoice = currentChoice != IntersectionChoice.UseLeft ? IntersectionChoice.UseLeft : IntersectionChoice.UseNone;
                    }
                    else
                    {
                        newChoice = currentChoice != IntersectionChoice.UseRight ? IntersectionChoice.UseRight : IntersectionChoice.UseNone;
                    }
                    _combiner.UpdateIntersection(intersectionDecider.Key, newChoice);
                }
                if (side == Side.Right) {
                    EditorGUI.BeginDisabledGroup(!valueAsBool);
                    GUILayout.Label(formattedName, _normalFont);
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
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
            var property = serializedObject.FindProperty(propertyPath);
            GUILayout.Label(_driver.ShortTranslation(propertyPath), _driver.IsSymmetrical(propertyPath) ? _middleAlignedBold : _middleAligned);

            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, singleLineHeight, PictureWidth, PictureHeight));
            var element = property.objectReferenceValue != null ? (AnimationClip) property.objectReferenceValue : null;
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            if (_driver.IsAPropertyThatCanBeCombined(propertyPath))
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
                    OpenMergeWindowFor(merge.Left, merge.Right, propertyPath);
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

        private static void BeginInvisibleRankPreservingArea()
        {
            GUILayout.BeginArea(new Rect(-1000, -1000, 0, 0));
        }

        private static void EndInvisibleRankPreservingArea()
        {
            GUILayout.EndArea();
        }

        private void OpenMergeWindowFor(string left, string right, string propertyPath)
        {
            var leftAnim = serializedObject.FindProperty(left).objectReferenceValue;
            var rightAnim = serializedObject.FindProperty(right).objectReferenceValue;

            var areBothAnimations = leftAnim is AnimationClip && rightAnim is AnimationClip;
            if (!areBothAnimations) return;

            _combiner = new CgeActivityEditorCombiner(activity, (AnimationClip) leftAnim, (AnimationClip) rightAnim, Repaint);
            _combiner.Prepare();

            _combinerTarget = propertyPath;
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

            if (isRegisteredAsBlinking) {
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

        private void DrawPreviewOrRefreshButton(AnimationClip element)
        {
            if (element != null) {
                var clipIsInDict = _animationClipToTextureDict.ContainsKey(element);

                if (clipIsInDict)
                {
                    GUILayout.Box(_animationClipToTextureDict[element], GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    InvisibleRankPreservingButton();
                }
                else
                {
                    InvisibleRankPreservingBox();
                    EditorGUI.BeginDisabledGroup(AnimationMode.InAnimationMode());
                    if (GUILayout.Button(activity.previewSetup ? "Generate\npreview" : "Setup\npreview", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (activity.previewSetup)
                        {
                            new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, element);
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
    }
}
