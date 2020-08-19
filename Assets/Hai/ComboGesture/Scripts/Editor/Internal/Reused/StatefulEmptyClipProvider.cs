using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.Reused
{
    public class StatefulEmptyClipProvider
    {
        private readonly ClipGenerator _clipGenerator;
        private AnimationClip _selectedEmptyClip;

        public StatefulEmptyClipProvider(ClipGenerator clipGenerator)
        {
            _clipGenerator = clipGenerator;
        }

        public AnimationClip Get()
        {
            if (_selectedEmptyClip == null) { 
                _selectedEmptyClip = _clipGenerator.GetOrCreateEmptyClip();
            }

            return _selectedEmptyClip;
        }
    }
}
