using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using Hai.ComboGesture.Scripts.Editor.Internal.Model;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class GestureCExpressionCombiner
    {
        private readonly AssetContainer _assetContainer;
        private CgeAacFlLayer _layer;
        private readonly List<IComposedBehaviour> _composedBehaviours;
        private readonly string _activityStageName;
        private readonly bool _writeDefaultsForFaceExpressions;
        private readonly bool _useGestureWeightCorrection;
        private readonly bool _useSmoothing;
        private readonly CgeAacFlState _defaultState;

        public GestureCExpressionCombiner(AssetContainer assetContainer, CgeAacFlLayer layer,
            List<IComposedBehaviour> composedBehaviours, string activityStageName, bool writeDefaultsForFaceExpressions, bool useGestureWeightCorrection, bool useSmoothing, CgeAacFlState defaultState)
        {
            _assetContainer = assetContainer;
            _layer = layer;
            _activityStageName = activityStageName;
            _writeDefaultsForFaceExpressions = writeDefaultsForFaceExpressions;
            _useGestureWeightCorrection = useGestureWeightCorrection;
            _useSmoothing = useSmoothing;
            _defaultState = defaultState;
            _composedBehaviours = composedBehaviours;
        }

        public void Populate()
        {
            var intern = _layer.NewSubStateMachine("Internal");
            intern.Restarts();
            _defaultState.CGE_AutomaticallyMovesTo(intern);

            var ssms = _composedBehaviours
                .Select(behaviour => intern.NewSubStateMachine($"Activity {behaviour.StageValue}"))
                .ToArray();

            for (var index = 0; index < ssms.Length; index++)
            {
                var ssm = ssms[index];
                var composed = _composedBehaviours[index];
                switch (composed)
                {
                    case PermutationComposedBehaviour pcb:
                        BuildPcb(ssm, pcb);
                        break;
                    case OneHandComposedBehaviour ocb:
                        BuildOcb(ssm, ocb);
                        break;
                    case SingularComposedBehaviour scb:
                        BuildScb(ssm, scb);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ssm.Restarts().When(_layer.IntParameter(_activityStageName).IsEqualTo(composed.StageValue));
                ssm.Exits();
            }

            if (_activityStageName != null)
            {
                // Order of execution matters here
                for (var index = 0; index < ssms.Length; index++)
                {
                    var destSsm = ssms[index];
                    var composed = _composedBehaviours[index];
                    intern.EntryTransitionsTo(destSsm)
                        .When(_layer.IntParameter(_activityStageName).IsEqualTo(composed.StageValue));
                }

                // Order of execution matters here
                var neutral = intern.NewState("Neutral");
                intern.WithDefaultState(neutral);
                intern.EntryTransitionsTo(neutral);

                foreach (var composedBehaviour in _composedBehaviours)
                {
                    neutral.Exits()
                        .When(_layer.IntParameter(_activityStageName).IsEqualTo(composedBehaviour.StageValue));
                }
            }
            else
            {
                foreach (var destSsm in ssms)
                {
                    intern.EntryTransitionsTo(destSsm);
                }
            }
        }

        private void BuildPcb(CgeAacFlStateMachine ssm, PermutationComposedBehaviour composed)
        {
            foreach (var pair in composed.Behaviors)
            {
                var permutation = pair.Key;
                var behavior = pair.Value;

                var state = AppendToSsm(ssm, behavior).At((int)permutation.Right, (int)permutation.Left);
                ssm.EntryTransitionsTo(state)
                    .When(_layer.Av3().GestureLeft.IsEqualTo((int) permutation.Left))
                    .And(_layer.Av3().GestureRight.IsEqualTo((int) permutation.Right));
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(_layer.Av3().GestureLeft.IsNotEqualTo((int) permutation.Left))
                    .Or().When(_layer.Av3().GestureRight.IsNotEqualTo((int) permutation.Right));
                if (_activityStageName != null)
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(_layer.IntParameter(_activityStageName).IsNotEqualTo(composed.StageValue));
                }
            }
        }

        private void BuildOcb(CgeAacFlStateMachine ssm, OneHandComposedBehaviour composed)
        {
            var which = composed.IsLeftHand ? _layer.Av3().GestureLeft : _layer.Av3().GestureRight;
            foreach (var pair in composed.Behaviors)
            {
                var handPose = pair.Key;
                var behavior = pair.Value;

                var state = AppendToSsm(ssm, behavior).At(0, (int)handPose);
                ssm.EntryTransitionsTo(state)
                    .When(which.IsEqualTo((int) handPose));
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(which.IsNotEqualTo((int) handPose));
                if (_activityStageName != null)
                {
                    state.Exits()
                        .WithTransitionDurationSeconds(composed.TransitionDuration)
                        .When(_layer.IntParameter(_activityStageName).IsNotEqualTo(composed.StageValue));
                }
            }
        }

        private void BuildScb(CgeAacFlStateMachine ssm, SingularComposedBehaviour composed)
        {
            var state = AppendToSsm(ssm, composed.Behavior);
            ssm.EntryTransitionsTo(state);
            if (_activityStageName != null)
            {
                state.Exits()
                    .WithTransitionDurationSeconds(composed.TransitionDuration)
                    .When(_layer.IntParameter(_activityStageName).IsNotEqualTo(composed.StageValue));
            }
        }

        private CgeAacFlState AppendToSsm(CgeAacFlStateMachine ssm, IAnimatedBehavior behaviour)
        {
            switch (behaviour.Nature())
            {
                case AnimatedBehaviorNature.Single:
                    var sab = (SingleAnimatedBehavior)behaviour;
                    return ForSingle(ssm, sab);
                case AnimatedBehaviorNature.Analog:
                    AnalogAnimatedBehavior aab = (AnalogAnimatedBehavior)behaviour;
                    return ForAnalog(ssm, aab.Squeezing.Clip, aab.Resting.Clip, aab.HandSide);
                case AnimatedBehaviorNature.PuppetToAnalog:
                    PuppetToAnalogAnimatedBehavior ptaab = (PuppetToAnalogAnimatedBehavior)behaviour;
                    return ForAnalog(ssm, ptaab.Squeezing.Clip, ptaab.Resting, ptaab.HandSide);
                case AnimatedBehaviorNature.DualAnalog:
                    DualAnalogAnimatedBehavior daab = (DualAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, daab.BothSqueezing.Clip, daab.Resting.Clip, daab.LeftSqueezing.Clip, daab.RightSqueezing.Clip);
                case AnimatedBehaviorNature.PuppetToDualAnalog:
                    PuppetToDualAnalogAnimatedBehavior ptdaab = (PuppetToDualAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, ptdaab.BothSqueezing.Clip, ptdaab.Resting, ptdaab.LeftSqueezing.Clip, ptdaab.RightSqueezing.Clip);
                case AnimatedBehaviorNature.Puppet:
                    var pab = (PuppetAnimatedBehavior)behaviour;
                    return ForPuppet(ssm, pab);
                case AnimatedBehaviorNature.SimpleMassiveBlend:
                    SimpleMassiveBlendAnimatedBehavior smbab = (SimpleMassiveBlendAnimatedBehavior)behaviour;
                    return ForSimpleMassiveBlend(ssm, smbab.Zero, smbab.One, smbab.ParameterName);
                case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                    TwoDirectionsMassiveBlendAnimatedBehavior tdmb = (TwoDirectionsMassiveBlendAnimatedBehavior)behaviour;
                    return ForTwoDirectionsMassiveBlend(ssm, tdmb.Zero, tdmb.One, tdmb.MinusOne, tdmb.ParameterName);
                case AnimatedBehaviorNature.ComplexMassiveBlend:
                    ComplexMassiveBlendAnimatedBehavior cbtmbab = (ComplexMassiveBlendAnimatedBehavior)behaviour;
                    return ForComplexMassiveBlend(ssm, cbtmbab.Behaviors, cbtmbab.OriginalBlendTreeTemplate);
                case AnimatedBehaviorNature.UniversalAnalog:
                    UniversalAnalogAnimatedBehavior uaab = (UniversalAnalogAnimatedBehavior)behaviour;
                    return ForDualAnalog(ssm, uaab.BothSqueezing.Clip, uaab.Resting.ToMotion(), uaab.LeftSqueezing.ToMotion(), uaab.RightSqueezing.ToMotion());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Motion Derive(IAnimatedBehavior behavior)
        {
            switch (behavior.Nature())
            {
                case AnimatedBehaviorNature.Single:
                    return ((SingleAnimatedBehavior)behavior).Posing.Clip;
                case AnimatedBehaviorNature.Analog:
                    AnalogAnimatedBehavior aab = (AnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        aab.Resting.Clip,
                        aab.Squeezing.Clip,
                        aab.HandSide == HandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing),
                        SanitizeName(UnshimName(aab.Resting.Clip.name) + " MB " + UnshimName((aab.Squeezing.Clip.name))));
                case AnimatedBehaviorNature.PuppetToAnalog:
                    PuppetToAnalogAnimatedBehavior pta = (PuppetToAnalogAnimatedBehavior)behavior;
                    return CreateBlendTree(
                        pta.Resting,
                        pta.Squeezing.Clip,
                        pta.HandSide == HandSide.LeftHand
                            ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                            : RightParam(_useGestureWeightCorrection, _useSmoothing),
                        SanitizeName(UnshimName(pta.Resting.name) + " MB " + UnshimName((pta.Squeezing.Clip.name))));
                case AnimatedBehaviorNature.DualAnalog:
                    DualAnalogAnimatedBehavior da = (DualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(da.Resting.Clip, da.BothSqueezing.Clip, da.LeftSqueezing.Clip, da.LeftSqueezing.Clip, SanitizeName(UnshimName(da.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.PuppetToDualAnalog:
                    PuppetToDualAnalogAnimatedBehavior ptda = (PuppetToDualAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(ptda.Resting, ptda.BothSqueezing.Clip, ptda.LeftSqueezing.Clip, ptda.LeftSqueezing.Clip, SanitizeName(UnshimName(ptda.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.Puppet:
                    return ((PuppetAnimatedBehavior)behavior).Tree;
                case AnimatedBehaviorNature.UniversalAnalog:
                    UniversalAnalogAnimatedBehavior uaab = (UniversalAnalogAnimatedBehavior)behavior;
                    return CreateDualBlendTree(uaab.Resting.ToMotion(), uaab.BothSqueezing.Clip, uaab.LeftSqueezing.ToMotion(), uaab.LeftSqueezing.ToMotion(), SanitizeName(UnshimName(uaab.BothSqueezing.Clip.name)), _useGestureWeightCorrection, _useSmoothing);
                case AnimatedBehaviorNature.SimpleMassiveBlend:
                case AnimatedBehaviorNature.TwoDirectionsMassiveBlend:
                case AnimatedBehaviorNature.ComplexMassiveBlend:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgeAacFlState ForSimpleMassiveBlend(CgeAacFlStateMachine ssm, IAnimatedBehavior zero, IAnimatedBehavior one, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            return CreateSimpleMassiveBlendState(zeroMotion, oneMotion, parameterName, ssm);
        }

        private CgeAacFlState ForTwoDirectionsMassiveBlend(CgeAacFlStateMachine ssm, IAnimatedBehavior zero, IAnimatedBehavior one, IAnimatedBehavior minusOne, string parameterName)
        {
            var zeroMotion = Derive(zero);
            var oneMotion = Derive(one);
            var minusOneMotion = Derive(minusOne);
            return CreateTwoDirectionsMassiveBlendState(zeroMotion, oneMotion, minusOneMotion, parameterName, ssm);
        }

        private CgeAacFlState ForComplexMassiveBlend(CgeAacFlStateMachine ssm, List<IAnimatedBehavior> behaviors, BlendTree originalBlendTreeTemplate)
        {
            var motions = behaviors.Select(Derive).ToList();
            return CreateComplexMassiveBlendState(motions, originalBlendTreeTemplate, ssm);
        }

        private CgeAacFlState CreateSimpleMassiveBlendState(Motion zero, Motion one, string parameterName, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(zero.name) + " massive " + UnshimName(one.name);
            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(CreateBlendTree(
                    zero,
                    one,
                    parameterName,
                    clipName))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreateTwoDirectionsMassiveBlendState(Motion zero, Motion one, Motion minusOne, string parameterName, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(zero.name) + " - " + UnshimName(one.name) + " - " + UnshimName(minusOne.name);
            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = parameterName,
                blendType = BlendTreeType.Simple1D,
                minThreshold = -1,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                    { new ChildMotion {motion = minusOne, timeScale = 1}, new ChildMotion {motion = zero, timeScale = 1}, new ChildMotion {motion = one, timeScale = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(blendTree);

            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(blendTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState ForSingle(CgeAacFlStateMachine ssm, SingleAnimatedBehavior intermediateAnimationGroup)
        {
            return CreateMotionState(intermediateAnimationGroup.Posing.Clip, ssm);
        }

        private CgeAacFlState CreateComplexMassiveBlendState(List<Motion> motions, BlendTree originalBlendTreeTemplate, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(originalBlendTreeTemplate.name) + " complex";

            var newBlendTree = CopyTreeIdentically(originalBlendTreeTemplate);
            newBlendTree.children = newBlendTree.children
                .Select((childMotion, index) =>
                {
                    var copy = childMotion;
                    copy.motion = motions[index];
                    return copy;
                })
                .ToArray();

            RegisterBlendTreeAsAsset(newBlendTree);

            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(newBlendTree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private BlendTree CopyTreeIdentically(BlendTree originalTree)
        {
            var newTree = new BlendTree();

            // Object.Instantiate(...) is triggering some weird issues about assertions failures.
            // Copy the blend tree manually
            newTree.name = "autoBT_" + originalTree.name;
            newTree.blendType = originalTree.blendType;
            newTree.blendParameter = originalTree.blendParameter;
            newTree.blendParameterY = originalTree.blendParameterY;
            newTree.minThreshold = originalTree.minThreshold;
            newTree.maxThreshold = originalTree.maxThreshold;
            newTree.useAutomaticThresholds = originalTree.useAutomaticThresholds;

            var copyOfChildren = originalTree.children;
            while (newTree.children.Length > 0) {
                newTree.RemoveChild(0);
            }

            newTree.children = copyOfChildren
                .Select(childMotion => new ChildMotion
                {
                    motion = childMotion.motion,
                    threshold = childMotion.threshold,
                    position = childMotion.position,
                    timeScale = childMotion.timeScale,
                    cycleOffset = childMotion.cycleOffset,
                    directBlendParameter = childMotion.directBlendParameter,
                    mirror = childMotion.mirror
                })
                .ToArray();

            return newTree;
        }

        private CgeAacFlState ForAnalog(CgeAacFlStateMachine ssm, Motion squeezing, Motion resting, HandSide handSide)
        {
            return CreateSidedBlendState(squeezing, resting, handSide == HandSide.LeftHand, ssm);
        }

        private CgeAacFlState ForDualAnalog(CgeAacFlStateMachine ssm, Motion bothSqueezing, Motion resting, Motion leftSqueezingClip, Motion rightSqueezingClip)
        {
            return CreateDualBlendState(bothSqueezing,
                resting,
                leftSqueezingClip, rightSqueezingClip, ssm);
        }

        private CgeAacFlState ForPuppet(CgeAacFlStateMachine ssm, PuppetAnimatedBehavior intermediateAnimationGroup)
        {
            return CreatePuppetState(intermediateAnimationGroup.Tree, ssm);
        }

        private CgeAacFlState CreateMotionState(AnimationClip clip, CgeAacFlStateMachine ssm)
        {
            return ssm.NewState(SanitizeName(UnshimName(clip.name)))
                .WithAnimation(clip)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreateSidedBlendState(Motion squeezing, Motion resting, bool isLeftSide, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(squeezing.name) + " " + (isLeftSide ? "BlendLeft" : "BlendRight") + " " + UnshimName(resting.name);
            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(CreateBlendTree(
                    resting,
                    squeezing,
                    _layer.FloatParameter(isLeftSide
                        ? LeftParam(_useGestureWeightCorrection, _useSmoothing)
                        : RightParam(_useGestureWeightCorrection, _useSmoothing)).Name,
                    clipName))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private static string LeftParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return SharedLayerUtils.HaiGestureComboLeftWeightSmoothing;

            if (useGestureWeightCorrection)
                return SharedLayerUtils.HaiGestureComboLeftWeightProxy;

            return "GestureLeftWeight";
        }

        private static string RightParam(bool useGestureWeightCorrection, bool useSmoothing)
        {
            if (useSmoothing)
                return SharedLayerUtils.HaiGestureComboRightWeightSmoothing;

            if (useGestureWeightCorrection)
                return SharedLayerUtils.HaiGestureComboRightWeightProxy;

            return "GestureRightWeight";
        }

        private CgeAacFlState CreateDualBlendState(Motion clip, Motion resting, Motion posingLeft, Motion posingRight, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(clip.name) + " Dual " + UnshimName(resting.name);
            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(CreateDualBlendTree(resting, clip, posingLeft, posingRight, clipName, _useGestureWeightCorrection, _useSmoothing))
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        private CgeAacFlState CreatePuppetState(BlendTree tree, CgeAacFlStateMachine ssm)
        {
            var clipName = UnshimName(tree.name) + " Puppet";
            return ssm.NewState(SanitizeName(clipName))
                .WithAnimation(tree)
                .WithWriteDefaultsSetTo(_writeDefaultsForFaceExpressions);
        }

        public Motion CreateBlendTree(Motion atZero, Motion atOne, string weight, string clipName)
        {
            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = weight,
                blendType = BlendTreeType.Simple1D,
                minThreshold = 0,
                maxThreshold = 1,
                useAutomaticThresholds = true,
                children = new[]
                    {new ChildMotion {motion = atZero, timeScale = 1}, new ChildMotion {motion = atOne, timeScale = 1}},
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(blendTree);

            return blendTree;
        }

        private Motion CreateDualBlendTree(Motion atZero, Motion atOne, Motion atLeft, Motion atRight, string clipName, bool useGestureWeightCorrection, bool useSmoothing)
        {
            ChildMotion[] motions;
            if (atOne == atLeft && atOne == atRight)
            {
                motions = new[]
                {
                    new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.up},
                };
            }
            else
            {
                motions = new[]
                {
                    new ChildMotion {motion = atZero, timeScale = 1, position = Vector2.zero},
                    new ChildMotion {motion = atLeft, timeScale = 1, position = Vector2.right},
                    new ChildMotion {motion = atRight, timeScale = 1, position = Vector2.up},
                    new ChildMotion {motion = atOne, timeScale = 1, position = Vector2.right + Vector2.up},
                };
            }


            var blendTree = new BlendTree
            {
                name = "autoBT_" + clipName,
                blendParameter = LeftParam(useGestureWeightCorrection, useSmoothing),
                blendParameterY = RightParam(useGestureWeightCorrection, useSmoothing),
                blendType = BlendTreeType.FreeformDirectional2D,
                children = motions,
                hideFlags = HideFlags.HideInHierarchy
            };

            RegisterBlendTreeAsAsset(blendTree);

            return blendTree;
        }

        private void RegisterBlendTreeAsAsset(BlendTree blendTree)
        {
            _assetContainer.ExposeCgeAac().CGE_StoringMotion(blendTree);
        }

        private static string UnshimName(string shimmedName)
        {
            return shimmedName
                .Replace("zAutogeneratedExp_", "")
                .Replace("zAutogeneratedPup_", "")
                .Replace("_DO_NOT_EDIT", "");
        }

        private static string SanitizeName(string clipName)
        {
            clipName = clipName
                .Replace("Hai_ComboGesture_EmptyClip", "empty")
                .Replace("cge_", "")
                .Replace("__combined__", "+");

            return clipName.Length <= 16 ? clipName : clipName.Substring(0, 16);
        }
    }
}

