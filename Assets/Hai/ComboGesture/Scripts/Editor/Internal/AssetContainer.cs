using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class AssetContainer
    {
        private readonly AnimatorController _holder;

        private AssetContainer(AnimatorController holder)
        {
            _holder = holder;
        }

        public static AssetContainer CreateNew(string folderToCreateAssetIn)
        {
            var holder = new AnimatorController();
            var container = new AssetContainer(holder);
            AssetDatabase.CreateAsset(holder, folderToCreateAssetIn + "/GeneratedCGE__" + DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HHmmss") + ".asset");
            return container;
        }

        public static AssetContainer FromExisting(RuntimeAnimatorController existingContainer)
        {
            var assetContainer = (AnimatorController) existingContainer;
            if (assetContainer == null)
            {
                throw new ArgumentException("An asset container must not be null.");
            }
            if (assetContainer.layers.Length != 0)
            {
                throw new ArgumentException("An asset container must have 0 layers to be safely used.");
            }
            if (!AssetDatabase.Contains(assetContainer))
            {
                throw new ArgumentException("The asset container must already be an asset");
            }

            return new AssetContainer(assetContainer);
        }

        public void AddAnimation(AnimationClip animation)
        {
            AssetDatabase.AddObjectToAsset(animation, _holder);
        }

        public void AddBlendTree(BlendTree blendTree)
        {
            AssetDatabase.AddObjectToAsset(blendTree, _holder);
        }

        public void RemoveAssetsStartingWith(string prefix, Type typeOfAssets)
        {
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_holder));
            foreach (var subAsset in allSubAssets)
            {
                if (subAsset.name.StartsWith(prefix) && subAsset.GetType() == typeOfAssets)
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                }
            }
        }

        public RuntimeAnimatorController ExposeContainerAsset()
        {
            return _holder;
        }

        public static void GlobalSave()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
