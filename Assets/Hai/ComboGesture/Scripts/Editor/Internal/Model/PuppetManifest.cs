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
            return Behavior.QualifiedAnimations().Any(qualifiedAnimation => qualifiedAnimation.Qualification.IsBlinking);
        }

        public bool RequiresLimitedLipsync()
        {
            return Behavior.QualifiedAnimations().Any(qualifiedAnimation => qualifiedAnimation.Qualification.Limitation != QualifiedLimitation.None);
        }

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return Behavior.QualifiedAnimations();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return FindAllBlendTreesIncludingItself(Behavior.Tree);
        }

        public static IEnumerable<BlendTree> FindAllBlendTreesIncludingItself(BlendTree tree)
        {
            var blendTrees = new HashSet<BlendTree> {tree};
            foreach (var blendTree in FindAllBlendTreesOnlyInside(tree))
            {
                blendTrees.Add(blendTree);
            }

            return blendTrees.ToList();
        }

        private static List<BlendTree> FindAllBlendTreesOnlyInside(BlendTree tree)
        {
            return tree.children
                .Select(motion => motion.motion)
                .Where(motion => motion is BlendTree)
                .SelectMany(motion =>
                {
                    var foundTree = (BlendTree) motion;
                    var foundTreeAndSubtrees = new List<BlendTree> {foundTree};
                    foreach (var subtree in FindAllBlendTreesIncludingItself(foundTree))
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

        public IManifest UsingRemappedWeights(Dictionary<BlendTree, AutoWeightTreeMapping> autoWeightRemapping)
        {
            return this;
        }
    }
}
