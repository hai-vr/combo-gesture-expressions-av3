using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    // Lipsync layer no longer exists. Still, delete it from existing animators.
    internal class LayerForLipsyncOverrideView
    {
        private const string LipsyncLayerName = "Hai_GestureLipsync";

        public static void Delete(AssetContainer assetContainer, AnimatorController animatorController)
        {
            assetContainer.ExposeCgeAac().CGE_RemoveSupportingArbitraryControllerLayer(animatorController, LipsyncLayerName);
        }
    }
}
