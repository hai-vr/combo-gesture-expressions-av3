using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.EditorUI;
using Hai.ComboGesture.Scripts.Editor.EditorUI.AnimationEditor;
using Hai.ComboGesture.Scripts.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Modules
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
        private readonly Dictionary<string, Texture2D> _basedOnSomething = new Dictionary<string, Texture2D>();
        private AnimationClip _currentClip;
        private List<CgeAnimationEditorSubInfo> _editableProperties;
        private List<CgePropertyExplorerSubInfo> _smrBlendShapeProperties;
        private Action _action;
        private bool _isCalling;
        private bool _maintain;
        private CgeAnimationEditorMetadata _metadataAsset;
        private Action _generatePreviewsFromPropertyExplorer__DropPrevious;

        public CgeAnimationEditor(CgeRenderingCommands renderingCommands)
        {
            _renderingCommands = renderingCommands;
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
            EnsureMetadataAssetInitialized();
            foreach (var based in _metadataAsset.AllBased())
            {
                _basedOnSomething[based] = NewTexture2D();
            }
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

            CgeAnimationEditorWindow.Obtain().OnNewClipSelected(_currentClip);
            CgePropertyExplorerWindow.Obtain().OnNewClipSelected(_currentClip);

            GeneratePreviewsFromCurrentClip(DummyNullable());
        }

        private void GeneratePreviewsFromCurrentClip(ComboGesturePreviewSetup previewSetup)
        {
            RenderMain(previewSetup);
            RenderBased(previewSetup);
            CalculateAndRenderEditables();
        }

        public void NotifyNewScan()
        {
            GeneratePreviewsFromPropertyExplorer(DummyNullable());
        }

        private void GeneratePreviewsFromPropertyExplorer(ComboGesturePreviewSetup previewSetup)
        {
            if (_generatePreviewsFromPropertyExplorer__DropPrevious != null)
            {
                _generatePreviewsFromPropertyExplorer__DropPrevious.Invoke();
            }
            RenderBased(previewSetup);
            _generatePreviewsFromPropertyExplorer__DropPrevious = CalculateAndRenderBlendshapes(previewSetup);
        }

        private void GeneratePreviewsFromSubjectNamesAssignments(ComboGesturePreviewSetup previewSetup, List<string> subjects)
        {
            RenderBased(previewSetup);
            RenderBlendshapes(previewSetup, info => subjects.Contains(info.Property));
        }

        private void RenderMain(ComboGesturePreviewSetup previewSetup)
        {
            EnsureActivePreviewInitialized();
            _renderingCommands.GenerateSpecificFastMode(
                new List<RenderingSample> {new RenderingSample(AnimationUtility.GetCurveBindings(_currentClip).Length == 0 ? NothingClip() : _currentClip, _activePreview, preview =>
                {
                    CgeAnimationEditorWindow.Obtain().Repaint();
                })},
                previewSetup,
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide,
                CgeRenderingCommands.CgePriority.High
            );
        }

        private void RenderBased(ComboGesturePreviewSetup previewSetup)
        {
            EnsureBasedInitialized();

            var smr = previewSetup.avatarDescriptor.VisemeSkinnedMesh;
            _renderingCommands.GenerateSpecificFastMode(
                new List<RenderingSample>
                {
                    new RenderingSample(NothingClip(), _based, preview => { })
                }.Concat(_basedOnSomething.Select(basedToTexture => new RenderingSample(BlendShapeClip(new EditorCurveBinding()
                {
                    path = SharedLayerUtils.ResolveRelativePath(previewSetup.avatarDescriptor.transform, smr.transform),
                    propertyName = basedToTexture.Key,
                    type = typeof(SkinnedMeshRenderer)
                }), basedToTexture.Value,
                    preview => { }))).ToList(),
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

        private void CalculateAndRenderEditables()
        {
            _editableProperties = AnimationUtility.GetCurveBindings(_currentClip)
                .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                .Where(binding => AnimationUtility.GetEditorCurve(_currentClip, binding).keys.All(keyframe => keyframe.value != 0))
                .Select(binding => SubInfoFromActive(_currentClip, binding)).ToList();

            if (_editableProperties.Count == 0) return;
        }

        private AnimationClip CreateAnimationClipForBinding(AnimationClip active, EditorCurveBinding binding)
        {
            var clip = new AnimationClip();
            AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(active, binding));
            var basedShape = _metadataAsset.GetBased(binding.propertyName);
            if (basedShape != null)
            {
                AnimationUtility.SetEditorCurve(clip, new EditorCurveBinding
                {
                    path = binding.path,
                    propertyName = basedShape,
                    type = typeof(SkinnedMeshRenderer)
                }, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            }

            return clip;
        }

        private void GenerateEditables(List<CgeAnimationEditorSubInfo> currents)
        {
            _renderingCommands.GenerateSpecificFastMode(
                currents.Select(current =>
                    new RenderingSample(
                        CreateAnimationClipForBinding(_currentClip, current.Binding),
                        NewTexture2D(),
                        preview =>
                        {
                            EnsureBasedInitialized();
                            CgeRenderingSupport.MutateMultilevelHighlightDifferences(current.BoundaryTexture, preview.RenderTexture, BasedTexture(current.Property));
                            CgeAnimationEditorWindow.Obtain().Repaint();
                        }
                    )).ToList(),
                DummyNullable(),
                CgeRenderingCommands.CgeDummyAutoHide.DoNotHide
            );
        }

        private Texture2D BasedTexture(string property)
        {
            var basedShape = _metadataAsset.GetBased(property);
            return basedShape != null ? _basedOnSomething[basedShape] : _based;
        }

        private CgeAnimationEditorSubInfo SubInfoFromActive(AnimationClip active, EditorCurveBinding binding)
        {
            return new CgeAnimationEditorSubInfo
            {
                Property = binding.propertyName,
                Value = AnimationUtility.GetEditorCurve(active, binding).keys.Select(keyframe => keyframe.value).Min(),
                Binding = binding,
                BoundaryTexture = new Texture2D(HalfWidth, HalfHeight, TextureFormat.ARGB32, false),
                Path = binding.path
            };
        }

        private Action CalculateAndRenderBlendshapes(ComboGesturePreviewSetup previewSetup)
        {
            var smr = previewSetup.avatarDescriptor.VisemeSkinnedMesh;
            var mesh = smr.sharedMesh;
            _smrBlendShapeProperties = AllBlendshapes(mesh, SharedLayerUtils.ResolveRelativePath(previewSetup.avatarDescriptor.transform, smr.transform));
            return RenderBlendshapes(previewSetup, info => true);
        }

        private Action RenderBlendshapes(ComboGesturePreviewSetup previewSetup, Func<CgePropertyExplorerSubInfo, bool> predicate)
        {
            return _renderingCommands.GenerateSpecificFastMode(
                _smrBlendShapeProperties.Where(predicate).Select(info => new RenderingSample(
                    CreateBlendShapeClipForBinding(info.Binding),
                    NewTexture2D(),
                    preview =>
                    {
                        CgeRenderingSupport.MutateHighlightHotspots(info.HotspotTexture, preview.RenderTexture, BasedTexture(info.Property));
                        CgeRenderingSupport.MutateMultilevelHighlightDifferences(info.BoundaryTexture, preview.RenderTexture, BasedTexture(info.Property));
                        CgePropertyExplorerWindow.Obtain().Repaint();
                    }
                )).ToList(),
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

        public void UpdateEditable(CgeAnimationEditorSubInfo info, int index, float newValue)
        {
            Undo.RecordObject(_currentClip, "Value modified");
            var oldCurve = AnimationUtility.GetEditorCurve(_currentClip, info.Binding);
            var newCurve = new AnimationCurve(oldCurve.keys.Select(keyframe =>
            {
                keyframe.value = newValue;
                return keyframe;
            }).ToArray());
            info.Value = newValue;
            _editableProperties[index] = info;
            AnimationUtility.SetEditorCurve(_currentClip, info.Binding, newCurve);

            _action = () =>
            {
                RenderMain(DummyNullable());
                _renderingCommands.GenerateSpecificFastMode(
                    new List<RenderingSample> { new RenderingSample(
                        CreateAnimationClipForBinding(_currentClip, info.Binding),
                        NewTexture2D(),
                        preview =>
                        {
                            EnsureBasedInitialized();
                            CgeRenderingSupport.MutateMultilevelHighlightDifferences(info.BoundaryTexture, preview.RenderTexture, BasedTexture(info.Property));
                            CgeAnimationEditorWindow.Obtain().Repaint();
                        }
                    ) },
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
            RenderMain(DummyNullable());
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
            GenerateEditables(new [] { current }.ToList());
            RenderMain(DummyNullable());
        }

        public void RemoveBlendShape(string path, string property)
        {
            Undo.RecordObject(_currentClip, "Value modified");

            var binding = new EditorCurveBinding
            {
                path = path,
                propertyName = property,
                type = typeof(SkinnedMeshRenderer)
            };
            AnimationUtility.SetEditorCurve(_currentClip, binding, null);

            _editableProperties = _editableProperties.Where(info => info.Property != property && info.Path == path).ToList();
            RenderMain(DummyNullable());
        }

        private CgePropertyExplorerSubInfo SubInfoFromBinding(string smrPath, EditorCurveBinding binding)
        {
            return new CgePropertyExplorerSubInfo
            {
                Property = binding.propertyName,
                Binding = binding,
                Path = smrPath,
                BoundaryTexture = NewTexture2D(),
                HotspotTexture = NewTexture2D()
            };
        }

        private AnimationClip CreateBlendShapeClipForBinding(EditorCurveBinding binding)
        {
            var blendShapeClip = BlendShapeClip(binding);
            var basedShape = _metadataAsset.GetBased(binding.propertyName);
            if (basedShape != null)
            {
                AnimationUtility.SetEditorCurve(blendShapeClip, new EditorCurveBinding
                {
                    path = binding.path,
                    propertyName = basedShape,
                    type = typeof(SkinnedMeshRenderer)
                }, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            }

            return blendShapeClip;
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
            GeneratePreviewsFromSubjectNamesAssignments(DummyNullable(), subjects);
        }

        private void EnsureMetadataAssetInitialized()
        {
            _metadataAsset = GetOrCreateMetadataAsset();
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

        public void RemoveBasedSubject(string subject)
        {
            EnsureMetadataAssetInitialized();
            EditorUtility.SetDirty(_metadataAsset);

            _metadataAsset.RemoveBasedBlendshape(subject);
            GeneratePreviewsFromSubjectNamesAssignments(DummyNullable(), new [] { subject }.ToList());
        }

        public bool ActiveHas(string path, string property)
        {
            if (_currentClip == null) return true;

            // FIXME: handle animations with value 0 (or equal to skinned mesh)
            return AnimationUtility.GetCurveBindings(_currentClip).Any(binding => binding.propertyName == property && binding.path == path && binding.type == typeof(SkinnedMeshRenderer));
        }

        public void Delete0Values()
        {
            EditorUtility.SetDirty(_currentClip);
            var bindings = AnimationUtility.GetCurveBindings(_currentClip);
            foreach (var binding in bindings
                .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                .Where(binding => AnimationUtility.GetEditorCurve(_currentClip, binding).keys.All(keyframe => keyframe.value == 0f)))
            {
                AnimationUtility.SetEditorCurve(_currentClip, binding, null);
            }
        }

        public bool HasActiveClip()
        {
            return _currentClip != null;
        }

        public bool IsBased(string potentiallyBased)
        {
            return _metadataAsset.IsBased(potentiallyBased);
        }

        public string GetBased(string subject)
        {
            EnsureMetadataAssetInitialized();

            return _metadataAsset.GetBased(subject);
        }

        public AnimationClip ActiveClip()
        {
            return _currentClip;
        }
    }
}
