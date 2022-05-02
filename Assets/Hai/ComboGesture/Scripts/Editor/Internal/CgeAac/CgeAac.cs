﻿using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    public static class CgeAacV0
    {
        public static CgeAacFlBase Create(CgeAacConfiguration configuration)
        {
            return new CgeAacFlBase(configuration);
        }

        internal static AnimatorController AnimatorOf(VRCAvatarDescriptor ad, VRCAvatarDescriptor.AnimLayerType animLayerType)
        {
            return (AnimatorController) ad.baseAnimationLayers.First(it => it.type == animLayerType).animatorController;
        }

        internal static AnimationClip NewClip(CgeAacConfiguration component, string suffix)
        {
            return RegisterClip(component, suffix, new AnimationClip());
        }

        internal static T CGE_RegisterAsset<T>(CgeAacConfiguration component, string suffix, T asset) where T : Object
        {
            asset.name = "zAutogenerated__" + component.AssetKey + "__" + suffix + "_" + Random.Range(0, Int32.MaxValue); // FIXME animation name conflict
            asset.hideFlags = HideFlags.None;
            if (EditorUtility.IsPersistent(component.AssetContainer))
            {
                AssetDatabase.AddObjectToAsset(asset, component.AssetContainer);
            }
            return asset;
        }

        internal static AnimationClip RegisterClip(CgeAacConfiguration component, string suffix, AnimationClip clip)
        {
            clip.name = "zAutogenerated__" + component.AssetKey + "__" + suffix + "_" + Random.Range(0, Int32.MaxValue); // FIXME animation name conflict
            clip.hideFlags = HideFlags.None;
            if (EditorUtility.IsPersistent(component.AssetContainer))
            {
                AssetDatabase.AddObjectToAsset(clip, component.AssetContainer);
            }
            return clip;
        }

        internal static BlendTree NewBlendTreeAsRaw(CgeAacConfiguration component, string suffix)
        {
            var clip = new BlendTree();
            clip.name = "zAutogenerated__" + component.AssetKey + "__" + suffix + "_" + Random.Range(0, Int32.MaxValue); // FIXME animation name conflict
            clip.hideFlags = HideFlags.None;
            if (EditorUtility.IsPersistent(component.AssetContainer))
            {
                AssetDatabase.AddObjectToAsset(clip, component.AssetContainer);
            }
            return clip;
        }

        internal static EditorCurveBinding Binding(CgeAacConfiguration component, Type type, Transform transform, string propertyName)
        {
            return new EditorCurveBinding
            {
                path = ResolveRelativePath(component.AnimatorRoot, transform),
                type = type,
                propertyName = propertyName
            };
        }

        internal static AnimationCurve OneFrame(float desiredValue)
        {
            return AnimationCurve.Constant(0f, 1 / 60f, desiredValue);
        }

        internal static AnimationCurve ConstantSeconds(float seconds, float desiredValue)
        {
            return AnimationCurve.Constant(0f, seconds, desiredValue);
        }

        internal static string ResolveRelativePath(Transform avatar, Transform item)
        {
            if (item.parent != avatar && item.parent != null)
            {
                return ResolveRelativePath(avatar, item.parent) + "/" + item.name;
            }

            return item.name;
        }

        internal static EditorCurveBinding ToSubBinding(EditorCurveBinding binding, string suffix)
        {
            return new EditorCurveBinding {path = binding.path, type = binding.type, propertyName = binding.propertyName + "." + suffix};
        }
    }

    public struct CgeAacConfiguration
    {
        public string SystemName;
        public VRCAvatarDescriptor AvatarDescriptor;
        public Transform AnimatorRoot;
        public Transform DefaultValueRoot;
        public AnimatorController AssetContainer;
        public string AssetKey;
        public ICgeAacDefaultsProvider DefaultsProvider;
    }

    public struct CgeAacFlLayer
    {
        private readonly AnimatorController _animatorController;
        private readonly CgeAacConfiguration _configuration;
        private readonly string _fullLayerName;
        private readonly CgeAacFlStateMachine _stateMachine;

        internal CgeAacFlLayer(AnimatorController animatorController, CgeAacConfiguration configuration, CgeAacFlStateMachine stateMachine, string fullLayerName)
        {
            _animatorController = animatorController;
            _configuration = configuration;
            _fullLayerName = fullLayerName;
            _stateMachine = stateMachine;
        }

        public CgeAacFlState NewState(string name)
        {
            var lastState = _stateMachine.LastNodePosition();
            var state = _stateMachine.NewState(name, 0, 0).Shift(lastState, 0, 1);
            return state;
        }

        public CgeAacFlState NewState(string name, int x, int y)
        {
            return _stateMachine.NewState(name, x, y);
        }

        public CgeAacFlStateMachine NewSubStateMachine(string name)
        {
            return _stateMachine.NewSubStateMachine(name);
        }

        public CgeAacFlStateMachine NewSubStateMachine(string name, int x, int y)
        {
            return _stateMachine.NewSubStateMachine(name, x, y);
        }

        public CgeAacFlTransition AnyTransitionsTo(CgeAacFlState destination)
        {
            return _stateMachine.AnyTransitionsTo(destination);
        }

        public CgeAacFlEntryTransition EntryTransitionsTo(CgeAacFlState destination)
        {
            return _stateMachine.EntryTransitionsTo(destination);
        }
        
        public CgeAacFlEntryTransition EntryTransitionsTo(CgeAacFlStateMachine destination)
        {
            return _stateMachine.EntryTransitionsTo(destination);
        }

        public CgeAacFlBoolParameter BoolParameter(string parameterName) => _stateMachine.BackingAnimator().BoolParameter(parameterName);
        public CgeAacFlBoolParameter TriggerParameterAsBool(string parameterName) => _stateMachine.BackingAnimator().TriggerParameter(parameterName);
        public CgeAacFlFloatParameter FloatParameter(string parameterName) => _stateMachine.BackingAnimator().FloatParameter(parameterName);
        public CgeAacFlIntParameter IntParameter(string parameterName) => _stateMachine.BackingAnimator().IntParameter(parameterName);
        public CgeAacFlBoolParameterGroup BoolParameters(params string[] parameterNames) => _stateMachine.BackingAnimator().BoolParameters(parameterNames);
        public CgeAacFlBoolParameterGroup TriggerParametersAsBools(params string[] parameterNames) => _stateMachine.BackingAnimator().TriggerParameters(parameterNames);
        public CgeAacFlFloatParameterGroup FloatParameters(params string[] parameterNames) => _stateMachine.BackingAnimator().FloatParameters(parameterNames);
        public CgeAacFlIntParameterGroup IntParameters(params string[] parameterNames) => _stateMachine.BackingAnimator().IntParameters(parameterNames);
        public CgeAacFlBoolParameterGroup BoolParameters(params CgeAacFlBoolParameter[] parameters) => _stateMachine.BackingAnimator().BoolParameters(parameters);
        public CgeAacFlBoolParameterGroup TriggerParametersAsBools(params CgeAacFlBoolParameter[] parameters) => _stateMachine.BackingAnimator().TriggerParameters(parameters);
        public CgeAacFlFloatParameterGroup FloatParameters(params CgeAacFlFloatParameter[] parameters) => _stateMachine.BackingAnimator().FloatParameters(parameters);
        public CgeAacFlIntParameterGroup IntParameters(params CgeAacFlIntParameter[] parameters) => _stateMachine.BackingAnimator().IntParameters(parameters);
        public CgeAacAv3 Av3() => new CgeAacAv3(_stateMachine.BackingAnimator());

        public CgeAacFlLayer OverrideValue(CgeAacFlBoolParameter toBeForced, bool value)
        {
            var parameters = _animatorController.parameters;
            foreach (var param in parameters)
            {
                if (param.name == toBeForced.Name)
                {
                    param.defaultBool = value;
                }
            }

            _animatorController.parameters = parameters;

            return this;
        }

        public CgeAacFlLayer OverrideValue(CgeAacFlFloatParameter toBeForced, float value)
        {
            var parameters = _animatorController.parameters;
            foreach (var param in parameters)
            {
                if (param.name == toBeForced.Name)
                {
                    param.defaultFloat = value;
                }
            }

            _animatorController.parameters = parameters;

            return this;
        }

        public CgeAacFlLayer OverrideValue(CgeAacFlIntParameter toBeForced, int value)
        {
            var parameters = _animatorController.parameters;
            foreach (var param in parameters)
            {
                if (param.name == toBeForced.Name)
                {
                    param.defaultInt = value;
                }
            }

            _animatorController.parameters = parameters;

            return this;
        }

        public CgeAacFlLayer CGE_WithLayerWeight(float layerWeight)
        {
            var finalFullLayerName = _fullLayerName;
            _animatorController.layers = _animatorController.layers
                .Select(layer =>
                {
                    if (layer.name == finalFullLayerName)
                    {
                        layer.defaultWeight = layerWeight;
                    }

                    return layer;
                })
                .ToArray();

            return this;
        }

        public CgeAacFlLayer WithAvatarMask(AvatarMask avatarMask)
        {
            var finalFullLayerName = _fullLayerName;
            _animatorController.layers = _animatorController.layers
                .Select(layer =>
                {
                    if (layer.name == finalFullLayerName)
                    {
                        layer.avatarMask = avatarMask;
                    }

                    return layer;
                })
                .ToArray();

            return this;
        }

        public CgeAacFlLayer WithAvatarMaskNoTransforms()
        {
            ResolveAvatarMask(new Transform[0]);

            return this;
        }

        public CgeAacFlLayer ResolveAvatarMask(Transform[] paths)
        {
            // FIXME: Fragile
            var avatarMask = new AvatarMask();
            avatarMask.name = "zAutogenerated__" + _configuration.AssetKey + "_" + _fullLayerName + "__AvatarMask";
            avatarMask.hideFlags = HideFlags.None;

            if (paths.Length == 0)
            {
                avatarMask.transformCount = 1;
                avatarMask.SetTransformActive(0, false);
                avatarMask.SetTransformPath(0, "_ignored");
            }
            else
            {
                avatarMask.transformCount = paths.Length;
                for (var index = 0; index < paths.Length; index++)
                {
                    var transform = paths[index];
                    avatarMask.SetTransformActive(index, true);
                    avatarMask.SetTransformPath(index, CgeAacV0.ResolveRelativePath(_configuration.AnimatorRoot, transform));
                }
            }

            for (int i = 0; i < (int) AvatarMaskBodyPart.LastBodyPart; i++)
            {
                avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart) i, false);
            }

            if (EditorUtility.IsPersistent(_animatorController))
            {
                AssetDatabase.AddObjectToAsset(avatarMask, _animatorController);
            }

            WithAvatarMask(avatarMask);

            return this;
        }

        public CgeAacFlLayer WithDefaultState(CgeAacFlState newDefaultState)
        {
            _stateMachine.WithDefaultState(newDefaultState);
            return this;
        }
    }

    public class CgeAacFlBase
    {
        private readonly CgeAacConfiguration _configuration;

        internal CgeAacFlBase(CgeAacConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CgeAacFlClip NewClip()
        {
            var clip = CgeAacV0.NewClip(_configuration, Guid.NewGuid().ToString());
            return new CgeAacFlClip(_configuration, clip);
        }

        public CgeAacFlClip CopyClip(AnimationClip originalClip)
        {
            var newClip = UnityEngine.Object.Instantiate(originalClip);
            var clip = CgeAacV0.RegisterClip(_configuration, Guid.NewGuid().ToString(), newClip);
            return new CgeAacFlClip(_configuration, clip);
        }

        public Motion CGE_StoringMotion(Motion motion)
        {
            return CgeAacV0.CGE_RegisterAsset(_configuration, Guid.NewGuid().ToString(), motion);
        }

        public T CGE_StoringAsset<T>(T obj) where T : Object
        {
            CgeAacV0.CGE_RegisterAsset(_configuration, Guid.NewGuid().ToString(), obj);
            return obj;
        }

        public BlendTree NewBlendTreeAsRaw()
        {
            return CgeAacV0.NewBlendTreeAsRaw(_configuration, Guid.NewGuid().ToString());
        }

        public CgeAacFlClip NewClip(string name)
        {
            var clip = CgeAacV0.NewClip(_configuration, name);
            return new CgeAacFlClip(_configuration, clip);
        }

        public CgeAacFlClip DummyClipLasting(float numberOf, CgeAacFlUnit unit)
        {
            var dummyClip = CgeAacV0.NewClip(_configuration, $"D({numberOf} {Enum.GetName(typeof(CgeAacFlUnit), unit)})");

            var duration = unit == CgeAacFlUnit.Frames ? numberOf / 60f : numberOf;
            return new CgeAacFlClip(_configuration, dummyClip)
                .Animating(clip => clip.Animates("_ignored", typeof(GameObject), "m_IsActive")
                    .WithUnit(unit, keyframes => keyframes.Constant(0, 0f).Constant(duration, 0f)));
        }

        public void RemoveAllMainLayers()
        {
            var layerName = _configuration.SystemName;
            RemoveLayerOnAllControllers(_configuration.DefaultsProvider.ConvertLayerName(layerName));
        }

        public void RemoveAllSupportingLayers(string suffix)
        {
            var layerName = _configuration.SystemName;
            RemoveLayerOnAllControllers(_configuration.DefaultsProvider.ConvertLayerNameWithSuffix(layerName, suffix));
        }

        public void CGE_RemoveMainArbitraryControllerLayer(AnimatorController controller)
        {
            var layerName = _configuration.SystemName;
            var convertedName = _configuration.DefaultsProvider.ConvertLayerName(layerName);
            new CgeAacAnimatorRemoval(controller).RemoveLayer(convertedName);
        }

        public void CGE_RemoveSupportingArbitraryControllerLayer(AnimatorController controller, string suffix)
        {
            var layerName = _configuration.SystemName;
            var convertedName = _configuration.DefaultsProvider.ConvertLayerNameWithSuffix(layerName, suffix);
            new CgeAacAnimatorRemoval(controller).RemoveLayer(convertedName);
        }

        private void RemoveLayerOnAllControllers(string layerName)
        {
            var layers = _configuration.AvatarDescriptor.baseAnimationLayers.Select(layer => layer.animatorController).Where(layer => layer != null).Distinct().ToList();
            foreach (var customAnimLayer in layers)
            {
                new CgeAacAnimatorRemoval((AnimatorController) customAnimLayer).RemoveLayer(_configuration.DefaultsProvider.ConvertLayerName(layerName));
            }
        }

        public CgeAacFlLayer CreateMainFxLayer() => DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType.FX);
        public CgeAacFlLayer CreateMainGestureLayer() => DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType.Gesture);
        public CgeAacFlLayer CreateMainActionLayer() => DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType.Action);
        public CgeAacFlLayer CreateMainIdleLayer() => DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType.Additive);
        public CgeAacFlLayer CreateMainLocomotionLayer() => DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType.Base);
        public CgeAacFlLayer CreateMainAv3Layer(VRCAvatarDescriptor.AnimLayerType animLayerType) => DoCreateMainLayerOnController(animLayerType);

        public CgeAacFlLayer CreateSupportingFxLayer(string suffix) => DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType.FX, suffix);
        public CgeAacFlLayer CreateSupportingGestureLayer(string suffix) => DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType.Gesture, suffix);
        public CgeAacFlLayer CreateSupportingActionLayer(string suffix) => DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType.Action, suffix);
        public CgeAacFlLayer CreateSupportingIdleLayer(string suffix) => DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType.Additive, suffix);
        public CgeAacFlLayer CreateSupportingLocomotionLayer(string suffix) => DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType.Base, suffix);
        public CgeAacFlLayer CreateSupportingAv3Layer(VRCAvatarDescriptor.AnimLayerType animLayerType, string suffix) => DoCreateSupportingLayerOnController(animLayerType, suffix);

        public CgeAacFlLayer CreateMainArbitraryControllerLayer(AnimatorController controller) => DoCreateLayer(controller, _configuration.DefaultsProvider.ConvertLayerName(_configuration.SystemName));
        public CgeAacFlLayer CreateSupportingArbitraryControllerLayer(AnimatorController controller, string suffix) => DoCreateLayer(controller, _configuration.DefaultsProvider.ConvertLayerNameWithSuffix(_configuration.SystemName, suffix));
        public CgeAacFlLayer CreateFirstArbitraryControllerLayer(AnimatorController controller) => DoCreateLayer(controller, controller.layers[0].name);

        private CgeAacFlLayer DoCreateMainLayerOnController(VRCAvatarDescriptor.AnimLayerType animType)
        {
            var animator = CgeAacV0.AnimatorOf(_configuration.AvatarDescriptor, animType);
            var layerName = _configuration.DefaultsProvider.ConvertLayerName(_configuration.SystemName);

            return DoCreateLayer(animator, layerName);
        }

        private CgeAacFlLayer DoCreateSupportingLayerOnController(VRCAvatarDescriptor.AnimLayerType animType, string suffix)
        {
            var animator = CgeAacV0.AnimatorOf(_configuration.AvatarDescriptor, animType);
            var layerName = _configuration.DefaultsProvider.ConvertLayerNameWithSuffix(_configuration.SystemName, suffix);

            return DoCreateLayer(animator, layerName);
        }

        private CgeAacFlLayer DoCreateLayer(AnimatorController animator, string layerName)
        {
            var ag = new CgeAacAnimatorGenerator(animator, CreateEmptyClip().Clip, _configuration.DefaultsProvider);
            var machine = ag.CreateOrRemakeLayerAtSameIndex(layerName, 1f);

            return new CgeAacFlLayer(animator, _configuration, machine, layerName);
        }

        private CgeAacFlClip CreateEmptyClip()
        {
            var emptyClip = DummyClipLasting(1, CgeAacFlUnit.Frames);
            return emptyClip;
        }

        public CgeAacVrcAssetLibrary VrcAssets()
        {
            return new CgeAacVrcAssetLibrary();
        }

        public void ClearPreviousAssets()
        {
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_configuration.AssetContainer));
            foreach (var subAsset in allSubAssets)
            {
                if (subAsset.name.StartsWith($"zAutogenerated__{_configuration.AssetKey}__")
                    && (subAsset is AnimationClip || subAsset is BlendTree || subAsset is AvatarMask))
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                }
            }
        }
    }

    public class CgeAacAv3
    {
        private readonly CgeAacBackingAnimator _backingAnimator;

        internal CgeAacAv3(CgeAacBackingAnimator backingAnimator)
        {
            _backingAnimator = backingAnimator;
        }

        // ReSharper disable InconsistentNaming
        public CgeAacFlBoolParameter IsLocal => _backingAnimator.BoolParameter("IsLocal");
        public CgeAacFlEnumIntParameter<Av3Viseme> Viseme => _backingAnimator.EnumParameter<Av3Viseme>("Viseme");
        public CgeAacFlEnumIntParameter<Av3Gesture> GestureLeft => _backingAnimator.EnumParameter<Av3Gesture>("GestureLeft");
        public CgeAacFlEnumIntParameter<Av3Gesture> GestureRight => _backingAnimator.EnumParameter<Av3Gesture>("GestureRight");
        public CgeAacFlFloatParameter GestureLeftWeight => _backingAnimator.FloatParameter("GestureLeftWeight");
        public CgeAacFlFloatParameter GestureRightWeight => _backingAnimator.FloatParameter("GestureRightWeight");
        public CgeAacFlFloatParameter AngularY => _backingAnimator.FloatParameter("AngularY");
        public CgeAacFlFloatParameter VelocityX => _backingAnimator.FloatParameter("VelocityX");
        public CgeAacFlFloatParameter VelocityY => _backingAnimator.FloatParameter("VelocityY");
        public CgeAacFlFloatParameter VelocityZ => _backingAnimator.FloatParameter("VelocityZ");
        public CgeAacFlFloatParameter Upright => _backingAnimator.FloatParameter("Upright");
        public CgeAacFlBoolParameter Grounded => _backingAnimator.BoolParameter("Grounded");
        public CgeAacFlBoolParameter Seated => _backingAnimator.BoolParameter("Seated");
        public CgeAacFlBoolParameter AFK => _backingAnimator.BoolParameter("AFK");
        public CgeAacFlIntParameter TrackingType => _backingAnimator.IntParameter("TrackingType");
        public CgeAacFlIntParameter VRMode => _backingAnimator.IntParameter("VRMode");
        public CgeAacFlBoolParameter MuteSelf => _backingAnimator.BoolParameter("MuteSelf");
        public CgeAacFlBoolParameter InStation => _backingAnimator.BoolParameter("InStation");
        public CgeAacFlFloatParameter Voice => _backingAnimator.FloatParameter("Voice");
        // ReSharper restore InconsistentNaming

        public ICgeAacFlCondition ItIsRemote() => IsLocal.IsFalse();
        public ICgeAacFlCondition ItIsLocal() => IsLocal.IsTrue();

        public enum Av3Gesture
        {
            // Specify all the values explicitly because they should be dictated by VRChat, not enumeration order.
            Neutral = 0,
            Fist = 1,
            HandOpen = 2,
            Fingerpoint = 3,
            Victory = 4,
            RockNRoll = 5,
            HandGun = 6,
            ThumbsUp = 7
        }

        public enum Av3Viseme
        {
            // Specify all the values explicitly because they should be dictated by VRChat, not enumeration order.
            // ReSharper disable InconsistentNaming
            sil = 0,
            pp = 1,
            ff = 2,
            th = 3,
            dd = 4,
            kk = 5,
            ch = 6,
            ss = 7,
            nn = 8,
            rr = 9,
            aa = 10,
            e = 11,
            ih = 12,
            oh = 13,
            ou = 14
            // ReSharper restore InconsistentNaming
        }
    }

    public class CgeAacVrcAssetLibrary
    {
        public AvatarMask LeftHandAvatarMask()
        {
            return AssetDatabase.LoadAssetAtPath<AvatarMask>("Assets/VRCSDK/Examples3/Animation/Masks/vrc_Hand Left.mask");
        }

        public AvatarMask RightHandAvatarMask()
        {
            return AssetDatabase.LoadAssetAtPath<AvatarMask>("Assets/VRCSDK/Examples3/Animation/Masks/vrc_Hand Right.mask");
        }

        public AnimationClip ProxyForGesture(CgeAacAv3.Av3Gesture gesture, bool masculine)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/VRCSDK/Examples3/Animation/ProxyAnim/" + ResolveProxyFilename(gesture, masculine));
        }

        private static string ResolveProxyFilename(CgeAacAv3.Av3Gesture gesture, bool masculine)
        {
            switch (gesture)
            {
                case CgeAacAv3.Av3Gesture.Neutral: return masculine ? "proxy_hands_idle.anim" : "proxy_hands_idle2.anim";
                case CgeAacAv3.Av3Gesture.Fist: return "proxy_hands_fist.anim";
                case CgeAacAv3.Av3Gesture.HandOpen: return "proxy_hands_open.anim";
                case CgeAacAv3.Av3Gesture.Fingerpoint: return "proxy_hands_point.anim";
                case CgeAacAv3.Av3Gesture.Victory: return "proxy_hands_peace.anim";
                case CgeAacAv3.Av3Gesture.RockNRoll: return "proxy_hands_rock.anim";
                case CgeAacAv3.Av3Gesture.HandGun: return "proxy_hands_gun.anim";
                case CgeAacAv3.Av3Gesture.ThumbsUp: return "proxy_hands_thumbs_up.anim";
                default:
                    throw new ArgumentOutOfRangeException(nameof(gesture), gesture, null);
            }
        }
    }

    public class CgeAacAnimatorRemoval
    {
        private readonly AnimatorController _animatorController;

        public CgeAacAnimatorRemoval(AnimatorController animatorController)
        {
            _animatorController = animatorController;
        }

        public void RemoveLayer(string layerName)
        {
            var index = FindIndexOf(layerName);
            if (index == -1) return;

            _animatorController.RemoveLayer(index);
        }

        private int FindIndexOf(string layerName)
        {
            return _animatorController.layers.ToList().FindIndex(layer => layer.name == layerName);
        }
    }

    public class CgeAacAnimatorGenerator
    {
        private readonly AnimatorController _animatorController;
        private readonly AnimationClip _emptyClip;
        private readonly ICgeAacDefaultsProvider _defaultsProvider;

        internal CgeAacAnimatorGenerator(AnimatorController animatorController, AnimationClip emptyClip, ICgeAacDefaultsProvider defaultsProvider)
        {
            _animatorController = animatorController;
            _emptyClip = emptyClip;
            _defaultsProvider = defaultsProvider;
        }

        internal void CreateParamsAsNeeded(params CgeAacFlParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                switch (parameter)
                {
                    case CgeAacFlIntParameter _:
                        CreateParamIfNotExists(parameter.Name, AnimatorControllerParameterType.Int);
                        break;
                    case CgeAacFlFloatParameter _:
                        CreateParamIfNotExists(parameter.Name, AnimatorControllerParameterType.Float);
                        break;
                    case CgeAacFlBoolParameter _:
                        CreateParamIfNotExists(parameter.Name, AnimatorControllerParameterType.Bool);
                        break;
                }
            }
        }
        internal void CreateTriggerParamsAsNeeded(params CgeAacFlBoolParameter[] parameters)
        {
            foreach (var parameter in parameters)
            {
                CreateParamIfNotExists(parameter.Name, AnimatorControllerParameterType.Trigger);
            }
        }

        private void CreateParamIfNotExists(string paramName, AnimatorControllerParameterType type)
        {
            if (_animatorController.parameters.FirstOrDefault(param => param.name == paramName) == null)
            {
                _animatorController.AddParameter(paramName, type);
            }
        }

        // DEPRECATED: This causes the editor window to glitch by deselecting, which is jarring for experimentation
        // Re-enabled in CGE to reduce regeneration times.
        internal CgeAacFlStateMachine CreateOrRemakeLayerAtSameIndex(string layerName, float weightWhenCreating, AvatarMask maskWhenCreating = null)
        {
            var originalIndexToPreserveOrdering = FindIndexOf(layerName);
            if (originalIndexToPreserveOrdering != -1)
            {
                _animatorController.RemoveLayer(originalIndexToPreserveOrdering);
            }

            AddLayerWithWeight(layerName, weightWhenCreating, maskWhenCreating);
            if (originalIndexToPreserveOrdering != -1)
            {
                var items = _animatorController.layers.ToList();
                var last = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                items.Insert(originalIndexToPreserveOrdering, last);
                _animatorController.layers = items.ToArray();
            }

            var layer = TryGetLayer(layerName);
            var machinist = new CgeAacFlStateMachine(layer.stateMachine, _emptyClip, new CgeAacBackingAnimator(this), _defaultsProvider);
            _defaultsProvider.ConfigureStateMachine(layer.stateMachine);
            return machinist;
        }

        internal CgeAacFlStateMachine CreateOrClearLayerAtSameIndex(string layerName, float weightWhenCreating, AvatarMask maskWhenCreating = null)
        {
            var originalIndexToPreserveOrdering = FindIndexOf(layerName);
            if (originalIndexToPreserveOrdering != -1)
            {
                RecursivelyClearChildrenMachines(_animatorController.layers[originalIndexToPreserveOrdering].stateMachine);

                _animatorController.layers[originalIndexToPreserveOrdering].stateMachine.stateMachines = new ChildAnimatorStateMachine[0];
                _animatorController.layers[originalIndexToPreserveOrdering].stateMachine.states = new ChildAnimatorState[0];
                _animatorController.layers[originalIndexToPreserveOrdering].stateMachine.entryTransitions = new AnimatorTransition[0];
                _animatorController.layers[originalIndexToPreserveOrdering].stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
            }
            else
            {
                _animatorController.AddLayer(_animatorController.MakeUniqueLayerName(layerName));
                originalIndexToPreserveOrdering = _animatorController.layers.Length - 1;
            }

            var layers = _animatorController.layers;
            layers[originalIndexToPreserveOrdering].avatarMask = maskWhenCreating;
            layers[originalIndexToPreserveOrdering].defaultWeight = weightWhenCreating;
            _animatorController.layers = layers;

            var layer = TryGetLayer(layerName);
            var machinist = new CgeAacFlStateMachine(layer.stateMachine, _emptyClip, new CgeAacBackingAnimator(this), _defaultsProvider);
            _defaultsProvider.ConfigureStateMachine(layer.stateMachine);
            return machinist;
        }

        private void RecursivelyClearChildrenMachines(AnimatorStateMachine parentMachine)
        {
            // TODO: RemoveStateMachine might already be recursive
            foreach (var childStateMachineHolder in parentMachine.stateMachines)
            {
                RecursivelyClearChildrenMachines(childStateMachineHolder.stateMachine);
                parentMachine.RemoveStateMachine(childStateMachineHolder.stateMachine);
            }
        }

        private int FindIndexOf(string layerName)
        {
            return _animatorController.layers.ToList().FindIndex(layer1 => layer1.name == layerName);
        }

        private AnimatorControllerLayer TryGetLayer(string layerName)
        {
            return _animatorController.layers.FirstOrDefault(it => it.name == layerName);
        }

        private void AddLayerWithWeight(string layerName, float weightWhenCreating, AvatarMask maskWhenCreating)
        {
            _animatorController.AddLayer(_animatorController.MakeUniqueLayerName(layerName));

            var mutatedLayers = _animatorController.layers;
            mutatedLayers[mutatedLayers.Length - 1].defaultWeight = weightWhenCreating;
            mutatedLayers[mutatedLayers.Length - 1].avatarMask = maskWhenCreating;
            _animatorController.layers = mutatedLayers;
        }
    }
}