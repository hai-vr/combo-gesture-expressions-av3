using System;
using System.IO;
using Hai.ComboGesture.Scripts.Editor.Internal.Modules;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.AnimationEditor
{
    public struct CgeAnimationEditorSubInfo
    {
        public string Property;
        public AnimationPreview Preview;
        public float Value;
        public EditorCurveBinding Binding;
        public Texture2D BoundaryTexture;
    }

    public class CgeAnimationEditorWindow : EditorWindow
    {
        private const int StandardWidth = 300;
        private const int StandardHeight = 200;
        private const int TempBorder = 10;
        private const int HalfWidth = StandardWidth / 2;
        private const int HalfHeight = StandardHeight / 2;

        private CgeAnimationEditor _animationEditor;

        private AnimationClip _currentClip;
        private string _currentClipAssetRename;
        private Vector2 _scrollPos;
        private bool _disabledUndo;

        private void OnEnable()
        {
            _animationEditor = Cge.Get().AnimationEditor;
            titleContent = new GUIContent("CGE Animation Editor");
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

        private void OnInspectorUpdate()
        {
            var active = Selection.activeObject;
            if (active == null) return;
            if (!(active is AnimationClip)) return;

            _animationEditor.NotifyCurrentClip((AnimationClip)active);
        }

        private void OnUndoRedoPerformed()
        {
            if (_currentClip == null) return;

            OnNewClipSelected(_currentClip);
        }

        public void OnNewClipSelected(AnimationClip active)
        {
            _currentClip = active;
            _currentClipAssetRename = _currentClip.name;

            Repaint();
        }

        private void OnGUI()
        {
            if (_currentClip == null)
            {
                GUILayout.Label("No animation selected.");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Box(_animationEditor.ActivePreview(), GUILayout.Width(StandardWidth), GUILayout.Height(StandardHeight));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            _currentClipAssetRename = EditorGUILayout.TextField(_currentClipAssetRename, GUILayout.ExpandWidth(true));
            EditorGUI.BeginDisabledGroup(_currentClipAssetRename == _currentClip.name || File.Exists(NewPath()));
            if (GUILayout.Button("Rename", GUILayout.Width(70)))
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(_currentClip), NewPath());
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            var previewSetup = _animationEditor.DummyNullable();
            string[] options = { previewSetup != null ? previewSetup.name : "None" };
            EditorGUILayout.Popup("Preview dummy", 0, options);
            if (GreenBackground(_animationEditor.IsMaintaining(), () => GUILayout.Button("Preview animation in Scene")))
            {
                _animationEditor.MaintainPreviewToggled();
            }
            if (GUILayout.Button("Delete 0-values"))
            {
                _animationEditor.Delete0Values();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - StandardHeight - EditorGUIUtility.singleLineHeight * 2));
            GUILayout.BeginHorizontal();

            int widthRun = HalfWidth + TempBorder;
            var editableProperties = _animationEditor.Editables();
            for (var index = 0; index < editableProperties.Count; index++)
            {
                var editableProperty = editableProperties[index];
                GUILayout.BeginVertical();
                GUILayout.Box(editableProperty.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                var blendshapePrefix = "blendShape.";
                GUILayout.Label(editableProperty.Property.StartsWith(blendshapePrefix) ? editableProperty.Property.Substring(blendshapePrefix.Length) : editableProperty.Property, GUILayout.Width(HalfWidth));
                var newValue = EditorGUILayout.Slider(editableProperty.Value, 0, 100, GUILayout.Width(HalfWidth));
                if (newValue != editableProperty.Value)
                {
                    _animationEditor.UpdateEditable(editableProperty, index, newValue);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Delete", GUILayout.Width(StandardWidth / 5)))
                {
                    _animationEditor.DeleteEditable(index);
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

            if (GUILayout.Button("+", GUILayout.Width(HalfWidth / 2), GUILayout.Height(HalfHeight)))
            {
                CgePropertyExplorerWindow.OpenEditor();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private string NewPath()
        {
            return $"{FolderOfClip()}{_currentClipAssetRename}.anim";
        }

        private string FolderOfClip()
        {
            var assetPath = AssetDatabase.GetAssetPath(_currentClip);
            return assetPath.Replace(Path.GetFileName(assetPath), "");
        }

        public static CgeAnimationEditorWindow Obtain()
        {
            var editor = GetWindow<CgeAnimationEditorWindow>(false, null, false);
            return editor;
        }

        [MenuItem("Window/Haï/CGE Animation Editor")]
        public static void OpenEditor()
        {
            Obtain().Show();
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
    }
}
