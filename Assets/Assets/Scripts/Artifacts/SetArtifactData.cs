using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Core;

namespace RPGFramework.Artifacts
{
    /// <summary>
    /// Artifact that belongs to a set and grants additional bonuses when multiple set pieces are equipped
    /// </summary>
    [CreateAssetMenu(fileName = "New Set Artifact", menuName = "RPG Framework/Artifacts/Set Artifact")]
    public class SetArtifactData : ArtifactData
    {
        [Header("Set Information")]
        public ArtifactSetData SetData;
        
        public override void OnEquip(GameObject owner)
        {
            base.OnEquip(owner);
            
            // Notify the set manager about equip
            var equipmentManager = owner.GetComponent<ArtifactEquipmentManager>();
            if (equipmentManager != null && SetData != null)
            {
                equipmentManager.NotifySetPieceEquipped(SetData, this);
            }
        }

        public override void OnUnequip(GameObject owner)
        {
            base.OnUnequip(owner);
            
            // Notify the set manager about unequip
            var equipmentManager = owner.GetComponent<ArtifactEquipmentManager>();
            if (equipmentManager != null && SetData != null)
            {
                equipmentManager.NotifySetPieceUnequipped(SetData, this);
            }
        }

        public override string GetFullDescription()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(base.GetFullDescription());
            
            if (SetData != null)
            {
                sb.AppendLine();
                sb.AppendLine($"<color=yellow>Set: {SetData.SetName}</color>");
                foreach (var bonus in SetData.SetBonuses)
                {
                    sb.AppendLine($"  ({bonus.PiecesRequired}) {bonus.Description}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Defines a set of artifacts that grant bonuses when equipped together
    /// </summary>
    [CreateAssetMenu(fileName = "New Artifact Set", menuName = "RPG Framework/Artifacts/Artifact Set")]
    public class ArtifactSetData : ScriptableObject
    {
        [Header("Set Info")]
        public string SetId;
        public string SetName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Set Pieces")]
        public List<SetArtifactData> SetPieces = new List<SetArtifactData>();

        [Header("Set Bonuses")]
        public List<SetBonus> SetBonuses = new List<SetBonus>();

        /// <summary>
        /// Get bonuses for a given number of equipped pieces
        /// </summary>
        public List<StatModifier> GetBonusesForPieceCount(int pieceCount)
        {
            var modifiers = new List<StatModifier>();
            
            foreach (var bonus in SetBonuses)
            {
                if (pieceCount >= bonus.PiecesRequired)
                {
                    foreach (var stat in bonus.StatBonuses)
                    {
                        modifiers.Add(stat.ToStatModifier(this));
                    }
                }
            }

            return modifiers;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(SetId))
            {
                SetId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }

    /// <summary>
    /// Defines a bonus granted by having a certain number of set pieces equipped
    /// </summary>
    [System.Serializable]
    public class SetBonus
    {
        [Tooltip("How many pieces needed for this bonus")]
        public int PiecesRequired = 2;
        [TextArea(1, 2)]
        public string Description;
        public List<SerializableStatModifier> StatBonuses = new List<SerializableStatModifier>();
    }
}
