namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class ComboValue
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
    }
}
