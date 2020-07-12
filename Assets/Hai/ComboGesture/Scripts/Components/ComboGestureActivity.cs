#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Internal;
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

        public List<AnimationClip> blinking;
    }
}

#endif