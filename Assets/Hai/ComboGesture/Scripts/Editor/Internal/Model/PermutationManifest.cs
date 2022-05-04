using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public interface ICgeManifest
    {
        CgeManifestKind Kind();
        bool RequiresBlinking();
        IEnumerable<CgeQualifiedAnimation> AllQualifiedAnimations();
        IEnumerable<BlendTree> AllBlendTreesFoundRecursively();
        ICgeManifest NewFromRemappedAnimations(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendToRemappedBlend);
        ICgeManifest UsingRemappedWeights(Dictionary<BlendTree,CgeAutoWeightTreeMapping> autoWeightRemapping);
        CgePermutationManifest ToEquatedPermutation();
    }

    public readonly struct CgeAutoWeightTreeMapping
    {
        public CgeAutoWeightTreeMapping(BlendTree original, BlendTree leftHand, BlendTree rightHand)
        {
            Original = original;
            LeftHand = leftHand;
            RightHand = rightHand;
        }

        public BlendTree Original { get; }
        public BlendTree LeftHand { get; }
        public BlendTree RightHand { get; }
    }

    public enum CgeManifestKind
    {
        Permutation, Puppet, Massive, OneHand
    }

    public class CgeOneHandManifest : ICgeManifest
    {
        public ReadOnlyDictionary<CgeHandPose, ICgeAnimatedBehavior> Poses { get; }
        public bool IsLeftHand;

        private readonly float _transitionDuration;

        public CgeOneHandManifest(Dictionary<CgeHandPose, ICgeAnimatedBehavior> poses, float transitionDuration, bool isLeftHand)
        {
            Poses = new ReadOnlyDictionary<CgeHandPose, ICgeAnimatedBehavior>(poses);
            IsLeftHand = isLeftHand;
            _transitionDuration = transitionDuration;
        }

        public CgeManifestKind Kind()
        {
            return CgeManifestKind.OneHand;
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Poses.Values.Any(behavior => behavior.QualifiedAnimations().Any(animation => animation.Qualification.IsBlinking));
        }

        public IEnumerable<CgeQualifiedAnimation> AllQualifiedAnimations()
        {
            return Poses.Values.SelectMany(behavior => behavior.QualifiedAnimations()).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return Poses.Values.SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct().ToList();
        }

        public ICgeManifest NewFromRemappedAnimations(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newPoses = new Dictionary<CgeHandPose, ICgeAnimatedBehavior>(Poses);
            foreach (var handPose in newPoses.Keys.ToList())
            {
                newPoses[handPose] = newPoses[handPose].Remapping(remapping, blendRemapping);
            }

            return new CgeOneHandManifest(newPoses, _transitionDuration, IsLeftHand);
        }

        public ICgeManifest UsingRemappedWeights(Dictionary<BlendTree, CgeAutoWeightTreeMapping> autoWeightRemapping)
        {
            var remappingLeft = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.LeftHand);
            var remappingRight = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.RightHand);
            var emptyDict = new Dictionary<CgeQualifiedAnimation, AnimationClip>();

            var newPoses = new Dictionary<CgeHandPose, ICgeAnimatedBehavior>(Poses);
            foreach (var pair in Poses)
            {
                newPoses[pair.Key] = pair.Value.Remapping(emptyDict, IsLeftHand ? remappingLeft : remappingRight);
            }

            return new CgeOneHandManifest(newPoses, _transitionDuration, IsLeftHand);
        }

        public CgePermutationManifest ToEquatedPermutation()
        {
            var dict = new Dictionary<CgePermutation, ICgeAnimatedBehavior>();

            foreach (var permutation in CgePermutation.All())
            {
                dict.Add(permutation, IsLeftHand ? Poses[permutation.Left] : Poses[permutation.Right]);
            }

            return new CgePermutationManifest(dict, _transitionDuration);
        }
    }

    public class CgePermutationManifest : ICgeManifest
    {
        public ReadOnlyDictionary<CgePermutation, ICgeAnimatedBehavior> Poses { get; }
        private readonly float _transitionDuration;

        public CgePermutationManifest(Dictionary<CgePermutation, ICgeAnimatedBehavior> poses, float transitionDuration)
        {
            Poses = new ReadOnlyDictionary<CgePermutation, ICgeAnimatedBehavior>(poses);
            _transitionDuration = transitionDuration;
        }

        public CgeManifestKind Kind()
        {
            return CgeManifestKind.Permutation;
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public bool RequiresBlinking()
        {
            return Poses.Values.Any(behavior => behavior.QualifiedAnimations().Any(animation => animation.Qualification.IsBlinking));
        }

        public IEnumerable<CgeQualifiedAnimation> AllQualifiedAnimations()
        {
            return Poses.Values.SelectMany(behavior => behavior.QualifiedAnimations()).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return Poses.Values.SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct().ToList();
        }

        public ICgeManifest NewFromRemappedAnimations(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newPoses = new Dictionary<CgePermutation, ICgeAnimatedBehavior>(Poses);
            foreach (var permutation in newPoses.Keys.ToList())
            {
                newPoses[permutation] = newPoses[permutation].Remapping(remapping, blendRemapping);
            }

            return new CgePermutationManifest(newPoses, _transitionDuration);
        }

        public ICgeManifest UsingRemappedWeights(Dictionary<BlendTree, CgeAutoWeightTreeMapping> autoWeightRemapping)
        {
            var remappingLeft = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.LeftHand);
            var remappingRight = autoWeightRemapping.ToDictionary(pair => pair.Key, pair => pair.Value.RightHand);
            var emptyDict = new Dictionary<CgeQualifiedAnimation, AnimationClip>();

            var newPoses = new Dictionary<CgePermutation, ICgeAnimatedBehavior>(Poses);
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

            return new CgePermutationManifest(newPoses, _transitionDuration);
        }

        public CgePermutationManifest ToEquatedPermutation()
        {
            return this;
        }
    }

    public interface ICgeAnimatedBehavior
    {
        CgeAnimatedBehaviorNature Nature();
        IEnumerable<CgeQualifiedAnimation> QualifiedAnimations();
        IEnumerable<BlendTree> AllBlendTreesFoundRecursively();
        ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping);
    }

    public enum CgeAnimatedBehaviorNature
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

    class CgeSingleAnimatedBehavior : ICgeAnimatedBehavior
    {
        public CgeQualifiedAnimation Posing { get; }

        private CgeSingleAnimatedBehavior(CgeQualifiedAnimation posing)
        {
            Posing = posing;
        }

        public static ICgeAnimatedBehavior Of(CgeQualifiedAnimation posing)
        {
            return new CgeSingleAnimatedBehavior(posing);
        }

        CgeAnimatedBehaviorNature ICgeAnimatedBehavior.Nature()
        {
            return CgeAnimatedBehaviorNature.Single;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Posing};
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Of(Remap(remapping, Posing));
        }

        internal static CgeQualifiedAnimation Remap(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, CgeQualifiedAnimation key)
        {
            return remapping.ContainsKey(key) ? key.NewInstanceWithClip(remapping[key]) : key;
        }
    }

    class CgeAnalogAnimatedBehavior : ICgeAnimatedBehavior
    {
        public CgeQualifiedAnimation Resting { get; }
        public CgeQualifiedAnimation Squeezing { get; }
        public CgeHandSide HandSide { get; }

        private CgeAnalogAnimatedBehavior(CgeQualifiedAnimation resting, CgeQualifiedAnimation squeezing, CgeHandSide handSide)
        {
            if (resting == squeezing)
            {
                throw new ArgumentException("AnalogAnimatedBehavior must not have both identical qualified animations");
            }

            Resting = resting;
            Squeezing = squeezing;
            HandSide = handSide;
        }

        CgeAnimatedBehaviorNature ICgeAnimatedBehavior.Nature()
        {
            return CgeAnimatedBehaviorNature.Analog;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Resting, Squeezing};
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Maybe(
                CgeSingleAnimatedBehavior.Remap(remapping, Resting),
                CgeSingleAnimatedBehavior.Remap(remapping, Squeezing),
                HandSide
            );
        }

        public static CgeAnalogAnimatedBehavior Of(CgeQualifiedAnimation resting, CgeQualifiedAnimation squeezing, CgeHandSide handSide)
        {
            return new CgeAnalogAnimatedBehavior(resting, squeezing, handSide);
        }

        public static ICgeAnimatedBehavior Maybe(CgeQualifiedAnimation resting, CgeQualifiedAnimation squeezing, CgeHandSide handSide)
        {
            if (resting == squeezing)
            {
                return CgeSingleAnimatedBehavior.Of(squeezing);
            }

            return CgeAnalogAnimatedBehavior.Of(resting, squeezing, handSide);
        }
    }

    class CgeDualAnalogAnimatedBehavior : ICgeAnimatedBehavior
    {
        public CgeQualifiedAnimation Resting { get; }
        public CgeQualifiedAnimation LeftSqueezing { get; }
        public CgeQualifiedAnimation RightSqueezing { get; }
        public CgeQualifiedAnimation BothSqueezing { get; }

        private CgeDualAnalogAnimatedBehavior(CgeQualifiedAnimation resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing)
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

        CgeAnimatedBehaviorNature ICgeAnimatedBehavior.Nature()
        {
            return CgeAnimatedBehaviorNature.DualAnalog;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return new[] {Resting, LeftSqueezing, RightSqueezing, BothSqueezing};
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Maybe(
                CgeSingleAnimatedBehavior.Remap(remapping, Resting),
                CgeSingleAnimatedBehavior.Remap(remapping, LeftSqueezing),
                CgeSingleAnimatedBehavior.Remap(remapping, RightSqueezing),
                CgeSingleAnimatedBehavior.Remap(remapping, BothSqueezing)
            );
        }

        public static CgeDualAnalogAnimatedBehavior Of(CgeQualifiedAnimation resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing)
        {
            return new CgeDualAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing);
        }

        public static ICgeAnimatedBehavior Maybe(CgeQualifiedAnimation resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing)
        {
            if (AreAllIdentical(resting, leftSqueezing, rightSqueezing, bothSqueezing))
            {
                return CgeSingleAnimatedBehavior.Of(bothSqueezing);
            }

            return Of(resting, leftSqueezing, rightSqueezing, bothSqueezing);
        }

        private static bool AreAllIdentical(CgeQualifiedAnimation resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing)
        {
            return resting == bothSqueezing && leftSqueezing == bothSqueezing && rightSqueezing == bothSqueezing;
        }
    }

    public class CgePuppetAnimatedBehavior : ICgeAnimatedBehavior
    {
        public readonly BlendTree Tree;
        private readonly HashSet<CgeQualifiedAnimation> _qualifications;

        private CgePuppetAnimatedBehavior(BlendTree tree, List<CgeQualifiedAnimation> qualifications)
        {
            Tree = tree;
            _qualifications = new HashSet<CgeQualifiedAnimation>(qualifications);
        }

        public static CgePuppetAnimatedBehavior Of(BlendTree blendTree, List<CgeQualifiedAnimation> qualifications)
        {
            return new CgePuppetAnimatedBehavior(blendTree, qualifications);
        }

        public CgeAnimatedBehaviorNature Nature()
        {
            return CgeAnimatedBehaviorNature.Puppet;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return _qualifications.ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return CgeSingleManifest.FindAllBlendTreesIncludingItself(Tree);
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newQualifications = _qualifications
                .Select(qualification => remapping.ContainsKey(qualification)
                    ? qualification.NewInstanceWithClip(remapping[qualification])
                    : qualification)
                .ToList();

            return Of(blendRemapping.ContainsKey(Tree) ? blendRemapping[Tree] : Tree, newQualifications);
        }
    }

    class CgePuppetToAnalogAnimatedBehavior : ICgeAnimatedBehavior
    {
        public BlendTree Resting { get; }
        public CgeQualifiedAnimation Squeezing { get; }
        public List<CgeQualifiedAnimation> QualificationsOfTree { get; }
        public CgeHandSide HandSide { get; }

        private CgePuppetToAnalogAnimatedBehavior(BlendTree resting, CgeQualifiedAnimation squeezing, List<CgeQualifiedAnimation> qualificationsOfTreeOfTree, CgeHandSide handSide)
        {
            Resting = resting;
            Squeezing = squeezing;
            QualificationsOfTree = qualificationsOfTreeOfTree;
            HandSide = handSide;
        }

        public CgeAnimatedBehaviorNature Nature()
        {
            return CgeAnimatedBehaviorNature.PuppetToAnalog;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return new [] {Squeezing}.Concat(QualificationsOfTree).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return CgeSingleManifest.FindAllBlendTreesIncludingItself(Resting);
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
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

        public static CgePuppetToAnalogAnimatedBehavior Of(BlendTree resting, CgeQualifiedAnimation squeezing, List<CgeQualifiedAnimation> qualificationsOfTree, CgeHandSide handSide)
        {
            return new CgePuppetToAnalogAnimatedBehavior(resting, squeezing, qualificationsOfTree, handSide);
        }
    }

    class CgePuppetToDualAnalogAnimatedBehavior : ICgeAnimatedBehavior
    {
        public BlendTree Resting { get; }
        public CgeQualifiedAnimation LeftSqueezing { get; }
        public CgeQualifiedAnimation RightSqueezing { get; }
        public CgeQualifiedAnimation BothSqueezing { get; }
        public List<CgeQualifiedAnimation> QualificationsOfTree { get; }

        private CgePuppetToDualAnalogAnimatedBehavior(BlendTree resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing, List<CgeQualifiedAnimation> qualificationsOfTree)
        {
            QualificationsOfTree = qualificationsOfTree;

            Resting = resting;
            LeftSqueezing = leftSqueezing;
            RightSqueezing = rightSqueezing;
            BothSqueezing = bothSqueezing;
        }

        CgeAnimatedBehaviorNature ICgeAnimatedBehavior.Nature()
        {
            return CgeAnimatedBehaviorNature.PuppetToDualAnalog;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return new[] {LeftSqueezing, RightSqueezing, BothSqueezing}.Concat(QualificationsOfTree).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            var newQualificationsOfTree = QualificationsOfTree
                .Select(qualification => remapping.ContainsKey(qualification)
                    ? qualification.NewInstanceWithClip(remapping[qualification])
                    : qualification)
                .ToList();

            return Of(
                blendRemapping[Resting],
                CgeSingleAnimatedBehavior.Remap(remapping, LeftSqueezing),
                CgeSingleAnimatedBehavior.Remap(remapping, RightSqueezing),
                CgeSingleAnimatedBehavior.Remap(remapping, BothSqueezing),
                newQualificationsOfTree
            );
        }

        public static CgePuppetToDualAnalogAnimatedBehavior Of(BlendTree resting, CgeQualifiedAnimation leftSqueezing, CgeQualifiedAnimation rightSqueezing, CgeQualifiedAnimation bothSqueezing, List<CgeQualifiedAnimation> qualificationsOfTree)
        {
            return new CgePuppetToDualAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing, qualificationsOfTree);
        }
    }

    class CgeUniversalAnalogAnimatedBehavior : ICgeAnimatedBehavior
    {
        public CgeUniversalQualifier Resting { get; }
        public CgeUniversalQualifier LeftSqueezing { get; }
        public CgeUniversalQualifier RightSqueezing { get; }
        public CgeQualifiedAnimation BothSqueezing { get; }

        public struct CgeUniversalQualifier
        {
            public bool isBlendTree;
            public BlendTree blendTree;
            public CgeQualifiedAnimation? qualification;
            public List<CgeQualifiedAnimation> qualificationsOfTree;

            public List<CgeQualifiedAnimation> AllQualifications()
            {
                return isBlendTree ? qualificationsOfTree : new List<CgeQualifiedAnimation> {qualification.Value};
            }

            public Motion ToMotion()
            {
                return isBlendTree ? blendTree : (Motion)(qualification.Value.Clip);
            }

            public CgeUniversalQualifier Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
            {
                return new CgeUniversalQualifier
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

            public static CgeUniversalQualifier OfBlend(BlendTree tree, List<CgeQualifiedAnimation> qualificationsOfTree)
            {
                return new CgeUniversalQualifier
                {
                    qualification = null,
                    blendTree = tree,
                    isBlendTree = true,
                    qualificationsOfTree = qualificationsOfTree
                };
            }

            public static CgeUniversalQualifier OfQualification(CgeQualifiedAnimation qualification)
            {
                return new CgeUniversalQualifier
                {
                    qualification = qualification,
                    blendTree = null,
                    isBlendTree = false,
                    qualificationsOfTree = null
                };
            }
        }

        private CgeUniversalAnalogAnimatedBehavior(CgeUniversalQualifier resting, CgeUniversalQualifier leftSqueezing, CgeUniversalQualifier rightSqueezing, CgeQualifiedAnimation bothSqueezing)
        {
            Resting = resting;
            LeftSqueezing = leftSqueezing;
            RightSqueezing = rightSqueezing;
            BothSqueezing = bothSqueezing;
        }

        CgeAnimatedBehaviorNature ICgeAnimatedBehavior.Nature()
        {
            return CgeAnimatedBehaviorNature.UniversalAnalog;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return Resting.AllQualifications()
                .Concat(LeftSqueezing.AllQualifications())
                .Concat(RightSqueezing.AllQualifications())
                .Concat(new List<CgeQualifiedAnimation>() {BothSqueezing})
                .ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return new List<BlendTree>();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return Of(
                Resting.Remapping(remapping, blendRemapping),
                LeftSqueezing.Remapping(remapping, blendRemapping),
                RightSqueezing.Remapping(remapping, blendRemapping),
                remapping.ContainsKey(BothSqueezing) ? BothSqueezing.NewInstanceWithClip(remapping[BothSqueezing]) : BothSqueezing
            );
        }

        public static CgeUniversalAnalogAnimatedBehavior Of(CgeUniversalQualifier resting, CgeUniversalQualifier leftSqueezing, CgeUniversalQualifier rightSqueezing, CgeQualifiedAnimation bothSqueezing)
        {
            return new CgeUniversalAnalogAnimatedBehavior(resting, leftSqueezing, rightSqueezing, bothSqueezing);
        }
    }

    public class CgePermutation
    {
        public CgeHandPose Left { get; }
        public CgeHandPose Right { get; }

        private CgePermutation(CgeHandPose left, CgeHandPose right)
        {
            Left = left;
            Right = right;
        }

        public static CgePermutation LeftRight(CgeHandPose left, CgeHandPose right)
        {
            return new CgePermutation(left, right);
        }

        public static CgePermutation Symmetrical(CgeHandPose pose)
        {
            return new CgePermutation(pose, pose);
        }

        public static List<CgePermutation> All()
        {
            var poses = new List<CgePermutation>();
            for (var left = CgeHandPose.H0; left <= CgeHandPose.H7; left++)
            {
                for (var right = CgeHandPose.H0; right <= CgeHandPose.H7; right++)
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
            return Left == CgeHandPose.H1 || Right == CgeHandPose.H1;
        }

        public bool HasAnyPose(CgeHandPose pose)
        {
            return Left == pose || Right == pose;
        }

        public bool AreBoth(CgeHandPose pose)
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

        public CgePermutation ToOppositeSide()
        {
            return CgePermutation.LeftRight(Right, Left);
        }

        protected bool Equals(CgePermutation other)
        {
            return Left == other.Left && Right == other.Right;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CgePermutation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Left * 397) ^ (int) Right;
            }
        }

        public static bool operator ==(CgePermutation left, CgePermutation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CgePermutation left, CgePermutation right)
        {
            return !Equals(left, right);
        }
    }

    class CgeSimpleMassiveBlendAnimatedBehavior : ICgeAnimatedBehavior
    {
        public ICgeAnimatedBehavior Zero { get; }
        public ICgeAnimatedBehavior One { get; }
        public string ParameterName { get; }

        private CgeSimpleMassiveBlendAnimatedBehavior(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, string parameterName)
        {
            Zero = zero;
            One = one;
            ParameterName = parameterName;
        }

        private List<ICgeAnimatedBehavior> InternalBehaviors()
        {
            return new List<ICgeAnimatedBehavior> { Zero, One };
        }

        public CgeAnimatedBehaviorNature Nature()
        {
            return CgeAnimatedBehaviorNature.SimpleMassiveBlend;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new CgeSimpleMassiveBlendAnimatedBehavior(Zero.Remapping(remapping, blendRemapping), One.Remapping(remapping, blendRemapping), ParameterName);
        }

        private static CgeSimpleMassiveBlendAnimatedBehavior Of(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, string parameterName)
        {
            return new CgeSimpleMassiveBlendAnimatedBehavior(zero, one, parameterName);
        }

        public static ICgeAnimatedBehavior Maybe(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, string parameterName)
        {
            return zero.Equals(one) ? zero : CgeSimpleMassiveBlendAnimatedBehavior.Of(zero, one, parameterName);
        }
    }

    class CgeTwoDirectionsMassiveBlendAnimatedBehavior : ICgeAnimatedBehavior
    {
        public ICgeAnimatedBehavior Zero { get; }
        public ICgeAnimatedBehavior One { get; }
        public ICgeAnimatedBehavior MinusOne { get; }
        public string ParameterName { get; }

        private CgeTwoDirectionsMassiveBlendAnimatedBehavior(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, ICgeAnimatedBehavior minusOne, string parameterName)
        {
            Zero = zero;
            One = one;
            MinusOne = minusOne;
            ParameterName = parameterName;
        }

        private List<ICgeAnimatedBehavior> InternalBehaviors()
        {
            return new List<ICgeAnimatedBehavior> { Zero, One };
        }

        public CgeAnimatedBehaviorNature Nature()
        {
            return CgeAnimatedBehaviorNature.TwoDirectionsMassiveBlend;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new CgeTwoDirectionsMassiveBlendAnimatedBehavior(Zero.Remapping(remapping, blendRemapping), One.Remapping(remapping, blendRemapping), MinusOne.Remapping(remapping, blendRemapping), ParameterName);
        }

        private static CgeTwoDirectionsMassiveBlendAnimatedBehavior Of(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, ICgeAnimatedBehavior minusOne, string parameterName)
        {
            return new CgeTwoDirectionsMassiveBlendAnimatedBehavior(zero, one, minusOne, parameterName);
        }

        public static ICgeAnimatedBehavior Maybe(ICgeAnimatedBehavior zero, ICgeAnimatedBehavior one, ICgeAnimatedBehavior minusOne, string parameterName)
        {
            return zero.Equals(one) && one.Equals(minusOne) ? zero : CgeTwoDirectionsMassiveBlendAnimatedBehavior.Of(zero, one, minusOne, parameterName);
        }
    }

    class CgeComplexMassiveBlendAnimatedBehavior : ICgeAnimatedBehavior
    {
        public List<ICgeAnimatedBehavior> Behaviors { get; }
        public BlendTree OriginalBlendTreeTemplate { get; }

        private CgeComplexMassiveBlendAnimatedBehavior(List<ICgeAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            Behaviors = behaviors;
            OriginalBlendTreeTemplate = originalBlendTreeTemplate;
        }

        private List<ICgeAnimatedBehavior> InternalBehaviors()
        {
            return Behaviors.ToList();
        }

        public CgeAnimatedBehaviorNature Nature()
        {
            return CgeAnimatedBehaviorNature.ComplexMassiveBlend;
        }

        public IEnumerable<CgeQualifiedAnimation> QualifiedAnimations()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.QualifiedAnimations()).Distinct();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalBehaviors().SelectMany(behavior => behavior.AllBlendTreesFoundRecursively()).Distinct();
        }

        public ICgeAnimatedBehavior Remapping(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendRemapping)
        {
            return new CgeComplexMassiveBlendAnimatedBehavior(Behaviors.Select(behavior => behavior.Remapping(remapping, blendRemapping)).ToList(), OriginalBlendTreeTemplate);
        }

        public static CgeComplexMassiveBlendAnimatedBehavior Of(List<ICgeAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            return new CgeComplexMassiveBlendAnimatedBehavior(behaviors, originalBlendTreeTemplate);
        }
    }

    public enum CgeHandPose
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

    public enum CgeHandSide
    {
        LeftHand,
        RightHand
    }
}
