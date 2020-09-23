namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class IntermediateBlinkingGroup
    {
        public bool Posing { get; }
        public bool Resting { get; }
        public IntermediateNature Nature { get; }
        public bool PosingLeft { get; }
        public bool PosingRight { get; }

        public static IntermediateBlinkingGroup NewMotion(bool posing)
        {
            return new IntermediateBlinkingGroup(posing, posing, IntermediateNature.Motion);
        }

        public static IntermediateBlinkingGroup NewBlend(bool posing, bool resting)
        {
            return new IntermediateBlinkingGroup(posing, resting, IntermediateNature.Blend);
        }

        public static IntermediateBlinkingGroup NewTripleBlend(bool posingBoth, bool resting, bool posingLeft, bool posingRight)
        {
            return new IntermediateBlinkingGroup(posingBoth, resting, IntermediateNature.TripleBlend, posingLeft, posingRight);
        }

        private IntermediateBlinkingGroup(bool posing, bool resting, IntermediateNature nature)
        {
            Posing = posing;
            Resting = resting;
            Nature = nature;
        }

        private IntermediateBlinkingGroup(bool posing, bool resting, IntermediateNature nature, bool posingLeft, bool posingRight)
        {
            Posing = posing;
            Resting = resting;
            Nature = nature;
            PosingLeft = posingLeft;
            PosingRight = posingRight;
        }

        protected bool Equals(IntermediateBlinkingGroup other)
        {
            return Equals(Posing, other.Posing) && Equals(Resting, other.Resting) && Nature == other.Nature && Equals(PosingLeft, other.PosingLeft) && Equals(PosingRight, other.PosingRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntermediateBlinkingGroup) obj);
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

