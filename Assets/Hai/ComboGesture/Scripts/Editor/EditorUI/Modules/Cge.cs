using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Modules
{
    public class Cge
    {
        public readonly CgeMemoization Memoization;

        private static Cge _cge;
        public static Cge Get()
        {
            if (_cge != null) return _cge;

            _cge = new Cge();
            return _cge;
        }

        private Cge()
        {
            Memoization = new CgeMemoization();
        }
    }
}
