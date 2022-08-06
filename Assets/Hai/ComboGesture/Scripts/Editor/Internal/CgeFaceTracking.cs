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
    // Vendors define sensors. Each sensor may have several actuators, each actuator acts on a single element.
    // - An element can only be actuated by one actuator.
    // - If several sensors are supposed to actuate the same element, only the first sensor is selected for that element.
    // Different vendors may define different ways the sensor will actuate an element.
    // - For instance, a vendor may use EyeX to actuate an element as a Joystick from -1 to 1,
    //   but another vendor could actuate the same element as an Aperture from 0 to 1.
    // By convention, the element name is the blend shape name of a SkinnedMesh (case insensitive).
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
            // FIXME UPLOAD AVATAR FAILS

            // Prepare data structures
            var elementToActuatorDict = _faceTracking.vendor.ToElementActuators()
                .GroupBy(actuator => actuator.element)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Select(ea => ea.actuator).ToArray()
                );

            var sensorNames = elementToActuatorDict
                .SelectMany(pair => pair.Value.Select(actuator => actuator.sensorParameterName).ToArray())
                .Distinct()
                .ToArray();
            
            // Prepare animator layer
            var aac = _assetContainer.ExposeCgeAac();
            var layer = ReinitializeLayerAsMachinist();

            // Technique perfected by research lead by Razgriz, as follows.
            // To use a direct blend tree without causing conflicts in the animator:
            // - Let numberOfChildren be the children array count of the direct blend tree.
            // - To use a direct blend tree, set the weight to 1/(numberOfChildren)
            // - Multiply all values in animation clips by numberOfChildren
            //    - For example, if a blend shape is normally animated to 100 for full force,
            //      animate it to 1800 if there are 18 children in the blend tree.
            // - (Unverified) Keep "m_NormalizedBlendValues" OFF for Write Defaults stability
            var directBlendTreeBypassMultiplier = elementToActuatorDict.Keys.Count + sensorNames.Length;
            
            var normalizerParam = layer.FloatParameter("_Hai_GestureFTNormalizer");
            layer.OverrideValue(normalizerParam, 1f / directBlendTreeBypassMultiplier);
            
            var smoothingFactorParam = layer.FloatParameter("_Hai_GestureFTSmoothingFactor");
            layer.OverrideValue(smoothingFactorParam, 0.4f);
            
            var influenceParam = layer.FloatParameter(FTInfluenceParam);
            
            // Build the blend trees and animations
            var elementToBlendShapeBinding = elementToActuatorDict.Keys
                .Select(element => MakeBlendShapeAnimation(element, aac, directBlendTreeBypassMultiplier, elementToActuatorDict[element].Length))
                .ToDictionary(binding => binding.element, binding => binding);
            
            var blendShapeTrees = elementToActuatorDict
                .SelectMany(elementToActuator => elementToActuator.Value
                    .Select(actuator => MakeBlendShapeTree(elementToActuator.Key, actuator, aac, elementToBlendShapeBinding, layer))
                    .ToArray())
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

            // Collect all animated properties in generated clips
            var bindings = _generatedClips
                .SelectMany(clip => AnimationUtility.GetCurveBindings(clip.Clip))
                .ToArray();
            
            // Create muted clip
            var mutedClip = aac.NewClip()
                .Animating(clip =>
                {
                    foreach (var binding in bindings)
                    {
                        clip.Animates(binding.path, binding.type, binding.propertyName).WithOneFrame(0f);
                    }

                    clip.AnimatesAnimator(influenceParam).WithOneFrame(0f);
                });
            
            // Make sure all created clips are exhaustive
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

            // Create the animator states
            {
                var enableFaceTrackingParam = layer.BoolParameter("TEMP_EnableFaceTracking");
                layer.OverrideValue(enableFaceTrackingParam, true);
                
                var inactive = layer.NewState("Inactive").WithAnimation(mutedClip);
                var active = layer.NewState("Active").WithAnimation(octopusTree);
                inactive.TransitionsTo(active).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsTrue());
                active.TransitionsTo(inactive).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsFalse());

                if (false)
                {
                    // This only serves to help test the animator in Unity Editor. Has no use in-game. FIXME: No support for negative floats
                    var noop = aac.NewClip()
                        .Animating(clip => clip.Animates("_ignored", typeof(GameObject), "m_IsActive").WithOneFrame(0f));
                    var experimentorTree = aac.NewBlendTreeAsRaw();
                    experimentorTree.blendType = BlendTreeType.Direct;
                    experimentorTree.children = sensorNames.Select(sensorName => new ChildMotion
                    {
                        directBlendParameter = sensorName,
                        timeScale = 1f,
                        motion = noop.Clip
                    }).ToArray();
                    layer.NewState("Experimentor").WithAnimation(experimentorTree);
                }
            }

            CreateTempEyeLayers();
        }

        private void CreateTempEyeLayers()
        {
            // FIXME: This is vendor and config specific!!!
            var aac = _assetContainer.ExposeCgeAac();
            var left = aac.CreateSupportingArbitraryControllerLayer(_fx, FaceTrackingLayerName + "_EyeLeft");
            var right = aac.CreateSupportingArbitraryControllerLayer(_fx, FaceTrackingLayerName + "_EyeRight");
            FillIn(left, aac, _faceTracking.eyeLeftCenter, _faceTracking.eyeLeftDown, _faceTracking.eyeLeftUp, _faceTracking.eyeLeftLeft, _faceTracking.eyeLeftRight, left.FloatParameter("LeftEyeX__FPInterp"), left.FloatParameter("EyesY__FPInterp"));
            FillIn(right, aac, _faceTracking.eyeRightCenter, _faceTracking.eyeRightDown, _faceTracking.eyeRightUp, _faceTracking.eyeRightLeft, _faceTracking.eyeRightRight, left.FloatParameter("RightEyeX__FPInterp"), left.FloatParameter("EyesY__FPInterp"));
            
            // FIXME should be FT not FP
        }

        private void FillIn(CgeAacFlLayer layer, CgeAacFlBase aac, AnimationClip center, AnimationClip down, AnimationClip up, AnimationClip left, AnimationClip right, CgeAacFlFloatParameter x, CgeAacFlFloatParameter y)
        {
            var blendTree = aac.NewBlendTreeAsRaw();
            blendTree.blendType = BlendTreeType.SimpleDirectional2D;
            blendTree.blendParameter = x.Name;
            blendTree.blendParameterY = y.Name;
            var centerCopy = aac.CopyClip(center);
            blendTree.children = new[]
            {
                new ChildMotion { motion = centerCopy.Clip, position = Vector2.zero, timeScale = 1f },
                new ChildMotion { motion = aac.CopyClip(down).Clip, position = Vector2.down, timeScale = 1f },
                new ChildMotion { motion = aac.CopyClip(up).Clip, position = Vector2.up, timeScale = 1f },
                new ChildMotion { motion = aac.CopyClip(left).Clip, position = Vector2.left, timeScale = 1f },
                new ChildMotion { motion = aac.CopyClip(right).Clip, position = Vector2.right, timeScale = 1f },
            };
            
            var enableFaceTrackingParam = layer.BoolParameter("TEMP_EnableFaceTracking");
            var inactive = layer.NewState("Inactive").WithAnimation(centerCopy);
            var active = layer.NewState("Active").WithAnimation(blendTree);
            inactive.TransitionsTo(active).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsTrue());
            active.TransitionsTo(inactive).WithTransitionDurationSeconds(0.1f).When(enableFaceTrackingParam.IsFalse());
        }

        private BlendShapeAnimation MakeBlendShapeAnimation(string element, CgeAacFlBase aac, int directBlendTreeBypassMultiplier, int actuatorCountForThisElement)
        {
            var elementNameLower = element.ToLowerInvariant();
            var neutral = CreateNewClip(aac);
            var actuated = CreateNewClip(aac);
            foreach (var renderer in _faceTracking.automaticAnimations)
            {
                var blendShapes = FindBlendShapes(renderer, elementNameLower);
                foreach (var blendShape in blendShapes)
                {
                    neutral.BlendShape(renderer, blendShape, 0f);
                    actuated.BlendShape(renderer, blendShape, 100f * directBlendTreeBypassMultiplier / actuatorCountForThisElement);
                }
            }

            return new BlendShapeAnimation
            {
                element = element,
                neutral = neutral.Clip,
                actuated = actuated.Clip
            };
        }

        private static BlendTree MakeBlendShapeTree(string element, CgeActuator actuator, CgeAacFlBase aac, Dictionary<string, BlendShapeAnimation> elementToBlendShapeBinding, CgeAacFlLayer layer)
        {
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
                // This is the smoothing interpolator, where 0 is "take none of the previous value" and 1 is "take all of the previous value"
                factorTree.minThreshold = 0;
                factorTree.maxThreshold = 1;
                factorTree.useAutomaticThresholds = false;
                factorTree.children = new[]
                {
                    new ChildMotion {motion = sensorTree, timeScale = 1, threshold = 0},
                    new ChildMotion {motion = identityTree, timeScale = 1, threshold = 1}
                };
            }
            return factorTree;
        }

        private static string SensorNameToInterpolatedParameterName(string sensorName)
        {
            return sensorName + "__FPInterp"; // FIXME should be FT not FP
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
            public string element;
            public Motion neutral;
            public Motion actuated;
        }

        private static string[] FindBlendShapes(SkinnedMeshRenderer renderer, string elementNameLower)
        {
            // Prevent cases where a standard name might be also a prefix of another standard name, i.e. Example_Eye_X and Example_Eye_X_Append
            var elementNamePrefix = $"{elementNameLower}__";
            
            var found = new List<string>();
            // This is inefficient but whatever
            for (var i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
            {
                var originalBlendShapeName = renderer.sharedMesh.GetBlendShapeName(i);
                var blendShapeNameLower = originalBlendShapeName.ToLowerInvariant();
                if (blendShapeNameLower == elementNameLower || blendShapeNameLower.StartsWith(elementNamePrefix))
                {
                    found.Add(originalBlendShapeName);
                }
            }

            return found.ToArray();
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