# RPG Framework - Stats, Skills & Artifacts

A modular, souls-like RPG framework for Unity featuring:
- **Stat System** with primary stats (Vigor, Strength, etc.) and derived stats (Max HP, Attack, etc.)
- **Skill Tree** with prerequisites, ranks, and passive bonuses
- **Artifact Equipment** with set bonuses and conditional effects

## Quick Start

### 1. Setup the Player

1. Create a new GameObject for your player
2. Add these components:
   - `StatContainer`
   - `SkillTreeManager`
   - `ArtifactEquipmentManager`
   - `PlayerCharacter` (optional, example integration)

### 2. Create Stat Growth Config

Right-click in Project window → **Create → RPG Framework → Stats → Stat Growth Config**

This defines how derived stats scale from primary stats. Adjust values to balance your game.

Assign this to the `StatContainer` component.

### 3. Create Skills

Right-click → **Create → RPG Framework → Skill Tree → Skill Node**

Configure:
- `Skill Id` - Unique identifier
- `Skill Name` - Display name
- `Description` - What it does
- `Point Cost` - Skill points to unlock
- `Required Level` - Minimum character level
- `Prerequisites` - Skills that must be unlocked first
- `Stat Bonuses` - What stats it modifies

### 4. Create Skill Tree

Right-click → **Create → RPG Framework → Skill Tree → Skill Tree Definition**

Add all your skills to the `Skills` list and set `Root Skills` for starting points.

Assign skill trees to the `SkillTreeManager` component.

### 5. Create Artifacts

**Basic Artifact:**
Right-click → **Create → RPG Framework → Artifacts → Basic Artifact**

**Set Artifact (with set bonuses):**
1. Create set pieces: **Create → RPG Framework → Artifacts → Set Artifact**
2. Create the set definition: **Create → RPG Framework → Artifacts → Artifact Set**
3. Link pieces to the set

**Conditional Artifact:**
Right-click → **Create → RPG Framework → Artifacts → Conditional Artifact**

Provides bonuses only when conditions are met (low HP, in combat, etc.)

### 6. Create Artifact Database (Optional)

Right-click → **Create → RPG Framework → Artifacts → Artifact Database**

Use this to look up artifacts by ID. Right-click the asset → **Refresh Database** to auto-populate.

## Stat System

### Primary Stats (Levelable)
| Stat | Effect |
|------|--------|
| Vigor | Max HP, Physical Defense |
| Endurance | Max Stamina, Equip Load, Poise |
| Strength | Physical Attack (primary) |
| Dexterity | Physical Attack, Crit Chance, Attack Speed |
| Intelligence | Magic Attack, Max Mana |
| Faith | Magic Attack, Magic Defense |
| Arcane | Status Resistance, Item Discovery |

### Modifier Types
- **Flat** - Adds a fixed value
- **PercentAdd** - Adds percentage (stacks additively)
- **PercentMult** - Multiplies final value (stacks multiplicatively)

Example: Base 100, +20 Flat, +10% PercentAdd, +25% PercentMult
= (100 + 20) × 1.10 × 1.25 = 165

## Code Examples

### Allocating Stat Points
```csharp
var stats = GetComponent<StatContainer>();
stats.AllocateStatPoint(StatType.Strength);
```

### Unlocking Skills
```csharp
var skillTree = GetComponent<SkillTreeManager>();
skillTree.TryUnlockSkill(mySkillData);
```

### Equipping Artifacts
```csharp
var equipment = GetComponent<ArtifactEquipmentManager>();
equipment.TryEquip(myArtifact, ArtifactSlot.Ring);
```

### Adding Temporary Buff
```csharp
var stats = GetComponent<StatContainer>();
var buff = new StatModifier(StatType.Strength, 10f, ModifierType.Flat, this);
stats.AddModifier(buff);

// Later, remove it
stats.RemoveModifier(buff);
// Or remove all from source
stats.RemoveAllModifiersFromSource(this);
```

### Subscribing to Events
```csharp
void Start()
{
    var stats = GetComponent<StatContainer>();
    stats.OnStatChanged += (statType, oldVal, newVal) => {
        Debug.Log($"{statType} changed from {oldVal} to {newVal}");
    };
    stats.OnLevelUp += level => Debug.Log($"Now level {level}!");
}
```

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── StatType.cs           # Enum of all stats
│   │   └── StatModifier.cs       # Modifier classes
│   ├── Stats/
│   │   ├── Stat.cs               # Individual stat with modifiers
│   │   ├── StatContainer.cs      # Main component
│   │   └── StatGrowthConfig.cs   # ScriptableObject config
│   ├── SkillTree/
│   │   ├── SkillNodeData.cs      # ScriptableObject skill
│   │   ├── SkillTreeDefinition.cs # ScriptableObject tree
│   │   └── SkillTreeManager.cs   # Main component
│   ├── Artifacts/
│   │   ├── ArtifactData.cs       # Base artifact SO
│   │   ├── SetArtifactData.cs    # Set bonus artifacts
│   │   ├── ConditionalArtifactData.cs # Conditional artifacts
│   │   ├── ArtifactDatabase.cs   # Lookup database
│   │   └── ArtifactEquipmentManager.cs # Main component
│   └── Example/
│       └── PlayerCharacter.cs    # Integration example
└── ScriptableObjects/
    ├── Stats/                    # StatGrowthConfig assets
    ├── Skills/                   # Skill assets
    └── Artifacts/                # Artifact assets
```

## Extending the System

### Custom Artifact Effects

Create a new class inheriting from `ArtifactData`:

```csharp
[CreateAssetMenu(fileName = "Vampiric Artifact", menuName = "RPG Framework/Artifacts/Vampiric")]
public class VampiricArtifact : ArtifactData
{
    public float LifeStealPercent = 0.1f;
    
    public override void OnEquip(GameObject owner)
    {
        base.OnEquip(owner);
        // Subscribe to damage events
    }
    
    public override void OnUnequip(GameObject owner)
    {
        base.OnUnequip(owner);
        // Unsubscribe from events
    }
}
```

### Adding New Stats

1. Add to `StatType` enum in `Core/StatType.cs`
2. Add calculation in `StatGrowthConfig.CalculateDerivedStat()`
3. Initialize in `StatContainer.Initialize()`

## Save/Load

All managers provide `GetSaveData()` methods returning serializable data:

```csharp
// Save
var playerData = playerCharacter.GetSaveData();
var json = JsonUtility.ToJson(playerData);

// Load
var loadedData = JsonUtility.FromJson<CharacterSaveData>(json);
skillTreeManager.LoadSaveData(loadedData.SkillTreeData);
```

## Tips

- Use `[ContextMenu]` debug methods in Play mode to test
- Set breakpoints in `RecalculateDerivedStats()` to debug stat issues
- Use Artifact Database's "Refresh Database" to auto-find all artifacts
- Validate skill trees with the built-in validation tools
