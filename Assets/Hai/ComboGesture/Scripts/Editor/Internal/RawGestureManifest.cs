using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal
{
    internal class RawGestureManifest
    {
        private List<AnimationClip> Manifest { get; }
        public List<AnimationClip> Blinking { get; }
        public List<AnimationClip> LimitedLipsync { get; }
        public float TransitionDuration { get; }

        private RawGestureManifest(List<AnimationClip> manifest, List<AnimationClip> blinking, List<AnimationClip> limitedLipsync, float transitionDuration)
        {
            Manifest = manifest;
            Blinking = blinking.Intersect(manifest).ToList();
            LimitedLipsync = limitedLipsync.Intersect(manifest).ToList();
            TransitionDuration = transitionDuration;

            if (manifest.Any(clip => clip == null))
            {
                throw new ArgumentException();
            }
        }

        public static RawGestureManifest FromActivity(ComboGestureActivity activity, AnimationClip fallbackWhen00ClipIsNull)
        {
            var neutral = activity.anim00 ? activity.anim00 : fallbackWhen00ClipIsNull;
            return new RawGestureManifest(activity.OrderedAnimations().Select(clip => clip ? clip : neutral).ToList(), activity.blinking, activity.limitedLipsync.Select(lipsyncAnimation => lipsyncAnimation.clip).ToList(), activity.transitionDuration);
        }

        public static RawGestureManifest AllSlotsFittedWithSameClip(AnimationClip clip)
        {
            return new RawGestureManifest(
                Enumerable.Repeat(clip, 36 + 2).ToList(),
                new List<AnimationClip>(),
                new List<AnimationClip>(),
                0.1f);
        }

        public RawGestureManifest NewFromRemappedAnimations(Dictionary<QualifiedAnimation, AnimationClip> remapping)
        {
            return new RawGestureManifest(
                AnimationClips().Select(clip => remapping[Qualify(clip)]).ToList(),
                Blinking.Select(clip => remapping[Qualify(clip)]).ToList(),
                LimitedLipsync.Select(clip => remapping[Qualify(clip)]).ToList(),
                TransitionDuration
            );
        }

        public QualifiedAnimation Qualify(AnimationClip clip)
        {
            return new QualifiedAnimation(
                clip,
                new Qualification(
                    Blinking.Contains(clip),
                    LimitedLipsync.Contains(clip) ? QualifiedLimitation.Wide : QualifiedLimitation.None
                )
            );
        }

        internal struct QualifiedAnimation
        {
            public QualifiedAnimation(AnimationClip clip, Qualification qualification)
            {
                Clip = clip;
                Qualification = qualification;
            }

            public AnimationClip Clip { get; }
            public Qualification Qualification { get; }
        }

        internal struct Qualification
        {
            public Qualification(bool isBlinking, QualifiedLimitation limitation)
            {
                IsBlinking = isBlinking;
                Limitation = limitation;
            }

            public bool IsBlinking { get; }
            public QualifiedLimitation Limitation { get; }

            public bool Equals(Qualification other)
            {
                return IsBlinking == other.IsBlinking && Limitation == other.Limitation;
            }

            public override bool Equals(object obj)
            {
                return obj is Qualification other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (IsBlinking.GetHashCode() * 397) ^ (int) Limitation;
                }
            }

            public static bool operator ==(Qualification left, Qualification right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Qualification left, Qualification right)
            {
                return !left.Equals(right);
            }
        }

        internal enum QualifiedLimitation
        {
            None, Wide
        }

        public AnimationClip Anim00() { return Manifest[0]; }
        public AnimationClip Anim01() { return Manifest[1]; }
        public AnimationClip Anim02() { return Manifest[2]; }
        public AnimationClip Anim03() { return Manifest[3]; }
        public AnimationClip Anim04() { return Manifest[4]; }
        public AnimationClip Anim05() { return Manifest[5]; }
        public AnimationClip Anim06() { return Manifest[6]; }
        public AnimationClip Anim07() { return Manifest[7]; }
        public AnimationClip Anim11() { return Manifest[8]; }
        public AnimationClip Anim12() { return Manifest[9]; }
        public AnimationClip Anim13() { return Manifest[10]; }
        public AnimationClip Anim14() { return Manifest[11]; }
        public AnimationClip Anim15() { return Manifest[12]; }
        public AnimationClip Anim16() { return Manifest[13]; }
        public AnimationClip Anim17() { return Manifest[14]; }
        public AnimationClip Anim22() { return Manifest[15]; }
        public AnimationClip Anim23() { return Manifest[16]; }
        public AnimationClip Anim24() { return Manifest[17]; }
        public AnimationClip Anim25() { return Manifest[18]; }
        public AnimationClip Anim26() { return Manifest[19]; }
        public AnimationClip Anim27() { return Manifest[20]; }
        public AnimationClip Anim33() { return Manifest[21]; }
        public AnimationClip Anim34() { return Manifest[22]; }
        public AnimationClip Anim35() { return Manifest[23]; }
        public AnimationClip Anim36() { return Manifest[24]; }
        public AnimationClip Anim37() { return Manifest[25]; }
        public AnimationClip Anim44() { return Manifest[26]; }
        public AnimationClip Anim45() { return Manifest[27]; }
        public AnimationClip Anim46() { return Manifest[28]; }
        public AnimationClip Anim47() { return Manifest[29]; }
        public AnimationClip Anim55() { return Manifest[30]; }
        public AnimationClip Anim56() { return Manifest[31]; }
        public AnimationClip Anim57() { return Manifest[32]; }
        public AnimationClip Anim66() { return Manifest[33]; }
        public AnimationClip Anim67() { return Manifest[34]; }
        public AnimationClip Anim77() { return Manifest[35]; }
        //
        public AnimationClip Anim11_L() { return Manifest[36]; }
        public AnimationClip Anim11_R() { return Manifest[37]; }

        public IEnumerable<AnimationClip> AnimationClips()
        {
            return (IEnumerable<AnimationClip>) Manifest;
        }
    }
}
