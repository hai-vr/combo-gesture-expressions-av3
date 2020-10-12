using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
    public class PermutationManifest
    {
        public ReadOnlyDictionary<Permutation, IAnimatedBehavior> Poses { get; }
        public float TransitionDuration { get; }

        public PermutationManifest(Dictionary<Permutation, IAnimatedBehavior> poses, float transitionDuration)
        {
            Poses = new ReadOnlyDictionary<Permutation, IAnimatedBehavior>(poses);
            TransitionDuration = transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Poses.Values.Any(behavior => behavior.QualifiedAnimations().Any(animation => animation.Qualification.IsBlinking));
        }

        public bool RequiresLimitedLipsync()
        {
            return Poses.Values.Any(behavior => behavior.QualifiedAnimations().Any(animation => animation.Qualification.Limitation != QualifiedLimitation.None));
        }

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return Poses.Values.SelectMany(behavior => behavior.QualifiedAnimations()).ToList();
        }

        public PermutationManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping)
        {
            var newPoses = new Dictionary<Permutation, IAnimatedBehavior>(Poses);
            foreach (var permutation in newPoses.Keys.ToList())
            {
                newPoses[permutation] = newPoses[permutation].Remapping(remapping);
            }

            return new PermutationManifest(newPoses, TransitionDuration);
        }
    }

    public interface IAnimatedBehavior
    {
        AnimatedBehaviorNature Nature();
        IEnumerable<QualifiedAnimation> QualifiedAnimations();
        IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping);
    }

    public enum AnimatedBehaviorNature
    {
        Single,
        Analog,
        DualAnalog
    }

    class SingleAnimatedBehavior : IAnimatedBehavior
    {
        public QualifiedAnimation Posing { get; }

        private SingleAnimatedBehavior(QualifiedAnimation posing)
        {
            Posing = posing;
        }

        public static IAnimatedBehavior Of(QualifiedAnimation posing)
        {
            return new SingleAnimatedBehavior(posing);
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.Single;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Posing};
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping)
        {
            return Of(Remap(remapping, Posing));
        }

        internal static QualifiedAnimation Remap(Dictionary<QualifiedAnimation, AnimationClip> remapping, QualifiedAnimation key)
        {
            return remapping.ContainsKey(key) ? key.NewInstanceWithClip(remapping[key]) : key;
        }

        protected bool Equals(SingleAnimatedBehavior other)
        {
            return Posing.Equals(other.Posing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SingleAnimatedBehavior) obj);
        }

        public override int GetHashCode()
        {
            return Posing.GetHashCode();
        }

        public static bool operator ==(SingleAnimatedBehavior left, SingleAnimatedBehavior right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleAnimatedBehavior left, SingleAnimatedBehavior right)
        {
            return !Equals(left, right);
        }
    }

    class AnalogAnimatedBehavior : IAnimatedBehavior
    {
        public QualifiedAnimation Resting { get; }
        public QualifiedAnimation Squeezing { get; }

        private AnalogAnimatedBehavior(QualifiedAnimation resting, QualifiedAnimation squeezing)
        {
            if (resting == squeezing)
            {
                throw new ArgumentException("AnalogAnimatedBehavior must not have both identical qualified animations");
            }

            Resting = resting;
            Squeezing = squeezing;
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.Analog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Resting, Squeezing};
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping)
        {
            return Maybe(
                SingleAnimatedBehavior.Remap(remapping, Resting),
                SingleAnimatedBehavior.Remap(remapping, Squeezing)
            );
        }

        public static AnalogAnimatedBehavior Of(QualifiedAnimation resting, QualifiedAnimation squeezing)
        {
            return new AnalogAnimatedBehavior(resting, squeezing);
        }

        public static IAnimatedBehavior Maybe(QualifiedAnimation resting, QualifiedAnimation squeezing)
        {
            if (resting == squeezing)
            {
                return SingleAnimatedBehavior.Of(squeezing);
            }

            return AnalogAnimatedBehavior.Of(resting, squeezing);
        }

        protected bool Equals(AnalogAnimatedBehavior other)
        {
            return Resting.Equals(other.Resting) && Squeezing.Equals(other.Squeezing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnalogAnimatedBehavior) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Resting.GetHashCode() * 397) ^ Squeezing.GetHashCode();
            }
        }

        public static bool operator ==(AnalogAnimatedBehavior left, AnalogAnimatedBehavior right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AnalogAnimatedBehavior left, AnalogAnimatedBehavior right)
        {
            return !Equals(left, right);
        }
    }

    class DualAnalogAnimatedBehavior : IAnimatedBehavior
    {
        public QualifiedAnimation Resting { get; }
        public QualifiedAnimation LeftSqueezing { get; }
        public QualifiedAnimation RightSqueezing { get; }
        public QualifiedAnimation BothSqueezing { get; }

        private DualAnalogAnimatedBehavior(QualifiedAnimation resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            if (AreAllIdentical(resting, leftSqueezing, rightSqueezing, bothSqueezing))
            {
                throw new ArgumentException("DualAnalogAnimatedBehavior must not have all identical qualified animations");
            }

            Resting = resting;
            LeftSqueezing = leftSqueezing;
            RightSqueezing = rightSqueezing;
            BothSqueezing = bothSqueezing;
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.DualAnalog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Resting, LeftSqueezing, RightSqueezing, BothSqueezing};
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping)
        {
            return Maybe(
                SingleAnimatedBehavior.Remap(remapping, Resting),
                SingleAnimatedBehavior.Remap(remapping, LeftSqueezing),
                SingleAnimatedBehavior.Remap(remapping, RightSqueezing),
                SingleAnimatedBehavior.Remap(remapping, BothSqueezing)
            );
        }

        public static DualAnalogAnimatedBehavior Of(QualifiedAnimation resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            return new DualAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing);
        }

        public static IAnimatedBehavior Maybe(QualifiedAnimation resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            if (AreAllIdentical(resting, leftSqueezing, rightSqueezing, bothSqueezing))
            {
                return SingleAnimatedBehavior.Of(bothSqueezing);
            }

            return Of(resting, leftSqueezing, rightSqueezing, bothSqueezing);
        }

        private static bool AreAllIdentical(QualifiedAnimation resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            return resting == bothSqueezing && leftSqueezing == bothSqueezing && rightSqueezing == bothSqueezing;
        }

        protected bool Equals(DualAnalogAnimatedBehavior other)
        {
            return Resting.Equals(other.Resting) && LeftSqueezing.Equals(other.LeftSqueezing) && RightSqueezing.Equals(other.RightSqueezing) && BothSqueezing.Equals(other.BothSqueezing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DualAnalogAnimatedBehavior) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Resting.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftSqueezing.GetHashCode();
                hashCode = (hashCode * 397) ^ RightSqueezing.GetHashCode();
                hashCode = (hashCode * 397) ^ BothSqueezing.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DualAnalogAnimatedBehavior left, DualAnalogAnimatedBehavior right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DualAnalogAnimatedBehavior left, DualAnalogAnimatedBehavior right)
        {
            return !Equals(left, right);
        }
    }

    public class Permutation
    {
        public HandPose Left { get; }
        public HandPose Right { get; }

        private Permutation(HandPose left, HandPose right)
        {
            Left = left;
            Right = right;
        }

        public static Permutation LeftRight(HandPose left, HandPose right)
        {
            return new Permutation(left, right);
        }

        public static Permutation Symmetrical(HandPose pose)
        {
            return new Permutation(pose, pose);
        }

        public bool IsSymmetrical()
        {
            return Left == Right;
        }

        protected bool Equals(Permutation other)
        {
            return Left == other.Left && Right == other.Right;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Permutation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Left * 397) ^ (int) Right;
            }
        }

        public static bool operator ==(Permutation left, Permutation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Permutation left, Permutation right)
        {
            return !Equals(left, right);
        }
    }

    public enum HandPose
    {
        H0,
        H1,
        H2,
        H3,
        H4,
        H5,
        H6,
        H7,
    }
}
