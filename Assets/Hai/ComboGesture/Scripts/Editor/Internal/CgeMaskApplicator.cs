using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeMaskApplicator
    {
        private readonly AvatarMask _mask;
        private readonly AnimatorController _fxController;

        public CgeMaskApplicator(RuntimeAnimatorController fxController, AvatarMask mask)
        {
            _mask = mask;
            _fxController = (AnimatorController)fxController;
        }

        public static IEnumerable<AnimatorControllerLayer> FindAllLayersMissingAMask(RuntimeAnimatorController ctrl)
        {
            var animatorController = ctrl as AnimatorController;
            if (animatorController == null) return new List<AnimatorControllerLayer>();

            return animatorController.layers
                .Skip(1)
                .Where(layer => !layer.name.StartsWith("Hai_Gesture"))
                .Where(layer => layer.avatarMask == null)
                .Where(layer => FindAllStates(layer).Any(state => state.writeDefaultValues == false));
        }

        private static IEnumerable<AnimatorState> FindAllStates(AnimatorControllerLayer layer)
        {
            return layer.stateMachine.states.Concat(layer.stateMachine.stateMachines.SelectMany(AllChildStates)).Select(state => state.state);
        }

        private static IEnumerable<ChildAnimatorState> AllChildStates(ChildAnimatorStateMachine casm)
        {
            return casm.stateMachine.states.Concat(casm.stateMachine.stateMachines.SelectMany(AllChildStates));
        }

        public void AddMissingMasks()
        {
            var layerNamesMissingAMask = FindAllLayersMissingAMask(_fxController).Select(layer => layer.name).ToList();

            _fxController.layers = _fxController.layers
                .Select(layer =>
                {
                    if (layer.avatarMask == null && layerNamesMissingAMask.Contains(layer.name))
                    {
                        layer.avatarMask = _mask;
                    }
                    return layer;
                })
                .ToArray();

            AssetDatabase.Refresh();
        }

        public void RemoveAppliedMask()
        {
            _fxController.layers = _fxController.layers
                .Select(layer =>
                {
                    if (layer.avatarMask == _mask)
                    {
                        layer.avatarMask = null;
                    }
                    return layer;
                })
                .ToArray();

            AssetDatabase.Refresh();
        }

        public void UpdateMask()
        {
            foreach (AvatarMaskBodyPart part in Enum.GetValues(typeof(AvatarMaskBodyPart)))
            {
                if (part == AvatarMaskBodyPart.LastBodyPart) continue;
                _mask.SetHumanoidBodyPartActive(part, false);
            }

            var potentiallyAnimatedPaths = FindPotentiallyAnimatedPaths();

            if (potentiallyAnimatedPaths.Count == 0)
            {
                MutateMaskToContainAtLeastOneTransform();
            }
            else
            {
                MutateMaskToAllowNonEmptyAnimatedPaths(potentiallyAnimatedPaths);
            }

            AssetDatabase.Refresh();
        }

        private List<string> FindPotentiallyAnimatedPaths()
        {
            return SharedLayerUtils.FindAllReachableClipsAndBlendTrees(_fxController)
                .OfType<AnimationClip>()
                .SelectMany(clip =>
                {
                    var materialSwaps = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    var transforms = AnimationUtility.GetCurveBindings(clip)
                        .Where(binding => binding.type == typeof(Transform));
                    return transforms.Concat(materialSwaps)
                        .Select(binding => binding.path);
                })
                .Distinct()
                .ToList();
        }

        private void MutateMaskToContainAtLeastOneTransform()
        {
            _mask.transformCount = 1;
            _mask.SetTransformActive(0, false);
            _mask.SetTransformPath(0, "_ignored");
        }

        private void MutateMaskToAllowNonEmptyAnimatedPaths(List<string> potentiallyAnimatedPaths)
        {
            _mask.transformCount = potentiallyAnimatedPaths.Count;
            for (var index = 0; index < potentiallyAnimatedPaths.Count; index++)
            {
                var path = potentiallyAnimatedPaths[index];
                _mask.SetTransformPath(index, path);
                _mask.SetTransformActive(index, true);
            }
        }
    }
}
