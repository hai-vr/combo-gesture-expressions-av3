using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using static Hai.ComboGesture.Scripts.Editor.Internal.CgeAac.CgeAacFlConditionSimple;
using static UnityEditor.Animations.AnimatorConditionMode;

namespace Hai.ComboGesture.Scripts.Editor.Internal.CgeAac
{
    class CgeAacFlConditionSimple : ICgeAacFlCondition
    {
        private readonly Action<CgeAacFlCondition> _action;

        public CgeAacFlConditionSimple(Action<CgeAacFlCondition> action)
        {
            _action = action;
        }

        public static CgeAacFlConditionSimple Just(Action<CgeAacFlCondition> action)
        {
            return new CgeAacFlConditionSimple(action);
        }

        public static CgeAacFlConditionSimple ForEach(string[] subjects, Action<string, CgeAacFlCondition> action)
        {
            return new CgeAacFlConditionSimple(condition =>
            {
                foreach (var subject in subjects)
                {
                    action.Invoke(subject, condition);
                }
            });
        }

        public void ApplyTo(CgeAacFlCondition appender)
        {
            _action.Invoke(appender);
        }
    }

    public abstract class CgeAacFlParameter
    {
        public string Name { get; }

        protected CgeAacFlParameter(string name)
        {
            Name = name;
        }
    }

    public class CgeAacFlFloatParameter : CgeAacFlParameter
    {
        internal static CgeAacFlFloatParameter Internally(string name) => new CgeAacFlFloatParameter(name);
        protected CgeAacFlFloatParameter(string name) : base(name) { }
        public ICgeAacFlCondition IsGreaterThan(float other) => Just(condition => condition.Add(Name, Greater, other));
        public ICgeAacFlCondition IsLessThan(float other) => Just(condition => condition.Add(Name, Less, other));
    }

    public class CgeAacFlIntParameter : CgeAacFlParameter
    {
        internal static CgeAacFlIntParameter Internally(string name) => new CgeAacFlIntParameter(name);
        protected CgeAacFlIntParameter(string name) : base(name) { }
        public ICgeAacFlCondition IsGreaterThan(int other) => Just(condition => condition.Add(Name, Greater, other));
        public ICgeAacFlCondition IsLessThan(int other) => Just(condition => condition.Add(Name, Less, other));
        public ICgeAacFlCondition IsEqualTo(int other) => Just(condition => condition.Add(Name, AnimatorConditionMode.Equals, other));
        public ICgeAacFlCondition IsNotEqualTo(int other) => Just(condition => condition.Add(Name, NotEqual, other));
    }

    public class CgeAacFlEnumIntParameter<TEnum> : CgeAacFlIntParameter where TEnum : Enum
    {
        internal static CgeAacFlEnumIntParameter<TInEnum> Internally<TInEnum>(string name) where TInEnum : Enum => new CgeAacFlEnumIntParameter<TInEnum>(name);
        protected CgeAacFlEnumIntParameter(string name) : base(name)
        {
        }

        public ICgeAacFlCondition IsEqualTo(TEnum other) => IsEqualTo((int)(object)other);
        public ICgeAacFlCondition IsNotEqualTo(TEnum other) => IsNotEqualTo((int)(object)other);
    }

    public class CgeAacFlBoolParameter : CgeAacFlParameter
    {
        internal static CgeAacFlBoolParameter Internally(string name) => new CgeAacFlBoolParameter(name);
        protected CgeAacFlBoolParameter(string name) : base(name) { }
        public ICgeAacFlCondition IsTrue() => Just(condition => condition.Add(Name, If, 0));
        public ICgeAacFlCondition IsFalse() => Just(condition => condition.Add(Name, IfNot, 0));
        public ICgeAacFlCondition IsEqualTo(bool other) => Just(condition => condition.Add(Name, other ? If : IfNot, 0));
        public ICgeAacFlCondition IsNotEqualTo(bool other) => Just(condition => condition.Add(Name, other ? IfNot : If, 0));
    }

    public class CgeAacFlFloatParameterGroup
    {
        internal static CgeAacFlFloatParameterGroup Internally(params string[] names) => new CgeAacFlFloatParameterGroup(names);
        private readonly string[] _names;
        private CgeAacFlFloatParameterGroup(params string[] names) { _names = names; }
        public List<CgeAacFlBoolParameter> ToList() => _names.Select(CgeAacFlBoolParameter.Internally).ToList();

