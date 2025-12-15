using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.Artifacts
{
    /// <summary>
    /// Condition types for conditional artifacts
    /// </summary>
    public enum ArtifactConditionType
    {
        HealthBelow,        // HP below X%
        HealthAbove,        // HP above X%
        StaminaBelow,
        StaminaAbove,
        ManaBelow,
        ManaAbove,
        InCombat,
        OutOfCombat,
        EnemiesNearby,      // Number of enemies within range
        ConsecutiveHits,    // After hitting enemy X times
        RecentDodge,        // Recently dodged an attack
        RecentBlock,        // Recently blocked an attack
        LowEquipLoad,       // Below X% equip load
        FullHealth,
        HasStatusEffect     // Has a specific status effect
    }

    /// <summary>
    /// An artifact that provides bonuses only when certain conditions are met
    /// </summary>
    [CreateAssetMenu(fileName = "New Conditional Artifact", menuName = "RPG Framework/Artifacts/Conditional Artifact")]
    public class ConditionalArtifactData : ArtifactData
    {
        [Header("Condition")]
        public ArtifactConditionType ConditionType;
        
        [Tooltip("Threshold value (percentage for health/stamina/mana, count for enemies/hits)")]
        [Range(0f, 100f)]
        public float ConditionThreshold = 50f;
        
        [Tooltip("Duration the condition must be met (0 = instant)")]
        public float ConditionDuration = 0f;

        [Header("Conditional Bonuses")]
        [Tooltip("Additional stat bonuses when condition is met")]
        public List<SerializableStatModifier> ConditionalBonuses = new List<SerializableStatModifier>();

        // Runtime state
        private bool _conditionMet = false;
        private float _conditionTimer = 0f;
        private List<StatModifier> _conditionalModifiers = new List<StatModifier>();
        private Stats.StatContainer _cachedStats;

        public bool IsConditionMet => _conditionMet;

        public override void OnEquip(GameObject owner)
        {
            base.OnEquip(owner);
            _cachedStats = owner.GetComponent<Stats.StatContainer>();
            _conditionMet = false;
            _conditionTimer = 0f;
        }

        public override void OnUnequip(GameObject owner)
        {
            base.OnUnequip(owner);
            
            // Remove conditional modifiers if they were active
            if (_conditionMet && _cachedStats != null)
            {
                RemoveConditionalModifiers();
            }
            
            _cachedStats = null;
        }

        public override void OnUpdate(GameObject owner, float deltaTime)
        {
            base.OnUpdate(owner, deltaTime);

            if (_cachedStats == null) return;

            bool conditionNowMet = EvaluateCondition(owner);

            if (conditionNowMet && ConditionDuration > 0)
            {
                _conditionTimer += deltaTime;
                conditionNowMet = _conditionTimer >= ConditionDuration;
            }
            else if (!conditionNowMet)
            {
                _conditionTimer = 0f;
            }

            // State changed
            if (conditionNowMet != _conditionMet)
            {
                _conditionMet = conditionNowMet;
                
                if (_conditionMet)
                {
                    ApplyConditionalModifiers();
                }
                else
                {
                    RemoveConditionalModifiers();
                }
            }
        }

        private bool EvaluateCondition(GameObject owner)
        {
            // Note: Some conditions require additional components (health system, combat system)
            // This is a simplified example - expand based on your game's systems
            
            return ConditionType switch
            {
                ArtifactConditionType.HealthAbove => 
                    GetHealthPercent(owner) > ConditionThreshold,
                    
                ArtifactConditionType.HealthBelow => 
                    GetHealthPercent(owner) < ConditionThreshold,
                    
                ArtifactConditionType.FullHealth => 
                    GetHealthPercent(owner) >= 100f,
                    
                // Add more conditions as needed based on your game systems
                _ => false
            };
        }

        private float GetHealthPercent(GameObject owner)
        {
            // This would integrate with your health system
            // Example placeholder:
            float maxHealth = _cachedStats.GetStatValue(StatType.MaxHealth);
            // float currentHealth = owner.GetComponent<HealthSystem>()?.CurrentHealth ?? maxHealth;
            float currentHealth = maxHealth; // Placeholder
            return (currentHealth / maxHealth) * 100f;
        }

        private void ApplyConditionalModifiers()
        {
            if (_cachedStats == null) return;

            foreach (var bonus in ConditionalBonuses)
            {
                var modifier = bonus.ToStatModifier(this);
                _conditionalModifiers.Add(modifier);
                _cachedStats.AddModifier(modifier);
            }
        }

        private void RemoveConditionalModifiers()
        {
            if (_cachedStats == null) return;

            foreach (var modifier in _conditionalModifiers)
            {
                _cachedStats.RemoveModifier(modifier);
            }
            _conditionalModifiers.Clear();
        }

        public override string GetFullDescription()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(base.GetFullDescription());
            
            sb.AppendLine();
            sb.AppendLine($"<color=cyan>Condition: {GetConditionDescription()}</color>");
            
            if (ConditionalBonuses.Count > 0)
            {
                sb.AppendLine("When active:");
                foreach (var bonus in ConditionalBonuses)
                {
                    string sign = bonus.Value >= 0 ? "+" : "";
                    string format = bonus.ModifierType == ModifierType.Flat 
                        ? $"{sign}{bonus.Value}" 
                        : $"{sign}{bonus.Value * 100}%";
                    sb.AppendLine($"  {bonus.StatType}: {format}");
                }
            }

            return sb.ToString();
        }

        private string GetConditionDescription()
        {
            return ConditionType switch
            {
                ArtifactConditionType.HealthBelow => $"HP below {ConditionThreshold}%",
                ArtifactConditionType.HealthAbove => $"HP above {ConditionThreshold}%",
                ArtifactConditionType.FullHealth => "At full health",
                ArtifactConditionType.StaminaBelow => $"Stamina below {ConditionThreshold}%",
                ArtifactConditionType.StaminaAbove => $"Stamina above {ConditionThreshold}%",
                ArtifactConditionType.InCombat => "While in combat",
                ArtifactConditionType.OutOfCombat => "While out of combat",
                ArtifactConditionType.EnemiesNearby => $"{ConditionThreshold}+ enemies nearby",
                _ => ConditionType.ToString()
            };
        }
    }
}
