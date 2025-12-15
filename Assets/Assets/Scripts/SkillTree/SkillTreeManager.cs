using System;
using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Stats;
using RPGFramework.Core;

namespace RPGFramework.SkillTree
{
    /// <summary>
    /// Runtime data for a single unlocked skill
    /// </summary>
    [Serializable]
    public class UnlockedSkill
    {
        public string SkillId;
        public int CurrentRank;
        public SkillNodeData Data;

        public UnlockedSkill(SkillNodeData data, int rank = 1)
        {
            SkillId = data.SkillId;
            CurrentRank = rank;
            Data = data;
        }
    }

    /// <summary>
    /// Manages skill unlocking, progression, and effects for a character
    /// </summary>
    public class SkillTreeManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatContainer _statContainer;
        [SerializeField] private List<SkillTreeDefinition> _skillTrees = new List<SkillTreeDefinition>();

        [Header("Skill Points")]
        [SerializeField] private int _availableSkillPoints = 0;
        [SerializeField] private int _skillPointsPerLevel = 1;

        // Runtime data
        private Dictionary<string, UnlockedSkill> _unlockedSkills = new Dictionary<string, UnlockedSkill>();
        private HashSet<string> _unlockedSkillIds = new HashSet<string>();
        private List<StatModifier> _activeModifiers = new List<StatModifier>();

        // Events
        public event Action<SkillNodeData, int> OnSkillUnlocked;
        public event Action<SkillNodeData, int> OnSkillRankUp;
        public event Action<int> OnSkillPointsChanged;

        // Properties
        public int AvailableSkillPoints => _availableSkillPoints;
        public IReadOnlyDictionary<string, UnlockedSkill> UnlockedSkills => _unlockedSkills;
        public IReadOnlyCollection<SkillTreeDefinition> SkillTrees => _skillTrees;

