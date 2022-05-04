using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public interface IComposedBehaviour
    {
        bool IsActivityBound { get; }
        int StageValue { get; }
        float TransitionDuration { get; }
        bool IsAvatarDynamics { get; }
        CgeDynamicsRankedDescriptor DynamicsDescriptor { get; }
    }

    public class PermutationComposedBehaviour : IComposedBehaviour
    {
        public bool IsActivityBound { get; set; }
        public int StageValue { get; set; }
        public float TransitionDuration { get; set; }
        public bool IsAvatarDynamics { get; set; }
        public CgeDynamicsRankedDescriptor DynamicsDescriptor { get; set; }

        public Dictionary<CgePermutation, ICgeAnimatedBehavior> Behaviors;
    }

    public class OneHandComposedBehaviour : IComposedBehaviour
    {
        public bool IsActivityBound { get; set; }
        public int StageValue { get; set; }
        public float TransitionDuration { get; set; }
        public bool IsAvatarDynamics { get; set; }
        public CgeDynamicsRankedDescriptor DynamicsDescriptor { get; set; }
        public Dictionary<CgeHandPose, ICgeAnimatedBehavior> Behaviors;
        public bool IsLeftHand;
    }

    public class SingularComposedBehaviour : IComposedBehaviour
    {
        public bool IsActivityBound { get; set; }
        public int StageValue { get; set; }
        public float TransitionDuration { get; set; }
        public bool IsAvatarDynamics { get; set; }
        public CgeDynamicsRankedDescriptor DynamicsDescriptor { get; set; }
        public ICgeAnimatedBehavior Behavior;
    }

    internal class CgeIntermediateCombinator
    {
        public readonly List<IComposedBehaviour> ComposedBehaviours;

        public CgeIntermediateCombinator(List<CgeManifestBinding> activityManifests)
        {
            ComposedBehaviours = activityManifests.Select(binding =>
            {
                switch (binding.Manifest)
                {
                    case CgePermutationManifest permutationManifest:
                        return (IComposedBehaviour)new PermutationComposedBehaviour
                        {
                            IsActivityBound = binding.IsActivityBound,
                            StageValue = binding.StageValue,
                            TransitionDuration = permutationManifest.TransitionDuration(),
                            IsAvatarDynamics = binding.IsAvatarDynamics,
                            DynamicsDescriptor = binding.DynamicsDescriptor,
                            Behaviors = new Dictionary<CgePermutation, ICgeAnimatedBehavior>(permutationManifest.Poses)
                        };
                    case CgeSingleManifest puppetManifest:
                        return new SingularComposedBehaviour
                        {
                            IsActivityBound = binding.IsActivityBound,
                            StageValue = binding.StageValue,
                            TransitionDuration = puppetManifest.TransitionDuration(),
                            IsAvatarDynamics = binding.IsAvatarDynamics,
                            DynamicsDescriptor = binding.DynamicsDescriptor,
                            Behavior = puppetManifest.Behavior
                        };
                    case CgeMassiveBlendManifest massiveManifest:
                        return new PermutationComposedBehaviour
                        {
                            IsActivityBound = binding.IsActivityBound,
                            StageValue = binding.StageValue,
                            TransitionDuration = massiveManifest.TransitionDuration(),
                            IsAvatarDynamics = binding.IsAvatarDynamics,
                            DynamicsDescriptor = binding.DynamicsDescriptor,
                            Behaviors = DecomposeMassiveIntoBehaviors(massiveManifest)
                        };
                    case CgeOneHandManifest oneHandManifest:
                        return new OneHandComposedBehaviour
                        {
                            IsActivityBound = binding.IsActivityBound,
                            StageValue = binding.StageValue,
                            TransitionDuration = oneHandManifest.TransitionDuration(),
                            IsAvatarDynamics = binding.IsAvatarDynamics,
                            DynamicsDescriptor = binding.DynamicsDescriptor,
                            Behaviors = new Dictionary<CgeHandPose, ICgeAnimatedBehavior>(oneHandManifest.Poses),
                            IsLeftHand = oneHandManifest.IsLeftHand
                        };
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).ToList();
        }

        private static Dictionary<CgePermutation, ICgeAnimatedBehavior> DecomposeMassiveIntoBehaviors(CgeMassiveBlendManifest massiveManifest)
        {
            return CgePermutation.All().ToDictionary(
                permutation => permutation,
                permutation => MassiveBlendToAnimatedBehavior(massiveManifest, permutation)
            );
        }

        private static ICgeAnimatedBehavior MassiveBlendToAnimatedBehavior(CgeMassiveBlendManifest manifest, CgePermutation currentPermutation)
        {
            switch (manifest.Mode)
            {
                case CgeMassiveBlendMode.Simple:
                    return OfSimple(manifest, currentPermutation);
                case CgeMassiveBlendMode.TwoDirections:
                    return OfTwoDirections(manifest, currentPermutation);
                case CgeMassiveBlendMode.ComplexBlendTree:
                    return OfComplexBlendTree(manifest, currentPermutation);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ICgeAnimatedBehavior OfSimple(CgeMassiveBlendManifest manifest, CgePermutation currentPermutation)
        {
            var zero = manifest.EquatedManifests[0].Poses[currentPermutation];
            var one = manifest.EquatedManifests[1].Poses[currentPermutation];
            return CgeSimpleMassiveBlendAnimatedBehavior.Maybe(zero, one, manifest.SimpleParameterName);
        }

        private static ICgeAnimatedBehavior OfTwoDirections(CgeMassiveBlendManifest manifest, CgePermutation currentPermutation)
        {
            var zero = manifest.EquatedManifests[0].Poses[currentPermutation];
            var one = manifest.EquatedManifests[1].Poses[currentPermutation];
            var minusOne = manifest.EquatedManifests[2].Poses[currentPermutation];
            return CgeTwoDirectionsMassiveBlendAnimatedBehavior.Maybe(zero, one, minusOne, manifest.SimpleParameterName);
        }

        private static ICgeAnimatedBehavior OfComplexBlendTree(CgeMassiveBlendManifest manifest, CgePermutation currentPermutation)
        {
            var poses = manifest.EquatedManifests.Select(permutationManifest => permutationManifest.Poses[currentPermutation]).ToList();
            return CgeComplexMassiveBlendAnimatedBehavior.Of(poses, manifest.BlendTree);
        }
    }
}
