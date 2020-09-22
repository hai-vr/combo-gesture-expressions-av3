using System;

namespace Hai.ComboGesture.Scripts.Components
{
    [Serializable]
    public readonly struct BlendShapeKey
    {
        public BlendShapeKey(string path, string blendShapeName)
        {
            Path = path;
            BlendShapeName = blendShapeName;
        }

        public string Path { get; }
        public string BlendShapeName { get; }

        public static bool operator ==(BlendShapeKey left, BlendShapeKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlendShapeKey left, BlendShapeKey right)
        {
            return !left.Equals(right);
        }

        public bool Equals(BlendShapeKey other)
        {
            return Path == other.Path && BlendShapeName == other.BlendShapeName;
        }

        public override bool Equals(object obj)
        {
            return obj is BlendShapeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ (BlendShapeName != null ? BlendShapeName.GetHashCode() : 0);
            }
        }
    }
}
