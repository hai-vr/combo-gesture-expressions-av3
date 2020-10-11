using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    class AnimationNeutralizer
    {
        private readonly List<ActivityManifest> _originalActivityManifests;
        private readonly ConflictFxLayerMode _compilerConflictFxLayerMode;
        private readonly HashSet<CurveKey> _ignoreCurveKeys;
        private readonly Dictionary<CurveKey, float> _curveKeyToFallbackValue;
        private readonly List<CurveKey> _blinkBlendshapes;
        private readonly AssetContainer _assetContainer;

        public AnimationNeutralizer(List<ActivityManifest> originalActivityManifests,
            ConflictFxLayerMode compilerConflictFxLayerMode,
            AnimationClip compilerIgnoreParamList,
            AnimationClip compilerFallbackParamList,
            List<CurveKey> blinkBlendshapes,
            AssetContainer assetContainer)
        {
            _originalActivityManifests = originalActivityManifests;
            _compilerConflictFxLayerMode = compilerConflictFxLayerMode;
            _ignoreCurveKeys = compilerIgnoreParamList == null ? new HashSet<CurveKey>() : ExtractAllCurvesOf(compilerIgnoreParamList);
            _curveKeyToFallbackValue = compilerFallbackParamList == null ? new Dictionary<CurveKey, float>() : ExtractFirstKeyframeValueOf(compilerFallbackParamList);
            _blinkBlendshapes = blinkBlendshapes;
            _assetContainer = assetContainer;
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

        internal List<ActivityManifest> NeutralizeManifestAnimations()
        {
            var allQualifiedAnimations = QualifyAllAnimations();
            var allApplicableCurveKeys = FindAllApplicableCurveKeys(new HashSet<AnimationClip>(allQualifiedAnimations.Select(animation => animation.Clip).ToList()));

            var remapping = CreateAssetContainerWithNeutralizedAnimations(_assetContainer, allQualifiedAnimations, allApplicableCurveKeys);
            return _originalActivityManifests.Select(manifest => RemapManifest(manifest, remapping)).ToList();
        }

        private HashSet<RawGestureManifest.QualifiedAnimation> QualifyAllAnimations()
        {
            return new HashSet<RawGestureManifest.QualifiedAnimation>(_originalActivityManifests
                .SelectMany(manifest => manifest.Manifest.AnimationClips().Select(clip => manifest.Manifest.Qualify(clip)))
                .ToList());
        }

        private static ActivityManifest RemapManifest(ActivityManifest manifest, Dictionary<RawGestureManifest.QualifiedAnimation, AnimationClip> remapping)
        {
            var remappedManifest = manifest.Manifest.NewFromRemappedAnimations(remapping);
            return new ActivityManifest(manifest.StageValue, remappedManifest, manifest.LayerOrdinal);
        }

        private Dictionary<RawGestureManifest.QualifiedAnimation, AnimationClip> CreateAssetContainerWithNeutralizedAnimations(AssetContainer container, HashSet<RawGestureManifest.QualifiedAnimation> allQualifiedAnimations, HashSet<CurveKey> allApplicableCurveKeys)
        {
            var remapping = new Dictionary<RawGestureManifest.QualifiedAnimation, AnimationClip>();

            foreach (var qualifiedAnimation in allQualifiedAnimations)
            {
                var neutralizedAnimation = CopyAndNeutralize(qualifiedAnimation.Clip, allApplicableCurveKeys);

                MutateCurvesForAnimatedAnimatorParameters(neutralizedAnimation, qualifiedAnimation);

                container.AddAnimation(neutralizedAnimation);

                remapping.Add(qualifiedAnimation, neutralizedAnimation);
            }

            AssetContainer.GlobalSave();

            return remapping;
        }

        private void MutateCurvesForAnimatedAnimatorParameters(AnimationClip neutralizedAnimation, RawGestureManifest.QualifiedAnimation qualifiedAnimation)
        {
            if (_compilerConflictFxLayerMode != ConflictFxLayerMode.KeepOnlyTransformsAndMuscles)
            {
                var blinking = qualifiedAnimation.Qualification.IsBlinking ? 1 : 0;
                var wide = qualifiedAnimation.Qualification.Limitation == RawGestureManifest.QualifiedLimitation.Wide ? 1 : 0;
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
