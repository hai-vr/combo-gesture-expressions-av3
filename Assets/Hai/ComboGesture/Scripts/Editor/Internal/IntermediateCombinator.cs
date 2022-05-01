using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public interface IComposedBehaviour
    {
        int StageValue { get; }
        float TransitionDuration { get; }
    }

    public class PermutationComposedBehaviour : IComposedBehaviour
    {
        public int StageValue { get; set; }
        public float TransitionDuration { get; set; }
        public Dictionary<Permutation, IAnimatedBehavior> Behaviors;
    }

    public class SingularComposedBehaviour : IComposedBehaviour
    {
        public int StageValue { get; set; }
        public float TransitionDuration { get; set; }
        public IAnimatedBehavior Behavior;
    }

    internal class IntermediateCombinator
    {
        public List<IComposedBehaviour> ComposedBehaviours;

        public IntermediateCombinator(List<ManifestBinding> activityManifests)
        {
            ComposedBehaviours = activityManifests.Select(binding =>
            {
                switch (binding.Manifest)
                {
                    case PermutationManifest permutationManifest:
                        return (IComposedBehaviour)new PermutationComposedBehaviour
                        {
                            StageValue = binding.StageValue,
                            TransitionDuration = permutationManifest.TransitionDuration(),
                            Behaviors = new Dictionary<Permutation, IAnimatedBehavior>(permutationManifest.Poses)
                        };
                    case PuppetManifest puppetManifest:
                        return new SingularComposedBehaviour
                        {
                            StageValue = binding.StageValue,
                            TransitionDuration = puppetManifest.TransitionDuration(),
                            Behavior = puppetManifest.Behavior
                        };
                    case MassiveBlendManifest massiveManifest:
                        return new PermutationComposedBehaviour
                        {
                            StageValue = binding.StageValue,
                            TransitionDuration = massiveManifest.TransitionDuration(),
                            Behaviors = DecomposeMassiveIntoBehaviors(massiveManifest)
                        };
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).ToList();
        }

        private static Dictionary<Permutation, IAnimatedBehavior> DecomposeMassiveIntoBehaviors(MassiveBlendManifest massiveManifest)
        {
            return Permutation.All().ToDictionary(
                permutation => permutation,
                permutation => MassiveBlendToAnimatedBehavior(massiveManifest, permutation)
            );
        }

        private static IAnimatedBehavior MassiveBlendToAnimatedBehavior(MassiveBlendManifest manifest, Permutation currentPermutation)
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

        private static IAnimatedBehavior OfSimple(MassiveBlendManifest manifest, Permutation currentPermutation)
        {
            var zero = manifest.EquatedManifests[0].Poses[currentPermutation];
            var one = manifest.EquatedManifests[1].Poses[currentPermutation];
            return SimpleMassiveBlendAnimatedBehavior.Maybe(zero, one, manifest.SimpleParameterName);
        }

        private static IAnimatedBehavior OfTwoDirections(MassiveBlendManifest manifest, Permutation currentPermutation)
        {
            var zero = manifest.EquatedManifests[0].Poses[currentPermutation];
            var one = manifest.EquatedManifests[1].Poses[currentPermutation];
            var minusOne = manifest.EquatedManifests[2].Poses[currentPermutation];
            return TwoDirectionsMassiveBlendAnimatedBehavior.Maybe(zero, one, minusOne, manifest.SimpleParameterName);
        }

        private static IAnimatedBehavior OfComplexBlendTree(MassiveBlendManifest manifest, Permutation currentPermutation)
        {
            var poses = manifest.EquatedManifests.Select(permutationManifest => permutationManifest.Poses[currentPermutation]).ToList();
            return ComplexMassiveBlendAnimatedBehavior.Of(poses, manifest.BlendTree);
        }
    }

    internal abstract class TransitionCondition
    {
        public float TransitionDuration { get; }
        public Permutation PermutationNullable { get; }
        public int StageValue { get; }

        private TransitionCondition(float transitionDuration, Permutation permutationNullable, int stageValue)
        {
            TransitionDuration = transitionDuration;
            PermutationNullable = permutationNullable;
            StageValue = stageValue;
        }

        internal class ActivityBoundTransitionCondition : TransitionCondition
        {
            public ActivityBoundTransitionCondition(int stageValue, float transitionDuration, Permutation permutationNullable) : base(transitionDuration, permutationNullable, stageValue)
            {
            }
        }

        internal class PuppetBoundTransitionCondition : TransitionCondition
        {
            public PuppetBoundTransitionCondition(int stageValue, float transitionDuration) : base(transitionDuration, null, stageValue)
            {
            }
        }
    }
}
