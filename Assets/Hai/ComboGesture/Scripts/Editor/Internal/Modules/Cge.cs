using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Modules
{
    public class Cge
    {
        public readonly CgeMemoization Memoization;
        public readonly CgeRenderingCommands RenderingCommands;
        public readonly CgeAnimationEditor AnimationEditor;

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
            RenderingCommands = new CgeRenderingCommands();
            AnimationEditor = new CgeAnimationEditor(RenderingCommands);
        }
    }
}
