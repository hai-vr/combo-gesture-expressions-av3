using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeMaskApplicator
    {
        private readonly AnimatorController _fxController;

        public CgeMaskApplicator(RuntimeAnimatorController fxController)
        {
            _fxController = (AnimatorController)fxController;
        }

        public void AddMask()
        {
            var fxMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedLayerUtils.FxPlayableLayerAvatarMaskAutomaticPath);

            _fxController.layers = _fxController.layers
                .Select(layer =>
                {
                    if (layer.avatarMask == null)
                    {
                        layer.avatarMask = fxMask;
                    }
                    return layer;
                })
                .ToArray();

            AssetDatabase.SaveAssets();
        }

        public void RemoveMask()
        {
            var fxMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedLayerUtils.FxPlayableLayerAvatarMaskAutomaticPath);

            _fxController.layers = _fxController.layers
                .Select(layer =>
                {
                    if (layer.avatarMask == fxMask)
                    {
                        layer.avatarMask = null;
                    }
                    return layer;
                })
                .ToArray();

            AssetDatabase.SaveAssets();
        }
    }
}
