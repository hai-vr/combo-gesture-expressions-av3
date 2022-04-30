using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    public readonly struct CgeAacFlClip
    {
        private readonly CgeAacConfiguration _component;
        public AnimationClip Clip { get; }

        public CgeAacFlClip(CgeAacConfiguration component, AnimationClip clip)
        {
            _component = component;
            Clip = clip;
        }

        public CgeAacFlClip Looping()
        {
            var settings = AnimationUtility.GetAnimationClipSettings(Clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(Clip, settings);

            return this;
        }

        public CgeAacFlClip NonLooping()
        {
            var settings = AnimationUtility.GetAnimationClipSettings(Clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(Clip, settings);

            return this;
        }

        public CgeAacFlClip Animating(Action<CgeAacFlEditClip> action)
        {
            action.Invoke(new CgeAacFlEditClip(_component, Clip));
            return this;
        }

        public CgeAacFlClip Toggling(GameObject[] gameObjectsWithNulls, bool value)
        {
            var defensiveObjects = gameObjectsWithNulls.Where(o => o != null); // Allow users to remove an item in the middle of the array
            foreach (var component in defensiveObjects)
            {
                var binding = CgeAacV0.Binding(_component, typeof(GameObject), component.transform, "m_IsActive");

                AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value ? 1f : 0f));
            }

            return this;
        }

        public CgeAacFlClip BlendShape(SkinnedMeshRenderer renderer, string blendShapeName, float value)
        {
            var binding = CgeAacV0.Binding(_component, typeof(SkinnedMeshRenderer), renderer.transform, $"blendShape.{blendShapeName}");

            AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value));

            return this;
        }

        public CgeAacFlClip BlendShape(SkinnedMeshRenderer[] rendererWithNulls, string blendShapeName, float value)
        {
            var defensiveObjects = rendererWithNulls.Where(o => o != null); // Allow users to remove an item in the middle of the array
            foreach (var component in defensiveObjects)
            {
                var binding = CgeAacV0.Binding(_component, typeof(SkinnedMeshRenderer), component.transform, $"blendShape.{blendShapeName}");

                AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value));
            }

            return this;
        }

        public CgeAacFlClip Scaling(GameObject[] gameObjectsWithNulls, Vector3 scale)
        {
            var defensiveObjects = gameObjectsWithNulls.Where(o => o != null); // Allow users to remove an item in the middle of the array
            foreach (var component in defensiveObjects)
            {
                AnimationUtility.SetEditorCurve(Clip, CgeAacV0.Binding(_component, typeof(Transform), component.transform, "m_LocalScale.x"), CgeAacV0.OneFrame(scale.x));
                AnimationUtility.SetEditorCurve(Clip, CgeAacV0.Binding(_component, typeof(Transform), component.transform, "m_LocalScale.y"), CgeAacV0.OneFrame(scale.y));
                AnimationUtility.SetEditorCurve(Clip, CgeAacV0.Binding(_component, typeof(Transform), component.transform, "m_LocalScale.z"), CgeAacV0.OneFrame(scale.z));
            }

            return this;
        }

        public CgeAacFlClip Toggling(GameObject gameObject, bool value)
        {
            var binding = CgeAacV0.Binding(_component, typeof(GameObject), gameObject.transform, "m_IsActive");

            AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value ? 1f : 0f));

            return this;
        }

        public CgeAacFlClip TogglingComponent(Component[] componentsWithNulls, bool value)
        {
            var defensiveComponents = componentsWithNulls.Where(o => o != null); // Allow users to remove an item in the middle of the array
            foreach (var component in defensiveComponents)
            {
                var binding = CgeAacV0.Binding(_component, component.GetType(), component.transform, "m_Enabled");

                AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value ? 1f : 0f));
            }

            return this;
        }

        public CgeAacFlClip TogglingComponent(Component component, bool value)
        {
            var binding = CgeAacV0.Binding(_component, component.GetType(), component.transform, "m_Enabled");

            AnimationUtility.SetEditorCurve(Clip, binding, CgeAacV0.OneFrame(value ? 1f : 0f));

            return this;
        }

        public CgeAacFlClip SwappingMaterial(Renderer renderer, int slot, Material material)
        {
            var binding = CgeAacV0.Binding(_component, renderer.GetType(), renderer.transform, $"m_Materials.Array.data[{slot}]");

            AnimationUtility.SetObjectReferenceCurve(Clip, binding, new[] {
                new ObjectReferenceKeyframe { time = 0f, value = material },
                new ObjectReferenceKeyframe { time = 1/60f, value = material }
            });

            return this;
        }

        public CgeAacFlClip SwappingMaterial(ParticleSystem particleSystem, int slot, Material material)
        {
            var binding = CgeAacV0.Binding(_component, typeof(ParticleSystemRenderer), particleSystem.transform, $"m_Materials.Array.data[{slot}]");

            AnimationUtility.SetObjectReferenceCurve(Clip, binding, new[] {
                new ObjectReferenceKeyframe { time = 0f, value = material },
                new ObjectReferenceKeyframe { time = 1/60f, value = material }
            });

            return this;
        }
    }

    public readonly struct CgeAacFlEditClip
    {
        private readonly CgeAacConfiguration _component;
        public AnimationClip Clip { get; }

        public CgeAacFlEditClip(CgeAacConfiguration component, AnimationClip clip)
        {
            _component = component;
            Clip = clip;
        }

        public CgeAacFlSettingCurve Animates(string path, Type type, string propertyName)
        {
            var binding = new EditorCurveBinding
            {
                path = path,
                type = type,
                propertyName = propertyName
            };
            return new CgeAacFlSettingCurve(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurve Animates(Transform transform, Type type, string propertyName)
        {
            var binding = CgeAacV0.Binding(_component, type, transform, propertyName);

            return new CgeAacFlSettingCurve(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurve Animates(GameObject gameObject)
        {
            var binding = CgeAacV0.Binding(_component, typeof(GameObject), gameObject.transform, "m_IsActive");

            return new CgeAacFlSettingCurve(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurve Animates(Component anyComponent, string property)
        {
            var binding = Internal_BindingFromComponent(anyComponent, property);

            return new CgeAacFlSettingCurve(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurve Animates(Component[] anyComponents, string property)
        {
            var that = this;
            var bindings = anyComponents
                .Select(anyComponent => that.Internal_BindingFromComponent(anyComponent, property))
                .ToArray();

            return new CgeAacFlSettingCurve(Clip, bindings);
        }

        public CgeAacFlSettingCurve AnimatesAnimator(CgeAacFlParameter floatParameter)
        {
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = floatParameter.Name
            };
            return new CgeAacFlSettingCurve(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurveColor AnimatesColor(Component anyComponent, string property)
        {
            var binding = Internal_BindingFromComponent(anyComponent, property);
            return new CgeAacFlSettingCurveColor(Clip, new[] {binding});
        }

        public CgeAacFlSettingCurveColor AnimatesColor(Component[] anyComponents, string property)
        {
            var that = this;
            var bindings = anyComponents
                .Select(anyComponent => that.Internal_BindingFromComponent(anyComponent, property))
                .ToArray();

            return new CgeAacFlSettingCurveColor(Clip, bindings);
        }

        public EditorCurveBinding BindingFromComponent(Component anyComponent, string propertyName)
        {
            return Internal_BindingFromComponent(anyComponent, propertyName);
        }

        private EditorCurveBinding Internal_BindingFromComponent(Component anyComponent, string propertyName)
        {
            return CgeAacV0.Binding(_component, anyComponent.GetType(), anyComponent.transform, propertyName);
        }
    }

    public class CgeAacFlSettingCurve
    {
        private readonly AnimationClip _clip;
        private readonly EditorCurveBinding[] _bindings;

        public CgeAacFlSettingCurve(AnimationClip clip, EditorCurveBinding[] bindings)
        {
            _clip = clip;
            _bindings = bindings;
        }

        public void WithOneFrame(float desiredValue)
        {
            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, binding, CgeAacV0.OneFrame(desiredValue));
            }
        }

        public void WithFixedSeconds(float seconds, float desiredValue)
        {
            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, binding, CgeAacV0.ConstantSeconds(seconds, desiredValue));
            }
        }

        public void WithSecondsUnit(Action<CgeAacFlSettingKeyframes> action)
        {
            InternalWithUnit(CgeAacFlUnit.Seconds, action);
        }

        public void WithFrameCountUnit(Action<CgeAacFlSettingKeyframes> action)
        {
            InternalWithUnit(CgeAacFlUnit.Frames, action);
        }

        public void WithUnit(CgeAacFlUnit unit, Action<CgeAacFlSettingKeyframes> action)
        {
            InternalWithUnit(unit, action);
        }

        private void InternalWithUnit(CgeAacFlUnit unit, Action<CgeAacFlSettingKeyframes> action)
        {
            var mutatedKeyframes = new List<Keyframe>();
            var builder = new CgeAacFlSettingKeyframes(unit, mutatedKeyframes);
            action.Invoke(builder);

            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, binding, new AnimationCurve(mutatedKeyframes.ToArray()));
            }
        }

        public void WithAnimationCurve(AnimationCurve animationCurve)
        {
            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, binding, animationCurve);
            }
        }
    }

    public class CgeAacFlSettingCurveColor
    {
        private readonly AnimationClip _clip;
        private readonly EditorCurveBinding[] _bindings;

        public CgeAacFlSettingCurveColor(AnimationClip clip, EditorCurveBinding[] bindings)
        {
            _clip = clip;
            _bindings = bindings;
        }

        public void WithOneFrame(Color desiredValue)
        {
            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "r"), CgeAacV0.OneFrame(desiredValue.r));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "g"), CgeAacV0.OneFrame(desiredValue.g));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "b"), CgeAacV0.OneFrame(desiredValue.b));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "a"), CgeAacV0.OneFrame(desiredValue.a));
            }
        }

        public void WithKeyframes(CgeAacFlUnit unit, Action<CgeAacFlSettingKeyframesColor> action)
        {
            var mutatedKeyframesR = new List<Keyframe>();
            var mutatedKeyframesG = new List<Keyframe>();
            var mutatedKeyframesB = new List<Keyframe>();
            var mutatedKeyframesA = new List<Keyframe>();
            var builder = new CgeAacFlSettingKeyframesColor(unit, mutatedKeyframesR, mutatedKeyframesG, mutatedKeyframesB, mutatedKeyframesA);
            action.Invoke(builder);

            foreach (var binding in _bindings)
            {
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "r"), new AnimationCurve(mutatedKeyframesR.ToArray()));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "g"), new AnimationCurve(mutatedKeyframesG.ToArray()));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "b"), new AnimationCurve(mutatedKeyframesB.ToArray()));
                AnimationUtility.SetEditorCurve(_clip, CgeAacV0.ToSubBinding(binding, "a"), new AnimationCurve(mutatedKeyframesA.ToArray()));
            }
        }
    }

    public class CgeAacFlSettingKeyframes
    {
        private readonly CgeAacFlUnit _unit;
        private readonly List<Keyframe> _mutatedKeyframes;

        public CgeAacFlSettingKeyframes(CgeAacFlUnit unit, List<Keyframe> mutatedKeyframes)
        {
            _unit = unit;
            _mutatedKeyframes = mutatedKeyframes;
        }

        public CgeAacFlSettingKeyframes Easing(float timeInUnit, float value)
        {
            _mutatedKeyframes.Add(new Keyframe(AsSeconds(timeInUnit), value, 0, 0));

            return this;
        }

        public CgeAacFlSettingKeyframes Constant(float timeInUnit, float value)
        {
            _mutatedKeyframes.Add(new Keyframe(AsSeconds(timeInUnit), value, 0, float.PositiveInfinity));

            return this;
        }

        public CgeAacFlSettingKeyframes Linear(float timeInUnit, float value)
        {
            float valueEnd = value;
            float valueStart = _mutatedKeyframes.Count == 0 ? value : _mutatedKeyframes.Last().value;
            float timeEnd = AsSeconds(timeInUnit);
            float timeStart = _mutatedKeyframes.Count == 0 ? value : _mutatedKeyframes.Last().time;
            float num = (float) (((double) valueEnd - (double) valueStart) / ((double) timeEnd - (double) timeStart));
            // FIXME: This can cause NaN tangents which messes everything

            // return new AnimationCurve(new Keyframe[2]
            // {
                // new Keyframe(timeStart, valueStart, 0.0f, num),
                // new Keyframe(timeEnd, valueEnd, num, 0.0f)
            // });

            if (_mutatedKeyframes.Count > 0)
            {
                var lastKeyframe = _mutatedKeyframes.Last();
                lastKeyframe.outTangent = num;
                _mutatedKeyframes[_mutatedKeyframes.Count - 1] = lastKeyframe;
            }
            _mutatedKeyframes.Add(new Keyframe(AsSeconds(timeInUnit), value, num, 0.0f));

            return this;
        }

        private float AsSeconds(float timeInUnit)
        {
            switch (_unit)
            {
                case CgeAacFlUnit.Frames:
                    return timeInUnit / 60f;
                case CgeAacFlUnit.Seconds:
                    return timeInUnit;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CgeAacFlSettingKeyframesColor
    {
        private CgeAacFlSettingKeyframes _r;
        private CgeAacFlSettingKeyframes _g;
        private CgeAacFlSettingKeyframes _b;
        private CgeAacFlSettingKeyframes _a;

        public CgeAacFlSettingKeyframesColor(CgeAacFlUnit unit, List<Keyframe> mutatedKeyframesR, List<Keyframe> mutatedKeyframesG, List<Keyframe> mutatedKeyframesB, List<Keyframe> mutatedKeyframesA)
        {
            _r = new CgeAacFlSettingKeyframes(unit, mutatedKeyframesR);
            _g = new CgeAacFlSettingKeyframes(unit, mutatedKeyframesG);
            _b = new CgeAacFlSettingKeyframes(unit, mutatedKeyframesB);
            _a = new CgeAacFlSettingKeyframes(unit, mutatedKeyframesA);
        }

        public CgeAacFlSettingKeyframesColor Easing(int frame, Color value)
        {
            _r.Easing(frame, value.r);
            _g.Easing(frame, value.g);
            _b.Easing(frame, value.b);
            _a.Easing(frame, value.a);

            return this;
        }

        public CgeAacFlSettingKeyframesColor Linear(float frame, Color value)
        {
            _r.Linear(frame, value.r);
            _g.Linear(frame, value.g);
            _b.Linear(frame, value.b);
            _a.Linear(frame, value.a);

            return this;
        }

        public CgeAacFlSettingKeyframesColor Constant(int frame, Color value)
        {
            _r.Constant(frame, value.r);
            _g.Constant(frame, value.g);
            _b.Constant(frame, value.b);
            _a.Constant(frame, value.a);

            return this;
        }
    }

    public enum CgeAacFlUnit
    {
        Frames, Seconds
    }
}
