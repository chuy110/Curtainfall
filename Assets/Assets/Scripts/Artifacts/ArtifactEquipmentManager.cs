using System;
using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Stats;
using RPGFramework.Core;

namespace RPGFramework.Artifacts
{
    /// <summary>
    /// Represents an equipped artifact with runtime data
    /// </summary>
    [Serializable]
    public class EquippedArtifact
    {
        public ArtifactData Data;
        public ArtifactSlot EquippedSlot;
        public List<StatModifier> ActiveModifiers = new List<StatModifier>();

        public EquippedArtifact(ArtifactData data, ArtifactSlot slot)
        {
            Data = data;
            EquippedSlot = slot;
        }
    }

    /// <summary>
    /// Manages equipping and unequipping artifacts on a character
    /// </summary>
    public class ArtifactEquipmentManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatContainer _statContainer;

        [Header("Equipment Slots")]
        [SerializeField] private int _amuletSlots = 1;
        [SerializeField] private int _ringSlots = 2;
        [SerializeField] private int _beltSlots = 1;
        [SerializeField] private int _charmSlots = 2;
        [SerializeField] private int _relicSlots = 1;

        // Runtime data
        private Dictionary<ArtifactSlot, List<EquippedArtifact>> _equippedArtifacts = 
            new Dictionary<ArtifactSlot, List<EquippedArtifact>>();
        
        private Dictionary<ArtifactSetData, HashSet<SetArtifactData>> _equippedSetPieces = 
            new Dictionary<ArtifactSetData, HashSet<SetArtifactData>>();
        
        private Dictionary<ArtifactSetData, List<StatModifier>> _activeSetBonuses = 
            new Dictionary<ArtifactSetData, List<StatModifier>>();

        // Events
        public event Action<ArtifactData, ArtifactSlot> OnArtifactEquipped;
        public event Action<ArtifactData, ArtifactSlot> OnArtifactUnequipped;
        public event Action<ArtifactSetData, int> OnSetBonusChanged;

        // Properties
        public IReadOnlyDictionary<ArtifactSlot, List<EquippedArtifact>> EquippedArtifacts => _equippedArtifacts;

        private void Awake()
        {
            if (_statContainer == null)
            {
                _statContainer = GetComponent<StatContainer>();
            }

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            _equippedArtifacts[ArtifactSlot.Amulet] = new List<EquippedArtifact>();
            _equippedArtifacts[ArtifactSlot.Ring] = new List<EquippedArtifact>();
            _equippedArtifacts[ArtifactSlot.Belt] = new List<EquippedArtifact>();
            _equippedArtifacts[ArtifactSlot.Charm] = new List<EquippedArtifact>();
            _equippedArtifacts[ArtifactSlot.Relic] = new List<EquippedArtifact>();
        }

