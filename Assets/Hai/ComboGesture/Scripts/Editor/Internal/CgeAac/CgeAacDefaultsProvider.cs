using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    public interface ICgeAacDefaultsProvider
    {
        void ConfigureState(AnimatorState state, AnimationClip emptyClip);
        void ConfigureTransition(AnimatorStateTransition transition);
        string ConvertLayerName(string systemName);
        string ConvertLayerNameWithSuffix(string systemName, string suffix);
        Vector2 Grid();
        void ConfigureStateMachine(AnimatorStateMachine stateMachine);
    }

    public class CgeAacDefaultsProvider : ICgeAacDefaultsProvider
    {
        private readonly bool _writeDefaults;

        public CgeAacDefaultsProvider(bool writeDefaults = false)
        {
            _writeDefaults = writeDefaults;
        }

        public virtual void ConfigureState(AnimatorState state, AnimationClip emptyClip)
        {
            state.motion = emptyClip;
            state.writeDefaultValues = _writeDefaults;
        }

        public virtual void ConfigureTransition(AnimatorStateTransition transition)
        {
            transition.duration = 0;
            transition.hasExitTime = false;
            transition.exitTime = 0;
            transition.hasFixedDuration = true;
            transition.offset = 0;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

        public virtual string ConvertLayerName(string systemName)
        {
            return systemName;
        }

        public virtual string ConvertLayerNameWithSuffix(string systemName, string suffix)
        {
            return $"{systemName}__{suffix}";
        }

        public Vector2 Grid()
        {
            return new Vector2(250, 70);
        }

        public void ConfigureStateMachine(AnimatorStateMachine stateMachine)
        {
            var grid = Grid();
            stateMachine.anyStatePosition = grid * new Vector2(0, 7);
            stateMachine.entryPosition = grid * new Vector2(0, -1);
            stateMachine.exitPosition = grid * new Vector2(7, -1);
            stateMachine.parentStateMachinePosition = grid * new Vector2(3, -1);
        }
    }
}