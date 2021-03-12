using System.Collections.Generic;
using System.Linq;
using Hai.ExpressionsEditor.Scripts.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class EeEditCommands
    {
        private readonly EeAnimationEditorState _state;
        private readonly EeMetadata _metadata;
        private readonly EePreviewHandler _previewHandler;

        public EeEditCommands(EeAnimationEditorState state, EeMetadata metadata, EePreviewHandler previewHandler)
        {
            _state = state;
            _metadata = metadata;
            _previewHandler = previewHandler;
        }

        public void AddBlendshape(string path, string property)
        {
            Undo.RecordObject(_state.CurrentClip, "EE - Add blendshape");

            var binding = ToBlendshapeBinding(path, property);
            if (AnimationUtility.GetCurveBindings(_state.CurrentClip).Contains(binding))
            {
                var keyframes = AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys
                    .Select(keyframe =>
                    {
                        keyframe.value = 100;
                        return keyframe;
                    })
                    .ToArray();
                AnimationUtility.SetEditorCurve(_state.CurrentClip, binding, new AnimationCurve(keyframes));
            }
            else
            {
                AnimationUtility.SetEditorCurve(_state.CurrentClip, binding, AnimationCurve.Constant(0f, 1/60f, 100f));
            }

            _state.Altered(_previewHandler).NewBlendshapeAdded(binding);
        }

        public void UpdateBlendshape(string path, string property, float newValue)
        {
            Undo.RecordObject(_state.CurrentClip, "EE - Update blendshape");

            var binding = ToBlendshapeBinding(path, property);

            var oldCurve = AnimationUtility.GetEditorCurve(_state.CurrentClip, binding);
            var newCurve = new AnimationCurve(oldCurve.keys.Select(keyframe =>
            {
                keyframe.value = newValue;
                return keyframe;
            }).ToArray());

            AnimationUtility.SetEditorCurve(_state.CurrentClip, binding, newCurve);

            _state.Altered(_previewHandler).BlendshapeModified(path, property, newValue);
        }

        public void DeleteBlendshape(string path, string property)
        {
            Undo.RecordObject(_state.CurrentClip, "EE - Delete blendshape");

            var binding = ToBlendshapeBinding(path, property);
            AnimationUtility.SetEditorCurve(_state.CurrentClip, binding, null);

            _state.Altered(_previewHandler).SingleBlendshapeDeleted(path, property);
        }

        public void DeleteAllNeutralizedBlendshapes()
        {
            Undo.RecordObject(_state.CurrentClip, "EE - Delete all neutralized blendshapes");

            var bindings = AnimationUtility.GetCurveBindings(_state.CurrentClip);
            foreach (var binding in bindings
                .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                .Where(binding => AnimationUtility.GetEditorCurve(_state.CurrentClip, binding).keys.All(keyframe => keyframe.value == 0f)))
            {
                AnimationUtility.SetEditorCurve(_state.CurrentClip, binding, null);
            }

            _state.Altered(_previewHandler).AllNeutralizedBlendshapesDeleted();
        }

        public void AssignBased(string based, List<string> subjects)
        {
            _metadata.EnsureMetadataAssetInitialized();
            EditorUtility.SetDirty(_metadata.MetadataAsset);

            foreach (var basedBlendshape in subjects
                .Distinct()
                .Select(subject => new EeAnimationEditorMetadataBasedBlendshape { onWhat = based, based = subject }))
            {
                _metadata.MetadataAsset.PutBasedBlendshape(basedBlendshape);
            }

            _state.Altered(_previewHandler).SubjectsAreBased(subjects);
        }

        public void DeleteBasedSubject(string subject)
        {
            _metadata.EnsureMetadataAssetInitialized();
            EditorUtility.SetDirty(_metadata.MetadataAsset);

            _metadata.MetadataAsset.DeleteBasedBlendshape(subject);

            _state.Altered(_previewHandler).SubjectNoLongerBased(subject);
        }

        private static EditorCurveBinding ToBlendshapeBinding(string path, string property)
        {
            return new EditorCurveBinding
            {
                path = path,
                propertyName = property,
                type = typeof(SkinnedMeshRenderer)
            };
        }
    }
}
