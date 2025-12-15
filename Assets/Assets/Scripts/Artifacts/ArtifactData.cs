using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.Artifacts
{
    /// <summary>
    /// Rarity levels for artifacts
    /// </summary>
    public enum ArtifactRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    /// <summary>
    /// Slot types for equipping artifacts
    /// </summary>
    public enum ArtifactSlot
    {
        Amulet,
        Ring,
        Belt,
        Charm,
        Relic,
        Any // Can be equipped in any slot
    }

    /// <summary>
    /// Base class for all equipable artifacts
    /// Use CreateAssetMenu to create new artifact types that inherit from this
    /// </summary>
    [CreateAssetMenu(fileName = "New Artifact", menuName = "RPG Framework/Artifacts/Basic Artifact")]
    public class ArtifactData : ScriptableObject
    {
        [Header("Basic Info")]
        public string ArtifactId;
        public string ArtifactName;
        [TextArea(3, 6)]
        public string Description;
        public Sprite Icon;
        
        [Header("Classification")]
        public ArtifactRarity Rarity = ArtifactRarity.Common;
        public ArtifactSlot Slot = ArtifactSlot.Any;
        
        [Header("Requirements")]
        [Tooltip("Minimum character level to equip")]
        public int RequiredLevel = 1;
        [Tooltip("Stat requirements to equip")]
        public List<SkillTree.StatRequirement> StatRequirements = new List<SkillTree.StatRequirement>();

        [Header("Stat Bonuses")]
        [Tooltip("Stat modifiers applied when equipped")]
        public List<SerializableStatModifier> StatBonuses = new List<SerializableStatModifier>();

        [Header("Special Effects")]
        [Tooltip("Special passive effects (implement in derived classes)")]
        [TextArea(2, 4)]
        public string SpecialEffectDescription;

        [Header("Visual")]
        public Color RarityColor = Color.white;

        /// <summary>
        /// Check if this artifact can be equipped by the given character
        /// </summary>
        public virtual bool CanEquip(Stats.StatContainer stats)
        {
            // Check level
            if (stats.Level < RequiredLevel)
                return false;

            // Check stat requirements
            foreach (var req in StatRequirements)
            {
                if (stats.GetStatValue(req.StatType) < req.MinValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get all stat modifiers for this artifact
        /// </summary>
        public virtual List<StatModifier> GetStatModifiers()
        {
            var modifiers = new List<StatModifier>();
            foreach (var bonus in StatBonuses)
            {
                modifiers.Add(bonus.ToStatModifier(this));
            }
            return modifiers;
        }

        /// <summary>
        /// Called when the artifact is equipped
        /// Override in derived classes for special effects
        /// </summary>
        public virtual void OnEquip(GameObject owner)
        {
            // Base implementation does nothing
            // Override for special equip effects
        }

        /// <summary>
        /// Called when the artifact is unequipped
        /// Override in derived classes for cleanup
        /// </summary>
        public virtual void OnUnequip(GameObject owner)
        {
            // Base implementation does nothing
            // Override for special unequip effects
        }

        /// <summary>
        /// Called every frame while equipped (if needed)
        /// </summary>
        public virtual void OnUpdate(GameObject owner, float deltaTime)
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Get formatted description including stats
        /// </summary>
        public virtual string GetFullDescription()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Description);
            sb.AppendLine();
            
            if (StatBonuses.Count > 0)
            {
                sb.AppendLine("<b>Stats:</b>");
                foreach (var bonus in StatBonuses)
                {
                    string sign = bonus.Value >= 0 ? "+" : "";
                    string format = bonus.ModifierType == ModifierType.Flat 
                        ? $"{sign}{bonus.Value}" 
                        : $"{sign}{bonus.Value * 100}%";
                    sb.AppendLine($"  {bonus.StatType}: {format}");
                }
            }

            if (!string.IsNullOrEmpty(SpecialEffectDescription))
            {
                sb.AppendLine();
                sb.AppendLine($"<i>{SpecialEffectDescription}</i>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the display color based on rarity
        /// </summary>
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                ArtifactRarity.Common => new Color(0.8f, 0.8f, 0.8f),
                ArtifactRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ArtifactRarity.Rare => new Color(0.2f, 0.4f, 1f),
                ArtifactRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                ArtifactRarity.Legendary => new Color(1f, 0.6f, 0f),
                ArtifactRarity.Mythic => new Color(1f, 0.2f, 0.2f),
                _ => Color.white
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ArtifactId))
            {
                ArtifactId = name.ToLower().Replace(" ", "_");
            }
            RarityColor = GetRarityColor();
        }
#endif
    }
}
