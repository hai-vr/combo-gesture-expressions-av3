using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureCompiler : MonoBehaviour
    {
        public string activityStageName;
        public List<GestureComboStageMapper> comboLayers;
        public RuntimeAnimatorController animatorController;
        public RuntimeAnimatorController folderToGenerateNeutralizedAssetsIn;
        public RuntimeAnimatorController assetContainer;
        public bool generateNewContainerEveryTime;

        public AnimationClip customEmptyClip;
        public float analogBlinkingUpperThreshold = 0.95f;

        public bool exposeDisableExpressions;
        public bool exposeDisableBlinkingOverride;
        public bool exposeAreEyesClosed;

        public bool doNotGenerateControllerLayer;
        public bool doNotGenerateBlinkingOverrideLayer;

        public AvatarMask expressionsAvatarMask;
        public AvatarMask logicalAvatarMask;

        public ConflictPreventionMode conflictPreventionMode = ConflictPreventionMode.GenerateAnimationsWithWriteDefaults;
        public ConflictFxLayerMode conflictFxLayerMode = ConflictFxLayerMode.RemoveTransformsAndMuscles;

        public bool editorAdvancedFoldout;

        public AnimationClip ignoreParamList;
        public AnimationClip fallbackParamList;

        public VRCAvatarDescriptor avatarDescriptor;
        public bool bypassMandatoryAvatarDescriptor;

        public static List<BlendShapeKey> FindBlinkBlendshapes(ComboGestureCompiler that)
        {
            if (that.avatarDescriptor == null)
            {
                return new List<BlendShapeKey>();
            }

            var eyeLook = that.avatarDescriptor.customEyeLookSettings;
            if (eyeLook.eyelidsSkinnedMesh == null || eyeLook.eyelidsSkinnedMesh.sharedMesh == null)
            {
                return new List<BlendShapeKey>();
            }

            var relativePathToSkinnedMesh = ResolveRelativePath(that.avatarDescriptor.transform, eyeLook.eyelidsSkinnedMesh.transform);
            return eyeLook.eyelidsBlendshapes
                .Select(i => BlendShapeNameIfValid(i, eyeLook))
                .Where(blendShapeName => blendShapeName != null)
                .Select(blendShapeName => new BlendShapeKey(relativePathToSkinnedMesh, blendShapeName))
                .ToList();
        }

        private static string BlendShapeNameIfValid(int index, VRCAvatarDescriptor.CustomEyeLookSettings settings)
        {
            var count = settings.eyelidsSkinnedMesh.sharedMesh.blendShapeCount;
            return index >= 0 && index < count ? settings.eyelidsSkinnedMesh.sharedMesh.GetBlendShapeName(index) : null;
        }

        private static string ResolveRelativePath(Transform avatar, Transform item)
        {
            if (item.parent != avatar && item.parent != null)
            {
                return ResolveRelativePath(avatar, item.parent) + "/" + item.name;
            }

            return item.name;
        }
    }

    [System.Serializable]
    public struct GestureComboStageMapper
    {
        public ComboGestureActivity activity; // This can be null
        public int stageValue;
    }

    [System.Serializable]
    public enum ConflictPreventionMode
    {
        UseRecommendedConfiguration, OnlyWriteDefaults, GenerateAnimationsWithWriteDefaults, GenerateAnimationsWithoutWriteDefaults
    }

    [System.Serializable]
    public enum ConflictFxLayerMode
    {
        RemoveTransformsAndMuscles, KeepBoth, KeepOnlyTransformsAndMuscles
    }
}
