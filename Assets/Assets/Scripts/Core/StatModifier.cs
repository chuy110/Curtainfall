using System;
using UnityEngine;

namespace RPGFramework.Core
{
    /// <summary>
    /// Defines how a modifier is applied to a stat
    /// </summary>
    public enum ModifierType
    {
        Flat,           // Adds a flat value
        PercentAdd,     // Adds percentage (stacks additively with other PercentAdd)
        PercentMult     // Multiplies final value (stacks multiplicatively)
    }

    /// <summary>
    /// Represents a single modifier that can be applied to a stat
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        [SerializeField] private StatType _statType;
        [SerializeField] private float _value;
        [SerializeField] private ModifierType _modifierType;
        [SerializeField] private int _order; // For sorting application order
        [SerializeField] private object _source; // What applied this modifier

        public StatType StatType => _statType;
        public float Value => _value;
        public ModifierType ModifierType => _modifierType;
        public int Order => _order;
        public object Source => _source;

        public StatModifier(StatType statType, float value, ModifierType modifierType, int order = 0, object source = null)
        {
            _statType = statType;
            _value = value;
            _modifierType = modifierType;
            _order = order;
            _source = source;
        }

        // Constructor without order (uses default based on modifier type)
        public StatModifier(StatType statType, float value, ModifierType modifierType, object source)
            : this(statType, value, modifierType, (int)modifierType, source)
        {
        }
    }

    /// <summary>
    /// Serializable version for use in ScriptableObjects
    /// </summary>
    [Serializable]
    public class SerializableStatModifier
    {
        public StatType StatType;
        public float Value;
        public ModifierType ModifierType;

        public StatModifier ToStatModifier(object source)
        {
            return new StatModifier(StatType, Value, ModifierType, source);
        }
    }
}
