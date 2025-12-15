using System;
using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.Stats
{
    /// <summary>
    /// Container for all character stats. Attach to player or enemy GameObjects.
    /// Handles both primary stats (levelable) and derived stats (calculated).
    /// </summary>
    public class StatContainer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private StatGrowthConfig _growthConfig;
        
        [Header("Character Level")]
        [SerializeField] private int _level = 1;
        [SerializeField] private int _availableStatPoints = 0;
        
        [Header("Starting Primary Stats")]
        [SerializeField] private int _startingVigor = 10;
        [SerializeField] private int _startingEndurance = 10;
        [SerializeField] private int _startingStrength = 10;
        [SerializeField] private int _startingDexterity = 10;
        [SerializeField] private int _startingIntelligence = 10;
        [SerializeField] private int _startingFaith = 10;
        [SerializeField] private int _startingArcane = 10;

        private Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();
        private bool _initialized = false;

        // Events
        public event Action<StatType, float, float> OnStatChanged; // StatType, OldValue, NewValue
        public event Action<int> OnLevelUp;
        public event Action OnStatsRecalculated;

        // Properties
        public int Level => _level;
        public int AvailableStatPoints => _availableStatPoints;
        public StatGrowthConfig GrowthConfig => _growthConfig;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;
            
            // Initialize all primary stats
            InitializePrimaryStat(StatType.Vigor, _startingVigor);
            InitializePrimaryStat(StatType.Endurance, _startingEndurance);
            InitializePrimaryStat(StatType.Strength, _startingStrength);
            InitializePrimaryStat(StatType.Dexterity, _startingDexterity);
            InitializePrimaryStat(StatType.Intelligence, _startingIntelligence);
            InitializePrimaryStat(StatType.Faith, _startingFaith);
            InitializePrimaryStat(StatType.Arcane, _startingArcane);

            // Initialize all derived stats
            InitializeDerivedStat(StatType.MaxHealth);
            InitializeDerivedStat(StatType.MaxStamina);
            InitializeDerivedStat(StatType.MaxMana);
            InitializeDerivedStat(StatType.PhysicalAttack);
            InitializeDerivedStat(StatType.MagicAttack);
            InitializeDerivedStat(StatType.PhysicalDefense);
            InitializeDerivedStat(StatType.MagicDefense);
            InitializeDerivedStat(StatType.CriticalChance);
            InitializeDerivedStat(StatType.CriticalDamage);
            InitializeDerivedStat(StatType.AttackSpeed);
            InitializeDerivedStat(StatType.MovementSpeed);
            InitializeDerivedStat(StatType.EquipLoad);
            InitializeDerivedStat(StatType.Poise);
            InitializeDerivedStat(StatType.StatusResistance);
            InitializeDerivedStat(StatType.ItemDiscovery);

            RecalculateDerivedStats();
            _initialized = true;
        }

        private void InitializePrimaryStat(StatType type, float baseValue)
        {
            var stat = new Stat(type, baseValue);
            stat.OnValueChanged += HandleStatChanged;
            _stats[type] = stat;
        }

        private void InitializeDerivedStat(StatType type)
        {
            var stat = new Stat(type, 0);
            stat.OnValueChanged += HandleStatChanged;
            _stats[type] = stat;
        }

        private void HandleStatChanged(Stat stat)
        {
            // When a primary stat changes, recalculate derived stats
            if (IsPrimaryStat(stat.StatType))
            {
                RecalculateDerivedStats();
            }
        }

        /// <summary>
        /// Recalculates all derived stats based on current primary stats
        /// </summary>
        public void RecalculateDerivedStats()
        {
            if (_growthConfig == null)
            {
                Debug.LogWarning("StatContainer: No StatGrowthConfig assigned!");
                return;
            }

            UpdateDerivedStat(StatType.MaxHealth);
            UpdateDerivedStat(StatType.MaxStamina);
            UpdateDerivedStat(StatType.MaxMana);
            UpdateDerivedStat(StatType.PhysicalAttack);
            UpdateDerivedStat(StatType.MagicAttack);
            UpdateDerivedStat(StatType.PhysicalDefense);
            UpdateDerivedStat(StatType.MagicDefense);
            UpdateDerivedStat(StatType.CriticalChance);
            UpdateDerivedStat(StatType.CriticalDamage);
            UpdateDerivedStat(StatType.AttackSpeed);
            UpdateDerivedStat(StatType.MovementSpeed);
            UpdateDerivedStat(StatType.EquipLoad);
            UpdateDerivedStat(StatType.Poise);
            UpdateDerivedStat(StatType.StatusResistance);
            UpdateDerivedStat(StatType.ItemDiscovery);

            OnStatsRecalculated?.Invoke();
        }

        private void UpdateDerivedStat(StatType type)
        {
            if (_stats.TryGetValue(type, out Stat stat))
            {
                float oldValue = stat.Value;
                stat.BaseValue = _growthConfig.CalculateDerivedStat(type, this);
                float newValue = stat.Value;
                
                if (Math.Abs(oldValue - newValue) > 0.001f)
                {
                    OnStatChanged?.Invoke(type, oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// Get the final calculated value of a stat
        /// </summary>
        public float GetStatValue(StatType type)
        {
            if (_stats.TryGetValue(type, out Stat stat))
            {
                return stat.Value;
            }
            Debug.LogWarning($"StatContainer: Stat {type} not found!");
            return 0f;
        }

        /// <summary>
        /// Get the base value of a stat (without modifiers)
        /// </summary>
        public float GetStatBaseValue(StatType type)
        {
            if (_stats.TryGetValue(type, out Stat stat))
            {
                return stat.BaseValue;
            }
            return 0f;
        }

        /// <summary>
        /// Get the Stat object for direct manipulation
        /// </summary>
        public Stat GetStat(StatType type)
        {
            _stats.TryGetValue(type, out Stat stat);
            return stat;
        }

        /// <summary>
        /// Add a modifier to a specific stat
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            if (_stats.TryGetValue(modifier.StatType, out Stat stat))
            {
                float oldValue = stat.Value;
                stat.AddModifier(modifier);
                OnStatChanged?.Invoke(modifier.StatType, oldValue, stat.Value);
                
                // If modifying a primary stat, recalculate derived
                if (IsPrimaryStat(modifier.StatType))
                {
                    RecalculateDerivedStats();
                }
            }
        }

        /// <summary>
        /// Remove a specific modifier from a stat
        /// </summary>
        public void RemoveModifier(StatModifier modifier)
        {
            if (_stats.TryGetValue(modifier.StatType, out Stat stat))
            {
                float oldValue = stat.Value;
                stat.RemoveModifier(modifier);
                OnStatChanged?.Invoke(modifier.StatType, oldValue, stat.Value);
                
                if (IsPrimaryStat(modifier.StatType))
                {
                    RecalculateDerivedStats();
                }
            }
        }

        /// <summary>
        /// Remove all modifiers from a specific source (e.g., when unequipping an artifact)
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var stat in _stats.Values)
            {
                stat.RemoveAllModifiersFromSource(source);
            }
            RecalculateDerivedStats();
        }

        /// <summary>
        /// Allocate a stat point to a primary stat
        /// </summary>
        public bool AllocateStatPoint(StatType primaryStat)
        {
            if (!IsPrimaryStat(primaryStat))
            {
                Debug.LogWarning($"Cannot allocate points to derived stat: {primaryStat}");
                return false;
            }

            if (_availableStatPoints <= 0)
            {
                Debug.LogWarning("No stat points available!");
                return false;
            }

            if (_stats.TryGetValue(primaryStat, out Stat stat))
            {
                float oldValue = stat.Value;
                stat.BaseValue += 1;
                _availableStatPoints--;
                OnStatChanged?.Invoke(primaryStat, oldValue, stat.Value);
                RecalculateDerivedStats();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Level up the character
        /// </summary>
        public void LevelUp(int statPointsPerLevel = 1)
        {
            _level++;
            _availableStatPoints += statPointsPerLevel;
            OnLevelUp?.Invoke(_level);
        }

        /// <summary>
        /// Add stat points (from items, quests, etc.)
        /// </summary>
        public void AddStatPoints(int amount)
        {
            _availableStatPoints += amount;
        }

        /// <summary>
        /// Check if a stat type is a primary (levelable) stat
        /// </summary>
        public static bool IsPrimaryStat(StatType type)
        {
            return type switch
            {
                StatType.Vigor => true,
                StatType.Endurance => true,
                StatType.Strength => true,
                StatType.Dexterity => true,
                StatType.Intelligence => true,
                StatType.Faith => true,
                StatType.Arcane => true,
                _ => false
            };
        }

        /// <summary>
        /// Get all current stats as a dictionary (useful for UI)
        /// </summary>
        public Dictionary<StatType, float> GetAllStatValues()
        {
            var result = new Dictionary<StatType, float>();
            foreach (var kvp in _stats)
            {
                result[kvp.Key] = kvp.Value.Value;
            }
            return result;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug Print Stats")]
        private void DebugPrintStats()
        {
            Debug.Log($"=== Character Stats (Level {_level}) ===");
            Debug.Log($"Available Points: {_availableStatPoints}");
            Debug.Log("--- Primary Stats ---");
            foreach (var type in new[] { StatType.Vigor, StatType.Endurance, StatType.Strength, 
                StatType.Dexterity, StatType.Intelligence, StatType.Faith, StatType.Arcane })
            {
                Debug.Log($"{type}: {GetStatValue(type)}");
            }
            Debug.Log("--- Derived Stats ---");
            foreach (var kvp in _stats)
            {
                if (!IsPrimaryStat(kvp.Key))
                {
                    Debug.Log($"{kvp.Key}: {kvp.Value.Value}");
                }
            }
        }
#endif
    }
}
