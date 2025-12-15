using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.SkillTree
{
    /// <summary>
    /// Categories/branches of skills
    /// </summary>
    public enum SkillBranch
    {
        Combat,
        Magic,
        Defense,
        Utility,
        Crafting
    }

    /// <summary>
    /// Defines an entire skill tree or branch
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill Tree", menuName = "RPG Framework/Skill Tree/Skill Tree Definition")]
    public class SkillTreeDefinition : ScriptableObject
    {
        [Header("Tree Info")]
        public string TreeName;
        public string TreeId;
        [TextArea(2, 4)]
        public string Description;
        public Sprite TreeIcon;
        public SkillBranch Branch;

        [Header("Skills")]
        [Tooltip("All skills in this tree")]
        public List<SkillNodeData> Skills = new List<SkillNodeData>();

        [Header("Tree Settings")]
        [Tooltip("Starting skills that are always available")]
        public List<SkillNodeData> RootSkills = new List<SkillNodeData>();

        /// <summary>
        /// Get all skills that can currently be unlocked
        /// </summary>
        public List<SkillNodeData> GetAvailableSkills(int availablePoints, int characterLevel,
            HashSet<string> unlockedSkills, Stats.StatContainer stats)
        {
            var available = new List<SkillNodeData>();
            
            foreach (var skill in Skills)
            {
                if (!unlockedSkills.Contains(skill.SkillId) && 
                    skill.CanUnlock(availablePoints, characterLevel, unlockedSkills, stats))
                {
                    available.Add(skill);
                }
            }

            return available;
        }

        /// <summary>
        /// Get a skill by its ID
        /// </summary>
        public SkillNodeData GetSkillById(string skillId)
        {
            return Skills.Find(s => s.SkillId == skillId);
        }

        /// <summary>
        /// Validate the tree (check for circular dependencies, etc.)
        /// </summary>
        public bool ValidateTree(out List<string> errors)
        {
            errors = new List<string>();
            var visited = new HashSet<string>();

            foreach (var skill in Skills)
            {
                if (skill == null)
                {
                    errors.Add("Null skill reference found in tree");
                    continue;
                }

                if (string.IsNullOrEmpty(skill.SkillId))
                {
                    errors.Add($"Skill '{skill.name}' has no SkillId");
                }

                // Check for circular dependencies
                if (HasCircularDependency(skill, new HashSet<string>()))
                {
                    errors.Add($"Circular dependency detected involving '{skill.SkillId}'");
                }
            }

            return errors.Count == 0;
        }

        private bool HasCircularDependency(SkillNodeData skill, HashSet<string> path)
        {
            if (path.Contains(skill.SkillId))
                return true;

            path.Add(skill.SkillId);

            foreach (var prereq in skill.Prerequisites)
            {
                if (prereq != null && HasCircularDependency(prereq, new HashSet<string>(path)))
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Tree")]
        private void ValidateInEditor()
        {
            if (ValidateTree(out var errors))
            {
                Debug.Log($"Skill tree '{TreeName}' is valid!");
            }
            else
            {
                foreach (var error in errors)
                {
                    Debug.LogError(error);
                }
            }
        }

        [ContextMenu("Auto-Assign Skill IDs")]
        private void AutoAssignIds()
        {
            foreach (var skill in Skills)
            {
                if (skill != null && string.IsNullOrEmpty(skill.SkillId))
                {
                    skill.SkillId = $"{TreeId}_{skill.name}".ToLower().Replace(" ", "_");
                    UnityEditor.EditorUtility.SetDirty(skill);
                }
            }
        }
#endif
    }
}