        public ICgeAacFlCondition AreGreaterThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Greater, other));
        public ICgeAacFlCondition AreLessThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Less, other));
    }

    public class CgeAacFlIntParameterGroup
    {
        internal static CgeAacFlIntParameterGroup Internally(params string[] names) => new CgeAacFlIntParameterGroup(names);
        private readonly string[] _names;
        private CgeAacFlIntParameterGroup(params string[] names) { _names = names; }
        public List<CgeAacFlBoolParameter> ToList() => _names.Select(CgeAacFlBoolParameter.Internally).ToList();

        public ICgeAacFlCondition AreGreaterThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Greater, other));
        public ICgeAacFlCondition AreLessThan(float other) => ForEach(_names, (name, condition) => condition.Add(name, Less, other));
        public ICgeAacFlCondition AreEqualTo(float other) => ForEach(_names, (name, condition) => condition.Add(name, AnimatorConditionMode.Equals, other));
        public ICgeAacFlCondition AreNotEqualTo(float other) => ForEach(_names, (name, condition) => condition.Add(name, NotEqual, other));
    }

    public class CgeAacFlBoolParameterGroup
    {
        internal static CgeAacFlBoolParameterGroup Internally(params string[] names) => new CgeAacFlBoolParameterGroup(names);
        private readonly string[] _names;
        private CgeAacFlBoolParameterGroup(params string[] names) { _names = names; }
        public List<CgeAacFlBoolParameter> ToList() => _names.Select(CgeAacFlBoolParameter.Internally).ToList();

        public ICgeAacFlCondition AreTrue() => ForEach(_names, (name, condition) => condition.Add(name, If, 0));
        public ICgeAacFlCondition AreFalse() => ForEach(_names, (name, condition) => condition.Add(name, IfNot, 0));
        public ICgeAacFlCondition AreEqualTo(bool other) => ForEach(_names, (name, condition) => condition.Add(name, other ? If : IfNot, 0));

/// is true when all of the following conditions are met:
/// <ul>
/// <li>all of the parameters in the group must be false except for the parameter defined in exceptThisMustBeTrue if it is present in the group.</li>
/// <li>the parameter defined in exceptThisMustBeTrue must be true.</li>
/// </ul>
        public ICgeAacFlCondition AreFalseExcept(CgeAacFlBoolParameter exceptThisMustBeTrue)
        {
            var group = new CgeAacFlBoolParameterGroup(exceptThisMustBeTrue.Name);
            return AreFalseExcept(group);
        }

        public ICgeAacFlCondition AreFalseExcept(params CgeAacFlBoolParameter[] exceptTheseMustBeTrue)
        {
            var group = new CgeAacFlBoolParameterGroup(exceptTheseMustBeTrue.Select(parameter => parameter.Name).ToArray());
            return AreFalseExcept(group);
        }

        public ICgeAacFlCondition AreFalseExcept(CgeAacFlBoolParameterGroup exceptTheseMustBeTrue) => Just(condition =>
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

        public ICgeAacFlCondition AreTrueExcept(CgeAacFlBoolParameter exceptThisMustBeFalse)
        {
            var group = new CgeAacFlBoolParameterGroup(exceptThisMustBeFalse.Name);
            return AreTrueExcept(group);
        }

        public ICgeAacFlCondition AreTrueExcept(params CgeAacFlBoolParameter[] exceptTheseMustBeFalse)
        {
            var group = new CgeAacFlBoolParameterGroup(exceptTheseMustBeFalse.Select(parameter => parameter.Name).ToArray());
            return AreTrueExcept(group);
        }

        public ICgeAacFlCondition AreTrueExcept(CgeAacFlBoolParameterGroup exceptTheseMustBeFalse) => Just(condition =>
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

        public ICgeAacFlOrCondition IsAnyTrue()
        {
            return IsAnyEqualTo(true);
        }

        public ICgeAacFlOrCondition IsAnyFalse()
        {
            return IsAnyEqualTo(false);
        }

        private ICgeAacFlOrCondition IsAnyEqualTo(bool value)
        {
            return new CgeAacFlBoolParameterIsAnyOrCondition(_names, value);
        }
    }

    internal class CgeAacFlBoolParameterIsAnyOrCondition : ICgeAacFlOrCondition
    {
        private readonly string[] _names;
        private readonly bool _value;

        public CgeAacFlBoolParameterIsAnyOrCondition(string[] names, bool value)
        {
            _names = names;
            _value = value;
        }

        public List<CgeAacFlTransitionContinuation> ApplyTo(CgeAacFlNewTransitionContinuation firstContinuation)
        {
            var pendingContinuations = new List<CgeAacFlTransitionContinuation>();

            var newContinuation = firstContinuation;
            for (var index = 0; index < _names.Length; index++)
            {
                var name = _names[index];
                var pendingContinuation = newContinuation.When(CgeAacFlBoolParameter.Internally(name).IsEqualTo(_value));
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
