using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Stats
{
    /// <summary>
    /// Represents a single stat with base value and modifiers
    /// </summary>
    [Serializable]
    public class Stat
    {
        [SerializeField] private Core.StatType _statType;
        [SerializeField] private float _baseValue;
        
        private bool _isDirty = true;
        private float _cachedValue;
        private readonly List<Core.StatModifier> _modifiers = new List<Core.StatModifier>();

        public Core.StatType StatType => _statType;
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                _isDirty = true;
            }
        }

        public event Action<Stat> OnValueChanged;

        public Stat(Core.StatType statType, float baseValue = 0)
        {
            _statType = statType;
            _baseValue = baseValue;
        }

        /// <summary>
        /// Gets the final calculated value with all modifiers applied
        /// </summary>
        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _cachedValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _cachedValue;
            }
        }

        public void AddModifier(Core.StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            _isDirty = true;
            OnValueChanged?.Invoke(this);
        }

        public bool RemoveModifier(Core.StatModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _isDirty = true;
                OnValueChanged?.Invoke(this);
                return true;
            }
            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            bool removed = false;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].Source == source)
                {
                    _modifiers.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                _isDirty = true;
                OnValueChanged?.Invoke(this);
            }
            return removed;
        }

        public IReadOnlyList<Core.StatModifier> GetModifiers() => _modifiers.AsReadOnly();

        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;
            float sumPercentAdd = 0;

            foreach (var modifier in _modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case Core.ModifierType.Flat:
                        finalValue += modifier.Value;
                        break;
                    case Core.ModifierType.PercentAdd:
                        sumPercentAdd += modifier.Value;
                        break;
                    case Core.ModifierType.PercentMult:
                        // Apply any accumulated PercentAdd before PercentMult
                        if (sumPercentAdd != 0)
                        {
                            finalValue *= (1 + sumPercentAdd);
                            sumPercentAdd = 0;
                        }
                        finalValue *= (1 + modifier.Value);
                        break;
                }
            }

            // Apply any remaining PercentAdd
            if (sumPercentAdd != 0)
            {
                finalValue *= (1 + sumPercentAdd);
            }

            return (float)Math.Round(finalValue, 4);
        }

        public void SetDirty()
        {
            _isDirty = true;
        }
    }
}
