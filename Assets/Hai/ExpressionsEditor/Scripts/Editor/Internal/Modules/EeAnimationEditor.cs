using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Components;
using Hai.ExpressionsEditor.Scripts.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EeAnimationEditorState
    {
        public AnimationClip CurrentClip { get; private set; }
        public List<EeEditableBlendshape> EditableBlendshapes { get; private set; }
        public EeNonEditableStats Stats { get; private set; }
        public int CameraIndex { get; private set; }
        public List<EeExplorerBlendshape> ExplorerBlendshapes { get; private set; }
        public EeAnimationEditorScenePreviewMode ScenePreviewMode { get; private set; }
        public bool Maintain { get; private set; }
        public Texture2D ActivePreview { get; private set; }

        private ExpressionEditorPreviewable SelectedDummy { get; set; }

        internal EePreviewAvatar? InternalDummyOptional()
        {
            if (SelectedDummy == null)
            {
                var candidate = EePreviewSetupWizard.MaybeFindLastActiveAndValidPreviewComponentInRoot();
                if (candidate != null) SelectedDummy = candidate;
            }

            return SelectedDummy?.AsEePreviewAvatar();
        }

        public EeAnimationEditor Altered(EePreviewHandler previewHandler)
        {
            return new EeAnimationEditor(this, previewHandler);
        }

        public class EeAnimationEditor
        {
            private readonly EePreviewHandler _previewHandler;
            private readonly EeAnimationEditorState _state;

            public EeAnimationEditor(EeAnimationEditorState state, EePreviewHandler previewHandler)
            {
                _state = state;
                _previewHandler = previewHandler;
            }

            public void NewClipSelected(AnimationClip activeNonNull)
            {
                _state.CurrentClip = activeNonNull;

                // FIXME _state._editableProperties and animationEditorState._stats will have a content that depends on the currently active dummy!!!
                // FIXME: not only it depends on the currently active dummy, that dummy must not be animated too.
                // FIXME: For now, only consider blendshapes with a value of 0.
                _state.EditableBlendshapes = AnimationUtility.GetCurveBindings(_state.CurrentClip)
                    .Where(binding => binding.type == typeof(SkinnedMeshRenderer) && binding.propertyName.StartsWith("blendShape."))
                    .Where(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.All(keyframe => keyframe.value != 0))
                    .Select(binding => EditableBlendshapeFromActive(_state.CurrentClip, binding)).ToList();

                RecalculateStats();
                var dummy = _state.InternalDummyOptional();
                if (dummy.HasValue)
                {
                    var fakeController = new UnityEditor.Animations.AnimatorController();
                    fakeController.name = "Fake Temporary Controller__DoNotModify";
                    fakeController.AddLayer("Base");
                    var animatorState = fakeController.layers[0].stateMachine.AddState("recorder");
                    animatorState.motion = activeNonNull;
                    animatorState.writeDefaultValues = false;
                    dummy.Value.Dummy.runtimeAnimatorController = fakeController;
                }

                _previewHandler.GeneratePreviewsFromCurrentClip();
            }

            private EeEditableBlendshape EditableBlendshapeFromActive(AnimationClip active, EditorCurveBinding binding)
            {
                var curve = AnimationUtility.GetEditorCurve(active, binding);

                return new EeEditableBlendshape
                {
                    Property = binding.propertyName,
                    Value = curve.keys.Select(keyframe => keyframe.value).Max(),
                    IsVaryingOverTime = curve.keys.Select(keyframe => keyframe.value).Distinct().Count() > 1,
                    VaryingMinValue = curve.keys.Select(keyframe => keyframe.value).Min(),
                    Binding = binding,
                    BoundaryTexture = new Texture2D(EeMetadata.HalfWidth, EeMetadata.HalfHeight, TextureFormat.ARGB32, false),
                    Path = binding.path
                };
            }

            public void NewBlendshapeAdded(EditorCurveBinding binding)
            {
                var current = EditableBlendshapeFromActive(_state.CurrentClip, binding);
                _state.EditableBlendshapes.Add(current);
                RecalculateStats();

                _previewHandler.RenderAddBlendshape(_state.InternalDummyOptional(), current, _state.CurrentClip, _state.CameraIndex);
            }

            public void SingleBlendshapeDeleted(string path, string property)
            {
                _state.EditableBlendshapes.RemoveAt(IndexOfPathProperty(path, property));

                _previewHandler.RenderMain(_state.InternalDummyOptional());
            }

            public void BlendshapeModified(string path, string property, float newValue)
            {
                var editableBlendshape = _state.EditableBlendshapes[IndexOfPathProperty(path, property)];
                editableBlendshape.Value = newValue;
                _state.EditableBlendshapes[IndexOfPathProperty(path, property)] = editableBlendshape;

                RecalculateStats();
                _previewHandler.Throttle(_state.InternalDummyOptional(), editableBlendshape, _state.CurrentClip, _state.CameraIndex, _state.Maintain, _state.ScenePreviewMode);
            }

            public void DummySelected(ExpressionEditorPreviewable dummy)
            {
                _state.SelectedDummy = dummy;

                // FIXME: This will cause all blendshape previews to be forgotten
                // var previewAvatar = dummy.AsEePreviewAvatar();
                // _state.ExplorerBlendshapes = AllBlendshapes(previewAvatar.TempCxSmr.sharedMesh, ResolveRelativePath(previewAvatar.Dummy.transform, previewAvatar.TempCxSmr.transform));
                RecalculateStats();

                _previewHandler.RenderForced();
            }

            public void ExplorerBlendshapesRequested()
            {
                var dummy = _state.InternalDummyOptional();
                if (!dummy.HasValue) return;

                _state.ExplorerBlendshapes = AllBlendshapes(dummy.Value.TempCxSmr.sharedMesh, ResolveRelativePath(dummy.Value.Dummy.transform, dummy.Value.TempCxSmr.transform));
                _previewHandler.RenderFromPropertyExplorer();
            }

            public void DummyForgotten()
            {
                _state.SelectedDummy = null;
            }

            public void PreviewGenerationForced(EeAnimationEditorScenePreviewMode scenePreviewMode)
            {
                _state.ScenePreviewMode = scenePreviewMode;
            }

            public void MaintainPreviewToggled()
            {
                _state.Maintain = !_state.Maintain;
                _previewHandler.RenderFromMaintainToggled();
            }

            public void CameraSelected(int selectedPreviewCamera)
            {
                _state.CameraIndex = selectedPreviewCamera;
                _previewHandler.RenderForced();

            }

            public void TEMP_ActivePreviewEnsured()
            {
                _state.ActivePreview = new Texture2D(EeMetadata.StandardWidth, EeMetadata.StandardHeight, TextureFormat.ARGB32, false);
            }

            private int IndexOfPathProperty(string path, string property)
            {
                return _state.EditableBlendshapes.FindIndex(info => info.Property == property && info.Path == path);
            }

            private void RecalculateStats()
            {
                _state.Stats = CollectStatistics(AnimationUtility.GetCurveBindings(_state.CurrentClip));
            }

            private EeNonEditableStats CollectStatistics(EditorCurveBinding[] bindings)
            {
                // FIXME _state._editableProperties and animationEditorState._stats will have a content that depends on the currently active dummy!!!
                // FIXME: not only it depends on the currently active dummy, that dummy must not be animated too.
                // FIXME: For now, only consider blendshapes with a value of 0.
                var resetBlendshapes = bindings
                    .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                    .Where(binding => binding.propertyName.StartsWith("blendShape."))
                    .Where(binding =>
                    {
                        // var smrValue = MaybeFindSmrValue(binding);
                        // return smrValue != null && AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.All(keyframe => keyframe.value == smrValue);
                        return AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.All(keyframe => keyframe.value == 0);
                    })
                    .ToList();

                var other = bindings
                    .Where(binding => binding.type != typeof(SkinnedMeshRenderer) || !binding.propertyName.StartsWith("blendShape."))
                    .GroupBy(binding =>
                    {
                        if (binding.type == typeof(Transform)) return EeNonEditableLookup.Transform;
                        if (binding.type == typeof(Animator)) return EeNonEditableLookup.Animator;
                        if (binding.type == typeof(SkinnedMeshRenderer) && binding.propertyName.StartsWith("material.")) return EeNonEditableLookup.Shader;
                        if (binding.type == typeof(GameObject) && binding.propertyName == "m_IsActive") return EeNonEditableLookup.GameObjectToggle;
                        return EeNonEditableLookup.Other;
                    })
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
                var referenceBindingCount = AnimationUtility.GetObjectReferenceCurveBindings(_state.CurrentClip).Length;
                if (referenceBindingCount > 0) other[EeNonEditableLookup.MaterialSwap] = referenceBindingCount;

                var effectiveFrameDuration = CalculateEffectiveFrameDuration(bindings);
                var quirk = CalculateQuirk(bindings);

                return new EeNonEditableStats(resetBlendshapes, other, effectiveFrameDuration, quirk);
            }

            private int CalculateEffectiveFrameDuration(EditorCurveBinding[] bindings)
            {
                if (bindings.Length == 0) return Mathf.RoundToInt(_state.CurrentClip.frameRate);
                var allCurvesHaveOneKeyframe = bindings
                    .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding))
                    .All(curve => curve.keys.Length == 1);

                if (allCurvesHaveOneKeyframe)
                {
                    var lastTime = bindings
                        .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys[0].time)
                        .Max();
                    return Mathf.RoundToInt(lastTime == 0 ? _state.CurrentClip.frameRate : lastTime * _state.CurrentClip.frameRate);
                }

                return Mathf.RoundToInt(bindings
                    .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.Last().time)
                    .Max() * _state.CurrentClip.frameRate);
            }

            private EeQuirk CalculateQuirk(EditorCurveBinding[] bindings)
            {
                if (bindings.Length == 0) return EeQuirk.EmptyIssue;
                var allCurvesHaveOneKeyframe = bindings
                    .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding))
                    .All(curve => curve.keys.Length == 1);

                if (allCurvesHaveOneKeyframe)
                {
                    var lastTime = bindings
                        .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys[0].time)
                        .Max();
                    if (lastTime == 0) return EeQuirk.FirstFrameIssue;
                    if (lastTime == 1f / _state.CurrentClip.frameRate) return EeQuirk.OneFrame;
                    return EeQuirk.MoreThanOneFrame;
                }
                else
                {
                    var lastTime = Mathf.RoundToInt(bindings
                        .Select(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.Last().time)
                        .Max() * _state.CurrentClip.frameRate);
                    if (lastTime == 1f / _state.CurrentClip.frameRate) return EeQuirk.OneFrame;
                    return EeQuirk.MoreThanOneFrame;
                }
            }

            private float? MaybeFindSmrValue(EditorCurveBinding binding)
            {
                var dummy = _state.InternalDummyOptional();
                if (!dummy.HasValue) return null;

                var animatedElement = MaybeTraversePathInDummy(binding.path, dummy.Value);
                if (animatedElement == null) return null;

                var smr = animatedElement.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) return null;

                var blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(binding.propertyName.Substring("blendShape.".Length));
                if (blendShapeIndex == -1) return null;

                var weightOfBlendShape = smr.GetBlendShapeWeight(blendShapeIndex);
                return weightOfBlendShape;
            }

            private Transform MaybeTraversePathInDummy(string path, EePreviewAvatar dummy)
            {
                var pathing = path.Split('/');
                var visiting = dummy.Dummy.transform;
                foreach (var subPath in pathing)
                {
                    visiting = visiting.Find(subPath);
                    if (visiting == null) return null;
                }

                return visiting;
            }

            private static string ResolveRelativePath(Transform avatar, Transform item)
            {
                if (item.parent != avatar && item.parent != null) return ResolveRelativePath(avatar, item.parent) + "/" + item.name;

                return item.name;
            }

            private List<EeExplorerBlendshape> AllBlendshapes(Mesh mesh, string smrPath)
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

                var lookup = allBlendshapes.ToLookup(info => info.Property.ToLowerInvariant().StartsWith("blendshape.vrc."));
                return lookup[false].Concat(lookup[true]).ToList();
            }

            private EeExplorerBlendshape SubInfoFromBinding(string smrPath, EditorCurveBinding binding)
            {
                return new EeExplorerBlendshape
                {
                    Property = binding.propertyName,
                    Binding = binding,
                    Path = smrPath,
                    BoundaryTexture = EePreviewHandler.NewActualTexture2D(),
                    HotspotTexture = EePreviewHandler.NewActualTexture2D()
                };
            }

            public void SubjectsAreBased(List<string> subjects)
            {
                _previewHandler.EnsureBasedInitialized();
                _previewHandler.SubjectsAreBased(subjects);
            }

            public void SubjectNoLongerBased(string subject)
            {
                _previewHandler.SubjectNoLongerBased(subject);
            }

            public void AllNeutralizedBlendshapesDeleted()
            {
            }
        }
    }

    public class EeMetadata
    {
        internal const int PreviewResolutionMultiplier = 1;
        internal const int StandardWidth = 300;
        internal const int StandardHeight = 200;
        internal const int HalfWidth = StandardWidth / 2;
        internal const int HalfHeight = StandardHeight / 2;
        private const string EeAnimationEditorMetadataAssetPath = "Assets/Hai/EeMetadata.asset";

        internal ExpressionEditorMetadata MetadataAsset;

        internal void EnsureMetadataAssetInitialized()
        {
            MetadataAsset = GetOrCreateMetadataAsset();
        }

        private static ExpressionEditorMetadata GetOrCreateMetadataAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ExpressionEditorMetadata>(EeAnimationEditorMetadataAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ExpressionEditorMetadata>();
                AssetDatabase.CreateAsset(asset, EeAnimationEditorMetadataAssetPath);
            }

            return asset;
        }
    }
}
