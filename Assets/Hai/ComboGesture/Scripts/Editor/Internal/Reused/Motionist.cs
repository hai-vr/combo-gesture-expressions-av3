using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Reused
{
    internal interface IAssetist
    {
        Object AsAsset();
    }

    internal class Motionist : IAssetist
    {
        private readonly AnimationClip _animationClip;

        public static Motionist FromScratch()
        {
            return new Motionist(new AnimationClip())
                .NonLooping();
        }

        public Motionist(AnimationClip animationClip)
        {
            _animationClip = animationClip;
        }

        public Motionist WithName(string name)
        {
            _animationClip.name = name;
            return this;
        }

        public Motionist NonLooping()
        {
            var settings = AnimationUtility.GetAnimationClipSettings(_animationClip);
            settings.loopTime = false;

            return this;
        }

        public Motionist Looping()
        {
            var settings = AnimationUtility.GetAnimationClipSettings(_animationClip);
            settings.loopTime = true;

            return this;
        }

        public Motionist TogglesGameObjectOff(string relativePath)
        {
            _animationClip.SetCurve( relativePath, typeof(GameObject), "m_IsActive", TwoKeyframesWithValue(0));
            return this;
        }

        public Motionist TogglesGameObjectOn(string relativePath)
        {
            _animationClip.SetCurve( relativePath, typeof(GameObject), "m_IsActive", TwoKeyframesWithValue(1));
            return this;
        }

        private static AnimationCurve TwoKeyframesWithValue(float value)
        {
            return new AnimationCurve(new Keyframe(0, value), new Keyframe(1 / 60f, value));
        }

        public Object AsAsset()
        {
            return _animationClip;
        }

        public Motion Expose()
        {
            return _animationClip;
        }
    }
}
