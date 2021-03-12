using System.Collections.Generic;
using Hai.ExpressionsEditor.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Components
{
    public class ComboGestureActivity : MonoBehaviour
    {
        public float transitionDuration = 0.1f;

        public Motion anim00;
        public Motion anim01;
        public Motion anim02;
        public Motion anim03;
        public Motion anim04;
        public Motion anim05;
        public Motion anim06;
        public Motion anim07;
        public Motion anim11;
        public Motion anim12;
        public Motion anim13;
        public Motion anim14;
        public Motion anim15;
        public Motion anim16;
        public Motion anim17;
        public Motion anim22;
        public Motion anim23;
        public Motion anim24;
        public Motion anim25;
        public Motion anim26;
        public Motion anim27;
        public Motion anim33;
        public Motion anim34;
        public Motion anim35;
        public Motion anim36;
        public Motion anim37;
        public Motion anim44;
        public Motion anim45;
        public Motion anim46;
        public Motion anim47;
        public Motion anim55;
        public Motion anim56;
        public Motion anim57;
        public Motion anim66;
        public Motion anim67;
        public Motion anim77;
        // ReSharper disable once InconsistentNaming
        public AnimationClip anim11_L;
        // ReSharper disable once InconsistentNaming
        public AnimationClip anim11_R;

        public bool enablePermutations;
        public Motion anim10;
        public Motion anim20;
        public Motion anim21;
        public Motion anim30;
        public Motion anim31;
        public Motion anim32;
        public Motion anim40;
        public Motion anim41;
        public Motion anim42;
        public Motion anim43;
        public Motion anim50;
        public Motion anim51;
        public Motion anim52;
        public Motion anim53;
        public Motion anim54;
        public Motion anim60;
        public Motion anim61;
        public Motion anim62;
        public Motion anim63;
        public Motion anim64;
        public Motion anim65;
        public Motion anim70;
        public Motion anim71;
        public Motion anim72;
        public Motion anim73;
        public Motion anim74;
        public Motion anim75;
        public Motion anim76;

        public List<AnimationClip> blinking;
        public List<LimitedLipsyncAnimation> limitedLipsync;

        public ExpressionEditorPreviewable previewSetup;
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

        public Motion[] AllMotions()
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
                anim11_L, anim11_R,
                //
                anim10,
                anim20, anim21,
                anim30, anim31, anim32,
                anim40, anim41, anim42, anim43,
                anim50, anim51, anim52, anim53, anim54,
                anim60, anim61, anim62, anim63, anim64, anim65,
                anim70, anim71, anim72, anim73, anim74, anim75, anim76,
            };
        }
    }
}
