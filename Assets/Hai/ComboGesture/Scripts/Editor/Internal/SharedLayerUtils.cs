using System;
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
        internal static void SetupImmediateTransition(AnimatorStateTransition transition)
        {
            SetupCommonTransition(transition);

            transition.duration = 0;
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

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
    }
}
