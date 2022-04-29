using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using static Hai.ComboGesture.Scripts.Editor.Internal.CgeAac.AacFlConditionSimple;
using static UnityEditor.Animations.AnimatorConditionMode;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    class AacFlConditionSimple : IAacFlCondition
    {
        private readonly Action<AacFlCondition> _action;

        public AacFlConditionSimple(Action<AacFlCondition> action)
        {
            _action = action;
        }

        public static AacFlConditionSimple Just(Action<AacFlCondition> action)
        {
            return new AacFlConditionSimple(action);
        }

        public static AacFlConditionSimple ForEach(string[] subjects, Action<string, AacFlCondition> action)
        {
            return new AacFlConditionSimple(condition =>
            {
                foreach (var subject in subjects)
                {
                    action.Invoke(subject, condition);
                }
            });
        }

        public void ApplyTo(AacFlCondition appender)
        {
            _action.Invoke(appender);
        }
    }

    public abstract class AacFlParameter
    {
        public string Name { get; }

        protected AacFlParameter(string name)
        {
            Name = name;
        }
    }

    public class AacFlFloatParameter : AacFlParameter
    {
        internal static AacFlFloatParameter Internally(string name) => new AacFlFloatParameter(name);
        protected AacFlFloatParameter(string name) : base(name) { }
        public IAacFlCondition IsGreaterThan(float other) => Just(condition => condition.Add(Name, Greater, other));
        public IAacFlCondition IsLessThan(float other) => Just(condition => condition.Add(Name, Less, other));
    }

    public class AacFlIntParameter : AacFlParameter
    {
        internal static AacFlIntParameter Internally(string name) => new AacFlIntParameter(name);
        protected AacFlIntParameter(string name) : base(name) { }
        public IAacFlCondition IsGreaterThan(int other) => Just(condition => condition.Add(Name, Greater, other));
        public IAacFlCondition IsLessThan(int other) => Just(condition => condition.Add(Name, Less, other));
        public IAacFlCondition IsEqualTo(int other) => Just(condition => condition.Add(Name, AnimatorConditionMode.Equals, other));
        public IAacFlCondition IsNotEqualTo(int other) => Just(condition => condition.Add(Name, NotEqual, other));
    }

    public class AacFlEnumIntParameter<TEnum> : AacFlIntParameter where TEnum : Enum
    {
        internal static AacFlEnumIntParameter<TInEnum> Internally<TInEnum>(string name) where TInEnum : Enum => new AacFlEnumIntParameter<TInEnum>(name);
        protected AacFlEnumIntParameter(string name) : base(name)
        {
        }

        public IAacFlCondition IsEqualTo(TEnum other) => IsEqualTo((int)(object)other);
        public IAacFlCondition IsNotEqualTo(TEnum other) => IsNotEqualTo((int)(object)other);
    }

    public class AacFlBoolParameter : AacFlParameter
    {
        internal static AacFlBoolParameter Internally(string name) => new AacFlBoolParameter(name);
        protected AacFlBoolParameter(string name) : base(name) { }
        public IAacFlCondition IsTrue() => Just(condition => condition.Add(Name, If, 0));
        public IAacFlCondition IsFalse() => Just(condition => condition.Add(Name, IfNot, 0));
        public IAacFlCondition IsEqualTo(bool other) => Just(condition => condition.Add(Name, other ? If : IfNot, 0));
        public IAacFlCondition IsNotEqualTo(bool other) => Just(condition => condition.Add(Name, other ? IfNot : If, 0));
    }

    public class AacFlFloatParameterGroup
    {
        internal static AacFlFloatParameterGroup Internally(params string[] names) => new AacFlFloatParameterGroup(names);
        private readonly string[] _names;
        private AacFlFloatParameterGroup(params string[] names) { _names = names; }
        public List<AacFlBoolParameter> ToList() => _names.Select(AacFlBoolParameter.Internally).ToList();

        public IAacFlCondition AreGreaterThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Greater, other));
        public IAacFlCondition AreLessThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Less, other));
    }

    public class AacFlIntParameterGroup
    {
        internal static AacFlIntParameterGroup Internally(params string[] names) => new AacFlIntParameterGroup(names);
        private readonly string[] _names;
        private AacFlIntParameterGroup(params string[] names) { _names = names; }
        public List<AacFlBoolParameter> ToList() => _names.Select(AacFlBoolParameter.Internally).ToList();

        public IAacFlCondition AreGreaterThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Greater, other));
        public IAacFlCondition AreLessThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Less, other));
        public IAacFlCondition AreEqualTo(float other) => ForEach(_names, (name, condition) => condition.Add(name, AnimatorConditionMode.Equals, other));
        public IAacFlCondition AreNotEqualTo(float other) => ForEach(_names, (name, condition) => condition.Add(name, NotEqual, other));
    }

    public class AacFlBoolParameterGroup
    {
        internal static AacFlBoolParameterGroup Internally(params string[] names) => new AacFlBoolParameterGroup(names);
        private readonly string[] _names;
        private AacFlBoolParameterGroup(params string[] names) { _names = names; }
        public List<AacFlBoolParameter> ToList() => _names.Select(AacFlBoolParameter.Internally).ToList();

        public IAacFlCondition AreTrue() => ForEach(_names, (name, condition) => condition.Add(name, If, 0));
        public IAacFlCondition AreFalse() => ForEach(_names, (name, condition) => condition.Add(name, IfNot, 0));
        public IAacFlCondition AreEqualTo(bool other) => ForEach(_names, (name, condition) => condition.Add(name, other ? If : IfNot, 0));

/// is true when all of the following conditions are met:
/// <ul>
/// <li>all of the parameters in the group must be false except for the parameter defined in exceptThisMustBeTrue if it is present in the group.</li>
/// <li>the parameter defined in exceptThisMustBeTrue must be true.</li>
/// </ul>
        public IAacFlCondition AreFalseExcept(AacFlBoolParameter exceptThisMustBeTrue)
        {
            var group = new AacFlBoolParameterGroup(exceptThisMustBeTrue.Name);
            return AreFalseExcept(group);
        }

        public IAacFlCondition AreFalseExcept(params AacFlBoolParameter[] exceptTheseMustBeTrue)
        {
            var group = new AacFlBoolParameterGroup(exceptTheseMustBeTrue.Select(parameter => parameter.Name).ToArray());
            return AreFalseExcept(group);
        }

        public IAacFlCondition AreFalseExcept(AacFlBoolParameterGroup exceptTheseMustBeTrue) => Just(condition =>
        {
            foreach (var name in _names.Where(name => !exceptTheseMustBeTrue._names.Contains(name)))
            {
                condition.Add(name, IfNot, 0);
            }
            foreach (var name in exceptTheseMustBeTrue._names)
            {
                condition.Add(name, If, 0);
            }
        });

        public IAacFlCondition AreTrueExcept(AacFlBoolParameter exceptThisMustBeFalse)
        {
            var group = new AacFlBoolParameterGroup(exceptThisMustBeFalse.Name);
            return AreTrueExcept(group);
        }

        public IAacFlCondition AreTrueExcept(params AacFlBoolParameter[] exceptTheseMustBeFalse)
        {
            var group = new AacFlBoolParameterGroup(exceptTheseMustBeFalse.Select(parameter => parameter.Name).ToArray());
            return AreTrueExcept(group);
        }

        public IAacFlCondition AreTrueExcept(AacFlBoolParameterGroup exceptTheseMustBeFalse) => Just(condition =>
        {
            foreach (var name in _names.Where(name => !exceptTheseMustBeFalse._names.Contains(name)))
            {
                condition.Add(name, If, 0);
            }
            foreach (var name in exceptTheseMustBeFalse._names)
            {
                condition.Add(name, IfNot, 0);
            }
        });

        public IAacFlOrCondition IsAnyTrue()
        {
            return IsAnyEqualTo(true);
        }

        public IAacFlOrCondition IsAnyFalse()
        {
            return IsAnyEqualTo(false);
        }

        private IAacFlOrCondition IsAnyEqualTo(bool value)
        {
            return new AacFlBoolParameterIsAnyOrCondition(_names, value);
        }
    }

    internal class AacFlBoolParameterIsAnyOrCondition : IAacFlOrCondition
    {
        private readonly string[] _names;
        private readonly bool _value;

        public AacFlBoolParameterIsAnyOrCondition(string[] names, bool value)
        {
            _names = names;
            _value = value;
        }

        public List<AacFlTransitionContinuation> ApplyTo(AacFlNewTransitionContinuation firstContinuation)
        {
            var pendingContinuations = new List<AacFlTransitionContinuation>();

            var newContinuation = firstContinuation;
            for (var index = 0; index < _names.Length; index++)
            {
                var name = _names[index];
                var pendingContinuation = newContinuation.When(AacFlBoolParameter.Internally(name).IsEqualTo(_value));
                pendingContinuations.Add(pendingContinuation);
                if (index < _names.Length - 1)
                {
                    newContinuation = pendingContinuation.Or();
                }
            }

            return pendingContinuations;
        }
    }
}
