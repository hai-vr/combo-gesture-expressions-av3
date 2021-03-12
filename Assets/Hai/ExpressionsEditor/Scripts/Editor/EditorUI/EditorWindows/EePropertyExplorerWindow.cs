using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.EditorUI.EditorWindows
{
    public class EePropertyExplorerWindow : EditorWindow
    {
        private const int StandardWidth = 300;
        private const int StandardHeight = 200;
        private const int TempBorder = 10;
        private const int HalfWidth = StandardWidth / 2;
        private const int HalfHeight = StandardHeight / 2;
        private const string BlendshapePrefix = "blendShape.";

        private Texture2D _based;
        private Action _action;
        private bool _isCalling;
        private Vector2 _scrollPos;
        private bool _disabledUndo;

        private bool _hotspotMode;
        private readonly HashSet<string> _basedOnSomethingElseSelection = new HashSet<string>();
        private bool _basedOnSomethingElseMode;
        private bool _foldoutMoreTools;

        private EeEditCommands _editCommands;
        private EePreviewCommands _previewCommands;
        private EeAccessCommands _accessCommands;

        private void OnEnable()
        {
            _editCommands = Ee.Get().EditCommands;
            _previewCommands = Ee.Get().PreviewCommands;
            _accessCommands = Ee.Get().AccessCommands;

            titleContent = new GUIContent("EE Property Explorer");
            Undo.undoRedoPerformed += () =>
            {
                if (_disabledUndo) return;
                OnUndoRedoPerformed();
            };
        }

        private void OnDestroy()
        {
            _disabledUndo = true;
        }

        private void OnUndoRedoPerformed()
        {
        }

        private void OnNewScanRequested()
        {
            _previewCommands.RequestExplorerBlendshapes();
            Repaint();
        }

        private void OnGUI()
        {
            var e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                _hotspotMode = !_hotspotMode;
                Repaint();
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            _hotspotMode = GUILayout.Toggle(_hotspotMode, "Show Hotspots (press SPACE key)");

            if (GUILayout.Button("Generate previews"))
            {
                OnNewScanRequested();
            }

            _foldoutMoreTools = EditorGUILayout.Foldout(_foldoutMoreTools, "Other tools");
            if (_foldoutMoreTools)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (EeAnimationEditorWindow.ColoredBackground(_basedOnSomethingElseMode, Color.green, () => GUILayout.Button("Fix Tooth and other hidden blendshapes")))
                {
                    _basedOnSomethingElseMode = !_basedOnSomethingElseMode;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - EditorGUIUtility.singleLineHeight * 6));
            GUILayout.BeginHorizontal();

            var widthRun = HalfWidth + TempBorder;
            var smrBlendShapeProperties = _accessCommands.SmrBlendShapeProperties() ?? new List<EeExplorerBlendshape>();
            var activeCouldDeleteValidables = _accessCommands.AllCurveBindingsCache();
            var nonResetBlendshapes = _accessCommands.AllNonResetBlendshapes(_accessCommands.AllCurveBindingsCache());
            foreach (var info in smrBlendShapeProperties)
            {
                GUILayout.BeginVertical();
                GUILayout.Box(_hotspotMode ? info.HotspotTexture : info.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                GUILayout.Label(info.Property.StartsWith(BlendshapePrefix) ? info.Property.Substring(BlendshapePrefix.Length) : info.Property, GUILayout.Width(HalfWidth));

                GUILayout.BeginHorizontal();
                var onWhat = _accessCommands.GetBasedOnWhat(info.Property);
                if (!IsInBasedOnSomethingElseMode())
                {
                    EditorGUI.BeginDisabledGroup(!_accessCommands.HasActiveClip());
                    var blendshapeExistsAndIsNotReset = _accessCommands.ActiveHas(nonResetBlendshapes, info.Path, info.Property);
                    EditorGUI.BeginDisabledGroup(blendshapeExistsAndIsNotReset);
                    if (GUILayout.Button("+", GUILayout.ExpandWidth(true)))
                    {
                        _editCommands.AddBlendshape(info.Path, info.Property);
                    }
                    EditorGUI.EndDisabledGroup();

                    var blendshapeIsRemovable = _accessCommands.ActiveCouldDelete(activeCouldDeleteValidables, info.Path, info.Property);
                    EditorGUI.BeginDisabledGroup(!blendshapeIsRemovable);
                    if (EeAnimationEditorWindow.ColoredBackground(blendshapeIsRemovable, !blendshapeExistsAndIsNotReset ? Color.yellow : Color.red, () => GUILayout.Button("-", GUILayout.ExpandWidth(true))))
                    {
                        _editCommands.DeleteBlendshape(info.Path, info.Property);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.EndDisabledGroup();

                    if (onWhat != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.Label("Based", GUILayout.Width(40));
                        EditorGUI.EndDisabledGroup();
                    }
                }
                else
                {
                    if (onWhat != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.TextField(onWhat.Substring(BlendshapePrefix.Length), GUILayout.ExpandWidth(true));
                        EditorGUI.EndDisabledGroup();
                        if (EeAnimationEditorWindow.ColoredBackground(true, Color.red, () => GUILayout.Button("Forget", GUILayout.Width(70))))
                        {
                            _editCommands.DeleteBasedSubject(info.Property);
                        }
                    }
                    else
                    {
                        var isSelected = _basedOnSomethingElseSelection.Contains(info.Property);
                        EditorGUI.BeginDisabledGroup(_accessCommands.IsOnWhat(info.Property));
                        if (EeAnimationEditorWindow.ColoredBackground(isSelected, Color.green, () => GUILayout.Button("Select", GUILayout.Width(70))))
                        {
                            if (isSelected)
                            {
                                _basedOnSomethingElseSelection.Remove(info.Property);
                            }
                            else
                            {
                                _basedOnSomethingElseSelection.Add(info.Property);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(isSelected || _basedOnSomethingElseSelection.Count == 0);
                        if (GUILayout.Button("Assign"))
                        {
                            _editCommands.AssignBased(info.Property, _basedOnSomethingElseSelection.ToList());
                            _basedOnSomethingElseSelection.Clear();
                            _basedOnSomethingElseMode = false;
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                widthRun += HalfWidth + TempBorder;
                if (Screen.width < widthRun)
                {
                    widthRun = HalfWidth + TempBorder;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private bool IsInBasedOnSomethingElseMode()
        {
            return _basedOnSomethingElseMode;
        }

        public static EePropertyExplorerWindow Obtain()
        {
            var editor = GetWindow<EePropertyExplorerWindow>(false, null, false);
            return editor;
        }

        [MenuItem("Window/Haï/EE Property Explorer")]
        public static void OpenEditor()
        {
            Obtain().Show();
        }
    }
}
