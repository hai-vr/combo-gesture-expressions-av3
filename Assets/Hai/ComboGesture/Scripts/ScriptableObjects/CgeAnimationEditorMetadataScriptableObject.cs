using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.ScriptableObjects
{
    public class CgeAnimationEditorMetadata : ScriptableObject
    {
        public List<CgeAnimationEditorMetadataBasedBlendshape> basedBlendshapes = new List<CgeAnimationEditorMetadataBasedBlendshape>();

        public void PutBasedBlendshape(CgeAnimationEditorMetadataBasedBlendshape toAdd)
        {
            basedBlendshapes = basedBlendshapes.Where(it => it.subject != toAdd.subject).Concat(new[] {toAdd}).ToList();
        }

        public void RemoveBasedBlendshape(string subjectToRemove)
        {
            basedBlendshapes = basedBlendshapes.Where(it => it.subject != subjectToRemove).ToList();
        }

        public string GetBased(string subject)
        {
            var onWhat = basedBlendshapes.Where(it => it.subject == subject).Select(it => it.based).FirstOrDefault();
            return onWhat;
        }

        public List<string> AllBased()
        {
            return basedBlendshapes.Select(blendshape => blendshape.based).Distinct().ToList();
        }
    }

    [Serializable]
    public struct CgeAnimationEditorMetadataBasedBlendshape
    {
        public string subject;
        public string based;
    }
}
