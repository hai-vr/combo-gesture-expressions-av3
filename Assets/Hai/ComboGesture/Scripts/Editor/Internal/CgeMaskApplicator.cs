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

        public void AddMissingMasks()
        {
            _fxController.layers = _fxController.layers
                .Select(layer =>
                {
                    if (layer.avatarMask == null)
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

        public void UpdateMask(Transform avatar)
        {
            foreach (AvatarMaskBodyPart part in Enum.GetValues(typeof(AvatarMaskBodyPart)))
            {
                if (part == AvatarMaskBodyPart.LastBodyPart) continue;
                _mask.SetHumanoidBodyPartActive(part, false);
            }

            var potentiallyAnimatedPaths = FindPotentiallyAnimatedPaths(avatar);

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

        private static List<string> FindPotentiallyAnimatedPaths(Transform avatar)
        {
            var materialSwappableComponents = avatar.GetComponentsInChildren<MeshRenderer>(true)
                .Concat<Component>(avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true));

            var potentiallyAnimatedPaths = materialSwappableComponents
                .Select(component => component.transform)
                .Select(transform => SharedLayerUtils.ResolveRelativePath(avatar, transform))
                .Distinct()
                .ToList();
            return potentiallyAnimatedPaths;
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
