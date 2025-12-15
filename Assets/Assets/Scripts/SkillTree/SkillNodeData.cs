using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.SkillTree
{
    /// <summary>
    /// Types of skills in the tree
    /// </summary>
    public enum SkillType
    {
        Passive,        // Always active stat bonus
        Active,         // Unlocks an ability
        Mastery         // Enhances existing abilities
    }

    /// <summary>
    /// Represents a single node in the skill tree
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "RPG Framework/Skill Tree/Skill Node")]
    public class SkillNodeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string SkillId;
        public string SkillName;
        [TextArea(3, 5)]
        public string Description;
        public Sprite Icon;
        public SkillType Type = SkillType.Passive;

        [Header("Unlock Requirements")]
        [Tooltip("Skill points required to unlock")]
        public int PointCost = 1;
        [Tooltip("Character level required")]
        public int RequiredLevel = 1;
        [Tooltip("Skills that must be unlocked first")]
        public List<SkillNodeData> Prerequisites = new List<SkillNodeData>();

        [Header("Stat Requirements")]
        [Tooltip("Minimum stat values required to unlock this skill")]
        public List<StatRequirement> StatRequirements = new List<StatRequirement>();

        [Header("Passive Bonuses")]
        [Tooltip("Stat modifiers granted when this skill is unlocked")]
        public List<SerializableStatModifier> StatBonuses = new List<SerializableStatModifier>();

        [Header("Skill Ranks")]
        [Tooltip("Maximum times this skill can be upgraded")]
        public int MaxRanks = 1;
        [Tooltip("Multiplier for stat bonuses per rank (1 = same bonus each rank)")]
        public float BonusScalingPerRank = 1f;

        [Header("Active Skill Data (if Type = Active)")]
        [Tooltip("Cooldown in seconds")]
        public float Cooldown = 0f;
        [Tooltip("Resource cost (mana, stamina, etc.)")]
        public float ResourceCost = 0f;
        [Tooltip("Reference to the ability prefab or scriptable object")]
        public Object AbilityReference;

        [Header("Visual (for UI)")]
        public Vector2 TreePosition;
        public Color NodeColor = Color.white;

        /// <summary>
        /// Check if this skill can be unlocked given current state
        /// </summary>
        public bool CanUnlock(int availablePoints, int characterLevel, 
            HashSet<string> unlockedSkills, Stats.StatContainer stats)
        {
            // Check points
            if (availablePoints < PointCost)
                return false;

            // Check level
            if (characterLevel < RequiredLevel)
                return false;

            // Check prerequisites
            foreach (var prereq in Prerequisites)
            {
                if (prereq != null && !unlockedSkills.Contains(prereq.SkillId))
                    return false;
            }

            // Check stat requirements
            foreach (var req in StatRequirements)
            {
                if (stats.GetStatValue(req.StatType) < req.MinValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get stat modifiers for a specific rank
        /// </summary>
        public List<StatModifier> GetModifiersForRank(int rank)
        {
            var modifiers = new List<StatModifier>();
            float multiplier = 1f + (BonusScalingPerRank * (rank - 1));

            foreach (var bonus in StatBonuses)
            {
                modifiers.Add(new StatModifier(
                    bonus.StatType,
                    bonus.Value * multiplier,
                    bonus.ModifierType,
                    this
                ));
            }

            return modifiers;
        }
    }

    /// <summary>
    /// Represents a stat requirement for unlocking a skill
    /// </summary>
    [System.Serializable]
    public class StatRequirement
    {
        public StatType StatType;
        public float MinValue;
    }
}
