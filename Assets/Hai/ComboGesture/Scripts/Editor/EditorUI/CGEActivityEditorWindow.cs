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
    public class CgeEditorWindow : EditorWindow
    {
        private const int PictureWidth = 120;
        private const int PictureHeight = 80;
        private const int GuiSquareWidth = 140;
        private static readonly int GuiSquareHeight = (int) (singleLineHeight * 3 + PictureHeight);

        private static readonly Dictionary<string, string> equivs = new Dictionary<string, string>
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

        public AnimationClip noAnimationClipNullObject;
        private Dictionary<AnimationClip, Texture2D> animationClipToTextureDict;
        private RenderTexture _renderTexture;
        public ComboGestureActivity activity;
        public SerializedObject serializedObject;

        private SerializedProperty transitionDuration;
        private SerializedProperty editorTool;
        private SerializedProperty editorArbitraryAnimations;
        private SerializedProperty previewSetup;
        private int _currentEditorToolValue = -1;
        private int _editorMode;
        private bool _firstTimeSetup;
        private AutoSetupPreview.SetupResult? _setupResult;


        private void OnEnable()
        {
            noAnimationClipNullObject = new AnimationClip();
            animationClipToTextureDict = new Dictionary<AnimationClip, Texture2D>();
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
                transitionDuration = serializedObject.FindProperty("transitionDuration");
                editorTool = serializedObject.FindProperty("editorTool");
                editorArbitraryAnimations = serializedObject.FindProperty("editorArbitraryAnimations");
                previewSetup = serializedObject.FindProperty("previewSetup");

                if (editorTool.intValue != _currentEditorToolValue && _currentEditorToolValue >= 0)
                {
                    editorTool.intValue = _currentEditorToolValue;
                    serializedObject.ApplyModifiedProperties();
                }

                if (activity.previewSetup == null)
                {
                    _editorMode = 2;
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
            _editorMode = GUILayout.Toolbar(_editorMode, new []
            {
                "Edit face expressions", "Select closed eyes animations", "Other options"
            });
            if (_editorMode == 0) {
                editorTool.intValue = GUILayout.Toolbar(editorTool.intValue, new []
                {
                    "All gestures", "Singles", "Analog Fist", "Combos"
                });
                _currentEditorToolValue = editorTool.intValue;
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, singleLineHeight * 4, position.width, GuiSquareHeight * 8));
            switch (_editorMode)
            {
                case 1:
                    DrawClosedEyesManifest();
                    break;
                case 2:
                    DrawOtherOptions();
                    break;
                default:
                    switch (editorTool.intValue)
                    {
                        case 1:
                            DrawSinglesDoublesMatrixProjection();
                            break;
                        case 2:
                            DrawFistMatrixProjection();
                            break;
                        case 3:
                            DrawComboMatrixProjection();
                            break;
                        default:
                            DrawFullMatrixProjection();
                            break;
                    }

                    break;
            }
            GUILayout.EndArea();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOtherOptions()
        {
            if (_firstTimeSetup || activity.previewSetup == null)
            {
                EditorGUILayout.PropertyField(previewSetup, new GUIContent("Preview setup"));
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
                EditorGUILayout.PropertyField(transitionDuration, new GUIContent("Transition duration (s)"));
                EditorGUILayout.PropertyField(previewSetup, new GUIContent("Preview setup"));
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
                if (GUILayout.Button("Generate missing previews"))
                {
                    new CgeActivityPreviewInternal(activity, animationClipToTextureDict, noAnimationClipNullObject, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing);
                }

                if (GUILayout.Button("Regenerate all previews"))
                {
                    new CgeActivityPreviewInternal(activity, animationClipToTextureDict, noAnimationClipNullObject, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.RecalculateEverything);
                }
                EditorGUILayout.PropertyField(editorArbitraryAnimations, new GUIContent("List of arbitrary animations to generate previews (Drag and drop assets directly on this title)"), true);

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

        private void DrawFistMatrixProjection()
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

        private void DrawSinglesDoublesMatrixProjection()
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

        private void DrawComboMatrixProjection()
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

        private void DrawFullMatrixProjection()
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

        private void DrawClosedEyesManifest()
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

        private static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * GuiSquareWidth, yGrid * GuiSquareHeight, GuiSquareWidth, GuiSquareHeight);
        }

        private void DrawInner(string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);
            if (equivs[propertyPath].Contains("x2"))
            {
                var guiStyle = new GUIStyle(EditorStyles.boldLabel);
                guiStyle.alignment = TextAnchor.MiddleCenter;
                guiStyle.clipping = TextClipping.Overflow;
                GUILayout.Label(equivs[propertyPath], guiStyle);
            }
            else
            {
                var guiStyle = new GUIStyle();
                guiStyle.alignment = TextAnchor.MiddleCenter;
                guiStyle.clipping = TextClipping.Overflow;
                GUILayout.Label(equivs[propertyPath], guiStyle);
            }

            GUILayout.BeginArea(new Rect((GuiSquareWidth - PictureWidth) / 2, singleLineHeight, PictureWidth, PictureHeight));
            var element = property.objectReferenceValue != null ? (AnimationClip) property.objectReferenceValue : null;
            DrawPreviewOrRefreshButton(element);
            GUILayout.EndArea();

            GUILayout.Space(PictureHeight);
            EditorGUILayout.PropertyField(property, GUIContent.none);
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

            var clipIsInDict = animationClipToTextureDict.ContainsKey(element);
            if (clipIsInDict)
            {
                GUILayout.Box(animationClipToTextureDict[element], GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
            else
            {
                // GUILayout.Box((Texture)null, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true), GUILayout.MaxWidth(0), GUILayout.MaxHeight(0));
            }

            EditorGUILayout.BeginFadeGroup(!clipIsInDict ? 1 : 0);
            if (GUILayout.Button(activity.previewSetup ? "Generate\npreview" : "Setup\npreview", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                if (activity.previewSetup)
                {
                    new CgeActivityPreviewInternal(activity, animationClipToTextureDict, noAnimationClipNullObject, PictureWidth, PictureHeight, activity.editorArbitraryAnimations).Process(CgeActivityPreviewInternal.ProcessMode.CalculateMissing);
                }
                else
                {
                    _editorMode = 2;
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DoAutoSetupPreview()
        {
            _setupResult = new AutoSetupPreview(activity).AutoSetup();
        }
    }
}
