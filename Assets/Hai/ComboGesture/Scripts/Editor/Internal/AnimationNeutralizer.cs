using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    class AnimationNeutralizer
    {
        private readonly List<ManifestBinding> _originalBindings;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly HashSet<CurveKey> _ignoreCurveKeys;
        private readonly Dictionary<CurveKey, float> _curveKeyToFallbackValue;
        private readonly List<CurveKey> _blinkBlendshapes;
        private readonly AssetContainer _assetContainer;
        private readonly bool _useGestureWeightCorrection;

        public AnimationNeutralizer(List<ManifestBinding> originalBindings,
            ConflictFxLayerMode compilerConflictFxLayerMode,
            AnimationClip compilerIgnoreParamList,
            AnimationClip compilerFallbackParamList,
            List<CurveKey> blinkBlendshapes,
            AssetContainer assetContainer,
            bool useGestureWeightCorrection)
        {
            _originalBindings = originalBindings;
            _compilerConflictFxLayerMode = compilerConflictFxLayerMode;
            _ignoreCurveKeys = compilerIgnoreParamList == null ? new HashSet<CurveKey>() : ExtractAllCurvesOf(compilerIgnoreParamList);
            _curveKeyToFallbackValue = compilerFallbackParamList == null ? new Dictionary<CurveKey, float>() : ExtractFirstKeyframeValueOf(compilerFallbackParamList);
            _blinkBlendshapes = blinkBlendshapes;
            _assetContainer = assetContainer;
            _useGestureWeightCorrection = useGestureWeightCorrection;
        }

        private static HashSet<CurveKey> ExtractAllCurvesOf(AnimationClip clip)
        {
            return new HashSet<CurveKey>(AnimationUtility.GetCurveBindings(clip).Select(CurveKey.FromBinding));
        }

        private static Dictionary<CurveKey, float> ExtractFirstKeyframeValueOf(AnimationClip clip)
        {
            var curveKeyToFallbackValue = new Dictionary<CurveKey, float>();
            foreach (var editorCurveBinding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);

                if (curve.keys.Length > 0)
                {
                    curveKeyToFallbackValue.Add(CurveKey.FromBinding(editorCurveBinding), curve.keys[0].value);
                }
            }

            return curveKeyToFallbackValue;
        }

        internal List<ManifestBinding> NeutralizeManifestAnimations()
        {
            var allQualifiedAnimations = QualifyAllAnimations();
            var allApplicableCurveKeys = FindAllApplicableCurveKeys(new HashSet<AnimationClip>(allQualifiedAnimations.Select(animation => animation.Clip).ToList()));
            var animationRemapping = CreateAssetContainerWithNeutralizedAnimations(_assetContainer, allQualifiedAnimations, allApplicableCurveKeys);

            var neutralizeManifestAnimations = _originalBindings
                .Select(binding =>
                {
                    // Since BlendTrees of different manifests can have different qualifications for a same animation,
                    // for the sake of simplicity, we do not deduplicate blend trees across multiple Manifests
                    // even if there would have been cases where they could have been deduplicated.
                    var qualifiedAnimations = binding.Manifest.AllQualifiedAnimations().ToList();
                    var biTreesReferences = binding.Manifest.AllBlendTreesFoundRecursively()
                        .ToDictionary(
                            originalTree => originalTree,
                            originalTree => new BlendTree { hideFlags = HideFlags.HideInHierarchy }
                        );

                    var blendToRemappedBlend = binding.Manifest.AllBlendTreesFoundRecursively()
                        .ToDictionary(
                            originalTree => originalTree,
                            originalTree => RemapAnimationsOfBlendTree(originalTree, animationRemapping, qualifiedAnimations, biTreesReferences)
                        );
                    foreach (var blendTree in blendToRemappedBlend.Values)
                    {
                        _assetContainer.AddBlendTree(blendTree);
                    }

                    return RemapManifest(binding, animationRemapping, blendToRemappedBlend);
                })
                .ToList();

            AssetContainer.GlobalSave();

            return neutralizeManifestAnimations;
        }

        private BlendTree RemapAnimationsOfBlendTree(BlendTree originalTree, Dictionary<QualifiedAnimation, AnimationClip> remapping, List<QualifiedAnimation> qualifiedAnimations, Dictionary<BlendTree, BlendTree> biTreesReferences)
        {
            // Object.Instantiate(...) is triggering some weird issues about assertions failures.
            // Copy the blend tree manually
            var newTree = biTreesReferences[originalTree];
            newTree.name = "zAutogeneratedPup_" + originalTree.name + "_DO_NOT_EDIT";
            newTree.blendType = originalTree.blendType;
            newTree.blendParameter = HandleWeightCorrection(originalTree.blendParameter);
            newTree.blendParameterY = HandleWeightCorrection(originalTree.blendParameterY);
            newTree.minThreshold = originalTree.minThreshold;
            newTree.maxThreshold = originalTree.maxThreshold;
            newTree.useAutomaticThresholds = originalTree.useAutomaticThresholds;

            var copyOfChildren = originalTree.children;
            while (newTree.children.Length > 0) {
                newTree.RemoveChild(0);
            }

            var blendType = newTree.blendType;
            foreach (var copyOfChild in copyOfChildren)
            {
                var remappedMotion = RemapMotion(remapping, qualifiedAnimations, biTreesReferences, copyOfChild);

                switch (blendType)
                {
                    case BlendTreeType.Direct:
                        newTree.AddChild(remappedMotion);
                        break;
                    case BlendTreeType.Simple1D:
                        newTree.AddChild(remappedMotion, copyOfChild.threshold);
                        break;
                    case BlendTreeType.SimpleDirectional2D:
                    case BlendTreeType.FreeformDirectional2D:
                    case BlendTreeType.FreeformCartesian2D:
                        newTree.AddChild(remappedMotion, copyOfChild.position);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return newTree;
        }

        private string HandleWeightCorrection(string originalTreeBlendParameter)
        {
            if (!_useGestureWeightCorrection)
            {
                return originalTreeBlendParameter;
            }

            switch (originalTreeBlendParameter)
            {
                case "GestureLeftWeight":
                    return SharedLayerUtils.HaiGestureComboLeftWeightProxy;
                case "GestureRightWeight":
                    return SharedLayerUtils.HaiGestureComboRightWeightProxy;
                default:
                    return originalTreeBlendParameter;
            }
        }

        private static Motion RemapMotion(Dictionary<QualifiedAnimation, AnimationClip> remapping, List<QualifiedAnimation> qualifiedAnimations, Dictionary<BlendTree, BlendTree> biTreesReferences, ChildMotion copyOfChild)
        {
            switch (copyOfChild.motion)
            {
                case AnimationClip clip:
                    return remapping[qualifiedAnimations.First(animation => animation.Clip == clip)];
                case BlendTree tree:
                    return biTreesReferences[tree];
                default:
                    return copyOfChild.motion;
            }
        }

        private HashSet<QualifiedAnimation> QualifyAllAnimations()
        {
            return new HashSet<QualifiedAnimation>(_originalBindings
                .SelectMany(binding => binding.Manifest.AllQualifiedAnimations())
                .ToList());
        }

        private static ManifestBinding RemapManifest(ManifestBinding manifestBinding, Dictionary<QualifiedAnimation, AnimationClip> remapping, Dictionary<BlendTree, BlendTree> blendToRemappedBlend)
        {
            var remappedManifest = manifestBinding.Manifest.NewFromRemappedAnimations(remapping, blendToRemappedBlend);
            return new ManifestBinding(manifestBinding.StageValue, remappedManifest, manifestBinding.LayerOrdinal);
        }

        private Dictionary<QualifiedAnimation, AnimationClip> CreateAssetContainerWithNeutralizedAnimations(AssetContainer container, HashSet<QualifiedAnimation> allQualifiedAnimations, HashSet<CurveKey> allApplicableCurveKeys)
        {
            var remapping = new Dictionary<QualifiedAnimation, AnimationClip>();

            foreach (var qualifiedAnimation in allQualifiedAnimations)
            {
                var neutralizedAnimation = CopyAndNeutralize(qualifiedAnimation.Clip, allApplicableCurveKeys);

                MutateCurvesForAnimatedAnimatorParameters(neutralizedAnimation, qualifiedAnimation);

                container.AddAnimation(neutralizedAnimation);

                remapping.Add(qualifiedAnimation, neutralizedAnimation);
            }

            return remapping;
        }

        private void MutateCurvesForAnimatedAnimatorParameters(AnimationClip neutralizedAnimation, QualifiedAnimation qualifiedAnimation)
        {
            if (_compilerConflictFxLayerMode != ConflictFxLayerMode.KeepOnlyTransformsAndMuscles)
            {
                var blinking = qualifiedAnimation.Qualification.IsBlinking ? 1 : 0;
                var wide = qualifiedAnimation.Qualification.Limitation == QualifiedLimitation.Wide ? 1 : 0;
                neutralizedAnimation.SetCurve("", typeof(Animator), "_Hai_GestureAnimBlink", AnimationCurve.Linear(0, blinking, 1 / 60f, blinking));
                neutralizedAnimation.SetCurve("", typeof(Animator), "_Hai_GestureAnimWide", AnimationCurve.Linear(0, wide, 1 / 60f, wide));
            }
        }

        private AnimationClip CopyAndNeutralize(AnimationClip animationClipToBePreserved, HashSet<CurveKey> allApplicableCurveKeys)
        {
            var copyOfAnimationClip = Object.Instantiate(animationClipToBePreserved);
            copyOfAnimationClip.name = "zAutogeneratedExp_" + animationClipToBePreserved.name + "_DO_NOT_EDIT";

            var bindings = AnimationUtility.GetCurveBindings(copyOfAnimationClip);
            var thisAnimationPaths = bindings
                .Select(CurveKey.FromBinding)
                .ToList();

            AddMissingCurveKeys(allApplicableCurveKeys, thisAnimationPaths, copyOfAnimationClip);
            RemoveExistingTransformsAndMuscleAnimations(thisAnimationPaths, copyOfAnimationClip);

            return copyOfAnimationClip;
        }

        private void RemoveExistingTransformsAndMuscleAnimations(List<CurveKey> thisAnimationPaths, AnimationClip copyOfAnimationClip)
        {
            if (_compilerConflictFxLayerMode != ConflictFxLayerMode.KeepBoth)
            {
                foreach (var curveKey in thisAnimationPaths)
                {
                    var isTransformOrMuscleCurve = curveKey.IsTransformOrMuscleCurve();
                    if (_compilerConflictFxLayerMode == ConflictFxLayerMode.KeepOnlyTransformsAndMuscles ? !isTransformOrMuscleCurve : isTransformOrMuscleCurve)
                    {
                        copyOfAnimationClip.SetCurve(curveKey.Path, curveKey.Type, curveKey.PropertyName, null);
                    }
                }
            }
        }

        private void AddMissingCurveKeys(HashSet<CurveKey> allApplicableCurveKeys, List<CurveKey> thisAnimationPaths, AnimationClip copyOfAnimationClip)
        {
            foreach (var curveKey in allApplicableCurveKeys)
            {
                if (!thisAnimationPaths.Contains(curveKey))
                {
                    var fallbackValue = _curveKeyToFallbackValue.ContainsKey(curveKey) ? _curveKeyToFallbackValue[curveKey] : 0;

                    Keyframe[] keyframes = {new Keyframe(0, fallbackValue), new Keyframe(1 / 60f, fallbackValue)};
                    var curve = new AnimationCurve(keyframes);
                    copyOfAnimationClip.SetCurve(curveKey.Path, curveKey.Type, curveKey.PropertyName, curve);
                }
            }
        }

        private HashSet<CurveKey> FindAllApplicableCurveKeys(HashSet<AnimationClip> allAnimationClips)
        {
            var allCurveKeysFromAnimations = allAnimationClips
                .SelectMany(AnimationUtility.GetCurveBindings)
                .Select(CurveKey.FromBinding)
                .ToList();

            var curveKeys = new[]{allCurveKeysFromAnimations, _blinkBlendshapes}
                .SelectMany(keys => keys)
                .Where(curveKey => !_ignoreCurveKeys.Contains(curveKey))
                .Where(curveKey =>
                {
                    switch (_compilerConflictFxLayerMode)
                    {
                        case ConflictFxLayerMode.RemoveTransformsAndMuscles: return !curveKey.IsTransformOrMuscleCurve();
                        case ConflictFxLayerMode.KeepBoth: return true;
                        case ConflictFxLayerMode.KeepOnlyTransformsAndMuscles: return curveKey.IsTransformOrMuscleCurve();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                })
                .ToList();

            return new HashSet<CurveKey>(curveKeys);
        }
    }
}
