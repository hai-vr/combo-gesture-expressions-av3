using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
        /**
         * A qualified animation is an animation clip with expression metadata: Are eyes blinking? What shape is the mouth?
         * A single animation file may be qualified differently in different places.
         * Usually, the animation file qualification is consistent within a single Manifest, and may vary across other Manifests.
         */
        public readonly struct QualifiedAnimation
        {
            public QualifiedAnimation(AnimationClip clip, Qualification qualification)
            {
                Clip = clip;
                Qualification = qualification;
            }

            public AnimationClip Clip { get; }
            public Qualification Qualification { get; }

            public bool Equals(QualifiedAnimation other)
            {
                return Equals(Clip, other.Clip) && Qualification.Equals(other.Qualification);
            }

            public override bool Equals(object obj)
            {
                return obj is QualifiedAnimation other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Clip != null ? Clip.GetHashCode() : 0) * 397) ^ Qualification.GetHashCode();
                }
            }

            public static bool operator ==(QualifiedAnimation left, QualifiedAnimation right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(QualifiedAnimation left, QualifiedAnimation right)
            {
                return !left.Equals(right);
            }

            public QualifiedAnimation NewInstanceWithClip(AnimationClip clip)
            {
                return new QualifiedAnimation(clip, Qualification);
            }

            public override string ToString()
            {
                return $"{nameof(Clip)}: {Clip}, {nameof(Qualification)}: {Qualification}";
            }
        }

        public readonly struct Qualification
        {
            public Qualification(bool isBlinking, QualifiedLimitation limitation)
            {
                IsBlinking = isBlinking;
                Limitation = limitation;
            }

            public bool IsBlinking { get; }
            public QualifiedLimitation Limitation { get; }

            public bool Equals(Qualification other)
            {
                return IsBlinking == other.IsBlinking && Limitation == other.Limitation;
            }

            public override bool Equals(object obj)
            {
                return obj is Qualification other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (IsBlinking.GetHashCode() * 397) ^ (int) Limitation;
                }
            }

            public static bool operator ==(Qualification left, Qualification right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Qualification left, Qualification right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"{nameof(IsBlinking)}: {IsBlinking}, {nameof(Limitation)}: {Limitation}";
            }
        }

        public enum QualifiedLimitation
        {
            None, Wide
        }

}
