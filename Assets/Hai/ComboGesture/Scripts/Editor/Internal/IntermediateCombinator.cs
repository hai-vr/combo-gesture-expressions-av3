using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateCombinator
    {
        public IntermediateCombinator(List<ActivityManifest> activityManifests)
        {
            var exhaustive = DecomposeIntoIntermediateToTransitions(activityManifests);

            var optimized = Optimize(exhaustive, activityManifests.Count);

            IntermediateToTransition = optimized;
        }

        readonly struct ComboAndTransitionDuration
        {
            public ComboAndTransitionDuration(int combo, float transitionDuration)
            {
                Combo = combo;
                TransitionDuration = transitionDuration;
            }

            private int Combo { get; }
            private float TransitionDuration { get; }

            public bool Equals(ComboAndTransitionDuration other)
            {
                return Combo == other.Combo && TransitionDuration.Equals(other.TransitionDuration);
            }

            public override bool Equals(object obj)
            {
                return obj is ComboAndTransitionDuration other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Combo * 397) ^ TransitionDuration.GetHashCode();
                }
            }

            public static bool operator ==(ComboAndTransitionDuration left, ComboAndTransitionDuration right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ComboAndTransitionDuration left, ComboAndTransitionDuration right)
            {
                return !left.Equals(right);
            }
        }

        private static Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> Optimize(Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> exhaustive, int activityManifestsCount)
        {
            return exhaustive
                .Select(pair =>
                {
                    var conditions = pair.Value
                        .GroupBy(condition => new ComboAndTransitionDuration(((TransitionCondition.ActivityBoundTransitionCondition) condition).Combo.RawValue, condition.TransitionDuration))
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
                                new TransitionCondition.AlwaysTransitionCondition(item.TransitionDuration, item.Combo,
                                    item.LayerOrdinal)
                            };
                        })
                        .ToList();

                    return new KeyValuePair<IntermediateAnimationGroup, List<TransitionCondition>>(pair.Key, conditions);
                })
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> DecomposeIntoIntermediateToTransitions(List<ActivityManifest> activityManifests)
        {
            return activityManifests
                .SelectMany(Decompose)
                .GroupBy(entry => entry.IntermediateAnimationGroup)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(combosition => combosition.TransitionCondition).ToList());
        }

        public Dictionary<IntermediateAnimationGroup, List<TransitionCondition>> IntermediateToTransition { get; }

        private static List<AnimToTransitionEntry> Decompose(ActivityManifest activityManifest)
        {
            var manifest = activityManifest.Manifest;
            var stageValue = activityManifest.StageValue;
            var transitionDuration = manifest.TransitionDuration;

            TransitionCondition NewTransition(int rawComboValue) => new TransitionCondition.ActivityBoundTransitionCondition(stageValue, transitionDuration, new ComboValue(rawComboValue), activityManifest.LayerOrdinal);

            return new List<AnimToTransitionEntry>
            {
                CreateTransitionToMotion(NewTransition(0), manifest.Anim00()),
                CreateTransitionToPossibleBlend(NewTransition(1), manifest.Anim01(), manifest.Anim00()),
                CreateTransitionToMotion(NewTransition(2), manifest.Anim02()),
                CreateTransitionToMotion(NewTransition(3), manifest.Anim03()),
                CreateTransitionToMotion(NewTransition(4), manifest.Anim04()),
                CreateTransitionToMotion(NewTransition(5), manifest.Anim05()),
                CreateTransitionToMotion(NewTransition(6), manifest.Anim06()),
                CreateTransitionToMotion(NewTransition(7), manifest.Anim07()),
                CreateTransitionToPossibleTripleBlend(NewTransition(11), manifest.Anim11(), manifest.Anim00(), manifest.Anim11_L(), manifest.Anim11_R()),
                CreateTransitionToPossibleBlend(NewTransition(12), manifest.Anim12(), manifest.Anim02()),
                CreateTransitionToPossibleBlend(NewTransition(13), manifest.Anim13(), manifest.Anim03()),
                CreateTransitionToPossibleBlend(NewTransition(14), manifest.Anim14(), manifest.Anim04()),
                CreateTransitionToPossibleBlend(NewTransition(15), manifest.Anim15(), manifest.Anim05()),
                CreateTransitionToPossibleBlend(NewTransition(16), manifest.Anim16(), manifest.Anim06()),
                CreateTransitionToPossibleBlend(NewTransition(17), manifest.Anim17(), manifest.Anim07()),
                CreateTransitionToMotion(NewTransition(22), manifest.Anim22()),
                CreateTransitionToMotion(NewTransition(23), manifest.Anim23()),
                CreateTransitionToMotion(NewTransition(24), manifest.Anim24()),
                CreateTransitionToMotion(NewTransition(25), manifest.Anim25()),
                CreateTransitionToMotion(NewTransition(26), manifest.Anim26()),
                CreateTransitionToMotion(NewTransition(27), manifest.Anim27()),
                CreateTransitionToMotion(NewTransition(33), manifest.Anim33()),
                CreateTransitionToMotion(NewTransition(34), manifest.Anim34()),
                CreateTransitionToMotion(NewTransition(35), manifest.Anim35()),
                CreateTransitionToMotion(NewTransition(36), manifest.Anim36()),
                CreateTransitionToMotion(NewTransition(37), manifest.Anim37()),
                CreateTransitionToMotion(NewTransition(44), manifest.Anim44()),
                CreateTransitionToMotion(NewTransition(45), manifest.Anim45()),
                CreateTransitionToMotion(NewTransition(46), manifest.Anim46()),
                CreateTransitionToMotion(NewTransition(47), manifest.Anim47()),
                CreateTransitionToMotion(NewTransition(55), manifest.Anim55()),
                CreateTransitionToMotion(NewTransition(56), manifest.Anim56()),
                CreateTransitionToMotion(NewTransition(57), manifest.Anim57()),
                CreateTransitionToMotion(NewTransition(66), manifest.Anim66()),
                CreateTransitionToMotion(NewTransition(67), manifest.Anim67()),
                CreateTransitionToMotion(NewTransition(77), manifest.Anim77())
            };
        }

        private static AnimToTransitionEntry CreateTransitionToMotion(TransitionCondition transitionCondition, AnimationClip anim)
        {
            return new AnimToTransitionEntry(transitionCondition, IntermediateAnimationGroup.NewMotion(anim));
        }

        private static AnimToTransitionEntry CreateTransitionToPossibleBlend(TransitionCondition transitionCondition, AnimationClip posing, AnimationClip resting)
        {
            return posing == resting
                ? CreateTransitionToMotion(transitionCondition, posing)
                : new AnimToTransitionEntry(transitionCondition, IntermediateAnimationGroup.NewBlend(posing, resting));
        }

        private static AnimToTransitionEntry CreateTransitionToPossibleTripleBlend(TransitionCondition transitionCondition, AnimationClip posingBoth, AnimationClip resting, AnimationClip posingLeft, AnimationClip posingRight)
        {
            return posingBoth == resting && posingLeft == posingBoth && posingRight == posingBoth
                ? CreateTransitionToMotion(transitionCondition, posingBoth)
                : new AnimToTransitionEntry(transitionCondition, IntermediateAnimationGroup.NewTripleBlend(posingBoth, resting, posingLeft, posingRight));
        }
    }

    internal class AnimToTransitionEntry
    {
        public TransitionCondition TransitionCondition { get; }
        public IntermediateAnimationGroup IntermediateAnimationGroup { get; }

        public AnimToTransitionEntry(TransitionCondition transitionCondition, IntermediateAnimationGroup intermediateAnimationGroup)
        {
            TransitionCondition = transitionCondition;
            IntermediateAnimationGroup = intermediateAnimationGroup;
        }
    }

    internal abstract class TransitionCondition
    {
        public float TransitionDuration { get; }
        public ComboValue Combo { get; }
        public int LayerOrdinal { get; }

        private TransitionCondition(float transitionDuration, ComboValue combo, int layerOrdinal)
        {
            TransitionDuration = transitionDuration;
            Combo = combo;
            LayerOrdinal = layerOrdinal;
        }

        internal class ActivityBoundTransitionCondition : TransitionCondition
        {
            public int StageValue { get; }

            public ActivityBoundTransitionCondition(int stageValue, float transitionDuration, ComboValue combo,
                int layerOrdinal) : base(transitionDuration, combo, layerOrdinal)
            {
                StageValue = stageValue;
            }
        }
        internal class AlwaysTransitionCondition : TransitionCondition
        {
            public AlwaysTransitionCondition(float transitionDuration, ComboValue combo,
                int layerOrdinal) : base(transitionDuration, combo, layerOrdinal)
            {
            }
        }
    }
}
