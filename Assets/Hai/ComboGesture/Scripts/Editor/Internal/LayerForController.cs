using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    // Controller layer no longer exists. Still, delete it from existing animators.
    internal static class LayerForController
    {
        private const string ControllerLayerName = "Hai_GestureCtrl";

        public static void Delete(AssetContainer assetContainer, AnimatorController animatorController)
        {
            assetContainer.ExposeAac().CGE_RemoveSupportingArbitraryControllerLayer(animatorController, ControllerLayerName);
        }
    }
}
