using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeSingleManifest : ICgeManifest
    {
        public ICgeAnimatedBehavior Behavior { get; }
        private readonly float _transitionDuration;

        public CgeSingleManifest(float transitionDuration, ICgeAnimatedBehavior behavior)
        {
            Behavior = behavior;
            _transitionDuration = transitionDuration;
        }

        public CgeManifestKind Kind()
        {
            return CgeManifestKind.Puppet;
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Behavior.QualifiedAnimations().Any(qualifiedAnimation => qualifiedAnimation.Qualification.IsBlinking);
        }

        public IEnumerable<CgeQualifiedAnimation> AllQualifiedAnimations()
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

        public ICgeManifest NewFromRemappedAnimations(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new CgeSingleManifest(_transitionDuration, Behavior.Remapping(remapping, blendRemapping));
        }

        public ICgeManifest UsingRemappedWeights(Dictionary<BlendTree, CgeAutoWeightTreeMapping> autoWeightRemapping)
        {
            return this;
        }

        public CgePermutationManifest ToEquatedPermutation()
        {
            var poses = CgePermutation.All().ToDictionary(
                permutation => permutation,
                permutation => Behavior
            );

            return new CgePermutationManifest(poses, _transitionDuration);
        }
    }
}
