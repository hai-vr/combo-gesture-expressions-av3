using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.CgeAac;
using UnityEditor.Animations;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    public class CgeNativeFaceTracking
    {
        private const string NativeFaceTrackingLayerName = "Hai_GestureNativeFaceTracking";
        
        private readonly ComboGestureFaceTracking _faceTracking;
        private readonly AnimatorController _fx;
        private readonly CgeAssetContainer _assetContainer;

        internal CgeNativeFaceTracking(ComboGestureFaceTracking faceTracking, AnimatorController fx, CgeAssetContainer assetContainer)
        {
            _faceTracking = faceTracking;
            _fx = fx;
            _assetContainer = assetContainer;
        }

        public void DoOverwriteNativeFaceTrackingLayer()
        {
            var layer = ReinitializeLayerAsMachinist();
        }

        private CgeAacFlLayer ReinitializeLayerAsMachinist()
        {
            return _assetContainer.ExposeCgeAac().CreateSupportingArbitraryControllerLayer(_fx, NativeFaceTrackingLayerName);
            // TODO: AvatarMask
        }
    }
}