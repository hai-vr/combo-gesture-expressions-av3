using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeFaceTracking
    {
        private const string FaceTrackingLayerName = "Hai_GestureFaceTracking";
        public const string FTInfluenceParam = "_Hai_GestureFTInfluence";

        private readonly ComboGestureFaceTracking _faceTracking;
        private readonly AnimatorController _fx;
        private readonly CgeAssetContainer _assetContainer;
        private readonly List<CgeAacFlClip> _generatedClips = new List<CgeAacFlClip>();

        internal CgeFaceTracking(ComboGestureFaceTracking faceTracking, AnimatorController fx, CgeAssetContainer assetContainer)
        {
            _faceTracking = faceTracking;
            _fx = fx;
            _assetContainer = assetContainer;
        }

        public void DoOverwriteFaceTrackingLayer()
        {
            // FIXME WRITE DEFAULTS
            // FIXME AVATAR MASK
            var aac = _assetContainer.ExposeCgeAac();
            var layer = ReinitializeLayerAsMachinist();

            var elementToActuatorDict = _faceTracking.vendor.ToElementActuators()
                .GroupBy(actuator => actuator.element)
                .ToDictionary(
                    grouping => grouping.Key,
                    // Actuation might contain duplicates in a misconfiguration, so take the first
                    grouping => grouping.First().actuator
                );

            var sensorNames = elementToActuatorDict
                .Select(pair => pair.Value.sensorParameterName)
                .Distinct()
                .ToArray();

            var directBlendTreeBypassMultiplier = elementToActuatorDict.Keys.Count + sensorNames.Length;
            
            var normalizerParam = layer.FloatParameter("_Hai_GestureFTNormalizer");
            layer.OverrideValue(normalizerParam, 1f / directBlendTreeBypassMultiplier);
            
            var smoothingFactorParam = layer.FloatParameter("_Hai_GestureFTSmoothingFactor");
            layer.OverrideValue(smoothingFactorParam, 0.2f);
            
            var influenceParam = layer.FloatParameter(FTInfluenceParam);
            
            var elementToBlendShapeBinding = elementToActuatorDict.Keys
                .Select(element => MakeBlendShapeAnimation(element, aac, directBlendTreeBypassMultiplier))
                .ToDictionary(binding => binding.element, binding => binding);
            
            var blendShapeTrees = elementToActuatorDict
                .Select(elementToActuator => MakeBlendShapeTree(elementToActuator, aac, elementToBlendShapeBinding, layer))
                .ToArray();
            var sensorInterpolatorTrees = sensorNames
                .Select(sensorName => MakeSensorInterpolatorTree(sensorName, aac, layer, smoothingFactorParam, directBlendTreeBypassMultiplier))
                .ToArray();
            var allTrees = blendShapeTrees.Concat(sensorInterpolatorTrees);
            
            var octopusTree = aac.NewBlendTreeAsRaw();
            octopusTree.blendType = BlendTreeType.Direct;
            octopusTree.children = allTrees.Select(blendShapeTree => new ChildMotion
            {
                directBlendParameter = normalizerParam.Name,
                motion = blendShapeTree,
                timeScale = 1f
            }).ToArray();
            
            // var soOctopus = new SerializedObject(octopusTree);
            // soOctopus.FindProperty("m_NormalizedBlendValues").boolValue = true;
            // soOctopus.ApplyModifiedProperties();

            var bindings = _generatedClips
                .SelectMany(clip => AnimationUtility.GetCurveBindings(clip.Clip))
                .ToArray();
            var mutedClip = aac.NewClip()
                .Animating(clip =>
                {
                    foreach (var binding in bindings)
                    {
                        clip.Animates(binding.path, binding.type, binding.propertyName).WithOneFrame(0f);
                    }

                    clip.AnimatesAnimator(influenceParam).WithOneFrame(0f);
                });
            
            foreach (var generatedClip in _generatedClips)
            {
                foreach (var binding in bindings)
                {
                    var existingCurve = AnimationUtility.GetEditorCurve(generatedClip.Clip, binding);
                    if (existingCurve == null)
                    {
                        generatedClip.Animating(clip => clip.Animates(binding.path, binding.type, binding.propertyName).WithOneFrame(0f));
                    }
                }

                generatedClip.Animating(clip => clip.AnimatesAnimator(influenceParam).WithOneFrame(1f));
            }

            {
                var enableFaceTrackingParam = layer.BoolParameter("TEMP_EnableFaceTracking");
                layer.OverrideValue(enableFaceTrackingParam, true);
                
                var inactive = layer.NewState("Inactive").WithAnimation(mutedClip);
                var active = layer.NewState("Active").WithAnimation(octopusTree);
                inactive.TransitionsTo(active).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsTrue());
                active.TransitionsTo(inactive).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsFalse());
            }
        }

        private BlendShapeAnimation MakeBlendShapeAnimation(CgeElement element, CgeAacFlBase aac, int directBlendTreeBypassMultiplier)
        {
            var elementNameLower = Enum.GetName(typeof(CgeElement), element).ToLowerInvariant();
            var neutral = CreateNewClip(aac);
            var actuated = CreateNewClip(aac);
            foreach (var renderer in _faceTracking.automaticAnimations)
            {
                var blendShape = FindBlendShapeOrNull(renderer, elementNameLower);
                if (blendShape != null)
                {
                    neutral.BlendShape(renderer, blendShape, 0f);
                    actuated.BlendShape(renderer, blendShape, 100f * directBlendTreeBypassMultiplier);
                }
            }

            return new BlendShapeAnimation
            {
                element = element,
                neutral = neutral.Clip,
                actuated = actuated.Clip
            };
        }

        private static BlendTree MakeBlendShapeTree(KeyValuePair<CgeElement, CgeActuator> elementToActuator, CgeAacFlBase aac, Dictionary<CgeElement, BlendShapeAnimation> elementToBlendShapeBinding, CgeAacFlLayer layer)
        {
            var element = elementToActuator.Key;
            var actuator = elementToActuator.Value;

            var blendTree = aac.NewBlendTreeAsRaw();
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = layer.FloatParameter(SensorNameToInterpolatedParameterName(actuator.sensorParameterName)).Name;
            blendTree.useAutomaticThresholds = false;
            blendTree.children = new[]
            {
                new ChildMotion
                {
                    threshold = actuator.neutral,
                    motion = elementToBlendShapeBinding[element].neutral,
                    timeScale = 1f
                },
                new ChildMotion
                {
                    threshold = actuator.actuated,
                    motion = elementToBlendShapeBinding[element].actuated,
                    timeScale = 1f
                }
            }.OrderBy(motion => motion.threshold).ToArray();

            return blendTree;
        }

        private BlendTree MakeSensorInterpolatorTree(string sensorName, CgeAacFlBase aac, CgeAacFlLayer layer, CgeAacFlFloatParameter factorParam, int directBlendTreeBypassMultiplier)
        {
            var sensorParameter = layer.FloatParameter(sensorName);
            var interpolatedParameter = layer.FloatParameter(SensorNameToInterpolatedParameterName(sensorName));

            var negativeClip = aac.NewClip().Animating(clip => clip.AnimatesAnimator(interpolatedParameter).WithOneFrame(-1f * directBlendTreeBypassMultiplier));
            var positiveClip = aac.NewClip().Animating(clip => clip.AnimatesAnimator(interpolatedParameter).WithOneFrame(1f * directBlendTreeBypassMultiplier));
            _generatedClips.Add(negativeClip);
            _generatedClips.Add(positiveClip);
            
            var sensorTree = CreateProxyTree(aac, sensorParameter, negativeClip, positiveClip);
            var identityTree = CreateProxyTree(aac, interpolatedParameter, negativeClip, positiveClip);
            var factorTree = aac.NewBlendTreeAsRaw();
            {
                factorTree.blendParameter = factorParam.Name;
                factorTree.blendType = BlendTreeType.Simple1D;
                factorTree.minThreshold = -1;
                factorTree.maxThreshold = 1;
                factorTree.useAutomaticThresholds = false;
                factorTree.children = new[]
                {
                    new ChildMotion {motion = sensorTree, timeScale = 1, threshold = -1},
                    new ChildMotion {motion = identityTree, timeScale = 1, threshold = 1}
                };
            }
            return factorTree;
        }

        private static string SensorNameToInterpolatedParameterName(string sensorName)
        {
            return sensorName + "__FPInterp";
        }

        private BlendTree CreateProxyTree(CgeAacFlBase aac, CgeAacFlFloatParameter parameter, CgeAacFlClip negativeClip, CgeAacFlClip positiveClip)
        {
            var proxyTree = aac.NewBlendTreeAsRaw();
            proxyTree.blendParameter = parameter.Name;
            proxyTree.blendType = BlendTreeType.Simple1D;
            proxyTree.minThreshold = -1;
            proxyTree.maxThreshold = 1;
            proxyTree.useAutomaticThresholds = false;
            proxyTree.children = new[]
            {
                new ChildMotion {motion = negativeClip.Clip, timeScale = 1, threshold = -1},
                new ChildMotion {motion = positiveClip.Clip, timeScale = 1, threshold = 1}
            };
            return proxyTree;
        }

        private struct BlendShapeAnimation
        {
            public CgeElement element;
            public Motion neutral;
            public Motion actuated;
        }

        private static string FindBlendShapeOrNull(SkinnedMeshRenderer renderer, string elementNameLower)
        {
            // This is inefficient but whatever
            for (var i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
            {
                var originalBlendShapeName = renderer.sharedMesh.GetBlendShapeName(i);
                var blendShapeNameLower = originalBlendShapeName.ToLowerInvariant();
                if (blendShapeNameLower == elementNameLower)
                {
                    return originalBlendShapeName;
                }
            }

            return null;
        }

        private CgeAacFlClip CreateNewClip(CgeAacFlBase aac)
        {
            var generated = aac.NewClip();
            _generatedClips.Add(generated);
            return generated
                .Animating(clip => clip.Animates("_ignored", typeof(GameObject), "m_IsActive").WithOneFrame(0f));
        }

        private CgeAacFlLayer ReinitializeLayerAsMachinist()
        {
            return _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_fx, FaceTrackingLayerName);

            // TODO: AvatarMask
        }
    }
}