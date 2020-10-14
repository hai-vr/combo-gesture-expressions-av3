using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
    public class PuppetManifest : IManifest
    {
        public PuppetAnimatedBehavior Behavior { get; }
        private readonly float _transitionDuration;

        public PuppetManifest(float transitionDuration, PuppetAnimatedBehavior behavior)
        {
            Behavior = behavior;
            _transitionDuration = transitionDuration;
        }

        public ManifestKind Kind()
        {
            return ManifestKind.Puppet;
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Behavior.Qualifications.Any(qualifiedAnimation => qualifiedAnimation.Qualification.IsBlinking);
        }

        public bool RequiresLimitedLipsync()
        {
            return Behavior.Qualifications.Any(qualifiedAnimation => qualifiedAnimation.Qualification.Limitation != QualifiedLimitation.None);
        }

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return Behavior.Qualifications;
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            var blendTrees = new HashSet<BlendTree> {Behavior.Tree};
            foreach (var blendTree in FindAllBlendTreesInside(Behavior.Tree))
            {
                blendTrees.Add(blendTree);
            }

            return blendTrees.ToList();
        }

        private static List<BlendTree> FindAllBlendTreesInside(BlendTree tree)
        {
            return tree.children
                .Select(motion => motion.motion)
                .Where(motion => motion is BlendTree)
                .SelectMany(motion =>
                {
                    var foundTree = (BlendTree) motion;
                    var foundTreeAndSubtrees = new List<BlendTree> {foundTree};
                    foreach (var subtree in FindAllBlendTreesInside(foundTree))
                    {
                        foundTreeAndSubtrees.Add(subtree);
                    }

                    return foundTreeAndSubtrees;
                })
                .Distinct()
                .ToList();
        }

        public IManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new PuppetManifest(_transitionDuration, (PuppetAnimatedBehavior)Behavior.Remapping(remapping, blendRemapping));
        }
    }
}
