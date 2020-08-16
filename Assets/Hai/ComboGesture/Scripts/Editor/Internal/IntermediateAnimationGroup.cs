using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateAnimationGroup
    {
        public AnimationClip Posing { get; }
        public AnimationClip Resting { get; }
        public IntermediateNature Nature { get; }
        public AnimationClip PosingLeft { get; }
        public AnimationClip PosingRight { get; }

        public static IntermediateAnimationGroup NewMotion(AnimationClip posing)
        {
            return new IntermediateAnimationGroup(posing, null, IntermediateNature.Motion);
        }

        public static IntermediateAnimationGroup NewBlend(AnimationClip posing, AnimationClip resting)
        {
            return new IntermediateAnimationGroup(posing, resting, IntermediateNature.Blend);
        }

        public static IntermediateAnimationGroup NewTripleBlend(AnimationClip posingBoth, AnimationClip resting, AnimationClip posingLeft, AnimationClip posingRight)
        {
            return new IntermediateAnimationGroup(posingBoth, resting, IntermediateNature.TripleBlend, posingLeft, posingRight);
        }

        private IntermediateAnimationGroup(AnimationClip posing, AnimationClip resting, IntermediateNature nature)
        {
            Posing = posing;
            Resting = resting;
            Nature = nature;
        }

        private IntermediateAnimationGroup(AnimationClip posing, AnimationClip resting, IntermediateNature nature, AnimationClip posingLeft, AnimationClip posingRight)
        {
            Posing = posing;
            Resting = resting;
            Nature = nature;
            PosingLeft = posingLeft;
            PosingRight = posingRight;
        }

        protected bool Equals(IntermediateAnimationGroup other)
        {
            return Equals(Posing, other.Posing) && Equals(Resting, other.Resting) && Nature == other.Nature && Equals(PosingLeft, other.PosingLeft) && Equals(PosingRight, other.PosingRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntermediateAnimationGroup) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Posing != null ? Posing.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Resting != null ? Resting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Nature;
                hashCode = (hashCode * 397) ^ (PosingLeft != null ? PosingLeft.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PosingRight != null ? PosingRight.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

