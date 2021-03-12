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
                    Boundaries = new PreviewBoundaries(),
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
                PostProcessPreview(preview);
                onClipRendered(preview);
            }, _editorEffector.PreviewSetup());
        }

        private void PostProcessPreview(AnimationPreview mutablePreview)
        {
            var tex = mutablePreview.RenderTexture;
            var boundaries = DifferenceAsBoundaries(tex, _noShapekeys.RenderTexture);

            // FIXME This is convoluted
            var r = _blendShapeNameToRegistry.Values.First(registry => registry.Preview == mutablePreview);
            r.Boundaries = boundaries;

            var thres = 4;
            var width = mutablePreview.RenderTexture.width;
            var height = mutablePreview.RenderTexture.height;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = tex.GetPixel(x, y);

                    var gsv = (value.r + value.g + value.b) / 3f;
                    var actualGsv = value * 0.3f + new Color(gsv, gsv, gsv, value.a) * 0.7f;

                    if (boundaries.MinX == -1)
                    {
                        tex.SetPixel(x, y, actualGsv * 0.2f);
                    }
                    else
                    {
                        var isIn = x >= boundaries.MinX - thres && x <= boundaries.MaxX + thres && y >= boundaries.MinY - thres && y <= boundaries.MaxY + thres;
                        tex.SetPixel(x, y, isIn ? value : actualGsv * 0.5f);
                    }
                }
            }
            tex.Apply(false);
        }

        private static PreviewBoundaries DifferenceAsBoundaries(Texture2D a, Texture2D b)
        {
            var minX = -1;
            var maxX = -1;
            var minY = -1;
            var maxY = -1;

            for (var y = 0; y < a.height; y++)
            {
                for (var x = 0; x < a.width; x++)
                {
                    if (a.GetPixel(x, y) != b.GetPixel(x, y))
                    {
                        if (minX == -1 || x < minX)
                        {
                            minX = x;
                        }

                        if (minY == -1 || y < minY)
                        {
                            minY = y;
                        }

                        if (x > maxX)
                        {
                            maxX = x;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }

            return new PreviewBoundaries(minX, maxX, minY, maxY);
        }

        private static AnimationClip NewSingleBlendshapeClip(string smrPath, string blendShapeName, float value)
        {
            var clip = new AnimationClip();
            clip.SetCurve(smrPath, typeof(SkinnedMeshRenderer), "blendShape." + blendShapeName, new AnimationCurve(new Keyframe(0, value), new Keyframe(1 / 60f, value)));
            return clip;
        }

        public List<ShapekeyRegistry> Previews()
        {
            return _blendShapeNameToRegistry.Values
                .Where(registry => !registry.Boundaries.IsEmpty())
                .OrderBy(registry => registry.Boundaries.MinY - registry.Boundaries.MaxY + 0)
                .ToList();
        }

        public struct ShapekeyRegistry
        {
            public string Name;
            public PreviewBoundaries Boundaries { get; set; }
            public AnimationPreview Preview;
        }

        public readonly struct PreviewBoundaries
        {
            public PreviewBoundaries(int minX, int maxX, int minY, int maxY)
            {
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }

            public readonly int MinX;
            public readonly int MaxX;
            public readonly int MinY;
            public readonly int MaxY;

            public bool IsEmpty()
            {
                return MinX == -1 && MaxX == -1 && MinY == -1 && MaxY == -1;
            }
        }
    }
}