        private void Update()
        {
            // Update all conditional artifacts
            foreach (var slotList in _equippedArtifacts.Values)
            {
                foreach (var equipped in slotList)
                {
                    equipped.Data.OnUpdate(gameObject, Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Get the maximum number of artifacts for a slot
        /// </summary>
        public int GetMaxSlotsForType(ArtifactSlot slot)
        {
            return slot switch
            {
                ArtifactSlot.Amulet => _amuletSlots,
                ArtifactSlot.Ring => _ringSlots,
                ArtifactSlot.Belt => _beltSlots,
                ArtifactSlot.Charm => _charmSlots,
                ArtifactSlot.Relic => _relicSlots,
                _ => 1
            };
        }

        /// <summary>
        /// Get all artifacts currently equipped in a slot
        /// </summary>
        public List<EquippedArtifact> GetEquippedInSlot(ArtifactSlot slot)
        {
            return _equippedArtifacts.TryGetValue(slot, out var list) 
                ? new List<EquippedArtifact>(list) 
                : new List<EquippedArtifact>();
        }

        /// <summary>
        /// Check if an artifact can be equipped
        /// </summary>
        public bool CanEquip(ArtifactData artifact, ArtifactSlot? targetSlot = null)
        {
            if (artifact == null) return false;

            // Check requirements
            if (!artifact.CanEquip(_statContainer))
                return false;

            // Determine slot to use
            ArtifactSlot slot = targetSlot ?? artifact.Slot;
            if (artifact.Slot != ArtifactSlot.Any && targetSlot.HasValue && targetSlot != artifact.Slot)
                return false;

            // Check if slot has room
            if (slot == ArtifactSlot.Any)
            {
                // Find any available slot
                return HasAnyAvailableSlot();
            }
            else
            {
                return _equippedArtifacts[slot].Count < GetMaxSlotsForType(slot);
            }
        }

        private bool HasAnyAvailableSlot()
        {
            foreach (var slot in _equippedArtifacts.Keys)
            {
                if (_equippedArtifacts[slot].Count < GetMaxSlotsForType(slot))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Try to equip an artifact
        /// </summary>
        public bool TryEquip(ArtifactData artifact, ArtifactSlot? targetSlot = null)
        {
            if (!CanEquip(artifact, targetSlot))
            {
                Debug.Log($"Cannot equip {artifact.ArtifactName}");
                return false;
            }

            // Determine slot
            ArtifactSlot slot = targetSlot ?? artifact.Slot;
            if (slot == ArtifactSlot.Any)
            {
                slot = FindFirstAvailableSlot();
            }

            // Create equipped artifact
            var equipped = new EquippedArtifact(artifact, slot);
            
            // Apply stat modifiers
            var modifiers = artifact.GetStatModifiers();
            foreach (var mod in modifiers)
            {
                _statContainer.AddModifier(mod);
                equipped.ActiveModifiers.Add(mod);
            }

            // Add to slot
            _equippedArtifacts[slot].Add(equipped);

            // Trigger OnEquip
            artifact.OnEquip(gameObject);

            OnArtifactEquipped?.Invoke(artifact, slot);
            Debug.Log($"Equipped {artifact.ArtifactName} in {slot} slot");

            return true;
        }

        /// <summary>
        /// Unequip an artifact by reference
        /// </summary>
        public bool TryUnequip(ArtifactData artifact)
        {
            foreach (var slot in _equippedArtifacts.Keys)
            {
                var equipped = _equippedArtifacts[slot].Find(e => e.Data == artifact);
                if (equipped != null)
                {
                    return TryUnequipFromSlot(slot, _equippedArtifacts[slot].IndexOf(equipped));
                }
            }
            return false;
        }

        /// <summary>
        /// Unequip artifact from a specific slot index
        /// </summary>
        public bool TryUnequipFromSlot(ArtifactSlot slot, int index)
        {
            if (!_equippedArtifacts.TryGetValue(slot, out var list) || 
                index < 0 || index >= list.Count)
            {
                return false;
            }

            var equipped = list[index];
            
            // Remove stat modifiers
            foreach (var mod in equipped.ActiveModifiers)
            {
                _statContainer.RemoveModifier(mod);
            }

            // Trigger OnUnequip
            equipped.Data.OnUnequip(gameObject);

            // Remove from slot
            list.RemoveAt(index);

            OnArtifactUnequipped?.Invoke(equipped.Data, slot);
            Debug.Log($"Unequipped {equipped.Data.ArtifactName} from {slot} slot");

            return true;
        }

        /// <summary>
        /// Unequip all artifacts
        /// </summary>
        public void UnequipAll()
        {
            foreach (var slot in _equippedArtifacts.Keys)
            {
                while (_equippedArtifacts[slot].Count > 0)
                {
                    TryUnequipFromSlot(slot, 0);
                }
            }
        }

        private ArtifactSlot FindFirstAvailableSlot()
        {
            foreach (var slot in _equippedArtifacts.Keys)
            {
                if (_equippedArtifacts[slot].Count < GetMaxSlotsForType(slot))
                    return slot;
            }
            return ArtifactSlot.Charm; // Fallback
        }

        /// <summary>
        /// Called by SetArtifactData when a set piece is equipped
        /// </summary>
        public void NotifySetPieceEquipped(ArtifactSetData setData, SetArtifactData piece)
        {
            if (!_equippedSetPieces.ContainsKey(setData))
            {
                _equippedSetPieces[setData] = new HashSet<SetArtifactData>();
                _activeSetBonuses[setData] = new List<StatModifier>();
            }

            _equippedSetPieces[setData].Add(piece);
            UpdateSetBonuses(setData);
        }

        /// <summary>
        /// Called by SetArtifactData when a set piece is unequipped
        /// </summary>
        public void NotifySetPieceUnequipped(ArtifactSetData setData, SetArtifactData piece)
        {
            if (_equippedSetPieces.TryGetValue(setData, out var pieces))
            {
                pieces.Remove(piece);
                UpdateSetBonuses(setData);
                
                if (pieces.Count == 0)
                {
                    _equippedSetPieces.Remove(setData);
                    _activeSetBonuses.Remove(setData);
                }
            }
        }

        private void UpdateSetBonuses(ArtifactSetData setData)
        {
            // Remove old bonuses
            if (_activeSetBonuses.TryGetValue(setData, out var oldModifiers))
            {
                foreach (var mod in oldModifiers)
                {
                    _statContainer.RemoveModifier(mod);
                }
                oldModifiers.Clear();
            }

            // Apply new bonuses based on piece count
            int pieceCount = _equippedSetPieces[setData].Count;
            var newModifiers = setData.GetBonusesForPieceCount(pieceCount);
            
            foreach (var mod in newModifiers)
            {
                _statContainer.AddModifier(mod);
                _activeSetBonuses[setData].Add(mod);
            }

            OnSetBonusChanged?.Invoke(setData, pieceCount);
        }

        /// <summary>
        /// Get the number of equipped pieces for a set
        /// </summary>
        public int GetEquippedSetPieceCount(ArtifactSetData setData)
        {
            return _equippedSetPieces.TryGetValue(setData, out var pieces) ? pieces.Count : 0;
        }

        /// <summary>
        /// Get all equipped artifacts as a flat list
        /// </summary>
        public List<EquippedArtifact> GetAllEquippedArtifacts()
        {
            var all = new List<EquippedArtifact>();
            foreach (var list in _equippedArtifacts.Values)
            {
                all.AddRange(list);
            }
            return all;
        }

        /// <summary>
        /// Check if a specific artifact is equipped
        /// </summary>
        public bool IsEquipped(ArtifactData artifact)
        {
            foreach (var list in _equippedArtifacts.Values)
            {
                if (list.Exists(e => e.Data == artifact))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get save data for serialization
        /// </summary>
        public ArtifactSaveData GetSaveData()
        {
            var data = new ArtifactSaveData
            {
                EquippedArtifacts = new List<ArtifactSlotSaveData>()
            };

            foreach (var kvp in _equippedArtifacts)
            {
                foreach (var equipped in kvp.Value)
                {
                    data.EquippedArtifacts.Add(new ArtifactSlotSaveData
                    {
                        ArtifactId = equipped.Data.ArtifactId,
                        Slot = kvp.Key
                    });
                }
            }

            return data;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug Print Equipment")]
        private void DebugPrintEquipment()
        {
            Debug.Log("=== Equipped Artifacts ===");
            foreach (var kvp in _equippedArtifacts)
            {
                Debug.Log($"{kvp.Key} ({kvp.Value.Count}/{GetMaxSlotsForType(kvp.Key)}):");
                foreach (var equipped in kvp.Value)
                {
                    Debug.Log($"  - {equipped.Data.ArtifactName} ({equipped.Data.Rarity})");
                }
            }
        }
#endif
    }

    /// <summary>
    /// Serializable save data for artifacts
    /// </summary>
    [Serializable]
    public class ArtifactSaveData
    {
        public List<ArtifactSlotSaveData> EquippedArtifacts;
    }

    [Serializable]
    public class ArtifactSlotSaveData
    {
        public string ArtifactId;
        public ArtifactSlot Slot;
    }
}
