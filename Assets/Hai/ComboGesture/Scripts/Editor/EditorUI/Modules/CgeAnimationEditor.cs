using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI.AnimationEditor;
using Hai.ComboGesture.Scripts.Editor.Internal;
using Hai.ComboGesture.Scripts.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Modules
{
    public class CgeAnimationEditor
    {
        private const int PreviewResolutionMultiplier = 1;
        private const int StandardWidth = 300;
        private const int StandardHeight = 200;
        private const int HalfWidth = StandardWidth / 2;
        private const int HalfHeight = StandardHeight / 2;
        private const string CgeAnimationEditorMetadataAssetPath = "Assets/Hai/CgeAnimationEditorMetadata.asset";

        private readonly CgeRenderingCommands _renderingCommands;
        private Texture2D _activePreview;
        private Texture2D _based;
        private AnimationClip _currentClip;
        private List<CgeAnimationEditorSubInfo> _editableProperties;
        private List<CgePropertyExplorerSubInfo> _smrBlendShapeProperties;
        private Action _action;
        private bool _isCalling;
        private bool _maintain;
        private CgeAnimationEditorMetadata _metadataAsset;

        public CgeAnimationEditor()
        {
            _renderingCommands = Cge.RenderingCommands;
            _renderingCommands.SetQueueEmptiedAction(() =>
            {
                if (!_maintain) return;

                Maintain();
            });
        }

        public Texture2D ActivePreview()
        {
            EnsureActivePreviewInitialized();
            return _activePreview;
        }

        public List<CgeAnimationEditorSubInfo> Editables()
        {
            return _editableProperties;
        }

        private void EnsureBasedInitialized()
        {
            if (_based != null) return;

            _based = NewTexture2D();
        }

        private void EnsureActivePreviewInitialized()
        {
            if (_activePreview != null) return;

            _activePreview = new Texture2D(StandardWidth, StandardHeight, TextureFormat.ARGB32, false);
        }

        public void NotifyCurrentClip(AnimationClip activeNonNull)
        {
            if (_currentClip == activeNonNull) return;

            _currentClip = activeNonNull;

            CgeAnimationWindow2.Obtain().OnNewClipSelected(_currentClip);
            CgePropertyExplorerWindow.Obtain().OnNewClipSelected(_currentClip);

            GeneratePreviewsFromCurrentClip(_currentClip, DummyNullable());
        }

        private void GeneratePreviewsFromCurrentClip(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            RenderMain(active, previewSetup);
            RenderBased(previewSetup);
            CalculateAndRenderEditables(active, previewSetup);
        }

        public void NotifyNewScan()
        {
            GeneratePreviewsFromPropertyExplorer(DummyNullable());
        }

        private void GeneratePreviewsFromPropertyExplorer(ComboGesturePreviewSetup previewSetup)
        {
            RenderBased(previewSetup);
            CalculateAndRenderBlendshapes(previewSetup);
        }

        private void RenderMain(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            EnsureActivePreviewInitialized();
            _renderingCommands.GenerateSpecificFastMode(
                new List<AnimationPreview> {new AnimationPreview(AnimationUtility.GetCurveBindings(active).Length == 0 ? NothingClip() : active, _activePreview)},
                preview =>
                {
                    CgeAnimationWindow2.Obtain().Repaint();
                },
                previewSetup,
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide,
                CgeRenderingCommands.CgePriority.High
            );
        }

        private void RenderBased(ComboGesturePreviewSetup previewSetup)
        {
            EnsureBasedInitialized();
            _renderingCommands.GenerateSpecificFastMode(
                new List<AnimationPreview> {new AnimationPreview(NothingClip(), _based)},
                preview => { },
                previewSetup,
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide
            );
        }

        private static AnimationClip NothingClip()
        {
            var nsk = new AnimationClip();
            AnimationUtility.SetEditorCurve(nsk, new EditorCurveBinding
            {
                path = "_ignored",
                propertyName = "m_Active",
                type = typeof(GameObject)
            }, AnimationCurve.Constant(0f, 0f, 0f));
            return nsk;
        }

        private void CalculateAndRenderEditables(AnimationClip active, ComboGesturePreviewSetup previewSetup)
        {
            _editableProperties = AnimationUtility.GetCurveBindings(active)
                .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                .Where(binding => AnimationUtility.GetEditorCurve(active, binding).keys.All(keyframe => keyframe.value != 0))
                .Select(binding => SubInfoFromActive(active, binding)).ToList();

            if (_editableProperties.Count == 0) return;
            GenerateEditables(previewSetup, _editableProperties.Select(info => info.Preview).ToList());
        }

        private void GenerateEditables(ComboGesturePreviewSetup previewSetup, List<AnimationPreview> toRender)
        {
            _renderingCommands.GenerateSpecificFastMode(
                toRender,
                preview =>
                {
                    var editable = _editableProperties
                        .First(info => info.Preview == preview);
                    EnsureBasedInitialized();
                    CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                    CgeAnimationWindow2.Obtain().Repaint();
                },
                previewSetup,
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide
            );
        }

        private static CgeAnimationEditorSubInfo SubInfoFromActive(AnimationClip active, EditorCurveBinding binding)
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
        }

        private void CalculateAndRenderBlendshapes(ComboGesturePreviewSetup previewSetup)
        {
            var smr = previewSetup.avatarDescriptor.VisemeSkinnedMesh;
            var mesh = smr.sharedMesh;
            _smrBlendShapeProperties = AllBlendshapes(mesh, SharedLayerUtils.ResolveRelativePath(previewSetup.avatarDescriptor.transform, smr.transform));
            _renderingCommands.GenerateSpecificFastMode(
                _smrBlendShapeProperties.Select(info => info.Preview).ToList(),
                preview =>
                {
                    var editable = _smrBlendShapeProperties
                        .First(info => info.Preview == preview);
                    CgeRenderingSupport.MutateHighlightHotspots(editable.HotspotTexture, preview.RenderTexture, _based);
                    CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                    CgePropertyExplorerWindow.Obtain().Repaint();
                },
                previewSetup,
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide,
                CgeRenderingCommands.CgePriority.Low
            );
        }

        private List<CgePropertyExplorerSubInfo> AllBlendshapes(Mesh mesh, string smrPath)
        {
            var allBlendshapes = Enumerable.Range(0, mesh.blendShapeCount)
                .Select(i => mesh.GetBlendShapeName(i))
                .Select(blendShapeName => new EditorCurveBinding
                {
                    path = smrPath,
                    propertyName = "blendShape." + blendShapeName,
                    type = typeof(SkinnedMeshRenderer)
                })
                .Select(binding => SubInfoFromBinding(smrPath, binding)).ToList();

            var lookup = allBlendshapes.ToLookup(info => info.Property.ToLowerInvariant().StartsWith("blendshape.vrc.v_"));
            return lookup[false].Concat(lookup[true]).ToList();
        }

        private static AnimationClip BlendShapeClip(EditorCurveBinding binding)
        {
            var clip = new AnimationClip();
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            return clip;
        }

        private Texture2D NewTexture2D()
        {
            return new Texture2D(HalfWidth * PreviewResolutionMultiplier, HalfHeight * PreviewResolutionMultiplier, TextureFormat.ARGB32, false);
        }

        public void UpdateEditable(CgeAnimationEditorSubInfo editableProperty, int index, float newValue)
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

            _action = () =>
            {
                RenderMain(_currentClip, DummyNullable());
                _renderingCommands.GenerateSpecificFastMode(
                    new List<AnimationPreview> {_editableProperties[index].Preview},
                    preview =>
                    {
                        var editable = _editableProperties
                            .First(info => info.Preview == preview);
                        EnsureBasedInitialized();
                        CgeRenderingSupport.MutateMultilevelHighlightDifferences(editable.BoundaryTexture, preview.RenderTexture, _based);
                        CgeAnimationWindow2.Obtain().Repaint();
                    },
                    DummyNullable(),
                    CgeRenderingCommands.CgeDummyAutoHide.DoNotHide
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

        public void DeleteEditable(int index)
        {
            // FIXME: Bad function signature
            Undo.RecordObject(_currentClip, "Value modified");

            var editableProperty = _editableProperties[index];
            AnimationUtility.SetEditorCurve(_currentClip, editableProperty.Binding, null);
            _editableProperties.RemoveAt(index);
            RenderMain(_currentClip, DummyNullable());
        }

        public ComboGesturePreviewSetup DummyNullable()
        {
            return AutoSetupPreview.MaybeFindLastActiveAndValidPreviewComponentInRoot();
        }

        public List<CgePropertyExplorerSubInfo> SmrBlendShapeProperties()
        {
            return _smrBlendShapeProperties;
        }

        public void AddBlendShape(string path, string property)
        {
            Undo.RecordObject(_currentClip, "Value modified");

            var binding = new EditorCurveBinding
            {
                path = path,
                propertyName = property,
                type = typeof(SkinnedMeshRenderer)
            };
            AnimationUtility.SetEditorCurve(_currentClip, binding, AnimationCurve.Constant(0f, 1/60f, 100f));

            var current = SubInfoFromActive(_currentClip, binding);
            _editableProperties.Add(current);
            GenerateEditables(DummyNullable(), new [] { current.Preview }.ToList());
            RenderMain(_currentClip, DummyNullable());
        }

        private CgePropertyExplorerSubInfo SubInfoFromBinding(string smrPath, EditorCurveBinding binding)
        {
            var preview = new AnimationPreview(BlendShapeClip(binding), NewTexture2D());
            return new CgePropertyExplorerSubInfo
            {
                Preview = preview,
                Property = binding.propertyName,
                Path = smrPath,
                BoundaryTexture = NewTexture2D(),
                HotspotTexture = NewTexture2D()
            };
        }

        public bool IsMaintaining()
        {
            return _maintain;
        }

        public void MaintainPreviewToggled()
        {
            _maintain = !_maintain;

            if (!_maintain)
            {
                AnimationMode.StopAnimationMode();
            }
            else
            {
                Maintain();
            }
        }

        private void Maintain()
        {
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(DummyNullable().previewDummy.gameObject, _currentClip, 1/60f);
            AnimationMode.EndSampling();
        }

        public void AssignBased(string based, List<string> subjects)
        {
            EnsureMetadataAssetInitialized();

            EditorUtility.SetDirty(_metadataAsset);
            foreach (var basedBlendshape in subjects
                .Distinct()
                .Select(subject => new CgeAnimationEditorMetadataBasedBlendshape { based = based, subject = subject }))
            {
                _metadataAsset.PutBasedBlendshape(basedBlendshape);
            }
        }

        private CgeAnimationEditorMetadata EnsureMetadataAssetInitialized()
        {
            return _metadataAsset = GetOrCreateMetadataAsset();
        }

        public string GetBased(string blendshapePrefix)
        {
            EnsureMetadataAssetInitialized();

            return _metadataAsset.GetBased(blendshapePrefix);
        }

        private static CgeAnimationEditorMetadata GetOrCreateMetadataAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<CgeAnimationEditorMetadata>(CgeAnimationEditorMetadataAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CgeAnimationEditorMetadata>();
                AssetDatabase.CreateAsset(asset, CgeAnimationEditorMetadataAssetPath);
            }

            return asset;
        }
    }
}
