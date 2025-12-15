using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.Stats
{
    /// <summary>
    /// Defines how derived stats scale from primary stats
    /// Configure this to balance your game's stat scaling
    /// </summary>
    [CreateAssetMenu(fileName = "StatGrowthConfig", menuName = "RPG Framework/Stats/Stat Growth Config")]
    public class StatGrowthConfig : ScriptableObject
    {
        [Header("Health & Resources")]
        [Tooltip("Base HP before any Vigor")]
        public float BaseHealth = 100f;
        [Tooltip("HP gained per point of Vigor")]
        public float HealthPerVigor = 25f;
        
        [Tooltip("Base Stamina before any Endurance")]
        public float BaseStamina = 80f;
        [Tooltip("Stamina gained per point of Endurance")]
        public float StaminaPerEndurance = 10f;
        
        [Tooltip("Base Mana before any Intelligence")]
        public float BaseMana = 50f;
        [Tooltip("Mana gained per point of Intelligence")]
        public float ManaPerIntelligence = 8f;

        [Header("Attack Scaling")]
        [Tooltip("Base physical attack")]
        public float BasePhysicalAttack = 10f;
        [Tooltip("Physical attack per point of Strength")]
        public float PhysicalAttackPerStrength = 2f;
        [Tooltip("Physical attack per point of Dexterity")]
        public float PhysicalAttackPerDexterity = 1f;
        
        [Tooltip("Base magic attack")]
        public float BaseMagicAttack = 10f;
        [Tooltip("Magic attack per point of Intelligence")]
        public float MagicAttackPerIntelligence = 2.5f;
        [Tooltip("Magic attack per point of Faith")]
        public float MagicAttackPerFaith = 1.5f;

        [Header("Defense Scaling")]
        [Tooltip("Base physical defense")]
        public float BasePhysicalDefense = 5f;
        [Tooltip("Physical defense per point of Vigor")]
        public float PhysicalDefensePerVigor = 0.5f;
        [Tooltip("Physical defense per point of Endurance")]
        public float PhysicalDefensePerEndurance = 0.5f;
        
        [Tooltip("Base magic defense")]
        public float BaseMagicDefense = 5f;
        [Tooltip("Magic defense per point of Intelligence")]
        public float MagicDefensePerIntelligence = 0.5f;
        [Tooltip("Magic defense per point of Faith")]
        public float MagicDefensePerFaith = 1f;

        [Header("Critical Stats")]
        [Tooltip("Base critical chance (0-1)")]
        [Range(0f, 1f)]
        public float BaseCriticalChance = 0.05f;
        [Tooltip("Critical chance per point of Dexterity")]
        public float CritChancePerDexterity = 0.005f;
        [Tooltip("Max critical chance cap")]
        [Range(0f, 1f)]
        public float MaxCriticalChance = 0.75f;
        
        [Tooltip("Base critical damage multiplier")]
        public float BaseCriticalDamage = 1.5f;
        [Tooltip("Critical damage per point of Dexterity")]
        public float CritDamagePerDexterity = 0.02f;

        [Header("Other Stats")]
        [Tooltip("Base attack speed multiplier")]
        public float BaseAttackSpeed = 1f;
        [Tooltip("Attack speed per point of Dexterity")]
        public float AttackSpeedPerDexterity = 0.01f;
        
        [Tooltip("Base movement speed")]
        public float BaseMovementSpeed = 5f;
        [Tooltip("Movement speed per point of Dexterity")]
        public float MovementSpeedPerDexterity = 0.05f;
        
        [Tooltip("Base equip load")]
        public float BaseEquipLoad = 50f;
        [Tooltip("Equip load per point of Endurance")]
        public float EquipLoadPerEndurance = 2f;
        
        [Tooltip("Base poise")]
        public float BasePoise = 10f;
        [Tooltip("Poise per point of Endurance")]
        public float PoisePerEndurance = 1f;
        
        [Tooltip("Base status resistance")]
        public float BaseStatusResistance = 10f;
        [Tooltip("Status resistance per point of Arcane")]
        public float StatusResistancePerArcane = 2f;
        
        [Tooltip("Base item discovery")]
        public float BaseItemDiscovery = 100f;
        [Tooltip("Item discovery per point of Arcane")]
        public float ItemDiscoveryPerArcane = 5f;

        /// <summary>
        /// Calculate a derived stat value based on primary stats
        /// </summary>
        public float CalculateDerivedStat(StatType derivedStat, StatContainer stats)
        {
            return derivedStat switch
            {
                StatType.MaxHealth => BaseHealth + (HealthPerVigor * stats.GetStatValue(StatType.Vigor)),
                
                StatType.MaxStamina => BaseStamina + (StaminaPerEndurance * stats.GetStatValue(StatType.Endurance)),
                
                StatType.MaxMana => BaseMana + (ManaPerIntelligence * stats.GetStatValue(StatType.Intelligence)),
                
                StatType.PhysicalAttack => BasePhysicalAttack 
                    + (PhysicalAttackPerStrength * stats.GetStatValue(StatType.Strength))
                    + (PhysicalAttackPerDexterity * stats.GetStatValue(StatType.Dexterity)),
                
                StatType.MagicAttack => BaseMagicAttack 
                    + (MagicAttackPerIntelligence * stats.GetStatValue(StatType.Intelligence))
                    + (MagicAttackPerFaith * stats.GetStatValue(StatType.Faith)),
                
                StatType.PhysicalDefense => BasePhysicalDefense 
                    + (PhysicalDefensePerVigor * stats.GetStatValue(StatType.Vigor))
                    + (PhysicalDefensePerEndurance * stats.GetStatValue(StatType.Endurance)),
                
                StatType.MagicDefense => BaseMagicDefense 
                    + (MagicDefensePerIntelligence * stats.GetStatValue(StatType.Intelligence))
                    + (MagicDefensePerFaith * stats.GetStatValue(StatType.Faith)),
                
                StatType.CriticalChance => Mathf.Min(
                    BaseCriticalChance + (CritChancePerDexterity * stats.GetStatValue(StatType.Dexterity)),
                    MaxCriticalChance),
                
                StatType.CriticalDamage => BaseCriticalDamage 
                    + (CritDamagePerDexterity * stats.GetStatValue(StatType.Dexterity)),
                
                StatType.AttackSpeed => BaseAttackSpeed 
                    + (AttackSpeedPerDexterity * stats.GetStatValue(StatType.Dexterity)),
                
                StatType.MovementSpeed => BaseMovementSpeed 
                    + (MovementSpeedPerDexterity * stats.GetStatValue(StatType.Dexterity)),
                
                StatType.EquipLoad => BaseEquipLoad 
                    + (EquipLoadPerEndurance * stats.GetStatValue(StatType.Endurance)),
                
                StatType.Poise => BasePoise 
                    + (PoisePerEndurance * stats.GetStatValue(StatType.Endurance)),
                
                StatType.StatusResistance => BaseStatusResistance 
                    + (StatusResistancePerArcane * stats.GetStatValue(StatType.Arcane)),
                
                StatType.ItemDiscovery => BaseItemDiscovery 
                    + (ItemDiscoveryPerArcane * stats.GetStatValue(StatType.Arcane)),
                
                _ => 0f
            };
        }
    }
}
