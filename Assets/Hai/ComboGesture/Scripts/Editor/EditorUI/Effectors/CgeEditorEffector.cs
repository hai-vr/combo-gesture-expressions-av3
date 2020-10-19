using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Processing;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    interface IActivityAccessor
    {
        List<AnimationClip> AllDistinctAnimations { get; }
        List<AnimationClip> Blinking { get; }
        ComboGesturePreviewSetup PreviewSetup { get; set; }
        List<ComboGestureActivity.LimitedLipsyncAnimation> LimitedLipsync { get; }
    }

    class CgeActivityAccessor : IActivityAccessor
    {
        private readonly ComboGestureActivity _activity;

        public CgeActivityAccessor(ComboGestureActivity activity)
        {
            _activity = activity;
        }

        public List<AnimationClip> AllDistinctAnimations => CgeEditorEffector.AllDistinctAnimations(_activity);
        public List<AnimationClip> Blinking => _activity.blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> LimitedLipsync => _activity.limitedLipsync;
        public ComboGesturePreviewSetup PreviewSetup
        {
            get => _activity.previewSetup;
            set => _activity.previewSetup = value;
        }
    }

    class CgePuppetAccessor : IActivityAccessor
    {
        private readonly ComboGesturePuppet _puppet;

        public CgePuppetAccessor(ComboGesturePuppet puppet)
        {
            _puppet = puppet;
        }

        public List<AnimationClip> AllDistinctAnimations => ManifestFromPuppet.AllDistinctAnimations(_puppet);
        public List<AnimationClip> Blinking => _puppet.blinking;
        public List<ComboGestureActivity.LimitedLipsyncAnimation> LimitedLipsync => _puppet.limitedLipsync;
        public ComboGesturePreviewSetup PreviewSetup
        {
            get => _puppet.previewSetup;
            set => _puppet.previewSetup = value;
        }
    }

    public class CgeEditorState
    {
        public CurrentlyEditing CurrentlyEditing = CurrentlyEditing.Nothing;
        public ComboGestureActivity Activity { get; internal set; }
        public ComboGesturePuppet Puppet { get; internal set; }
        internal IActivityAccessor ActivityAccessor { get; set; }
        internal SerializedObject SerializedObject;

        public int Mode { get; internal set; }
        public AdditionalEditorsMode AdditionalEditor { get; set; }

        public int CurrentEditorToolValue = -1;

        public bool FirstTimeSetup;
        public AutoSetupPreview.SetupResult? SetupResult;
    }

    public class CgeEditorEffector
    {
        private readonly CgeEditorState _state;

        public CgeEditorEffector(CgeEditorState state)
        {
            _state = state;
        }

        public SerializedProperty SpTransitionDuration() => _state.SerializedObject.FindProperty("transitionDuration");
        public SerializedProperty SpEditorTool() => _state.SerializedObject.FindProperty("editorTool");
        public SerializedProperty SpEditorArbitraryAnimations() => _state.SerializedObject.FindProperty("editorArbitraryAnimations");
        public SerializedProperty SpPreviewSetup() => _state.SerializedObject.FindProperty("previewSetup");
        public SerializedProperty SpEnablePermutations() => _state.SerializedObject.FindProperty("enablePermutations");

        public void NowEditingActivity(ComboGestureActivity selectedActivity)
        {
            _state.CurrentlyEditing = CurrentlyEditing.Activity;
            _state.Activity = selectedActivity;
            _state.Puppet = null;
            _state.SerializedObject = new SerializedObject(_state.Activity);
            _state.ActivityAccessor = new CgeActivityAccessor(_state.Activity);

            if (SpEditorTool().intValue != _state.CurrentEditorToolValue && _state.CurrentEditorToolValue >= 0)
            {
                SpEditorTool().intValue = _state.CurrentEditorToolValue;
                ApplyModifiedProperties();
            }
        }

        public void NowEditingPuppet(ComboGesturePuppet selectedPuppet)
        {
            _state.CurrentlyEditing = CurrentlyEditing.Puppet;
            _state.Activity = null;
            _state.Puppet = selectedPuppet;
            _state.SerializedObject = new SerializedObject(_state.Puppet);
            _state.ActivityAccessor = new CgePuppetAccessor(_state.Puppet);
        }

        public SerializedProperty SpProperty(string property)
        {
            return _state.SerializedObject.FindProperty(property);
        }

        public void SpUpdate()
        {
            _state.SerializedObject.Update();
        }

        public void ApplyModifiedProperties()
        {
            _state.SerializedObject.ApplyModifiedProperties();
        }

        public void SwitchTo(ActivityEditorMode mode)
        {
            _state.Mode = (int) mode;
        }

        public void SwitchTo(PuppetEditorMode mode)
        {
            _state.Mode = (int) mode;
        }

        public ActivityEditorMode CurrentActivityMode()
        {
            return (ActivityEditorMode) _state.Mode;
        }

        public PuppetEditorMode CurrentPuppetMode()
        {
            return (PuppetEditorMode) _state.Mode;
        }

        public ComboGestureActivity GetActivity()
        {
            return _state.Activity;
        }

        public ComboGesturePuppet GetPuppet()
        {
            return _state.Puppet;
        }

        public bool IsFirstTimeSetup()
        {
            return _state.FirstTimeSetup;
        }

        public AutoSetupPreview.SetupResult? GetSetupResult()
        {
            return _state.SetupResult;
        }

        public void TryAutoSetup()
        {
            _state.SetupResult = new AutoSetupPreview(this).AutoSetup();
        }

        public void MarkFirstTimeSetup()
        {
            _state.FirstTimeSetup = true;
        }

        public void ClearFirstTimeSetup()
        {
            _state.FirstTimeSetup = false;
        }

        public void SwitchCurrentEditorToolTo(int value)
        {
            _state.CurrentEditorToolValue = value;
        }

        public CurrentlyEditing GetCurrentlyEditing()
        {
            return _state.CurrentlyEditing;
        }

        public List<AnimationClip> AllDistinctAnimations()
        {
            return _state.ActivityAccessor.AllDistinctAnimations;
        }

        public List<AnimationClip> MutableBlinking()
        {
            return _state.ActivityAccessor.Blinking;
        }

        public ComboGesturePreviewSetup PreviewSetup()
        {
            return _state.ActivityAccessor.PreviewSetup;
        }

        public List<ComboGestureActivity.LimitedLipsyncAnimation> MutableLimitedLipsync()
        {
            return _state.ActivityAccessor.LimitedLipsync;
        }

        public bool IsPreviewSetupValid()
        {
            return _state.ActivityAccessor.PreviewSetup != null && _state.ActivityAccessor.PreviewSetup.IsValid();
        }

        public bool HasPreviewSetupWhichIsInvalid()
        {
            return _state.ActivityAccessor.PreviewSetup != null && !_state.ActivityAccessor.PreviewSetup.IsValid();
        }

        public void SetPreviewSetup(ComboGesturePreviewSetup previewSetup)
        {
            _state.ActivityAccessor.PreviewSetup = previewSetup;
        }

        public AdditionalEditorsMode GetAdditionalEditor()
        {
            return _state.AdditionalEditor;
        }

        public void SwitchAdditionalEditorTo(AdditionalEditorsMode mode)
        {
            _state.AdditionalEditor = mode;
        }

        public static List<AnimationClip> AllDistinctAnimations(ComboGestureActivity cga)
        {
            var direct = cga.AllMotions()
                .OfType<AnimationClip>()
                .ToList();
            var insideBlends = cga.AllMotions()
                .OfType<BlendTree>()
                .SelectMany(ManifestFromPuppet.AllAnimationsOf)
                .ToList();

            return direct.Concat(insideBlends)
                .Where(clip => clip != null)
                .Distinct()
                .ToList();
        }
    }

    public enum CurrentlyEditing
    {
        Nothing,
        Activity,
        Puppet
    }
}
