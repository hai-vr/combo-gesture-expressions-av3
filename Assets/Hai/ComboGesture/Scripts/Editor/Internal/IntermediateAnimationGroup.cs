using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateAnimationGroup
    {
        public AnimationClip Posing { get; }
        public AnimationClip Resting { get; }
        public IntermediateNature Nature { get; }

        public static IntermediateAnimationGroup NewMotion(AnimationClip posing)
        {
            return new IntermediateAnimationGroup(posing, null, IntermediateNature.Motion);
        }

        public static IntermediateAnimationGroup NewBlend(AnimationClip posing, AnimationClip resting)
        {
            return new IntermediateAnimationGroup(posing, resting, IntermediateNature.Blend);
        }

        private IntermediateAnimationGroup(AnimationClip posing, AnimationClip resting, IntermediateNature nature)
        {
            Posing = posing;
            Resting = resting;
            Nature = nature;
        }

        private bool Equals(IntermediateAnimationGroup other)
        {
            return Equals(Posing, other.Posing) && Equals(Resting, other.Resting) && Nature == other.Nature;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IntermediateAnimationGroup) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Posing != null ? Posing.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Resting != null ? Resting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Nature;
                return hashCode;
            }
        }
    }
}

