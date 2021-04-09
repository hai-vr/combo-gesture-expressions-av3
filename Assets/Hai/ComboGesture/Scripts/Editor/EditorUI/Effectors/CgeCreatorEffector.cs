using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Modules;
using Hai.ComboGesture.Scripts.Editor.Internal;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeCreatorEffector
    {
        private readonly CgeEditorEffector _editorEffector;
        public SkinnedMeshRenderer SkinnedMeshBeingFocused { get; set; }
        private readonly Dictionary<string, ShapekeyRegistry> _blendShapeNameToRegistry = new Dictionary<string, ShapekeyRegistry>();
        private AnimationPreview _noShapekeys;

        public CgeCreatorEffector(CgeEditorEffector editorEffector)
        {
            _editorEffector = editorEffector;
        }

        public void Update(Transform avatarDescriptorTransform, int lipsyncPreviewWidth, int lipsyncPreviewHeight)
        {
            if (SkinnedMeshBeingFocused == null)
            {
                _blendShapeNameToRegistry.Clear();
                return;
            }

            var smrPath = SharedLayerUtils.ResolveRelativePath(avatarDescriptorTransform, SkinnedMeshBeingFocused.transform);

            var mesh = SkinnedMeshBeingFocused.sharedMesh;
            for (var i = 0; i < mesh.blendShapeCount; i++)
            {
                var blendShapeName = mesh.GetBlendShapeName(i);
                _blendShapeNameToRegistry[blendShapeName] = new ShapekeyRegistry
                {
                    Name = blendShapeName,
                    Preview = new AnimationPreview(
                        NewSingleBlendshapeClip(smrPath, blendShapeName, 100),
                        CgeMemoryQuery.NewPreviewTexture2D(lipsyncPreviewWidth, lipsyncPreviewHeight)
                    )
                };

                if (i == 0)
                {
                    _noShapekeys = new AnimationPreview(
                        NewSingleBlendshapeClip(smrPath, "_IGNORED", 0),
                        CgeMemoryQuery.NewPreviewTexture2D(lipsyncPreviewWidth, lipsyncPreviewHeight)
                    );
                }
            }
        }

        public void GeneratePreviews(CgeRenderingCommands renderingCommands, Action<AnimationPreview> onClipRendered)
        {
            renderingCommands.GenerateSpecific(new List<AnimationPreview> {_noShapekeys}, onClipRendered, _editorEffector.PreviewSetup());
            renderingCommands.GenerateSpecificFastMode(_blendShapeNameToRegistry.Values.Select(registry => registry.Preview).ToList(), preview =>
            {
                CgeRenderingSupport.MutateHighlightDifferences(preview.RenderTexture, _noShapekeys.RenderTexture);
                onClipRendered(preview);
            }, _editorEffector.PreviewSetup());
        }

        private static AnimationClip NewSingleBlendshapeClip(string smrPath, string blendShapeName, float value)
        {
            var clip = new AnimationClip();
            clip.SetCurve(smrPath, typeof(SkinnedMeshRenderer), "blendShape." + blendShapeName, new AnimationCurve(new Keyframe(0, value), new Keyframe(1 / 60f, value)));
            return clip;
        }

        public List<ShapekeyRegistry> Previews()
        {
            return _blendShapeNameToRegistry.Values.ToList();
        }

        public struct ShapekeyRegistry
        {
            public string Name;
            public AnimationPreview Preview;
        }
    }
}
