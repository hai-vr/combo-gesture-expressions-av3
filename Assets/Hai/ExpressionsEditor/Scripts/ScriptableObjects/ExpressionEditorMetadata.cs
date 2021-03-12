using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.ScriptableObjects
{
    public class ExpressionEditorMetadata : ScriptableObject
    {
        public List<EeAnimationEditorMetadataBasedBlendshape> basedBlendshapes = new List<EeAnimationEditorMetadataBasedBlendshape>();

        public void PutBasedBlendshape(EeAnimationEditorMetadataBasedBlendshape toAdd)
        {
            basedBlendshapes = basedBlendshapes.Where(it => it.based != toAdd.based).Concat(new[] {toAdd}).ToList();
        }

        public void DeleteBasedBlendshape(string basedToRemove)
        {
            basedBlendshapes = basedBlendshapes.Where(it => it.based != basedToRemove).ToList();
        }

        public string GetBasedOnWhat(string based)
        {
            var onWhat = basedBlendshapes.Where(it => it.based == based).Select(it => it.onWhat).FirstOrDefault();
            return onWhat;
        }

        public bool IsOnWhat(string based)
        {
            return basedBlendshapes.Any(it => it.onWhat == based);
        }

        public List<string> AllOnWhat()
        {
            return basedBlendshapes.Select(blendshape => blendshape.onWhat).Distinct().ToList();
        }
    }

    [Serializable]
    public struct EeAnimationEditorMetadataBasedBlendshape
    {
        public string based;
        public string onWhat;
    }
}