        private void Awake()
        {
            if (_statContainer == null)
            {
                _statContainer = GetComponent<StatContainer>();
            }

            // Subscribe to level up events
            if (_statContainer != null)
            {
                _statContainer.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDestroy()
        {
            if (_statContainer != null)
            {
                _statContainer.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            AddSkillPoints(_skillPointsPerLevel);
        }

        /// <summary>
        /// Add skill points
        /// </summary>
        public void AddSkillPoints(int amount)
        {
            _availableSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
        }

        /// <summary>
        /// Try to unlock a skill
        /// </summary>
        public bool TryUnlockSkill(SkillNodeData skill)
        {
            if (skill == null) return false;

            // Check if already at max rank
            if (_unlockedSkills.TryGetValue(skill.SkillId, out var existing))
            {
                if (existing.CurrentRank >= skill.MaxRanks)
                {
                    Debug.Log($"Skill '{skill.SkillName}' is already at max rank!");
                    return false;
                }
                // This is a rank up
                return TryRankUpSkill(skill);
            }

            // Check if can unlock
            if (!skill.CanUnlock(_availableSkillPoints, _statContainer.Level, 
                _unlockedSkillIds, _statContainer))
            {
                Debug.Log($"Cannot unlock skill '{skill.SkillName}' - requirements not met");
                return false;
            }

            // Unlock the skill
            _availableSkillPoints -= skill.PointCost;
            
            var unlocked = new UnlockedSkill(skill, 1);
            _unlockedSkills[skill.SkillId] = unlocked;
            _unlockedSkillIds.Add(skill.SkillId);

            // Apply stat modifiers
            ApplySkillModifiers(skill, 1);

            OnSkillUnlocked?.Invoke(skill, 1);
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);

            Debug.Log($"Unlocked skill: {skill.SkillName}");
            return true;
        }

        /// <summary>
        /// Try to rank up an existing skill
        /// </summary>
        public bool TryRankUpSkill(SkillNodeData skill)
        {
            if (!_unlockedSkills.TryGetValue(skill.SkillId, out var existing))
            {
                Debug.Log($"Skill '{skill.SkillName}' is not unlocked!");
                return false;
            }

            if (existing.CurrentRank >= skill.MaxRanks)
            {
                Debug.Log($"Skill '{skill.SkillName}' is already at max rank!");
                return false;
            }

            if (_availableSkillPoints < skill.PointCost)
            {
                Debug.Log($"Not enough skill points to rank up '{skill.SkillName}'!");
                return false;
            }

            // Remove old modifiers
            RemoveSkillModifiers(skill);

            // Rank up
            _availableSkillPoints -= skill.PointCost;
            existing.CurrentRank++;

            // Apply new modifiers
            ApplySkillModifiers(skill, existing.CurrentRank);

            OnSkillRankUp?.Invoke(skill, existing.CurrentRank);
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);

            Debug.Log($"Ranked up skill: {skill.SkillName} to rank {existing.CurrentRank}");
            return true;
        }

        private void ApplySkillModifiers(SkillNodeData skill, int rank)
        {
            var modifiers = skill.GetModifiersForRank(rank);
            foreach (var mod in modifiers)
            {
                _statContainer.AddModifier(mod);
                _activeModifiers.Add(mod);
            }
        }

        private void RemoveSkillModifiers(SkillNodeData skill)
        {
            _statContainer.RemoveAllModifiersFromSource(skill);
            _activeModifiers.RemoveAll(m => m.Source == skill);
        }

        /// <summary>
        /// Check if a skill is unlocked
        /// </summary>
        public bool IsSkillUnlocked(string skillId)
        {
            return _unlockedSkillIds.Contains(skillId);
        }

        /// <summary>
        /// Check if a skill is unlocked
        /// </summary>
        public bool IsSkillUnlocked(SkillNodeData skill)
        {
            return skill != null && _unlockedSkillIds.Contains(skill.SkillId);
        }

        /// <summary>
        /// Get current rank of a skill (0 if not unlocked)
        /// </summary>
        public int GetSkillRank(string skillId)
        {
            return _unlockedSkills.TryGetValue(skillId, out var skill) ? skill.CurrentRank : 0;
        }

        /// <summary>
        /// Get all skills available for unlocking
        /// </summary>
        public List<SkillNodeData> GetAllAvailableSkills()
        {
            var available = new List<SkillNodeData>();
            foreach (var tree in _skillTrees)
            {
                available.AddRange(tree.GetAvailableSkills(
                    _availableSkillPoints,
                    _statContainer.Level,
                    _unlockedSkillIds,
                    _statContainer
                ));
            }
            return available;
        }

        /// <summary>
        /// Reset all skills and refund points
        /// </summary>
        public void ResetAllSkills()
        {
            int refundedPoints = 0;
            
            foreach (var unlocked in _unlockedSkills.Values)
            {
                RemoveSkillModifiers(unlocked.Data);
                refundedPoints += unlocked.Data.PointCost * unlocked.CurrentRank;
            }

            _unlockedSkills.Clear();
            _unlockedSkillIds.Clear();
            _activeModifiers.Clear();
            _availableSkillPoints += refundedPoints;

            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            Debug.Log($"Reset all skills. Refunded {refundedPoints} skill points.");
        }

        /// <summary>
        /// Get save data for serialization
        /// </summary>
        public SkillTreeSaveData GetSaveData()
        {
            var data = new SkillTreeSaveData
            {
                AvailablePoints = _availableSkillPoints,
                UnlockedSkills = new List<SkillSaveData>()
            };

            foreach (var kvp in _unlockedSkills)
            {
                data.UnlockedSkills.Add(new SkillSaveData
                {
                    SkillId = kvp.Key,
                    Rank = kvp.Value.CurrentRank
                });
            }

            return data;
        }

        /// <summary>
        /// Load from save data
        /// </summary>
        public void LoadSaveData(SkillTreeSaveData data)
        {
            ResetAllSkills();
            _availableSkillPoints = data.AvailablePoints;

            foreach (var skillData in data.UnlockedSkills)
            {
                // Find the skill definition
                SkillNodeData skillDef = null;
                foreach (var tree in _skillTrees)
                {
                    skillDef = tree.GetSkillById(skillData.SkillId);
                    if (skillDef != null) break;
                }

                if (skillDef != null)
                {
                    var unlocked = new UnlockedSkill(skillDef, skillData.Rank);
                    _unlockedSkills[skillData.SkillId] = unlocked;
                    _unlockedSkillIds.Add(skillData.SkillId);
                    ApplySkillModifiers(skillDef, skillData.Rank);
                }
            }

            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug Print Unlocked Skills")]
        private void DebugPrintSkills()
        {
            Debug.Log($"=== Skill Tree Status ===");
            Debug.Log($"Available Points: {_availableSkillPoints}");
            Debug.Log($"Unlocked Skills: {_unlockedSkills.Count}");
            foreach (var kvp in _unlockedSkills)
            {
                Debug.Log($"  - {kvp.Value.Data.SkillName} (Rank {kvp.Value.CurrentRank}/{kvp.Value.Data.MaxRanks})");
            }
        }

        [ContextMenu("Add 5 Skill Points")]
        private void DebugAddPoints()
        {
            AddSkillPoints(5);
        }
#endif
    }

    /// <summary>
    /// Serializable save data for skill trees
    /// </summary>
    [Serializable]
    public class SkillTreeSaveData
    {
        public int AvailablePoints;
        public List<SkillSaveData> UnlockedSkills;
    }

    [Serializable]
    public class SkillSaveData
    {
        public string SkillId;
        public int Rank;
    }
}
