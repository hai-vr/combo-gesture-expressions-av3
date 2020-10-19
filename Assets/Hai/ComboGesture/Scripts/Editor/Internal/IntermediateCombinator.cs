using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateCombinator
    {
        public Dictionary<IAnimatedBehavior, List<TransitionCondition>> IntermediateToTransition { get; }

        public IntermediateCombinator(List<ManifestBinding> activityManifests)
        {
            var permutationRepresentation = OptimizeCollapsableConditions(DecomposePermutationsIntoBehaviors(activityManifests), activityManifests.Count);
            var puppetRepresentation = DecomposePuppetsIntoBehaviors(activityManifests);

            IntermediateToTransition = permutationRepresentation
                .Concat(puppetRepresentation)
                .GroupBy(pair => pair.Key, pair => pair.Value)
                .ToDictionary(pair => pair.Key, grouping => grouping.SelectMany(list => list).ToList());
        }

        private static Dictionary<IAnimatedBehavior, List<TransitionCondition>> DecomposePuppetsIntoBehaviors(List<ManifestBinding> activityManifests)
        {
            Dictionary<IAnimatedBehavior, List<TransitionCondition>> puppetRepresentation = new Dictionary<IAnimatedBehavior, List<TransitionCondition>>();

            var puppetManifests = activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Puppet)
                .ToList();

            foreach (var binding in puppetManifests)
            {
                var puppet = (PuppetManifest) binding.Manifest;

                puppetRepresentation.Add(puppet.Behavior, new List<TransitionCondition>
                {
                    new TransitionCondition.PuppetBoundTransitionCondition(
                        binding.StageValue,
                        puppet.TransitionDuration(),
                        binding.LayerOrdinal
                    )
                });
            }

            return puppetRepresentation;
        }

        readonly struct PermutationAndTransitionDuration
        {
            public PermutationAndTransitionDuration(Permutation permutation, float transitionDuration)
            {
                Permutation = permutation;
                TransitionDuration = transitionDuration;
            }

            private Permutation Permutation { get; }
            private float TransitionDuration { get; }

            public bool Equals(PermutationAndTransitionDuration other)
            {
                return Equals(Permutation, other.Permutation) && TransitionDuration.Equals(other.TransitionDuration);
            }

            public override bool Equals(object obj)
            {
                return obj is PermutationAndTransitionDuration other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Permutation != null ? Permutation.GetHashCode() : 0) * 397) ^ TransitionDuration.GetHashCode();
                }
            }

            public static bool operator ==(PermutationAndTransitionDuration left, PermutationAndTransitionDuration right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PermutationAndTransitionDuration left, PermutationAndTransitionDuration right)
            {
                return !left.Equals(right);
            }
        }

        private static Dictionary<IAnimatedBehavior, List<TransitionCondition>> OptimizeCollapsableConditions(Dictionary<IAnimatedBehavior, List<TransitionCondition>> exhaustive, int activityManifestsCount)
        {
            return exhaustive
                .Select(pair =>
                {
                    var conditions = pair.Value
                        .GroupBy(condition => new PermutationAndTransitionDuration(((TransitionCondition.ActivityBoundTransitionCondition) condition).Permutation, condition.TransitionDuration))
                        .SelectMany(grouping =>
                        {
                            var groupingList = grouping.ToList();
                            if (groupingList.Count != activityManifestsCount)
                            {
                                return groupingList;
                            }

                            var item = groupingList[0];
                            return new List<TransitionCondition>
                            {
                                new TransitionCondition.AlwaysTransitionCondition(item.TransitionDuration, item.Permutation,
                                    item.LayerOrdinal)
                            };
                        })
                        .ToList();

                    return new KeyValuePair<IAnimatedBehavior, List<TransitionCondition>>(pair.Key, conditions);
                })
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static Dictionary<IAnimatedBehavior, List<TransitionCondition>> DecomposePermutationsIntoBehaviors(List<ManifestBinding> activityManifests)
        {
            return activityManifests
                .Where(binding => binding.Manifest.Kind() == ManifestKind.Permutation)
                .SelectMany(DecomposePermutation)
                .GroupBy(entry => entry.Behavior)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(combosition => combosition.TransitionCondition).ToList());
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
