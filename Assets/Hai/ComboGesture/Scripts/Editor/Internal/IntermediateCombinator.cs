using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateCombinator
    {
        public Dictionary<IAnimatedBehavior, List<TransitionCondition>> IntermediateToTransition { get; }

        public IntermediateCombinator(List<ActivityManifest> activityManifests)
        {
            var exhaustive = DecomposeIntoIntermediateToTransitions(activityManifests);

            var optimized = Optimize(exhaustive, activityManifests.Count);

            IntermediateToTransition = optimized;
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

        private static Dictionary<IAnimatedBehavior, List<TransitionCondition>> Optimize(Dictionary<IAnimatedBehavior, List<TransitionCondition>> exhaustive, int activityManifestsCount)
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

        private static Dictionary<IAnimatedBehavior, List<TransitionCondition>> DecomposeIntoIntermediateToTransitions(List<ActivityManifest> activityManifests)
        {
            return activityManifests
                .SelectMany(Decompose)
                .GroupBy(entry => entry.IntermediateAnimationGroup)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(combosition => combosition.TransitionCondition).ToList());
        }

        private static List<AnimToTransitionEntry> Decompose(ActivityManifest activityManifest)
        {
            return activityManifest.Manifest.Poses
                .Select(pair => new AnimToTransitionEntry(
                    new TransitionCondition.ActivityBoundTransitionCondition(activityManifest.StageValue, activityManifest.Manifest.TransitionDuration, pair.Key, activityManifest.LayerOrdinal),
                    pair.Value
                ))
                .ToList();
        }
    }

    internal class AnimToTransitionEntry
    {
        public TransitionCondition TransitionCondition { get; }
        public IAnimatedBehavior IntermediateAnimationGroup { get; }

        public AnimToTransitionEntry(TransitionCondition transitionCondition, IAnimatedBehavior intermediateAnimationGroup)
        {
            TransitionCondition = transitionCondition;
            IntermediateAnimationGroup = intermediateAnimationGroup;
        }
    }

    internal abstract class TransitionCondition
    {
        public float TransitionDuration { get; }
        public Permutation Permutation { get; }
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
        internal class AlwaysTransitionCondition : TransitionCondition
        {
            public AlwaysTransitionCondition(float transitionDuration, Permutation permutation,
                int layerOrdinal) : base(transitionDuration, permutation, layerOrdinal)
            {
            }
        }
    }
}
