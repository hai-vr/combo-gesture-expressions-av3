using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EeAccessCommands
    {
        private readonly EeAnimationEditorState _state;
        private readonly EeMetadata _metadata;
        private readonly EePreviewHandler _previewHandler;

        public EeAccessCommands(EeAnimationEditorState state, EeMetadata metadata, EePreviewHandler previewHandler)
        {
            _state = state;
            _metadata = metadata;
            _previewHandler = previewHandler;
        }

        public Texture2D ActivePreview()
        {
            _previewHandler.EnsureActivePreviewInitialized();
            return _state.ActivePreview;
        }

        public List<EeEditableBlendshape> Editables()
        {
            return _state.EditableBlendshapes;
        }

        public void ManuallyPreviewAll()
        {
            _previewHandler.RenderForced();
        }

        public List<EeExplorerBlendshape> SmrBlendShapeProperties()
        {
            return _state.ExplorerBlendshapes;
        }

        public bool IsMaintaining()
        {
            return _state.Maintain;
        }

        public EePreviewAvatar? DummyNullable()
        {
            return _state.InternalDummyOptional();
        }

        public EeNonEditableStats NonEditableStats()
        {
            return _state.Stats;
        }

        public bool HasActiveClip()
        {
            return _state.CurrentClip != null;
        }

        public AnimationClip ActiveClip()
        {
            return _state.CurrentClip;
        }

        public bool ActiveHas(HashSet<EditorCurveBinding> nonResetBlendshapes, string path, string property)
        {
            if (_state.CurrentClip == null) return true;

            return nonResetBlendshapes.Contains(new EditorCurveBinding
            {
                path = path,
                propertyName = property,
                type = typeof(SkinnedMeshRenderer)
            });
        }

        public bool ActiveCouldDelete(HashSet<EditorCurveBinding> curveBindingsCache, string path, string property)
        {
            return curveBindingsCache.Contains(new EditorCurveBinding
            {
                path = path,
                propertyName = property,
                type = typeof(SkinnedMeshRenderer)
            });
        }

        public HashSet<EditorCurveBinding> AllNonResetBlendshapes(HashSet<EditorCurveBinding> curveBindingsCache)
        {
            if (_state.CurrentClip == null) return new HashSet<EditorCurveBinding>();

            return new HashSet<EditorCurveBinding>(curveBindingsCache
                .Where(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.Any(keyframe => keyframe.value != 0))
                .ToList());
        }

        public HashSet<EditorCurveBinding> AllCurveBindingsCache()
        {
            if (_state.CurrentClip == null) return new HashSet<EditorCurveBinding>();

            return new HashSet<EditorCurveBinding>(AnimationUtility.GetCurveBindings(_state.CurrentClip).ToList());
        }

        public bool IsOnWhat(string potentiallyOnWhat)
        {
            return _metadata.MetadataAsset.IsOnWhat(potentiallyOnWhat);
        }

        public string GetBasedOnWhat(string subject)
        {
            _metadata.EnsureMetadataAssetInitialized();

            var onWhat = _metadata.MetadataAsset.GetBasedOnWhat(subject);
            return onWhat;
        }

        public EeAnimationEditorScenePreviewMode GetScenePreviewMode()
        {
            return _state.ScenePreviewMode;
        }

        public List<ExpressionEditorPreviewable> AllPreviewSetups()
        {
            return EePreviewSetupWizard.FindActiveAndValidPreviewComponentsInRoot();
        }
    }
}
