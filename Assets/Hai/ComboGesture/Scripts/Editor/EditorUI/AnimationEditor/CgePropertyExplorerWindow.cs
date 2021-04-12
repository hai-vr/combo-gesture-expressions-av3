﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.AnimationEditor
{
    public struct CgePropertyExplorerSubInfo
    {
        public string Property;
        public EditorCurveBinding Binding;
        public Texture2D BoundaryTexture;
        public Texture2D HotspotTexture;
        public string Path;
    }

    public class CgePropertyExplorerWindow : EditorWindow
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
        private CgeAnimationEditor _animationEditor;
        private bool _basedOnSomethingElseMode;

        private void OnEnable()
        {
            _animationEditor = Cge.Get().AnimationEditor;
            titleContent = new GUIContent("CGE Property Explorer");
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
            var previewSetup = _animationEditor.DummyNullable();
            if (previewSetup != null)
            {
                _animationEditor.NotifyNewScan();
            }

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
            // GUILayout.Box("", GUILayout.Width(StandardWidth), GUILayout.Height(StandardHeight));
            GUILayout.BeginVertical();
            var previewSetup = _animationEditor.DummyNullable();
            string[] options = { previewSetup != null ? previewSetup.name : "None" };
            EditorGUILayout.Popup("Preview dummy", 0, options);
            _hotspotMode = GUILayout.Toggle(_hotspotMode, "Show Hotspots (press SPACE key)");
            if (GreenBackground(_basedOnSomethingElseMode, () => GUILayout.Button("Fix Tooth and other hidden blendshapes")))
            {
                _basedOnSomethingElseMode = !_basedOnSomethingElseMode;
            }
            if (GUILayout.Button("(debug) Regenerate"))
            {
                OnNewScanRequested();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - EditorGUIUtility.singleLineHeight * 6));
            GUILayout.BeginHorizontal();

            int widthRun = HalfWidth + TempBorder;
            var smrBlendShapeProperties = _animationEditor.SmrBlendShapeProperties();
            foreach (var info in smrBlendShapeProperties)
            {
                GUILayout.BeginVertical();
                GUILayout.Box(_hotspotMode ? info.HotspotTexture : info.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                GUILayout.Label(info.Property.StartsWith(BlendshapePrefix) ? info.Property.Substring(BlendshapePrefix.Length) : info.Property, GUILayout.Width(HalfWidth));

                GUILayout.BeginHorizontal();
                var basedOnWhat = _animationEditor.GetBased(info.Property);
                if (!IsInBasedOnSomethingElseMode())
                {
                    EditorGUI.BeginDisabledGroup(!_animationEditor.HasActiveClip());
                    var blendshapeExistsInAnimation = _animationEditor.ActiveHas(info.Path, info.Property);
                    EditorGUI.BeginDisabledGroup(blendshapeExistsInAnimation);
                    if (GUILayout.Button("+", GUILayout.ExpandWidth(true)))
                    {
                        _animationEditor.AddBlendShape(info.Path, info.Property);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(!blendshapeExistsInAnimation);
                    if (RedBackground(blendshapeExistsInAnimation, () => GUILayout.Button("-", GUILayout.ExpandWidth(true))))
                    {
                        _animationEditor.RemoveBlendShape(info.Path, info.Property);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.EndDisabledGroup();

                    if (basedOnWhat != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.Label("Based", GUILayout.Width(40));
                        EditorGUI.EndDisabledGroup();
                    }
                }
                else
                {
                    if (basedOnWhat != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayout.TextField(basedOnWhat.Substring(BlendshapePrefix.Length), GUILayout.ExpandWidth(true));
                        EditorGUI.EndDisabledGroup();
                        if (RedBackground(true, () => GUILayout.Button("Forget", GUILayout.Width(70))))
                        {
                            _animationEditor.RemoveBasedSubject(info.Property);
                        }
                    }
                    else
                    {
                        var isSelected = _basedOnSomethingElseSelection.Contains(info.Property);
                        EditorGUI.BeginDisabledGroup(_animationEditor.IsBased(info.Property));
                        if (GreenBackground(isSelected, () => GUILayout.Button("Select", GUILayout.Width(70))))
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
                            _animationEditor.AssignBased(info.Property, _basedOnSomethingElseSelection.ToList());
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

        public static CgePropertyExplorerWindow Obtain()
        {
            var editor = GetWindow<CgePropertyExplorerWindow>(false, null, false);
            return editor;
        }

        [MenuItem("Window/Haï/CGE Property Explorer")]
        public static void OpenEditor()
        {
            Obtain().Show();
        }

        public void OnNewClipSelected(AnimationClip active)
        {
        }

        private static T GreenBackground<T>(bool isActive, Func<T> inside)
        {
            var col = GUI.color;
            try
            {
                if (isActive) GUI.color = Color.green;
                return inside();
            }
            finally
            {
                GUI.color = col;
            }
        }

        private static T RedBackground<T>(bool isActive, Func<T> inside)
        {
            var col = GUI.color;
            try
            {
                if (isActive) GUI.color = Color.red;
                return inside();
            }
            finally
            {
                GUI.color = col;
            }
        }
    }
}
