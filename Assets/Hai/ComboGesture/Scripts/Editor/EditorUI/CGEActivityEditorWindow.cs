using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    enum EditorMode
    {
        SetFaceExpressions,
        PreventEyesBlinking,
        CombineFaceExpressions,
        OtherOptions
    }

    public class CgeEditorWindow : EditorWindow
    {
        private const int PictureWidth = 120;
        private const int PictureHeight = 80;
        private const int GuiSquareWidth = 140;
        private static readonly int GuiSquareHeight = (int) (singleLineHeight * 3 + PictureHeight);

        private static readonly Dictionary<string, string> Equivs = new Dictionary<string, string>
        {
            {"anim00", "No gesture"},
            {"anim01", "FIST"},
            {"anim02", "OPEN"},
            {"anim03", "POINT"},
            {"anim04", "PEACE"},
            {"anim05", "ROCKNROLL"},
            {"anim06", "GUN"},
            {"anim07", "THUMBSUP"},
            {"anim11", "FIST x2, L+R trigger"},
            {"anim11_L", "FIST x2, LEFT trigger"},
            {"anim11_R", "FIST x2, RIGHT trigger"},
            {"anim12", "OPEN + FIST"},
            {"anim13", "POINT + FIST"},
            {"anim14", "PEACE + FIST"},
            {"anim15", "ROCKNROLL + FIST"},
            {"anim16", "GUN + FIST"},
            {"anim17", "THUMBSUP + FIST"},
            {"anim22", "OPEN x2"},
            {"anim23", "OPEN + POINT"},
            {"anim24", "OPEN + PEACE"},
            {"anim25", "OPEN + ROCKNROLL"},
            {"anim26", "OPEN + GUN"},
            {"anim27", "OPEN + THUMBSUP"},
            {"anim33", "POINT x2"},
            {"anim34", "POINT + PEACE"},
            {"anim35", "POINT + ROCKNROLL"},
            {"anim36", "POINT + GUN"},
            {"anim37", "POINT + THUMBSUP"},
            {"anim44", "PEACE x2"},
            {"anim45", "PEACE + ROCKNROLL"},
            {"anim46", "PEACE + GUN"},
            {"anim47", "PEACE + THUMBSUP"},
            {"anim55", "ROCKNROLL x2"},
            {"anim56", "ROCKNROLL + GUN"},
            {"anim57", "ROCKNROLL + THUMBSUP"},
            {"anim66", "GUN x2"},
            {"anim67", "GUN + THUMBSUP"},
            {"anim77", "THUMBSUP x2"}
        };

        private static readonly Dictionary<string, MergePair> ParameterToMerge = new Dictionary<string, MergePair>
        {
            {"anim12", new MergePair("anim01", "anim02")},
            {"anim13", new MergePair("anim01", "anim03")},
            {"anim14", new MergePair("anim01", "anim04")},
            {"anim15", new MergePair("anim01", "anim05")},
            {"anim16", new MergePair("anim01", "anim06")},
            {"anim17", new MergePair("anim01", "anim07")},
            {"anim23", new MergePair("anim02", "anim03")},
            {"anim24", new MergePair("anim02", "anim04")},
            {"anim25", new MergePair("anim02", "anim05")},
            {"anim26", new MergePair("anim02", "anim06")},
            {"anim27", new MergePair("anim02", "anim07")},
            {"anim34", new MergePair("anim03", "anim04")},
            {"anim35", new MergePair("anim03", "anim05")},
            {"anim36", new MergePair("anim03", "anim06")},
            {"anim37", new MergePair("anim03", "anim07")},
            {"anim45", new MergePair("anim04", "anim05")},
            {"anim46", new MergePair("anim04", "anim06")},
            {"anim47", new MergePair("anim04", "anim07")},
            {"anim56", new MergePair("anim05", "anim06")},
            {"anim57", new MergePair("anim05", "anim07")},
            {"anim67", new MergePair("anim06", "anim07")}
        };

        public AnimationClip noAnimationClipNullObject;
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

        private SerializedProperty _mergeTarget;
        private SerializedProperty _mergeLeft;
        private SerializedProperty _mergeRight;

        private void OnEnable()
        {
            noAnimationClipNullObject = new AnimationClip();
            _animationClipToTextureDict = new Dictionary<AnimationClip, Texture2D>();
        }

        private void OnGUI()
        {
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null)
            {
                var selectedActivity = activeGameObject.GetComponent<ComboGestureActivity>();
                if (selectedActivity != null && selectedActivity != activity)
                {
                    activity = selectedActivity;
                    serializedObject = null;
                }
            }

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

            GUILayout.BeginArea(new Rect(0, singleLineHeight, position.width, singleLineHeight * 3));
            _editorMode = (EditorMode) GUILayout.Toolbar((int) _editorMode, new []
            {
                "Set face expressions", "Prevent eyes blinking", "Combine face expressions", "Other options"
            });
            if (_editorMode == 0) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                _editorTool.intValue = GUILayout.Toolbar(_editorTool.intValue, new []
                {
                    "All gestures", "Singles", "Analog Fist", "Combos"
                }, GUILayout.ExpandWidth(true));

                GUILayout.Space(30);
                GUILayout.EndHorizontal();
                _currentEditorToolValue = _editorTool.intValue;
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, singleLineHeight * 4, position.width, GuiSquareHeight * 8));
            switch (_editorMode)
            {
                case EditorMode.PreventEyesBlinking:
                    LayoutPreventEyesBlinking();
                    break;
                case EditorMode.CombineFaceExpressions:
                    LayoutFaceExpressionCombiner();
                    break;
                case EditorMode.OtherOptions:
                    LayoutOtherOptions();
                    break;
                default:
                    switch (_editorTool.intValue)
                    {
                        case 1:
                            LayoutSinglesDoublesMatrixProjection();
                            break;
                        case 2:
                            LayoutFistMatrixProjection();
                            break;
                        case 3:
                            LayoutComboMatrixProjection();
                            break;
                        default:
                            LayoutFullMatrixProjection();
                            break;
                    }

                    break;
            }
            GUILayout.EndArea();

            serializedObject.ApplyModifiedProperties();
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
                    new CgeActivityPreviewInternal(Repaint, activity, _animationClipToTextureDict, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing, null);
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
                    CgeActivityPreviewInternal.Stop_Temp();
                }
                EditorGUI.EndDisabledGroup();

                if (activity.editorArbitraryAnimations != null) {
                    GUILayout.BeginArea(new Rect(0, singleLineHeight * 10, position.width, GuiSquareHeight * 8));
                    var allClips = new HashSet<AnimationClip>(activity.editorArbitraryAnimations.Where(clip => clip != null)).ToList();
                    var mod = 8;
                    for (var element = 0; element < allClips.Count; element++)
                    {
                        GUILayout.BeginArea(RectAt(element % mod, element / mod));
                        DrawArbitrary(allClips[element]);
                        GUILayout.EndArea();
                    }
                    GUILayout.EndArea();
                }
            }
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
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontSize = 20;
            GUILayout.Label("Select face expressions with <b>both eyes closed</b>.", myStyle);
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
        }

        private void LayoutFaceExpressionCombiner()
        {
            GUILayout.Label("TBD");
        }

        private static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * GuiSquareWidth, yGrid * GuiSquareHeight, GuiSquareWidth, GuiSquareHeight);
        }

        private void DrawInner(string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (Equivs[propertyPath].Contains("x2"))
            {
                var guiStyle = new GUIStyle(EditorStyles.boldLabel);
                guiStyle.alignment = TextAnchor.MiddleCenter;
                guiStyle.clipping = TextClipping.Overflow;
                GUILayout.Label(Equivs[propertyPath], guiStyle);
            }
            else
            {
                var guiStyle = new GUIStyle();
                guiStyle.alignment = TextAnchor.MiddleCenter;
                guiStyle.clipping = TextClipping.Overflow;
                GUILayout.Label(Equivs[propertyPath], guiStyle);
            }

            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, singleLineHeight, PictureWidth, PictureHeight));
            var element = property.objectReferenceValue != null ? (AnimationClip) property.objectReferenceValue : null;
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            if (ParameterToMerge.ContainsKey(propertyPath)) {
                var merge = ParameterToMerge[propertyPath];
                var left = serializedObject.FindProperty(merge.Left).objectReferenceValue;
                var right = serializedObject.FindProperty(merge.Right).objectReferenceValue;
                var areMergeAnimationsDefinedAndNotTheSame = left != null && right != null && left != right;

                EditorGUI.BeginDisabledGroup(!areMergeAnimationsDefinedAndNotTheSame);
                GUILayout.BeginArea(new Rect(GuiSquareWidth - 2 * singleLineHeight, PictureHeight - singleLineHeight * 0.5f, singleLineHeight * 2, singleLineHeight * 1.5f));
                if (GUILayout.Button("M"))
                {
                    OpenMergeWindowFor(propertyPath, merge.Left, merge.Right);
                }
                GUILayout.EndArea();
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.Space(PictureHeight);
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }

        private void OpenMergeWindowFor(string target, string left, string right)
        {
            _mergeTarget = serializedObject.FindProperty(target);
            _mergeLeft = serializedObject.FindProperty(left);
            _mergeRight = serializedObject.FindProperty(right);
            _editorMode = EditorMode.CombineFaceExpressions;
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
            if (element == null) return;

            var clipIsInDict = _animationClipToTextureDict.ContainsKey(element);
            if (clipIsInDict)
            {
                GUILayout.Box(_animationClipToTextureDict[element], GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.BeginFadeGroup(!clipIsInDict ? 1 : 0);
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
            EditorGUILayout.EndFadeGroup();
        }

        private void DoAutoSetupPreview()
        {
            _setupResult = new AutoSetupPreview(activity).AutoSetup();
        }
    }

    internal readonly struct MergePair
    {
        public MergePair(string left, string right)
        {
            Left = left;
            Right = right;
        }

        public string Left { get; }
        public string Right { get; }
    }
}
