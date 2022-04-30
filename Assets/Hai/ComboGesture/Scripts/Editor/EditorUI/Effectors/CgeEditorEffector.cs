using System.Collections.Generic;
using System.Linq;
using Hai.ComboGesture.Scripts.Components;
using Hai.ComboGesture.Scripts.Editor.Internal.Processing;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    interface IActivityAccessor
    {
        List<AnimationClip> AllDistinctAnimations { get; }
        List<AnimationClip> Blinking { get; }
        Animator PreviewSetup { get; set; }
        void RecordMutation();
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
        public void RecordMutation()
        {
            Undo.RecordObject(_activity, "Activity modified");
        }

        public Animator PreviewSetup
        {
            get => _activity.previewAnimator;
            set => _activity.previewAnimator = value;
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
        public Animator PreviewSetup
        {
            get => _puppet.previewAnimator;
            set => _puppet.previewAnimator = value;
        }
        public void RecordMutation()
        {
            Undo.RecordObject(_puppet, "Puppet modified");
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
    }

    public class CgeEditorEffector
    {
        private readonly CgeEditorState _state;

        public CgeEditorEffector(CgeEditorState state)
        {
            _state = state;
        }

        public SerializedProperty SpTransitionDuration() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.transitionDuration));
        public SerializedProperty SpEditorTool() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.editorTool));
        public SerializedProperty SpEditorArbitraryAnimations() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.editorArbitraryAnimations));
        public SerializedProperty SpPreviewSetup() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.previewAnimator));
        public SerializedProperty SpEnablePermutations() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.enablePermutations));
        public SerializedProperty SpOneHandMode() => _state.SerializedObject.FindProperty(nameof(ComboGestureActivity.oneHandMode));

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

        public void TryAutoSetup()
        {
            var firstAvatar = Object.FindObjectOfType<VRCAvatarDescriptor>();
            if (firstAvatar != null)
            {
                var itsAnimator = firstAvatar.GetComponent<Animator>();
                if (itsAnimator != null)
                {
                    SetPreviewSetup(itsAnimator);
                }
            }
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

        public bool BlinkingContains(AnimationClip clip)
        {
            return _state.ActivityAccessor.Blinking.Contains(clip);
        }

        public void AddToBlinking(AnimationClip clip)
        {
            _state.ActivityAccessor.RecordMutation();
            _state.ActivityAccessor.Blinking.Add(clip);
        }

        public void RemoveFromBlinking(AnimationClip clip)
        {
            _state.ActivityAccessor.RecordMutation();
            _state.ActivityAccessor.Blinking.Remove(clip);
        }

        public Animator PreviewSetup()
        {
            return _state.ActivityAccessor.PreviewSetup;
        }

        public bool IsPreviewSetupValid()
        {
            return _state.ActivityAccessor.PreviewSetup != null;
        }

        public bool HasPreviewSetupWhichIsInvalid()
        {
            return false;
        }

        public void SetPreviewSetup(Animator previewSetup)
        {
            _state.ActivityAccessor.RecordMutation();
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
