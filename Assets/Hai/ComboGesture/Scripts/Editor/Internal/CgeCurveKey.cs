using System;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public readonly struct CgeSampledCurveKey
    {
        public CgeCurveKey CurveKey { get; }
        public float SampleValue { get; }

        public CgeSampledCurveKey(CgeCurveKey curveKey, float sampleValue)
        {
            CurveKey = curveKey;
            SampleValue = sampleValue;
        }

        public bool Equals(CgeSampledCurveKey other)
        {
            return CurveKey.Equals(other.CurveKey) && SampleValue.Equals(other.SampleValue);
        }

        public override bool Equals(object obj)
        {
            return obj is CgeSampledCurveKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CurveKey.GetHashCode() * 397) ^ SampleValue.GetHashCode();
            }
        }

        public static bool operator ==(CgeSampledCurveKey left, CgeSampledCurveKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CgeSampledCurveKey left, CgeSampledCurveKey right)
        {
            return !left.Equals(right);
        }
    }

    [Serializable]
    public readonly struct CgeCurveKey
    {
        public static CgeCurveKey FromBinding(EditorCurveBinding binding)
        {
            return new CgeCurveKey(binding.path, binding.type, binding.propertyName);
        }

        public bool IsTransformOrMuscleCurve()
        {
            return Type == typeof(Transform) || Type == typeof(Animator);
        }

        public bool IsTransformCurve()
        {
            return Type == typeof(Transform);
        }

        public bool IsMuscleCurve()
        {
            return Type == typeof(Animator);
        }

        public CgeCurveKey(string path, Type type, string propertyName)
        {
            Path = path;
            Type = type;
            PropertyName = propertyName;
        }

        public string Path { get; }
        public Type Type { get; }
        public string PropertyName { get; }

        public static bool operator ==(CgeCurveKey left, CgeCurveKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CgeCurveKey left, CgeCurveKey right)
        {
            return !left.Equals(right);
        }

        public bool Equals(CgeCurveKey other)
        {
            return Path == other.Path && Equals(Type, other.Type) && PropertyName == other.PropertyName;
        }

        public override bool Equals(object obj)
        {
            return obj is CgeCurveKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
