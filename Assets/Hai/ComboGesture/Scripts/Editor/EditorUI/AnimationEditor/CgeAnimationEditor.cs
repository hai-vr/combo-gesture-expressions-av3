using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Modules;
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
        private readonly CgeRenderingCommands _renderingCommands;
        private Texture2D _activePreview;

        private AnimationClip _currentClip;
        private List<CgeAnimationEditorSubInfo> _editableProperties = new List<CgeAnimationEditorSubInfo>();
        private Texture2D _based;
        private Action _action;
        private bool _isCalling;
        private string _currentClipAssetRename;
        private Vector2 _scrollPos;
        private bool _disabledUndo;

        public CgeAnimationEditorWindow()
        {
            _renderingCommands = Cge.RenderingCommands;
            _activePreview = new Texture2D(StandardWidth, StandardHeight, TextureFormat.ARGB32, false);
        }

        private void OnEnable()
        {
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
            if (active == _currentClip) return;
            OnNewClipSelected((AnimationClip)active);
        }

        private void OnUndoRedoPerformed()
        {
            if (_currentClip == null) return;

            OnNewClipSelected(_currentClip);
        }

        private void OnNewClipSelected(AnimationClip active)
        {
            _currentClip = active;
            _currentClipAssetRename = _currentClip.name;
            var previewSetup = AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
            if (previewSetup != null)
            {
                GeneratePreviews(active, previewSetup);
            }

            Repaint();
        }

        private void GeneratePreviews(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            RenderMain(active, previewSetup);
            RenderBased(previewSetup);
            CalculateAndRenderEditables(active, previewSetup);
        }

        private void RenderMain(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            var currentPreview = new Texture2D(StandardWidth, StandardHeight, TextureFormat.ARGB32, false);
            _renderingCommands.GenerateSpecificFastMode(
                new List<AnimationPreview> {new AnimationPreview(active, currentPreview)},
                preview =>
                {
                    _activePreview = currentPreview;
                    Repaint();
                },
                previewSetup
            );
        }

        private void CalculateAndRenderEditables(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            _editableProperties = AnimationUtility.GetCurveBindings(active)
                .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                .Where(binding => AnimationUtility.GetEditorCurve(active, binding).keys.All(keyframe => keyframe.value != 0))
                .Select(binding =>
                {
                    var clip = new AnimationClip();
                    AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(active, binding));
                    var preview = new AnimationPreview(clip, new Texture2D(HalfWidth, HalfHeight, TextureFormat.ARGB32, false));
                    return new CgeAnimationEditorSubInfo
                    {
                        Preview = preview,
                        Property = binding.propertyName,
                        Value = AnimationUtility.GetEditorCurve(active, binding).keys.Select(keyframe => keyframe.value).Min(),
                        Binding = binding,
                        BoundaryTexture = new Texture2D(HalfWidth, HalfHeight, TextureFormat.ARGB32, false)
                    };
                }).ToList();
            _renderingCommands.GenerateSpecificFastMode(
                _editableProperties.Select(info => info.Preview).ToList(),
                preview =>
                {
                    var editable = _editableProperties
                        .First(info => info.Preview == preview);
                    CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                    Repaint();
                },
                previewSetup
            );
        }

        private void RenderBased(ComboGesturePreviewSetup previewSetup)
        {
            _based = new Texture2D(HalfWidth, HalfHeight, TextureFormat.ARGB32, false);
            var nsk = new AnimationClip();
            AnimationUtility.SetEditorCurve(nsk, new EditorCurveBinding
            {
                path = "_ignored",
                propertyName = "m_Active",
                type = typeof(GameObject)
            }, AnimationCurve.Constant(0f, 0f, 0f));
            _renderingCommands.GenerateSpecificFastMode(
                new List<AnimationPreview> {new AnimationPreview(nsk, _based)},
                preview => { },
                previewSetup
            );
        }

        private void OnGUI()
        {
            if (_currentClip == null)
            {
                GUILayout.Label("No animation selected.");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Box(_activePreview, GUILayout.Width(StandardWidth), GUILayout.Height(StandardHeight));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            _currentClipAssetRename = EditorGUILayout.TextField(_currentClipAssetRename,GUILayout.ExpandWidth(true));
            EditorGUI.BeginDisabledGroup(_currentClipAssetRename == _currentClip.name || File.Exists(NewPath()));
            if (GUILayout.Button("Rename", GUILayout.Width(HalfWidth)))
            {
                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(_currentClip), NewPath());
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            var previewSetup = AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
            string[] options = { previewSetup != null ? previewSetup.name : "None" };
            EditorGUILayout.Popup("Preview dummy", 0, options);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - StandardHeight - EditorGUIUtility.singleLineHeight * 2));
            GUILayout.BeginHorizontal();

            int widthRun = HalfWidth + TempBorder;
            for (var index = 0; index < _editableProperties.Count; index++)
            {
                var editableProperty = _editableProperties[index];
                GUILayout.BeginVertical();
                GUILayout.Box(editableProperty.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                var blendshapePrefix = "blendShape.";
                GUILayout.Label(editableProperty.Property.StartsWith(blendshapePrefix) ? editableProperty.Property.Substring(blendshapePrefix.Length) : editableProperty.Property, GUILayout.Width(HalfWidth));
                var newValue = EditorGUILayout.Slider(editableProperty.Value, 0, 100, GUILayout.Width(HalfWidth));
                if (newValue != editableProperty.Value)
                {
                    WhenValueModified(editableProperty, newValue, index);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Button("Swap...", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Delete", GUILayout.Width(StandardWidth / 5)))
                {
                    WhenDelete(index);
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

        private void WhenValueModified(CgeAnimationEditorSubInfo editableProperty, float newValue, int index)
        {
            Undo.RecordObject(_currentClip, "Value modified");
            var oldCurve = AnimationUtility.GetEditorCurve(_currentClip, editableProperty.Binding);
            var newCurve = new AnimationCurve(oldCurve.keys.Select(keyframe =>
            {
                keyframe.value = newValue;
                return keyframe;
            }).ToArray());
            editableProperty.Value = newValue;
            _editableProperties[index] = editableProperty;
            AnimationUtility.SetEditorCurve(_currentClip, editableProperty.Binding, newCurve);
            AnimationUtility.SetEditorCurve(editableProperty.Preview.Clip, editableProperty.Binding, newCurve);
            var previewSetup = AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
            UpdatePreview(previewSetup, editableProperty.Preview);
        }

        private void WhenDelete(int index)
        {
            var editableProperty = _editableProperties[index];
            Undo.RecordObject(_currentClip, "Value modified");
            AnimationUtility.SetEditorCurve(_currentClip, editableProperty.Binding, null);
            _editableProperties.RemoveAt(index);
        }

        private void UpdatePreview(ComboGesturePreviewSetup previewSetup, AnimationPreview animationPreview)
        {
            _action = () =>
            {
                RenderMain(_currentClip, previewSetup);
                _renderingCommands.GenerateSpecificFastMode(
                    new List<AnimationPreview> {animationPreview},
                    preview =>
                    {
                        var editable = _editableProperties
                            .First(info => info.Preview == preview);
                        CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                        Repaint();
                    },
                    previewSetup
                );
            };

            if (!_isCalling)
            {
                EditorApplication.delayCall += () =>
                {
                    _isCalling = false;
                    _action();
                };
                _isCalling = true;
            }
        }

        private static CgeAnimationEditorWindow Obtain()
        {
            var editor = GetWindow<CgeAnimationEditorWindow>();
            return editor;
        }

        [MenuItem("Window/Haï/CGE Animation Editor")]
        public static void OpenEditor()
        {
            Obtain().Show();
        }
    }
}
