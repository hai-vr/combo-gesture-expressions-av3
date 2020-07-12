#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace Hai.ComboGesture.Scripts.Internal
{
    internal class IntermediateBlinkingCombinator
    {
        public IntermediateBlinkingCombinator(List<ActivityManifest> activityManifests)
        {
            var exhaustive = DecomposeIntoIntermediateToTransitions(activityManifests);

            var optimized = Optimize(exhaustive, activityManifests.Count);

            IntermediateToBlinking = optimized;
        }

        private static Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> Optimize(Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> exhaustive, int activityManifestsCount)
        {
            return exhaustive
                .Select(pair =>
                {
                    var conditions = pair.Value
                        .GroupBy(
                            condition => ((BlinkingCondition.ActivityBoundBlinkingCondition) condition).Combo.RawValue)
                        .SelectMany(grouping =>
                        {
                            var groupingList = grouping.ToList();
                            if (groupingList.Count != activityManifestsCount)
                            {
                                return groupingList;
                            }

                            var item = groupingList[0];
                            return new List<BlinkingCondition>
                            {
                                new BlinkingCondition.AlwaysBlinkingCondition(item.Combo, item.LayerOrdinal)
                            };
                        })
                        .ToList();

                    return new KeyValuePair<IntermediateBlinkingGroup, List<BlinkingCondition>>(pair.Key, conditions);
                })
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> DecomposeIntoIntermediateToTransitions(List<ActivityManifest> activityManifests)
        {
            return activityManifests
                .SelectMany(Decompose)
                .GroupBy(entry => entry.IntermediateBlinkingGroup)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(combosition => combosition.BlinkingCondition).ToList());
        }

        public Dictionary<IntermediateBlinkingGroup, List<BlinkingCondition>> IntermediateToBlinking { get; }

        private static List<AnimToBlinkingConditionEntry> Decompose(ActivityManifest activityManifest)
        {
            var manifest = activityManifest.Manifest;
            var stageValue = activityManifest.StageValue;

            BlinkingCondition NewTransition(int rawComboValue) => new BlinkingCondition.ActivityBoundBlinkingCondition(stageValue, new ComboValue(rawComboValue), activityManifest.LayerOrdinal);

            var manifestBlinking = manifest.Blinking;
            return new List<AnimToBlinkingConditionEntry>
            {
                CreateTransitionToMotion(NewTransition(0), manifestBlinking.Contains(manifest.Anim00())),
                CreateTransitionToPossibleBlend(NewTransition(1), manifestBlinking.Contains(manifest.Anim01()), manifestBlinking.Contains(manifest.Anim00())),
                CreateTransitionToMotion(NewTransition(2), manifestBlinking.Contains(manifest.Anim02())),
                CreateTransitionToMotion(NewTransition(3), manifestBlinking.Contains(manifest.Anim03())),
                CreateTransitionToMotion(NewTransition(4), manifestBlinking.Contains(manifest.Anim04())),
                CreateTransitionToMotion(NewTransition(5), manifestBlinking.Contains(manifest.Anim05())),
                CreateTransitionToMotion(NewTransition(6), manifestBlinking.Contains(manifest.Anim06())),
                CreateTransitionToMotion(NewTransition(7), manifestBlinking.Contains(manifest.Anim07())),
                CreateTransitionToPossibleBlend(NewTransition(11), manifestBlinking.Contains(manifest.Anim11()), manifestBlinking.Contains(manifest.Anim00())),
                CreateTransitionToPossibleBlend(NewTransition(12), manifestBlinking.Contains(manifest.Anim12()), manifestBlinking.Contains(manifest.Anim02())),
                CreateTransitionToPossibleBlend(NewTransition(13), manifestBlinking.Contains(manifest.Anim13()), manifestBlinking.Contains(manifest.Anim03())),
                CreateTransitionToPossibleBlend(NewTransition(14), manifestBlinking.Contains(manifest.Anim14()), manifestBlinking.Contains(manifest.Anim04())),
                CreateTransitionToPossibleBlend(NewTransition(15), manifestBlinking.Contains(manifest.Anim15()), manifestBlinking.Contains(manifest.Anim05())),
                CreateTransitionToPossibleBlend(NewTransition(16), manifestBlinking.Contains(manifest.Anim16()), manifestBlinking.Contains(manifest.Anim06())),
                CreateTransitionToPossibleBlend(NewTransition(17), manifestBlinking.Contains(manifest.Anim17()), manifestBlinking.Contains(manifest.Anim07())),
                CreateTransitionToMotion(NewTransition(22), manifestBlinking.Contains(manifest.Anim22())),
                CreateTransitionToMotion(NewTransition(23), manifestBlinking.Contains(manifest.Anim23())),
                CreateTransitionToMotion(NewTransition(24), manifestBlinking.Contains(manifest.Anim24())),
                CreateTransitionToMotion(NewTransition(25), manifestBlinking.Contains(manifest.Anim25())),
                CreateTransitionToMotion(NewTransition(26), manifestBlinking.Contains(manifest.Anim26())),
                CreateTransitionToMotion(NewTransition(27), manifestBlinking.Contains(manifest.Anim27())),
                CreateTransitionToMotion(NewTransition(33), manifestBlinking.Contains(manifest.Anim33())),
                CreateTransitionToMotion(NewTransition(34), manifestBlinking.Contains(manifest.Anim34())),
                CreateTransitionToMotion(NewTransition(35), manifestBlinking.Contains(manifest.Anim35())),
                CreateTransitionToMotion(NewTransition(36), manifestBlinking.Contains(manifest.Anim36())),
                CreateTransitionToMotion(NewTransition(37), manifestBlinking.Contains(manifest.Anim37())),
                CreateTransitionToMotion(NewTransition(44), manifestBlinking.Contains(manifest.Anim44())),
                CreateTransitionToMotion(NewTransition(45), manifestBlinking.Contains(manifest.Anim45())),
                CreateTransitionToMotion(NewTransition(46), manifestBlinking.Contains(manifest.Anim46())),
                CreateTransitionToMotion(NewTransition(47), manifestBlinking.Contains(manifest.Anim47())),
                CreateTransitionToMotion(NewTransition(55), manifestBlinking.Contains(manifest.Anim55())),
                CreateTransitionToMotion(NewTransition(56), manifestBlinking.Contains(manifest.Anim56())),
                CreateTransitionToMotion(NewTransition(57), manifestBlinking.Contains(manifest.Anim57())),
                CreateTransitionToMotion(NewTransition(66), manifestBlinking.Contains(manifest.Anim66())),
                CreateTransitionToMotion(NewTransition(67), manifestBlinking.Contains(manifest.Anim67())),
                CreateTransitionToMotion(NewTransition(77), manifestBlinking.Contains(manifest.Anim77())),
            };
        }

        private static AnimToBlinkingConditionEntry CreateTransitionToMotion(BlinkingCondition blinkingCondition, bool anim)
        {
            return new AnimToBlinkingConditionEntry(blinkingCondition, IntermediateBlinkingGroup.NewMotion(anim)); 
        }

        private static AnimToBlinkingConditionEntry CreateTransitionToPossibleBlend(BlinkingCondition blinkingCondition, bool posing, bool resting)
        {
            return posing == resting
                ? CreateTransitionToMotion(blinkingCondition, posing)
                : new AnimToBlinkingConditionEntry(blinkingCondition, IntermediateBlinkingGroup.NewBlend(posing, resting));
        }
    }

    internal class AnimToBlinkingConditionEntry
    {
        public BlinkingCondition BlinkingCondition { get; }
        public IntermediateBlinkingGroup IntermediateBlinkingGroup { get; }

        public AnimToBlinkingConditionEntry(BlinkingCondition blinkingCondition, IntermediateBlinkingGroup intermediateAnimationGroup)
        {
            BlinkingCondition = blinkingCondition;
            IntermediateBlinkingGroup = intermediateAnimationGroup;
        }
    }

    internal abstract class BlinkingCondition
    {
        public ComboValue Combo { get; }
        public int LayerOrdinal { get; }

        private BlinkingCondition(ComboValue combo, int layerOrdinal)
        {
            Combo = combo;
            LayerOrdinal = layerOrdinal;
        }
    
        internal class ActivityBoundBlinkingCondition : BlinkingCondition
        {
            public int StageValue { get; }

            public ActivityBoundBlinkingCondition(int stageValue, ComboValue combo,
                int layerOrdinal) : base(combo, layerOrdinal)
            {
                StageValue = stageValue;
            }
        }
        internal class AlwaysBlinkingCondition : BlinkingCondition
        {
            public AlwaysBlinkingCondition(ComboValue combo,
                int layerOrdinal) : base(combo, layerOrdinal)
            {
            }
        }
    }
}
#endif