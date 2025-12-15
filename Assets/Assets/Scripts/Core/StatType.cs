using System;

namespace RPGFramework.Core
{
    /// <summary>
    /// All available stat types in the game, similar to Souls-like games
    /// </summary>
    [Serializable]
    public enum StatType
    {
        // Primary Stats (leveled directly)
        Vigor,          // Increases max HP
        Endurance,      // Increases max Stamina
        Strength,       // Physical damage scaling, heavy weapons
        Dexterity,      // Physical damage scaling, fast weapons, crit chance
        Intelligence,   // Magic damage scaling, max mana
        Faith,          // Miracle/holy damage scaling, resistances
        Arcane,         // Status effect scaling, item discovery
        
        // Derived Stats (calculated from primary stats)
        MaxHealth,
        MaxStamina,
        MaxMana,
        PhysicalAttack,
        MagicAttack,
        PhysicalDefense,
        MagicDefense,
        CriticalChance,
        CriticalDamage,
        AttackSpeed,
        MovementSpeed,
        EquipLoad,
        Poise,
        StatusResistance,
        ItemDiscovery
    }
}
