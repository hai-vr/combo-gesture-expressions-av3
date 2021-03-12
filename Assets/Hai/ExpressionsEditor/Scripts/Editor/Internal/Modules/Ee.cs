using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Editor.Internal.Modules
{
    public class Ee
    {
        public readonly EeRenderingCommands RenderingCommands;
        public readonly EeMetadata Metadata;
        public readonly EePreviewHandler PreviewHandler;
        public readonly EeAnimationEditorState AnimationEditorState;

        public readonly EeSelectionCommands SelectionCommands;
        public readonly EeEditCommands EditCommands;
        public readonly EePreviewCommands PreviewCommands;
        public readonly EeAccessCommands AccessCommands;

        public readonly bool IsCgeInstalled = Type.GetType($"Hai.ComboGesture.Scripts.Editor.EditorUI.Modules.Cge") != null;

        public readonly EeHooks Hooks;

        private static Ee _ee;

        public static Ee Get()
        {
            if (_ee != null) return _ee;

            _ee = new Ee();
            return _ee;
        }

        private Ee()
        {
            RenderingCommands = new EeRenderingCommands();
            AnimationEditorState = new EeAnimationEditorState();
            Metadata = new EeMetadata();
            PreviewHandler = new EePreviewHandler(RenderingCommands, AnimationEditorState, Metadata);
            SelectionCommands = new EeSelectionCommands(AnimationEditorState, PreviewHandler);
            EditCommands = new EeEditCommands(AnimationEditorState, Metadata, PreviewHandler);
            PreviewCommands = new EePreviewCommands(AnimationEditorState, PreviewHandler);
            AccessCommands = new EeAccessCommands(AnimationEditorState, Metadata, PreviewHandler);
            Hooks = new EeHooks();
        }
    }

    public readonly struct EeNonEditableStats
    {
        public readonly List<EditorCurveBinding> SmrMimicBlendshapes;
        public readonly Dictionary<EeNonEditableLookup, int> OtherPropertyToCountLookup;
        public readonly bool HasAnyOtherStats;
        public readonly int EffectiveFrameDuration;
        public readonly EeQuirk Quirk;

        public EeNonEditableStats(List<EditorCurveBinding> smrMimicBlendshapes, Dictionary<EeNonEditableLookup, int> otherPropertyToCountLookup, int effectiveFrameDuration, EeQuirk quirk)
        {
            SmrMimicBlendshapes = smrMimicBlendshapes;
            OtherPropertyToCountLookup = otherPropertyToCountLookup;
            HasAnyOtherStats = otherPropertyToCountLookup.Values.Sum() > 0;
            EffectiveFrameDuration = effectiveFrameDuration;
            Quirk = quirk;
        }
    }

    public enum EeAnimationEditorScenePreviewMode
    {
        WhenNewClipSelected,
        Always,
        Never
    }

    public enum EeNonEditableLookup
    {
        Other,
        Transform,
        Animator,
        MaterialSwap,
        Shader,
        GameObjectToggle
    }

    public enum EeQuirk
    {
        OneFrame,
        EmptyIssue,
        FirstFrameIssue,
        MoreThanOneFrame
    }

    public struct EeEditableBlendshape
    {
        public string Property;
        public float Value;
        public bool IsVaryingOverTime;
        public float VaryingMinValue;
        public EditorCurveBinding Binding;
        public Texture2D BoundaryTexture;
        public string Path;
    }

    public struct EeExplorerBlendshape
    {
        public string Property;
        public EditorCurveBinding Binding;
        public Texture2D BoundaryTexture;
        public Texture2D HotspotTexture;
        public string Path;
    }

    public class EeHooks
    {
        private Action<EeHookOnMainRendered> _onMainRenderedListener;

        public void SetOnMainRenderedListener(Action<EeHookOnMainRendered> listener)
        {
            _onMainRenderedListener = listener;
        }

        public void PushOnMainRendered(EeHookOnMainRendered rendered)
        {
            if (_onMainRenderedListener == null) return;

            _onMainRenderedListener.Invoke(rendered);
        }
    }

    public struct EeHookOnMainRendered
    {
        public AnimationClip Clip;
        public Texture2D OutputTexture;
    }
}
