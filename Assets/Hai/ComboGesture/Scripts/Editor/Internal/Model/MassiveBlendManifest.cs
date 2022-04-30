using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Model
{
    public class MassiveBlendManifest : IManifest
    {
        private readonly float _transitionDuration;
        public CgeMassiveBlendMode Mode;
        public string SimpleParameterName { get; } // null when mode is a Complex blend tree
        public BlendTree BlendTree { get; }
        public List<PermutationManifest> EquatedManifests { get; }

        private MassiveBlendManifest(CgeMassiveBlendMode mode, List<IManifest> moodSets, string simpleParameterName, BlendTree blendTree, float transitionDuration)
        {
            Mode = mode;
            _transitionDuration = transitionDuration;
            BlendTree = blendTree;
            SimpleParameterName = simpleParameterName;
            EquatedManifests = moodSets.Select(it => it.ToEquatedPermutation()).ToList();
        }

        public static MassiveBlendManifest OfParameterBased(CgeMassiveBlendMode mode, List<IManifest> moodSets, string simpleParameterName, float transitionDuration)
        {
            return new MassiveBlendManifest(mode, moodSets, simpleParameterName, null, transitionDuration);
        }

        public static MassiveBlendManifest OfComplex(CgeMassiveBlendMode mode, List<IManifest> moodSets, BlendTree blendTree, float transitionDuration)
        {
            return new MassiveBlendManifest(mode, moodSets, null, blendTree, transitionDuration);
        }

        public float TransitionDuration()
        {
            return _transitionDuration;
        }

        public ManifestKind Kind()
        {
            return ManifestKind.Massive;
        }

        public bool RequiresBlinking()
        {
            return InternalManifests().Any(manifest => manifest.RequiresBlinking());
        }

        public IEnumerable<QualifiedAnimation> AllQualifiedAnimations()
        {
            return InternalManifests().SelectMany(manifest => manifest.AllQualifiedAnimations()).ToList();
        }

        public IEnumerable<BlendTree> AllBlendTreesFoundRecursively()
        {
            return InternalManifests().SelectMany(manifest => manifest.AllBlendTreesFoundRecursively()).ToList();
        }

        public IManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendToRemappedBlend)
        {
            return new MassiveBlendManifest(
                Mode,
                EquatedManifests.Select(manifest => manifest.NewFromRemappedAnimations(remapping, blendToRemappedBlend)).ToList(),
                SimpleParameterName,
                BlendTree,
                _transitionDuration);
        }

        public IManifest UsingRemappedWeights(Dictionary<BlendTree, AutoWeightTreeMapping> autoWeightRemapping)
        {
            return new MassiveBlendManifest(
                Mode,
                EquatedManifests.Select(manifest => manifest.UsingRemappedWeights(autoWeightRemapping)).ToList(),
                SimpleParameterName,
                BlendTree,
                _transitionDuration);
        }

        public PermutationManifest ToEquatedPermutation()
        {
            throw new ArgumentException("Massive Blend Manifests are not yet equatable to Permutation Manifests.");
        }

        private List<IManifest> InternalManifests()
        {
            return EquatedManifests.Cast<IManifest>().ToList();
        }
    }
}
