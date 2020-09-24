using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class VisemeAnimationMaker
    {
        private readonly VRCAvatarDescriptor _avatar;

        public VisemeAnimationMaker(VRCAvatarDescriptor avatar)
        {
            _avatar = avatar;
        }

        public void OverrideAnimation(AnimationClip animationToMutate, int visemeNumber, float amplitude)
        {
            var faceMesh = _avatar.VisemeSkinnedMesh;
            var path = ResolveRelativePath(_avatar.transform, faceMesh.transform);
            for (var currentViseme = 0; currentViseme < 15; currentViseme++)
            {
                var binding = EditorCurveBinding.FloatCurve(
                    path,
                    typeof(SkinnedMeshRenderer),
                    "blendShape." + _avatar.VisemeBlendShapes[currentViseme]
                );

                var currentAmplitude = currentViseme == visemeNumber ? amplitude * 100 : 0;
                AnimationUtility.SetEditorCurve(animationToMutate, binding, new AnimationCurve(
                    new Keyframe(0, currentAmplitude),
                    new Keyframe(1 / 60f, currentAmplitude)
                ));
            }
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
}
