#if UNITY_EDITOR
using System.Collections.Generic;

public class ComboValue
{
    public int RawValue { get; }
    public int Right { get; }
    public bool IsSymmetrical { get; }
    public int Left { get; }

    public ComboValue(int rawValue)
    {
        RawValue = rawValue;
        Left = rawValue / 10;
        Right = rawValue % 10;
        IsSymmetrical = Left == Right;
    }

    private sealed class ComboEqualityComparer : IEqualityComparer<ComboValue>
    {
        public bool Equals(ComboValue x, ComboValue y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.RawValue == y.RawValue;
        }

        public int GetHashCode(ComboValue obj)
        {
            return obj.RawValue;
        }
    }

    public static IEqualityComparer<ComboValue> ComboComparer { get; } = new ComboEqualityComparer();
}
#endif