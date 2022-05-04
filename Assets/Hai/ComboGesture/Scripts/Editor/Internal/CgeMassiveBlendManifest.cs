using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeMassiveBlendManifest : ICgeManifest
    {
        private readonly float _transitionDuration;
        public CgeMassiveBlendMode Mode;
        public string SimpleParameterName { get; } // null when mode is a Complex blend tree
        public BlendTree BlendTree { get; }
        public List<CgePermutationManifest> EquatedManifests { get; }

        private CgeMassiveBlendManifest(CgeMassiveBlendMode mode, List<ICgeManifest> moodSets, string simpleParameterName, BlendTree blendTree, float transitionDuration)
        {
            Mode = mode;
            _transitionDuration = transitionDuration;
            BlendTree = blendTree;
            SimpleParameterName = simpleParameterName;
            EquatedManifests = moodSets.Select(it => it.ToEquatedPermutation()).ToList();
        }

        public static CgeMassiveBlendManifest OfParameterBased(CgeMassiveBlendMode mode, List<ICgeManifest> moodSets, string simpleParameterName, float transitionDuration)
        {
            return new CgeMassiveBlendManifest(mode, moodSets, simpleParameterName, null, transitionDuration);
        }

        public static CgeMassiveBlendManifest OfComplex(CgeMassiveBlendMode mode, List<ICgeManifest> moodSets, BlendTree blendTree, float transitionDuration)
        {
            return new CgeMassiveBlendManifest(mode, moodSets, null, blendTree, transitionDuration);
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public CgeManifestKind Kind()
        {
            return CgeManifestKind.Massive;
        }

        public bool RequiresBlinking()
        {
            return InternalManifests().Any(manifest => manifest.RequiresBlinking());
        }

        public IEnumerable<CgeQualifiedAnimation> AllQualifiedAnimations()
        {
            return InternalManifests().SelectMany(manifest => manifest.AllQualifiedAnimations()).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalManifests().SelectMany(manifest => manifest.AllBlendTreesFoundRecursively()).ToList();
        }

        public ICgeManifest NewFromRemappedAnimations(Dictionary<CgeQualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendToRemappedBlend)
        {
            return new CgeMassiveBlendManifest(
                Mode,
                EquatedManifests.Select(manifest => manifest.NewFromRemappedAnimations(remapping, blendToRemappedBlend)).ToList(),
                SimpleParameterName,
                BlendTree,
                _transitionDuration);
        }

        public ICgeManifest UsingRemappedWeights(Dictionary<BlendTree, CgeAutoWeightTreeMapping> autoWeightRemapping)
        {
            return new CgeMassiveBlendManifest(
                Mode,
                EquatedManifests.Select(manifest => manifest.UsingRemappedWeights(autoWeightRemapping)).ToList(),
                SimpleParameterName,
                BlendTree,
                _transitionDuration);
        }

        public CgePermutationManifest ToEquatedPermutation()
        {
            throw new ArgumentException("Massive Blend Manifests are not yet equatable to Permutation Manifests.");
        }

        private List<ICgeManifest> InternalManifests()
        {
            return EquatedManifests.Cast<ICgeManifest>().ToList();
        }
    }
}
