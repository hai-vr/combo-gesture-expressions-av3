using System;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    [Serializable]
    public readonly struct CurveKey
    {
        public static CurveKey FromBinding(EditorCurveBinding binding)
        {
            return new CurveKey(binding.path, binding.type, binding.propertyName);
        }

        public bool IsTransformOrMuscleCurve()
        {
            return Type == typeof(Transform) || Type == typeof(Animator);
        }

        public bool IsMuscleCurve()
        {
            return Type == typeof(Animator);
        }

        public CurveKey(string path, Type type, string propertyName)
        {
            Path = path;
            Type = type;
            PropertyName = propertyName;
        }

        public string Path { get; }
        public Type Type { get; }
        public string PropertyName { get; }

        public static bool operator ==(CurveKey left, CurveKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CurveKey left, CurveKey right)
        {
            return !left.Equals(right);
        }

        public bool Equals(CurveKey other)
        {
            return Path == other.Path && Equals(Type, other.Type) && PropertyName == other.PropertyName;
        }

        public override bool Equals(object obj)
        {
            return obj is CurveKey other && Equals(other);
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
