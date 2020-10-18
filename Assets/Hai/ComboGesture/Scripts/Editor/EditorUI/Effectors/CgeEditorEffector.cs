using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors
{
    public class CgeEditorState
    {
        public CurrentlyEditing CurrentlyEditing = CurrentlyEditing.Nothing;
        public ComboGestureActivity Activity { get; internal set; }
        public ComboGesturePuppet Puppet { get; internal set; }
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

        public bool IsPreviewSetupValid()
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return false;
                case CurrentlyEditing.Activity:
                    return  _state.Activity.previewSetup != null && _state.Activity.previewSetup.IsValid();
                case CurrentlyEditing.Puppet:
                    return  _state.Puppet.previewSetup != null && _state.Puppet.previewSetup.IsValid();
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    break;
                case CurrentlyEditing.Activity:
                    _state.SetupResult = new AutoSetupPreview(this).AutoSetup();
                    break;
                case CurrentlyEditing.Puppet:
                    _state.SetupResult = new AutoSetupPreview(this).AutoSetup();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            // FIXME: Use polymorphism
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return new List<AnimationClip>();
                case CurrentlyEditing.Activity:
                    return _state.Activity.AllDistinctAnimations();
                case CurrentlyEditing.Puppet:
                    return _state.Puppet.AllDistinctAnimations();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<AnimationClip> MutableBlinking()
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return new List<AnimationClip>();
                case CurrentlyEditing.Activity:
                    return _state.Activity.blinking;
                case CurrentlyEditing.Puppet:
                    return _state.Puppet.blinking;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ComboGesturePreviewSetup PreviewSetup()
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return null;
                case CurrentlyEditing.Activity:
                    return _state.Activity.previewSetup;
                case CurrentlyEditing.Puppet:
                    return _state.Puppet.previewSetup;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<ComboGestureActivity.LimitedLipsyncAnimation> MutableLimitedLipsync()
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return null;
                case CurrentlyEditing.Activity:
                    return _state.Activity.limitedLipsync;
                case CurrentlyEditing.Puppet:
                    return _state.Puppet.limitedLipsync;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool HasPreviewSetupWhichIsInvalid()
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    return false;
                case CurrentlyEditing.Activity:
                    return _state.Activity.previewSetup != null && !_state.Activity.previewSetup.IsValid();
                case CurrentlyEditing.Puppet:
                    return _state.Puppet.previewSetup != null && !_state.Puppet.previewSetup.IsValid();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetPreviewSetup(ComboGesturePreviewSetup previewSetup)
        {
            switch (_state.CurrentlyEditing)
            {
                case CurrentlyEditing.Nothing:
                    break;
                case CurrentlyEditing.Activity:
                    _state.Activity.previewSetup = previewSetup;
                    break;
                case CurrentlyEditing.Puppet:
                    _state.Puppet.previewSetup = previewSetup;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AdditionalEditorsMode GetAdditionalEditor()
        {
            return _state.AdditionalEditor;
        }

        public void SwitchAdditionalEditorTo(AdditionalEditorsMode mode)
        {
            _state.AdditionalEditor = mode;
        }
    }

    public enum CurrentlyEditing
    {
        Nothing,
        Activity,
        Puppet
    }
}
