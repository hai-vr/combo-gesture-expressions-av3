using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class LayerForLipsyncOverrideView
    {
        private const string LipsyncLayerName = "Hai_GestureLipsync";

        public static void Delete(AssetContainer assetContainer, AnimatorController animatorController)
        {
            assetContainer.ExposeAac().CGE_RemoveSupportingArbitraryControllerLayer(animatorController, LipsyncLayerName);
        }
    }
}
