using Hai.ComboGesture.Scripts.Editor.Internal.Reused;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    // Controller layer no longer exists. Still, delete it from existing animators.
    internal static class LayerForController
    {
        private const string ControllerLayerName = "Hai_GestureCtrl";

        public static void Delete(AnimatorGenerator animatorGenerator)
        {
            animatorGenerator.RemoveLayerIfExists(ControllerLayerName);
        }
    }
}
