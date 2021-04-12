using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public struct RenderingSample
    {
        public readonly AnimationClip Clip;
        public readonly Texture2D RenderTexture;
        public readonly Action<RenderingSample> Callback;

        public RenderingSample(AnimationClip clip, Texture2D renderTexture, Action<RenderingSample> callback)
        {
            Clip = clip;
            RenderTexture = renderTexture;
            Callback = callback;
        }
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
