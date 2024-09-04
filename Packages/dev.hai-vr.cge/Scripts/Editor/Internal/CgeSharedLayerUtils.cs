using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal static class CgeSharedLayerUtils
    {
        private const float DynamicsTransitionDuration = 0.1f;

        internal const string HaiGestureComboLeftWeightProxy = "_Hai_GestureLWProxy";
        internal const string HaiGestureComboRightWeightProxy = "_Hai_GestureRWProxy";
        internal const string HaiVirtualActivity = "_Hai_GestureVirtualActivity";
        internal const string HaiGestureComboLeftWeightSmoothing = "_Hai_GestureLWSmoothing";
        internal const string HaiGestureComboRightWeightSmoothing = "_Hai_GestureRWSmoothing";
        internal const string HaiGestureComboSmoothingFactor = "_Hai_GestureSmoothingFactor";

        public static ICgeManifest FromMapper(GestureComboStageMapper mapper, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            switch (mapper.kind)
            {
                case GestureComboStageKind.Activity:
                    return mapper.activity == null
                        ? CgeManifestFromActivity.FromNothing(fallbackWhenAnyClipIsNull) // TODO: It may be possible to create a specific manifest for that
                        : CgeManifestFromActivity.FromActivity(mapper.activity, fallbackWhenAnyClipIsNull, universalAnalogSupport, ignoreAnalogFist);
                case GestureComboStageKind.Puppet:
                    return CgeManifestFromSingle.FromPuppet(mapper.puppet);
                case GestureComboStageKind.Massive:
                    return CgeManifestFromMassiveBlend.FromMassiveBlend(mapper.massiveBlend, fallbackWhenAnyClipIsNull, universalAnalogSupport, ignoreAnalogFist);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ICgeManifest FromSimpleDynamics(ComboGestureDynamicsItem simpleDynamics, AnimationClip emptyClip, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            return ResolveSelfDynamics(simpleDynamics, emptyClip, universalAnalogSupport, ignoreAnalogFist);
        }

        public static ICgeManifest FromMassiveSimpleDynamics(ComboGestureDynamicsItem simpleDynamics, AnimationClip emptyClip, bool universalAnalogSupport, ICgeManifest zero, bool ignoreAnalogFist)
        {
            var selfDynamics = ResolveSelfDynamics(simpleDynamics, emptyClip, universalAnalogSupport, ignoreAnalogFist);
            return CgeManifestFromMassiveBlend.FromDynamics(zero, selfDynamics, simpleDynamics.ToDescriptor().parameter, DynamicsTransitionDuration);
        }

        private static ICgeManifest ResolveSelfDynamics(ComboGestureDynamicsItem simpleDynamics, AnimationClip emptyClip, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            switch (simpleDynamics.effect)
            {
                case ComboGestureDynamicsEffect.Clip:
                    return simpleDynamics.clip != null
                        ? CgeManifestFromSingle.FromAnim(simpleDynamics.clip, simpleDynamics.bothEyesClosed, DynamicsTransitionDuration)
                        : CgeManifestFromActivity.FromNothing(emptyClip);
                case ComboGestureDynamicsEffect.MoodSet:
                    return simpleDynamics.moodSet != null
                        ? FromMoodSet(simpleDynamics.moodSet, emptyClip, universalAnalogSupport, ignoreAnalogFist)
                        : CgeManifestFromActivity.FromNothing(emptyClip);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ICgeManifest FromMoodSet(ComboGestureMoodSet moodSet, AnimationClip fallbackWhenAnyClipIsNull, bool universalAnalogSupport, bool ignoreAnalogFist)
        {
            switch (moodSet)
            {
                case ComboGestureActivity activity:
                    return activity == null
                        ? CgeManifestFromActivity.FromNothing(fallbackWhenAnyClipIsNull) // TODO: It may be possible to create a specific manifest for that
                        : CgeManifestFromActivity.FromActivity(activity, fallbackWhenAnyClipIsNull, universalAnalogSupport, ignoreAnalogFist);
                case ComboGesturePuppet puppet:
                    return CgeManifestFromSingle.FromPuppet(puppet);
                case ComboGestureMassiveBlend massive:
                    return CgeManifestFromMassiveBlend.FromMassiveBlend(massive, fallbackWhenAnyClipIsNull, universalAnalogSupport, ignoreAnalogFist);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ResolveRelativePath(Transform avatar, Transform item)
        {
            if (item.parent != avatar && item.parent != null)
            {
                return ResolveRelativePath(avatar, item.parent) + "/" + item.name;
            }

            return item.name;
        }

        public static IEnumerable<Motion> FindAllReachableClipsAndBlendTrees(AnimatorController animatorController)
        {
            return ConcatStateMachines(animatorController)
                .SelectMany(machine => machine.states)
                .Select(state => state.state.motion)
                .Where(motion => motion != null)
                .SelectMany(Unwrap)
                .Distinct();
        }

        private static IEnumerable<AnimatorStateMachine> ConcatStateMachines(AnimatorController animatorController)
        {
            return animatorController.layers.Select(layer => layer.stateMachine)
                .Concat(animatorController.layers.SelectMany(layer => layer.stateMachine.stateMachines).Select(machine => machine.stateMachine));
        }

        private static IEnumerable<Motion> Unwrap(Motion motion)
        {
            var itself = new[] {motion};
            return motion is BlendTree bt ? itself.Concat(AllChildrenOf(bt)) : itself;
        }

        private static IEnumerable<Motion> AllChildrenOf(BlendTree blendTree)
        {
            return blendTree.children
                .Select(motion => motion.motion)
                .Where(motion => motion != null)
                .SelectMany(Unwrap)
                .ToList();
        }
    }
}
