using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureActivity : MonoBehaviour
    {
        public float transitionDuration = 0.1f;

        public AnimationClip anim00;
        public AnimationClip anim01;
        public AnimationClip anim02;
        public AnimationClip anim03;
        public AnimationClip anim04;
        public AnimationClip anim05;
        public AnimationClip anim06;
        public AnimationClip anim07;
        public AnimationClip anim11;
        public AnimationClip anim12;
        public AnimationClip anim13;
        public AnimationClip anim14;
        public AnimationClip anim15;
        public AnimationClip anim16;
        public AnimationClip anim17;
        public AnimationClip anim22;
        public AnimationClip anim23;
        public AnimationClip anim24;
        public AnimationClip anim25;
        public AnimationClip anim26;
        public AnimationClip anim27;
        public AnimationClip anim33;
        public AnimationClip anim34;
        public AnimationClip anim35;
        public AnimationClip anim36;
        public AnimationClip anim37;
        public AnimationClip anim44;
        public AnimationClip anim45;
        public AnimationClip anim46;
        public AnimationClip anim47;
        public AnimationClip anim55;
        public AnimationClip anim56;
        public AnimationClip anim57;
        public AnimationClip anim66;
        public AnimationClip anim67;
        public AnimationClip anim77;
        public AnimationClip anim11_L;
        public AnimationClip anim11_R;

        public bool enablePermutations;
        public AnimationClip anim10;
        public AnimationClip anim20;
        public AnimationClip anim21;
        public AnimationClip anim30;
        public AnimationClip anim31;
        public AnimationClip anim32;
        public AnimationClip anim40;
        public AnimationClip anim41;
        public AnimationClip anim42;
        public AnimationClip anim43;
        public AnimationClip anim50;
        public AnimationClip anim51;
        public AnimationClip anim52;
        public AnimationClip anim53;
        public AnimationClip anim54;
        public AnimationClip anim60;
        public AnimationClip anim61;
        public AnimationClip anim62;
        public AnimationClip anim63;
        public AnimationClip anim64;
        public AnimationClip anim65;
        public AnimationClip anim70;
        public AnimationClip anim71;
        public AnimationClip anim72;
        public AnimationClip anim73;
        public AnimationClip anim74;
        public AnimationClip anim75;
        public AnimationClip anim76;

        public List<AnimationClip> blinking;
        public List<LimitedLipsyncAnimation> limitedLipsync;

        public ComboGesturePreviewSetup previewSetup;
        public bool editorLegacyFoldout;
        public bool editorTool;
        public AnimationClip[] editorArbitraryAnimations;

        [System.Serializable]
        public struct LimitedLipsyncAnimation
        {
            public AnimationClip clip;
            public LipsyncLimitation limitation;
        }

        [System.Serializable]
        public enum LipsyncLimitation
        {
            WideOpenMouth
        }

        public List<AnimationClip> AllDistinctAnimations()
        {
            return new[]
            {
                anim00, anim01, anim02, anim03, anim04, anim05, anim06, anim07,
                anim11, anim12, anim13, anim14, anim15, anim16, anim17,
                anim22, anim23, anim24, anim25, anim26, anim27,
                anim33, anim34, anim35, anim36, anim37,
                anim44, anim45, anim46, anim47,
                anim55, anim56, anim57,
                anim66, anim67,
                anim77,
                //
                anim11_L == null ? anim11 : anim11_L, anim11_R == null ? anim11 : anim11_R,
                //
                anim10,
                anim20, anim21,
                anim30, anim31, anim32,
                anim40, anim41, anim42, anim43,
                anim50, anim51, anim52, anim53, anim54,
                anim60, anim61, anim62, anim63, anim64, anim65,
                anim70, anim71, anim72, anim73, anim74, anim75, anim76,
            }
                .Where(clip => clip != null)
                .Distinct()
                .ToList();
        }
    }
}
