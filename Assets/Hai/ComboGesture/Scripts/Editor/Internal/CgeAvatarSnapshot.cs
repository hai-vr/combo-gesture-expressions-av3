using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class CgeAvatarSnapshot
    {
        private readonly VRCAvatarDescriptor _vrcAvatarDescriptor;
        private readonly AnimationClip _compilerFallbackParamList;

        public CgeAvatarSnapshot(VRCAvatarDescriptor avatarDescriptor, AnimationClip compilerFallbackParamList)
        {
            _vrcAvatarDescriptor = avatarDescriptor;
            _compilerFallbackParamList = compilerFallbackParamList;
        }

        public AnimationClip CaptureFallbacks()
        {
            if (_vrcAvatarDescriptor == null)
            {
                return _compilerFallbackParamList == null ? null : _compilerFallbackParamList;
            }

            var that = _vrcAvatarDescriptor.transform;

            var snapshot = new AnimationClip();
            MutateAnimationToSnapshotAllChildrenOf(snapshot, that, "");
            if (_compilerFallbackParamList != null)
            {
                MutateAnimationToOverlayCompilerFallbacks(snapshot);
            }

            return snapshot;
        }

        public AvatarMask MaybeCaptureMask()
        {
            if (_vrcAvatarDescriptor == null)
            {
                return null;
            }

            var root = _vrcAvatarDescriptor.transform;

            var mask = CreateMaskDisablingAllHierarchyOf(root);

            AssetDatabase.CreateAsset(mask, "Assets/TempMask_more2.asset");

            return mask;
        }

        private static AvatarMask CreateMaskDisablingAllHierarchyOf(Transform root)
        {
            var mask = new AvatarMask();

            foreach (AvatarMaskBodyPart part in Enum.GetValues(typeof(AvatarMaskBodyPart)))
            {
                mask.SetHumanoidBodyPartActive(part, false);
            }


            mask.AddTransformPath(root, true);
            for (var index = 0; index < mask.transformCount; index++)
            {
                mask.SetTransformActive(index, false);
            }

            return mask;
        }

        private static string ChildPath(string activePath, Transform child)
        {
            return (activePath == "" ? "" : activePath + "/") + child.name;
        }

        private static void MutateAnimationToSnapshotAllChildrenOf(AnimationClip mutatedClip, Transform thatObject, string hierarchyPath)
        {
            var position = thatObject.localPosition;
            var euler = TransformUtils.GetInspectorRotation(thatObject);
            var scale = thatObject.localScale;
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.x", new AnimationCurve(new Keyframe(0, position.x)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.y", new AnimationCurve(new Keyframe(0, position.y)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.z", new AnimationCurve(new Keyframe(0, position.z)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "localEulerAnglesRaw.x", new AnimationCurve(new Keyframe(0, euler.x)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "localEulerAnglesRaw.y", new AnimationCurve(new Keyframe(0, euler.y)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "localEulerAnglesRaw.z", new AnimationCurve(new Keyframe(0, euler.z)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalScale.x", new AnimationCurve(new Keyframe(0, scale.x)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalScale.y", new AnimationCurve(new Keyframe(0, scale.y)));
            mutatedClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalScale.z", new AnimationCurve(new Keyframe(0, scale.z)));

            var isRootNode = hierarchyPath == "";
            if (!isRootNode)
            {
                mutatedClip.SetCurve(hierarchyPath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, thatObject.gameObject.activeSelf ? 1 : 0)));
            }

            foreach (Transform child in thatObject)
            {
                MutateAnimationToSnapshotAllChildrenOf(mutatedClip, child, ChildPath(hierarchyPath, child));
            }
        }

        private void MutateAnimationToOverlayCompilerFallbacks(AnimationClip snapshot)
        {
            var fallbackBindings = AnimationUtility.GetCurveBindings(_compilerFallbackParamList);
            foreach (var binding in fallbackBindings)
            {
                AnimationUtility.SetEditorCurve(snapshot, binding, AnimationUtility.GetEditorCurve(_compilerFallbackParamList, binding));
            }
        }
    }
}
