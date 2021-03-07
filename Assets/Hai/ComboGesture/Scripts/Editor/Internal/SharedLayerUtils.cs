using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using Hai.ComboGesture.Scripts.Editor.Internal.Processing;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class SharedLayerUtils
    {
        internal const string FxPlayableLayerAvatarMaskPath = "Assets/Hai/ComboGesture/Hai_ComboGesture_FX_HideTransformsAndMuscles.mask";

        internal static void SetupDefaultTransition(AnimatorStateTransition transition)
        {
            SetupCommonTransition(transition);

            transition.duration = 0.1f; // There seems to be a quirk if the duration is 0 when using DisableExpressions, so use 0.1f instead
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

        internal static void SetupCommonTransition(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.offset = 0;
            transition.interruptionSource = TransitionInterruptionSource.None;
        }

        internal static Vector3 GridPosition(int x, int y)
        {
            return new Vector3(x * 200 , y * 70, 0);
        }

        internal const string GestureLeft = "GestureLeft";
        internal const string GestureRight = "GestureRight";
        internal const string HaiGestureComboParamName = "_Hai_GestureComboValue";
        internal const string HaiGestureComboIsLipsyncLimitedParamName = "_Hai_GestureComboIsLipsyncLimited";
        internal const string HaiGestureComboDisableLipsyncOverrideParamName = "_Hai_GestureComboDisableLipsyncOverride";
        internal const string HaiGestureComboLeftWeightProxy = "_Hai_GestureLWProxy";
        internal const string HaiGestureComboRightWeightProxy = "_Hai_GestureRWProxy";
        internal const string HaiVirtualActivity = "_Hai_GestureVirtualActivity";
        internal const string HaiGestureComboLeftWeightSmoothing = "_Hai_GestureLWSmoothing";
        internal const string HaiGestureComboRightWeightSmoothing = "_Hai_GestureRWSmoothing";
        internal const string HaiGestureComboSmoothingFactor = "_Hai_GestureSmoothingFactor";

        public static IManifest FromMapper(GestureComboStageMapper mapper, AnimationClip fallbackWhenAnyClipIsNull)
        {
            switch (mapper.kind)
            {
                case GestureComboStageKind.Activity:
                    return mapper.activity == null
                        ? ManifestFromActivity.FromNothing(fallbackWhenAnyClipIsNull) // TODO: It may be possible to create a specific manifest for that
                        : ManifestFromActivity.FromActivity(mapper.activity, fallbackWhenAnyClipIsNull);
                case GestureComboStageKind.Puppet:
                    return ManifestFromPuppet.FromPuppet(mapper.puppet);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void CreateParamIfNotExists(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
        {
            if (controller.parameters.FirstOrDefault(param => param.name == paramName) == null)
            {
                controller.AddParameter(paramName, type);
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
