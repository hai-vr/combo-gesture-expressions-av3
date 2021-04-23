using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Components;
using Hai.ExpressionsEditor.Scripts.Editor.EditorUI.EditorWindows;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EePreviewHandler
    {
        private readonly EeRenderingCommands _renderingCommands;

        private readonly EeAnimationEditorState _state;
        private readonly EeMetadata _metadata;

        private Texture2D _bufferTexture;
        private Texture2D _bufferActive;
        private Texture2D _based;
        private readonly Dictionary<string, Texture2D> _basedOnSomething = new Dictionary<string, Texture2D>();
        private Action _generatePreviewsFromPropertyExplorerDropPrevious;
        private Action _throttleFinishedAction;
        private bool _isCalling;

        public EePreviewHandler(EeRenderingCommands renderingCommands, EeAnimationEditorState state, EeMetadata metadata)
        {
            _renderingCommands = renderingCommands;
            _state = state;
            _metadata = metadata;
            _renderingCommands.SetQueueEmptiedAction(() =>
            {
                if (!_state.Maintain) return;

                var dummy = _state.InternalDummyOptional();
                if (!dummy.HasValue) return;
                Maintain(dummy.Value);
            });
        }

        private void Maintain(EePreviewAvatar dummy)
        {
            dummy.Dummy.gameObject.SetActive(true);
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(dummy.Dummy.gameObject, _state.CurrentClip, 1/60f);
            AnimationMode.EndSampling();
        }

        internal void RenderAddBlendshape(EePreviewAvatar? dummy, EeEditableBlendshape editableBlendshape, AnimationClip currentClip, int cameraIndex)
        {
            if (!dummy.HasValue) return;

            _renderingCommands.GenerateSpecificFastMode(
                new List<EeRenderingSample> { new EeRenderingSample(
                        CreateAnimationClipForBinding(currentClip, editableBlendshape.Binding),
                        BufferTexture2D(),
                        preview =>
                        {
                            EnsureBasedInitialized();
                            EeRenderingSupport.MutateMultilevelHighlightDifferences(editableBlendshape.BoundaryTexture, preview.RenderTexture, BasedTexture(editableBlendshape.Property));
                            EeAnimationEditorWindow.Obtain().Repaint();
                        }
                    ) },
                dummy.Value,
                cameraIndex
            );
            RenderMain(dummy.Value);
        }

        internal void RenderMain(EePreviewAvatar? dummy)
        {
            if (!dummy.HasValue) return;

            RenderMain(dummy.Value);
        }

        internal void EnsureBasedInitialized()
        {
            if (_based != null) return;

            _based = NewActualTexture2D();
            _metadata.EnsureMetadataAssetInitialized();
            foreach (var onWhat in _metadata.MetadataAsset.AllOnWhat())
            {
                _basedOnSomething[onWhat] = NewActualTexture2D();
            }
        }

        internal void EnsureActivePreviewInitialized()
        {
            if (_state.ActivePreview != null) return;

            _bufferActive = new Texture2D(EeMetadata.StandardWidth, EeMetadata.StandardHeight, TextureFormat.RGB24, false);

            _state.Altered(this).TEMP_ActivePreviewEnsured();
        }

        internal static Texture2D NewActualTexture2D()
        {
            return new Texture2D(EeMetadata.HalfWidth * EeMetadata.PreviewResolutionMultiplier, EeMetadata.HalfHeight * EeMetadata.PreviewResolutionMultiplier, TextureFormat.ARGB32, false);
        }

        private Texture2D BufferTexture2D()
        {
            if (_bufferTexture == null) _bufferTexture = new Texture2D(EeMetadata.HalfWidth * EeMetadata.PreviewResolutionMultiplier, EeMetadata.HalfHeight * EeMetadata.PreviewResolutionMultiplier, TextureFormat.RGB24, false);
            return _bufferTexture;
        }

        private AnimationClip CreateAnimationClipForBinding(AnimationClip active, EditorCurveBinding binding)
        {
            var clip = new AnimationClip();
            AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(active, binding));

            _metadata.EnsureMetadataAssetInitialized();
            var onWhat = _metadata.MetadataAsset.GetBasedOnWhat(binding.propertyName);
            if (onWhat != null)
            {
                AnimationUtility.SetEditorCurve(clip, new EditorCurveBinding
                {
                    path = binding.path,
                    propertyName = onWhat,
                    type = typeof(SkinnedMeshRenderer)
                }, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            }

            return clip;
        }

        internal void Throttle(EePreviewAvatar? dummy, EeEditableBlendshape info, AnimationClip currentClip, int cameraIndex, bool stateMaintain, EeAnimationEditorScenePreviewMode stateScenePreviewMode)
        {
            if (!dummy.HasValue) return;

            _throttleFinishedAction = () =>
            {
                RenderMain(dummy.Value);
                _renderingCommands.GenerateSpecificFastMode(
                    new List<EeRenderingSample>
                    {
                        new EeRenderingSample(
                            CreateAnimationClipForBinding(currentClip, info.Binding),
                            BufferTexture2D(),
                            preview =>
                            {
                                EnsureBasedInitialized();
                                EeRenderingSupport.MutateMultilevelHighlightDifferences(info.BoundaryTexture, preview.RenderTexture, BasedTexture(info.Property));
                                EeAnimationEditorWindow.Obtain().Repaint();
                            }
                        )
                    },
                    dummy.Value,
                    cameraIndex
                );
            };

            if (stateMaintain && stateScenePreviewMode != EeAnimationEditorScenePreviewMode.Always)
            {
                Maintain(dummy.Value);
            }
            else if (!_isCalling)
            {
                EditorApplication.delayCall += () =>
                {
                    _isCalling = false;
                    _throttleFinishedAction();
                };
                _isCalling = true;
            }
        }

        private Action RenderBlendshapes(EePreviewAvatar dummy, Func<EeExplorerBlendshape, bool> predicate)
        {
            return _renderingCommands.GenerateSpecificFastMode(
                _state.ExplorerBlendshapes.Where(predicate).Select(info => new EeRenderingSample(
                    CreateBlendShapeClipForBinding(info.Binding),
                    BufferTexture2D(),
                    preview =>
                    {
                        EnsureBasedInitialized();
                        EeRenderingSupport.MutateHighlightHotspots(info.HotspotTexture, preview.RenderTexture, BasedTexture(info.Property));
                        EeRenderingSupport.MutateMultilevelHighlightDifferences(info.BoundaryTexture, preview.RenderTexture, BasedTexture(info.Property));
                        EePropertyExplorerWindow.Obtain().Repaint();
                    }
                )).ToList(),
                dummy,
                _state.CameraIndex,
                EeRenderingCommands.EeDummyAutoHide.Default,
                EeRenderingCommands.EePriority.Low
            );
        }

        internal void SubjectsAreBased(List<string> subjects)
        {
            foreach (var based in _metadata.MetadataAsset.AllOnWhat())
            {
                if (!_basedOnSomething.ContainsKey(based)) _basedOnSomething[based] = NewActualTexture2D();
            }

            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;
            GeneratePreviewsFromSubjectNamesAssignments(dummy.Value, subjects);
        }

        internal void SubjectNoLongerBased(string subject)
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;
            GeneratePreviewsFromSubjectNamesAssignments(dummy.Value, new[] {subject}.ToList());
        }

        internal void RenderFromMaintainToggled()
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;

            if (!_state.Maintain)
            {
                AnimationMode.StopAnimationMode();
                if (dummy.Value.AutoHide)
                {
                    dummy.Value.Dummy.gameObject.SetActive(false);
                }

                GeneratePreviewsFromCurrentClip(dummy.Value);
            }
            else
            {
                Maintain(dummy.Value);
            }
        }

        internal void RenderFromPropertyExplorer()
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;

            GeneratePreviewsFromPropertyExplorer(dummy.Value);
        }

        internal void RenderForced()
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;

            GeneratePreviewsFromCurrentClipForced(dummy.Value);
        }


        internal void GeneratePreviewsFromCurrentClip()
        {
            var dummy = _state.InternalDummyOptional();
            if (!dummy.HasValue) return;

            GeneratePreviewsFromCurrentClip(dummy.Value);
            if (_state.Maintain && _state.ScenePreviewMode == EeAnimationEditorScenePreviewMode.Never)
            {
                Maintain(dummy.Value);
            }
        }

        private void GeneratePreviewsFromCurrentClip(EePreviewAvatar dummy)
        {
            if (_state.CurrentClip == null) return;

            if (!_state.Maintain || _state.ScenePreviewMode != EeAnimationEditorScenePreviewMode.Never)
            {
                RenderMain(dummy);
                RenderBased(dummy);
            }
            RenderEditables(dummy);
        }

        private void GeneratePreviewsFromCurrentClipForced(EePreviewAvatar dummy)
        {
            if (_state.CurrentClip == null) return;

            RenderMain(dummy);
            RenderBased(dummy);
            RenderEditables(dummy);
        }


        private void GeneratePreviewsFromPropertyExplorer(EePreviewAvatar dummy)
        {
            _generatePreviewsFromPropertyExplorerDropPrevious?.Invoke();
            RenderBased(dummy);
            _generatePreviewsFromPropertyExplorerDropPrevious = RenderBlendshapes(dummy, info => true);
        }

        private void GeneratePreviewsFromSubjectNamesAssignments(EePreviewAvatar dummy, List<string> subjects)
        {
            RenderBased(dummy);
            RenderBlendshapes(dummy, info => subjects.Contains(info.Property));
        }

        private void RenderMain(EePreviewAvatar dummy)
        {
            EnsureActivePreviewInitialized();
            var clipBeingGenerated = _state.CurrentClip;
            _renderingCommands.GenerateSpecificFastMode(
                new List<EeRenderingSample> {new EeRenderingSample(AnimationUtility.GetCurveBindings(clipBeingGenerated).Length == 0 ? NothingClip() : clipBeingGenerated, _bufferActive, preview =>
                {
                    _state.ActivePreview.SetPixels(preview.RenderTexture.GetPixels());
                    _state.ActivePreview.Apply();
                    EeAnimationEditorWindow.Obtain().Repaint();

                    Ee.Get().Hooks.PushOnMainRendered(new EeHookOnMainRendered
                    {
                        Clip = clipBeingGenerated,
                        OutputTexture = preview.RenderTexture
                    });
                })},
                dummy,
                _state.CameraIndex,
                EeRenderingCommands.EeDummyAutoHide.Default,
                EeRenderingCommands.EePriority.High
            );
        }

        private void RenderBased(EePreviewAvatar dummy)
        {
            EnsureBasedInitialized();

            var smr = dummy.TempCxSmr;
            _renderingCommands.GenerateSpecificFastMode(
                new List<EeRenderingSample>
                {
                    new EeRenderingSample(NothingClip(), BufferTexture2D(), preview =>
                    {
                        _based.SetPixels(preview.RenderTexture.GetPixels());
                        _based.Apply();
                    })
                }.Concat(_basedOnSomething.Select(basedToTexture => new EeRenderingSample(BlendShapeClip(new EditorCurveBinding()
                {
                    path = ResolveRelativePath(dummy.Dummy.transform, smr.transform),
                    propertyName = basedToTexture.Key,
                    type = typeof(SkinnedMeshRenderer)
                }), BufferTexture2D(),
                    preview =>
                    {
                        basedToTexture.Value.SetPixels(preview.RenderTexture.GetPixels());
                        basedToTexture.Value.Apply();
                    }))).ToList(),
                dummy,
                _state.CameraIndex
            );
        }

        private void RenderEditables(EePreviewAvatar dummy)
        {
            if (_state.EditableBlendshapes.Count == 0) return;
            if (_state.Maintain && _state.ScenePreviewMode == EeAnimationEditorScenePreviewMode.Never) return;

            _renderingCommands.GenerateSpecificFastMode(
                _state.EditableBlendshapes.Select(current =>
                    new EeRenderingSample(
                        CreateAnimationClipForBinding(_state.CurrentClip, current.Binding),
                        BufferTexture2D(),
                        preview =>
                        {
                            EnsureBasedInitialized();
                            EeRenderingSupport.MutateMultilevelHighlightDifferences(current.BoundaryTexture, preview.RenderTexture, BasedTexture(current.Property));
                            EeAnimationEditorWindow.Obtain().Repaint();
                        }
                    )).ToList(),
                dummy,
                _state.CameraIndex
            );
        }

        private Texture2D BasedTexture(string property)
        {
            var onWhat = _metadata.MetadataAsset.GetBasedOnWhat(property);
            return onWhat != null ? _basedOnSomething[onWhat] : _based;
        }

        private static string ResolveRelativePath(Transform avatar, Transform item)
        {
            if (item.parent != avatar && item.parent != null)
            {
                return ResolveRelativePath(avatar, item.parent) + "/" + item.name;
            }

            return item.name;
        }

        private static AnimationClip NothingClip()
        {
            var nsk = new AnimationClip();
            AnimationUtility.SetEditorCurve(nsk, new EditorCurveBinding
            {
                path = "_ignored",
                propertyName = "m_IsActive",
                type = typeof(GameObject)
            }, AnimationCurve.Constant(0f, 0f, 0f));
            return nsk;
        }

        private AnimationClip CreateBlendShapeClipForBinding(EditorCurveBinding binding)
        {
            var blendShapeClip = BlendShapeClip(binding);
            var onWhat = _metadata.MetadataAsset.GetBasedOnWhat(binding.propertyName);
            if (onWhat != null)
            {
                AnimationUtility.SetEditorCurve(blendShapeClip, new EditorCurveBinding
                {
                    path = binding.path,
                    propertyName = onWhat,
                    type = typeof(SkinnedMeshRenderer)
                }, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            }

            return blendShapeClip;
        }

        private static AnimationClip BlendShapeClip(EditorCurveBinding binding)
        {
            var clip = new AnimationClip();
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1 / 60f, 100f));
            return clip;
        }
    }
}
