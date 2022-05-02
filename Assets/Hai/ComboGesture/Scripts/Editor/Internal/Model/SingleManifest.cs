using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
    public class SingleManifest : IManifest
    {
        public IAnimatedBehavior Behavior { get; }
        private readonly float _transitionDuration;

        public SingleManifest(float transitionDuration, IAnimatedBehavior behavior)
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

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return Behavior.QualifiedAnimations();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return Behavior.AllBlendTreesFoundRecursively();
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
            return new SingleManifest(_transitionDuration, Behavior.Remapping(remapping, blendRemapping));
        }

        public IManifest UsingRemappedWeights(Dictionary<BlendTree, AutoWeightTreeMapping> autoWeightRemapping)
        {
            return this;
        }

        public PermutationManifest ToEquatedPermutation()
        {
            var poses = Permutation.All().ToDictionary(
                permutation => permutation,
                permutation => (IAnimatedBehavior)Behavior
            );

            return new PermutationManifest(poses, _transitionDuration);
        }
    }
}
