using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public struct AnimationPreview
    {
        public AnimationPreview(AnimationClip clip, Texture2D renderTexture)
        {
            Clip = clip;
            RenderTexture = renderTexture;
        }

        private bool Equals(AnimationPreview other)
        {
            return Equals(Clip, other.Clip) && Equals(RenderTexture, other.RenderTexture);
        }

        public override bool Equals(object obj)
        {
            return obj is AnimationPreview other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Clip != null ? Clip.GetHashCode() : 0) * 397) ^ (RenderTexture != null ? RenderTexture.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AnimationPreview left, AnimationPreview right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnimationPreview left, AnimationPreview right)
        {
            return !left.Equals(right);
        }

        public AnimationClip Clip { get; }
        public Texture2D RenderTexture { get; }
    }

    internal static class CgeRenderingProcessorUnityCycle
    {
        private static readonly Queue<Action> InternalActionQueue = new Queue<Action>();

        private static bool _isInternalActionQueueRunning;
        private static bool _isEditorUpdateHooked;

        public static void AddToQueue(Action action)
        {
            if (!_isInternalActionQueueRunning)
            {
                if (!_isEditorUpdateHooked)
                {
                    EditorApplication.update += OnEditorUpdate;
                    _isEditorUpdateHooked = true;
                }
                _isInternalActionQueueRunning = true;
            }

            InternalActionQueue.Enqueue(action);
        }

        private static void OnEditorUpdate()
        {
            if (!_isInternalActionQueueRunning) return;

            // Animation sampling will not affect the blendshapes when the window is not focused.
            if (!IsUnityEditorWindowFocused()) return;

            if (InternalActionQueue.Count == 0)
            {
                _isInternalActionQueueRunning = false;
                return;
            }

            var action = InternalActionQueue.Dequeue();

            action.Invoke();
        }

        private static bool IsUnityEditorWindowFocused()
        {
            return UnityEditorInternal.InternalEditorUtility.isApplicationActive;
        }
    }
}
