using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateCombinator
    {
        public Dictionary<IAnimatedBehavior, TransitionCondition> IntermediateToTransition { get; }

        public IntermediateCombinator(List<ManifestBinding> activityManifests)
        {
            var permutationRepresentation = DecomposePermutationsIntoBehaviors(activityManifests);
            var puppetRepresentation = DecomposePuppetsIntoBehaviors(activityManifests);
            var massiveRepresentation = DecomposeMassiveIntoBehaviors(activityManifests);

            IntermediateToTransition = permutationRepresentation
                .Concat(puppetRepresentation)
                .Concat(massiveRepresentation)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static Dictionary<IAnimatedBehavior, TransitionCondition> DecomposeMassiveIntoBehaviors(List<ManifestBinding> activityManifests)
        {
            var representation = activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Massive)
                .SelectMany(binding =>
                {
                    MassiveBlendManifest manifest = (MassiveBlendManifest) binding.Manifest;

                    return Permutation.All().Select(currentPermutation =>
                    {
                        var animatedBehavior = MassiveBlendToAnimatedBehavior(manifest, currentPermutation);
                        return new KeyValuePair<IAnimatedBehavior, TransitionCondition.ActivityBoundTransitionCondition>(
                            animatedBehavior,
                            new TransitionCondition.ActivityBoundTransitionCondition(
                                binding.StageValue,
                                manifest.TransitionDuration(),
                                currentPermutation,
                                binding.LayerOrdinal
                            )
                        );

                    }).ToList();
                })
                .ToDictionary(pair => pair.Key, pair => (TransitionCondition)pair.Value);

            return representation;
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

        private static Dictionary<IAnimatedBehavior, TransitionCondition> DecomposePuppetsIntoBehaviors(List<ManifestBinding> activityManifests)
        {
            var puppetRepresentation = new Dictionary<IAnimatedBehavior, TransitionCondition>();

            var puppetManifests = activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Puppet)
                .ToList();

            foreach (var binding in puppetManifests)
            {
                var puppet = (PuppetManifest) binding.Manifest;

                puppetRepresentation.Add(puppet.Behavior, new TransitionCondition.PuppetBoundTransitionCondition(
                    binding.StageValue,
                    puppet.TransitionDuration(),
                    binding.LayerOrdinal
                ));
            }

            return puppetRepresentation;
        }

        private static Dictionary<IAnimatedBehavior, TransitionCondition> DecomposePermutationsIntoBehaviors(List<ManifestBinding> activityManifests)
        {
            return activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Permutation)
                .SelectMany(DecomposePermutation)
                .ToDictionary(entry => entry.Behavior, entry => entry.TransitionCondition);
        }

        private static List<AnimToTransitionEntry> DecomposePermutation(ManifestBinding manifestBinding)
        {
            var manifest = (PermutationManifest)manifestBinding.Manifest;
            return manifest.Poses
                .Select(pair => new AnimToTransitionEntry(
                    new TransitionCondition.ActivityBoundTransitionCondition(manifestBinding.StageValue, manifest.TransitionDuration(), pair.Key, manifestBinding.LayerOrdinal),
                    pair.Value
                ))
                .ToList();
        }
    }

    internal class AnimToTransitionEntry
    {
        public TransitionCondition TransitionCondition { get; }
        public IAnimatedBehavior Behavior { get; }

        public AnimToTransitionEntry(TransitionCondition transitionCondition, IAnimatedBehavior behavior)
        {
            TransitionCondition = transitionCondition;
            Behavior = behavior;
        }
    }

    internal abstract class TransitionCondition
    {
        public float TransitionDuration { get; }
        public Permutation Permutation { get; } // Can be null...?
        public int LayerOrdinal { get; }

        private TransitionCondition(float transitionDuration, Permutation permutation, int layerOrdinal)
        {
            TransitionDuration = transitionDuration;
            Permutation = permutation;
            LayerOrdinal = layerOrdinal;
        }

        internal class ActivityBoundTransitionCondition : TransitionCondition
        {
            public int StageValue { get; }

            public ActivityBoundTransitionCondition(int stageValue, float transitionDuration, Permutation permutation,
                int layerOrdinal) : base(transitionDuration, permutation, layerOrdinal)
            {
                StageValue = stageValue;
            }
        }

        internal class PuppetBoundTransitionCondition : TransitionCondition
        {
            public int StageValue { get; }

            public PuppetBoundTransitionCondition(int stageValue, float transitionDuration,
                int layerOrdinal) : base(transitionDuration, null, layerOrdinal)
            {
                StageValue = stageValue;
            }
        }

        internal class AlwaysTransitionCondition : TransitionCondition
        {
            public AlwaysTransitionCondition(float transitionDuration, Permutation permutation,
                int layerOrdinal) : base(transitionDuration, permutation, layerOrdinal)
            {
            }
        }
    }
}
