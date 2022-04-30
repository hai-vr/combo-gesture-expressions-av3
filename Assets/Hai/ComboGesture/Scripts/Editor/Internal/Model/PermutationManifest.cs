using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
    public interface IManifest
    {
        ManifestKind Kind();
        bool RequiresBlinking();
        IEnumerable<QualifiedAnimation> AllQualifiedAnimations();
        IEnumerable<BlendTree> AllBlendTreesFoundRecursively();
        IManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendToRemappedBlend);
        IManifest UsingRemappedWeights(Dictionary<BlendTree,AutoWeightTreeMapping> autoWeightRemapping);
        PermutationManifest ToEquatedPermutation();
    }

    public readonly struct AutoWeightTreeMapping
    {
        public AutoWeightTreeMapping(BlendTree original, BlendTree leftHand, BlendTree rightHand)
        {
            Original = original;
            LeftHand = leftHand;
            RightHand = rightHand;
        }

        public BlendTree Original { get; }
        public BlendTree LeftHand { get; }
        public BlendTree RightHand { get; }
    }

    public enum ManifestKind
    {
        Permutation, Puppet, Massive
    }

    public class PermutationManifest : IManifest
    {
        public ReadOnlyDictionary<Permutation, IAnimatedBehavior> Poses { get; }
        private readonly float _transitionDuration;

        public PermutationManifest(Dictionary<Permutation, IAnimatedBehavior> poses, float transitionDuration)
        {
            Poses = new ReadOnlyDictionary<Permutation, IAnimatedBehavior>(poses);
            _transitionDuration = transitionDuration;
        }

        public ManifestKind Kind()
        {
            return ManifestKind.Permutation;
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Poses.Values.Any(behavior => behavior.QualifiedAnimations().Any(animation => animation.Qualification.IsBlinking));
        }

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return Poses.Values.SelectMany(behavior => behavior.QualifiedAnimations()).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return Poses.Values.SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct().ToList();
        }

        public IManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newPoses = new Dictionary<Permutation, IAnimatedBehavior>(Poses);
            foreach (var permutation in newPoses.Keys.ToList())
            {
                newPoses[permutation] = newPoses[permutation].Remapping(remapping, blendRemapping);
            }

            return new PermutationManifest(newPoses, _transitionDuration);
        }

        public IManifest UsingRemappedWeights(Dictionary<BlendTree, AutoWeightTreeMapping> autoWeightRemapping)
        {
            var remappingLeft = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.LeftHand);
            var remappingRight = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.RightHand);
            var emptyDict = new Dictionary<QualifiedAnimation, AnimationClip>();

            var newPoses = new Dictionary<Permutation, IAnimatedBehavior>(Poses);
            foreach (var pair in Poses)
            {
                if (!pair.Key.IsSymmetrical()) {
                    if (pair.Key.IsOrangeSide())
                    {
                        newPoses[pair.Key] = pair.Value.Remapping(emptyDict, remappingLeft);
                    }
                    if (pair.Key.IsBlueSide())
                    {
                        newPoses[pair.Key] = pair.Value.Remapping(emptyDict, remappingRight);
                    }
                }
            }

            return new PermutationManifest(newPoses, _transitionDuration);
        }

        public PermutationManifest ToEquatedPermutation()
        {
            return this;
        }
    }

    public interface IAnimatedBehavior
    {
        AnimatedBehaviorNature Nature();
        IEnumerable<QualifiedAnimation> QualifiedAnimations();
        IEnumerable<BlendTree> AllBlendTreesFoundRecursively();
        IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping);
    }

    public enum AnimatedBehaviorNature
    {
        Single,
        Analog,
        DualAnalog,
        Puppet,
        PuppetToAnalog,
        PuppetToDualAnalog,
        SimpleMassiveBlend,
        TwoDirectionsMassiveBlend,
        ComplexMassiveBlend,
        UniversalAnalog
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

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Of(Remap(remapping, Posing));
        }

        internal static QualifiedAnimation Remap(Dictionary<QualifiedAnimation, AnimationClip> remapping, QualifiedAnimation key)
        {
            return remapping.ContainsKey(key) ? key.NewInstanceWithClip(remapping[key]) : key;
        }
    }

    class AnalogAnimatedBehavior : IAnimatedBehavior
    {
        public QualifiedAnimation Resting { get; }
        public QualifiedAnimation Squeezing { get; }
        public HandSide HandSide { get; }

        private AnalogAnimatedBehavior(QualifiedAnimation resting, QualifiedAnimation squeezing, HandSide handSide)
        {
            if (resting == squeezing)
            {
                throw new ArgumentException("AnalogAnimatedBehavior must not have both identical qualified animations");
            }

            Resting = resting;
            Squeezing = squeezing;
            HandSide = handSide;
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.Analog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Resting, Squeezing};
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Maybe(
                SingleAnimatedBehavior.Remap(remapping, Resting),
                SingleAnimatedBehavior.Remap(remapping, Squeezing),
                HandSide
            );
        }

        public static AnalogAnimatedBehavior Of(QualifiedAnimation resting, QualifiedAnimation squeezing, HandSide handSide)
        {
            return new AnalogAnimatedBehavior(resting, squeezing, handSide);
        }

        public static IAnimatedBehavior Maybe(QualifiedAnimation resting, QualifiedAnimation squeezing, HandSide handSide)
        {
            if (resting == squeezing)
            {
                return SingleAnimatedBehavior.Of(squeezing);
            }

            return AnalogAnimatedBehavior.Of(resting, squeezing, handSide);
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

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
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
    }

    public class PuppetAnimatedBehavior : IAnimatedBehavior
    {
        public readonly BlendTree Tree;
        private readonly HashSet<QualifiedAnimation> _qualifications;

        private PuppetAnimatedBehavior(BlendTree tree, List<QualifiedAnimation> qualifications)
        {
            Tree = tree;
            _qualifications = new HashSet<QualifiedAnimation>(qualifications);
        }

        public static PuppetAnimatedBehavior Of(BlendTree blendTree, List<QualifiedAnimation> qualifications)
        {
            return new PuppetAnimatedBehavior(blendTree, qualifications);
        }

        public AnimatedBehaviorNature Nature()
        {
            return AnimatedBehaviorNature.Puppet;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return _qualifications.ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return PuppetManifest.FindAllBlendTreesIncludingItself(Tree);
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newQualifications = _qualifications
                .Select(qualification => remapping.ContainsKey(qualification)
                    ? qualification.NewInstanceWithClip(remapping[qualification])
                    : qualification)
                .ToList();

            return Of(blendRemapping.ContainsKey(Tree) ? blendRemapping[Tree] : Tree, newQualifications);
        }
    }

    class PuppetToAnalogAnimatedBehavior : IAnimatedBehavior
    {
        public BlendTree Resting { get; }
        public QualifiedAnimation Squeezing { get; }
        public List<QualifiedAnimation> QualificationsOfTree { get; }
        public HandSide HandSide { get; }

        private PuppetToAnalogAnimatedBehavior(BlendTree resting, QualifiedAnimation squeezing, List<QualifiedAnimation> qualificationsOfTreeOfTree, HandSide handSide)
        {
            Resting = resting;
            Squeezing = squeezing;
            QualificationsOfTree = qualificationsOfTreeOfTree;
            HandSide = handSide;
        }

        public AnimatedBehaviorNature Nature()
        {
            return AnimatedBehaviorNature.PuppetToAnalog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new [] {Squeezing}.Concat(QualificationsOfTree).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return PuppetManifest.FindAllBlendTreesIncludingItself(Resting);
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newQualificationsOfTree = QualificationsOfTree
                .Select(qualification => remapping.ContainsKey(qualification)
                    ? qualification.NewInstanceWithClip(remapping[qualification])
                    : qualification)
                .ToList();
            var newSqueezing = remapping.ContainsKey(Squeezing) ? Squeezing.NewInstanceWithClip(remapping[Squeezing]) : Squeezing;
            var newBlendRemapping = blendRemapping.ContainsKey(Resting) ? blendRemapping[Resting] : Resting;

            return Of(newBlendRemapping, newSqueezing, newQualificationsOfTree, HandSide);
        }

        public static PuppetToAnalogAnimatedBehavior Of(BlendTree resting, QualifiedAnimation squeezing, List<QualifiedAnimation> qualificationsOfTree, HandSide handSide)
        {
            return new PuppetToAnalogAnimatedBehavior(resting, squeezing, qualificationsOfTree, handSide);
        }
    }

    class PuppetToDualAnalogAnimatedBehavior : IAnimatedBehavior
    {
        public BlendTree Resting { get; }
        public QualifiedAnimation LeftSqueezing { get; }
        public QualifiedAnimation RightSqueezing { get; }
        public QualifiedAnimation BothSqueezing { get; }
        public List<QualifiedAnimation> QualificationsOfTree { get; }

        private PuppetToDualAnalogAnimatedBehavior(BlendTree resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing, List<QualifiedAnimation> qualificationsOfTree)
        {
            QualificationsOfTree = qualificationsOfTree;

            Resting = resting;
            LeftSqueezing = leftSqueezing;
            RightSqueezing = rightSqueezing;
            BothSqueezing = bothSqueezing;
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.PuppetToDualAnalog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return new[] {LeftSqueezing, RightSqueezing, BothSqueezing}.Concat(QualificationsOfTree).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newQualificationsOfTree = QualificationsOfTree
                .Select(qualification => remapping.ContainsKey(qualification)
                    ? qualification.NewInstanceWithClip(remapping[qualification])
                    : qualification)
                .ToList();

            return Of(
                blendRemapping[Resting],
                SingleAnimatedBehavior.Remap(remapping, LeftSqueezing),
                SingleAnimatedBehavior.Remap(remapping, RightSqueezing),
                SingleAnimatedBehavior.Remap(remapping, BothSqueezing),
                newQualificationsOfTree
            );
        }

        public static PuppetToDualAnalogAnimatedBehavior Of(BlendTree resting, QualifiedAnimation leftSqueezing, QualifiedAnimation rightSqueezing, QualifiedAnimation bothSqueezing, List<QualifiedAnimation> qualificationsOfTree)
        {
            return new PuppetToDualAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing, qualificationsOfTree);
        }
    }

    class UniversalAnalogAnimatedBehavior : IAnimatedBehavior
    {
        public UniversalQualifier Resting { get; }
        public UniversalQualifier LeftSqueezing { get; }
        public UniversalQualifier RightSqueezing { get; }
        public QualifiedAnimation BothSqueezing { get; }

        public struct UniversalQualifier
        {
            public bool isBlendTree;
            public BlendTree blendTree;
            public QualifiedAnimation? qualification;
            public List<QualifiedAnimation> qualificationsOfTree;

            public List<QualifiedAnimation> AllQualifications()
            {
                return isBlendTree ? qualificationsOfTree : new List<QualifiedAnimation> {qualification.Value};
            }

            public Motion ToMotion()
            {
                return isBlendTree ? blendTree : (Motion)(qualification.Value.Clip);
            }

            public UniversalQualifier Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
            {
                return new UniversalQualifier
                {
                    isBlendTree = isBlendTree,
                    blendTree = isBlendTree ? blendRemapping[blendTree] : null,
                    qualification = isBlendTree ? null : remapping.ContainsKey(qualification.Value) ? qualification.Value.NewInstanceWithClip(remapping[qualification.Value]) : qualification,
                    qualificationsOfTree = isBlendTree ? qualificationsOfTree
                        .Select(qualification => remapping.ContainsKey(qualification)
                            ? qualification.NewInstanceWithClip(remapping[qualification])
                            : qualification)
                        .ToList() : null
                };
            }

            public static UniversalQualifier OfBlend(BlendTree tree, List<QualifiedAnimation> qualificationsOfTree)
            {
                return new UniversalQualifier
                {
                    qualification = null,
                    blendTree = tree,
                    isBlendTree = true,
                    qualificationsOfTree = qualificationsOfTree
                };
            }

            public static UniversalQualifier OfQualification(QualifiedAnimation qualification)
            {
                return new UniversalQualifier
                {
                    qualification = qualification,
                    blendTree = null,
                    isBlendTree = false,
                    qualificationsOfTree = null
                };
            }
        }

        private UniversalAnalogAnimatedBehavior(UniversalQualifier resting, UniversalQualifier leftSqueezing, UniversalQualifier rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            Resting = resting;
            LeftSqueezing = leftSqueezing;
            RightSqueezing = rightSqueezing;
            BothSqueezing = bothSqueezing;
        }

        AnimatedBehaviorNature IAnimatedBehavior.Nature()
        {
            return AnimatedBehaviorNature.UniversalAnalog;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return Resting.AllQualifications()
                .Concat(LeftSqueezing.AllQualifications())
                .Concat(RightSqueezing.AllQualifications())
                .Concat(new List<QualifiedAnimation>() {BothSqueezing})
                .ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Of(
                Resting.Remapping(remapping, blendRemapping),
                LeftSqueezing.Remapping(remapping, blendRemapping),
                RightSqueezing.Remapping(remapping, blendRemapping),
                remapping.ContainsKey(BothSqueezing) ? BothSqueezing.NewInstanceWithClip(remapping[BothSqueezing]) : BothSqueezing
            );
        }

        public static UniversalAnalogAnimatedBehavior Of(UniversalQualifier resting, UniversalQualifier leftSqueezing, UniversalQualifier rightSqueezing, QualifiedAnimation bothSqueezing)
        {
            return new UniversalAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing);
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

        public static List<Permutation> All()
        {
            var poses = new List<Permutation>();
            for (var left = HandPose.H0; left <= HandPose.H7; left++)
            {
                for (var right = HandPose.H0; right <= HandPose.H7; right++)
                {
                    poses.Add(LeftRight(left, right));
                }
            }

            return poses;
        }

        public bool IsSymmetrical()
        {
            return Left == Right;
        }

        public bool HasAnyFist()
        {
            return Left == HandPose.H1 || Right == HandPose.H1;
        }

        public bool HasAnyPose(HandPose pose)
        {
            return Left == pose || Right == pose;
        }

        public bool AreBoth(HandPose pose)
        {
            return Left == pose && Right == pose;
        }

        /// The blue side is visually the top-right triangle. This does not include the symmetrical diagonal.
        /// The blue side is where combos are defined.
        public bool IsBlueSide()
        {
            return Left < Right;
        }

        /// The blue side is visually the lower-left triangle. This does not include the symmetrical diagonal.
        /// When the orange side is not defined, it will usually default to the opposite (blue) side.
        public bool IsOrangeSide()
        {
            return Left > Right;
        }

        public Permutation ToOppositeSide()
        {
            return Permutation.LeftRight(Right, Left);
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

    class SimpleMassiveBlendAnimatedBehavior : IAnimatedBehavior
    {
        public IAnimatedBehavior Zero { get; }
        public IAnimatedBehavior One { get; }
        public string ParameterName { get; }

        private SimpleMassiveBlendAnimatedBehavior(IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            Zero = zero;
            One = one;
            ParameterName = parameterName;
        }

        private List<IAnimatedBehavior> InternalBehaviors()
        {
            return new List<IAnimatedBehavior> { Zero, One };
        }

        public AnimatedBehaviorNature Nature()
        {
            return AnimatedBehaviorNature.SimpleMassiveBlend;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new SimpleMassiveBlendAnimatedBehavior(Zero.Remapping(remapping, blendRemapping), One.Remapping(remapping, blendRemapping), ParameterName);
        }

        private static SimpleMassiveBlendAnimatedBehavior Of(IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            return new SimpleMassiveBlendAnimatedBehavior(zero, one, parameterName);
        }

        public static IAnimatedBehavior Maybe(IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            return zero.Equals(one) ? zero : SimpleMassiveBlendAnimatedBehavior.Of(zero, one, parameterName);
        }
    }

    class TwoDirectionsMassiveBlendAnimatedBehavior : IAnimatedBehavior
    {
        public IAnimatedBehavior Zero { get; }
        public IAnimatedBehavior One { get; }
        public IAnimatedBehavior MinusOne { get; }
        public string ParameterName { get; }

        private TwoDirectionsMassiveBlendAnimatedBehavior(IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            Zero = zero;
            One = one;
            MinusOne = minusOne;
            ParameterName = parameterName;
        }

        private List<IAnimatedBehavior> InternalBehaviors()
        {
            return new List<IAnimatedBehavior> { Zero, One };
        }

        public AnimatedBehaviorNature Nature()
        {
            return AnimatedBehaviorNature.TwoDirectionsMassiveBlend;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new TwoDirectionsMassiveBlendAnimatedBehavior(Zero.Remapping(remapping, blendRemapping), One.Remapping(remapping, blendRemapping), MinusOne.Remapping(remapping, blendRemapping), ParameterName);
        }

        private static TwoDirectionsMassiveBlendAnimatedBehavior Of(IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            return new TwoDirectionsMassiveBlendAnimatedBehavior(zero, one, minusOne, parameterName);
        }

        public static IAnimatedBehavior Maybe(IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            return zero.Equals(one) && one.Equals(minusOne) ? zero : TwoDirectionsMassiveBlendAnimatedBehavior.Of(zero, one, minusOne, parameterName);
        }
    }

    class ComplexMassiveBlendAnimatedBehavior : IAnimatedBehavior
    {
        public List<IAnimatedBehavior> Behaviors { get; }
        public BlendTree OriginalBlendTreeTemplate { get; }

        private ComplexMassiveBlendAnimatedBehavior(List<IAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            Behaviors = behaviors;
            OriginalBlendTreeTemplate = originalBlendTreeTemplate;
        }

        private List<IAnimatedBehavior> InternalBehaviors()
        {
            return Behaviors.ToList();
        }

        public AnimatedBehaviorNature Nature()
        {
            return AnimatedBehaviorNature.ComplexMassiveBlend;
        }

        public IEnumerable<QualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public IAnimatedBehavior Remapping(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new ComplexMassiveBlendAnimatedBehavior(Behaviors.Select(behavior => behavior.Remapping(remapping, blendRemapping)).ToList(), OriginalBlendTreeTemplate);
        }

        public static ComplexMassiveBlendAnimatedBehavior Of(List<IAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            return new ComplexMassiveBlendAnimatedBehavior(behaviors, originalBlendTreeTemplate);
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

    public enum HandSide
    {
        LeftHand,
        RightHand
    }
}
