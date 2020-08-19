using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Reused
{
    public class ClipGenerator
    {
        private readonly AnimationClip _customEmptyClip;
        private readonly string _emptyClipPath;
        private readonly string _pluginFolderName;

        public ClipGenerator(AnimationClip customEmptyClip, string emptyClipPath, string pluginFolderName)
        {
            _customEmptyClip = customEmptyClip;
            _pluginFolderName = pluginFolderName;
            _emptyClipPath = emptyClipPath;
        }

        internal AnimationClip GetOrCreateEmptyClip()
        {
            var emptyClip = _customEmptyClip;
            if (emptyClip == null)
            {
                emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(_emptyClipPath);
            }
            if (emptyClip == null)
            {
                emptyClip = GenerateEmptyClipAsset();
            }

            return emptyClip;
        }
        
        private AnimationClip GenerateEmptyClipAsset()
        {
            var emptyClip = new Motionist(new AnimationClip())
                .NonLooping()
                .TogglesGameObjectOff("_ignored");

            if (!AssetDatabase.IsValidFolder("Assets/Hai"))
            {
                AssetDatabase.CreateFolder("Assets", "Hai");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Hai/" + _pluginFolderName))
            {
                AssetDatabase.CreateFolder("Assets/Hai", _pluginFolderName);
            }

            AssetDatabase.CreateAsset(emptyClip.Expose(), _emptyClipPath);
            return (AnimationClip)emptyClip.Expose();
        }
    }
}
