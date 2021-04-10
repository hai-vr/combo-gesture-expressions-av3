using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Modules;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.AnimationEditor
{
    public struct CgePropertyExplorerSubInfo
    {
        public string Property;
        public AnimationPreview Preview;
        public Texture2D BoundaryTexture;
        public Texture2D HotspotTexture;
    }

    public class CgePropertyExplorerWindow : EditorWindow
    {
        private const int PreviewResolutionMultiplier = 1;
        private const int StandardWidth = 300;
        private const int StandardHeight = 200;
        private const int TempBorder = 10;
        private const int HalfWidth = StandardWidth / 2;
        private const int HalfHeight = StandardHeight / 2;
        private readonly CgeRenderingCommands _renderingCommands;
        private Texture2D _activePreview;

        private List<CgePropertyExplorerSubInfo> _editableProperties = new List<CgePropertyExplorerSubInfo>();
        private Texture2D _based;
        private Action _action;
        private bool _isCalling;
        private Vector2 _scrollPos;
        private bool _disabledUndo;

        private bool _hotspotMode;

        public CgePropertyExplorerWindow()
        {
            _renderingCommands = Cge.RenderingCommands;
            _activePreview = new Texture2D(StandardWidth * PreviewResolutionMultiplier, StandardHeight * PreviewResolutionMultiplier, TextureFormat.ARGB32, false);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("CGE Property Explorer");
            Undo.undoRedoPerformed += () =>
            {
                if (_disabledUndo) return;
                OnUndoRedoPerformed();
            };
            OnNewScanRequested();
        }

        private void OnDestroy()
        {
            _disabledUndo = true;
        }

        private void OnInspectorUpdate()
        {
        }

        private void OnUndoRedoPerformed()
        {
        }

        private void OnNewScanRequested()
        {
            var previewSetup = AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
            if (previewSetup != null)
            {
                GeneratePreviews(previewSetup);
            }

            Repaint();
        }

        private void GeneratePreviews(ComboGesturePreviewSetup previewSetup)
        {
            RenderBased(previewSetup);
            CalculateAndRenderBlendshapes(previewSetup);
        }

        private void CalculateAndRenderBlendshapes(ComboGesturePreviewSetup previewSetup)
        {
            var smr = previewSetup.avatarDescriptor.VisemeSkinnedMesh;
            var mesh = smr.sharedMesh;
            var smrPath = SharedLayerUtils.ResolveRelativePath(previewSetup.avatarDescriptor.transform, smr.transform);
            _editableProperties = Enumerable.Range(0, mesh.blendShapeCount)
                .Select(i => mesh.GetBlendShapeName(i))
                .Select(blendShapeName => new EditorCurveBinding
                {
                    path = smrPath,
                    propertyName = "blendShape." + blendShapeName,
                    type = typeof(SkinnedMeshRenderer)
                })
                .Select(binding =>
                {
                    var clip = new AnimationClip();
                    AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1/60f, 100f));
                    var preview = new AnimationPreview(clip, NewTexture2D());
                    return new CgePropertyExplorerSubInfo
                    {
                        Preview = preview,
                        Property = binding.propertyName,
                        BoundaryTexture = NewTexture2D(),
                        HotspotTexture = NewTexture2D()
                    };
                }).ToList();
            _renderingCommands.GenerateSpecificFastMode(
                _editableProperties.Select(info => info.Preview).ToList(),
                preview =>
                {
                    var editable = _editableProperties
                        .First(info => info.Preview == preview);
                    CgeRenderingSupport.MutateHighlightHotspots(editable.HotspotTexture, preview.RenderTexture, _based);
                    CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                    Repaint();
                },
                previewSetup
            );
        }

        private void RenderBased(ComboGesturePreviewSetup previewSetup)
        {
            var smr = previewSetup.avatarDescriptor.VisemeSkinnedMesh;
            var smrPath = SharedLayerUtils.ResolveRelativePath(previewSetup.avatarDescriptor.transform, smr.transform);
            _based = NewTexture2D();
            var nsk = new AnimationClip();
            // AnimationUtility.SetEditorCurve(nsk, new EditorCurveBinding
            // {
                // path = "_ignored",
                // propertyName = "m_Active",
                // type = typeof(GameObject)
            // }, AnimationCurve.Constant(0f, 0f, 0f));
            AnimationUtility.SetEditorCurve(nsk, new EditorCurveBinding
            {
                path = smrPath,
                propertyName = "blendShape._ignored",
                type = typeof(SkinnedMeshRenderer)
            }, AnimationCurve.Constant(0f, 0f, 0f));
            _renderingCommands.GenerateSpecificFastMode(
                new List<AnimationPreview> {new AnimationPreview(nsk, _based)},
                preview => { },
                previewSetup
            );
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
            GUILayout.Box(_activePreview, GUILayout.Width(StandardWidth), GUILayout.Height(StandardHeight));
            GUILayout.BeginVertical();
            var previewSetup = AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
            string[] options = { previewSetup != null ? previewSetup.name : "None" };
            EditorGUILayout.Popup("Preview dummy", 0, options);
            _hotspotMode = GUILayout.Toggle(_hotspotMode, "Show Hotspots (press SPACE key)");
            if (GUILayout.Button("(debug) Regenerate"))
            {
                OnNewScanRequested();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height - StandardHeight - EditorGUIUtility.singleLineHeight * 2));
            GUILayout.BeginHorizontal();

            int widthRun = HalfWidth + TempBorder;
            for (var index = 0; index < _editableProperties.Count; index++)
            {
                var editableProperty = _editableProperties[index];
                GUILayout.BeginVertical();
                GUILayout.Box(_hotspotMode ? editableProperty.HotspotTexture : editableProperty.BoundaryTexture, GUIStyle.none, GUILayout.Width(HalfWidth), GUILayout.Height(HalfHeight));
                var blendshapePrefix = "blendShape.";
                GUILayout.Label(editableProperty.Property.StartsWith(blendshapePrefix) ? editableProperty.Property.Substring(blendshapePrefix.Length) : editableProperty.Property, GUILayout.Width(HalfWidth));

                GUILayout.BeginHorizontal();
                GUILayout.Button("Swap...", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Delete", GUILayout.Width(StandardWidth / 5)))
                {
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

        private Texture2D NewTexture2D()
        {
            return new Texture2D(HalfWidth * PreviewResolutionMultiplier, HalfHeight * PreviewResolutionMultiplier, TextureFormat.ARGB32, false);
        }

        private static CgePropertyExplorerWindow Obtain()
        {
            var editor = GetWindow<CgePropertyExplorerWindow>();
            return editor;
        }

        [MenuItem("Window/Haï/CGE Property Explorer")]
        public static void OpenEditor()
        {
            Obtain().Show();
        }
    }
}
