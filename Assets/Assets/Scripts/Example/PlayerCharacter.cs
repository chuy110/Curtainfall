using UnityEngine;
using RPGFramework.Stats;
using RPGFramework.SkillTree;
using RPGFramework.Artifacts;
using RPGFramework.Core;

namespace RPGFramework.Example
{
    /// <summary>
    /// Example character controller that demonstrates how to use all the systems together
    /// Attach this to your player character along with the other managers
    /// </summary>
    [RequireComponent(typeof(StatContainer))]
    [RequireComponent(typeof(SkillTreeManager))]
    [RequireComponent(typeof(ArtifactEquipmentManager))]
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatContainer _stats;
        [SerializeField] private SkillTreeManager _skillTree;
        [SerializeField] private ArtifactEquipmentManager _artifacts;

        [Header("Current Resources")]
        [SerializeField] private float _currentHealth;
        [SerializeField] private float _currentStamina;
        [SerializeField] private float _currentMana;

        [Header("Resource Regeneration")]
        [SerializeField] private float _healthRegenRate = 0f;
        [SerializeField] private float _staminaRegenRate = 5f;
        [SerializeField] private float _manaRegenRate = 2f;

        // Properties for easy access
        public float CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }

        public float CurrentStamina
        {
            get => _currentStamina;
            set => _currentStamina = Mathf.Clamp(value, 0, MaxStamina);
        }

        public float CurrentMana
        {
            get => _currentMana;
            set => _currentMana = Mathf.Clamp(value, 0, MaxMana);
        }

        public float MaxHealth => _stats.GetStatValue(StatType.MaxHealth);
        public float MaxStamina => _stats.GetStatValue(StatType.MaxStamina);
        public float MaxMana => _stats.GetStatValue(StatType.MaxMana);
        public float PhysicalAttack => _stats.GetStatValue(StatType.PhysicalAttack);
        public float MagicAttack => _stats.GetStatValue(StatType.MagicAttack);
        public float PhysicalDefense => _stats.GetStatValue(StatType.PhysicalDefense);
        public float MagicDefense => _stats.GetStatValue(StatType.MagicDefense);

        public StatContainer Stats => _stats;
        public SkillTreeManager SkillTree => _skillTree;
        public ArtifactEquipmentManager Artifacts => _artifacts;

        private void Awake()
        {
            // Get references if not set
            if (_stats == null) _stats = GetComponent<StatContainer>();
            if (_skillTree == null) _skillTree = GetComponent<SkillTreeManager>();
            if (_artifacts == null) _artifacts = GetComponent<ArtifactEquipmentManager>();
        }

        private void Start()
        {
            // Initialize resources to max
            _currentHealth = MaxHealth;
            _currentStamina = MaxStamina;
            _currentMana = MaxMana;

            // Subscribe to stat changes to adjust current values
            _stats.OnStatChanged += HandleStatChanged;
            _stats.OnLevelUp += HandleLevelUp;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnStatChanged -= HandleStatChanged;
                _stats.OnLevelUp -= HandleLevelUp;
            }
        }

        private void Update()
        {
            // Regenerate resources
            RegenerateResources();
        }

        private void RegenerateResources()
        {
            if (_currentHealth < MaxHealth && _currentHealth > 0)
            {
                _currentHealth = Mathf.Min(_currentHealth + _healthRegenRate * Time.deltaTime, MaxHealth);
            }

            if (_currentStamina < MaxStamina)
            {
                _currentStamina = Mathf.Min(_currentStamina + _staminaRegenRate * Time.deltaTime, MaxStamina);
            }

            if (_currentMana < MaxMana)
            {
                _currentMana = Mathf.Min(_currentMana + _manaRegenRate * Time.deltaTime, MaxMana);
            }
        }

        private void HandleStatChanged(StatType statType, float oldValue, float newValue)
        {
            // When max resource stats increase, increase current by the same amount
            switch (statType)
            {
                case StatType.MaxHealth:
                    float healthDiff = newValue - oldValue;
                    if (healthDiff > 0) _currentHealth += healthDiff;
                    _currentHealth = Mathf.Min(_currentHealth, newValue);
                    break;

                case StatType.MaxStamina:
                    float staminaDiff = newValue - oldValue;
                    if (staminaDiff > 0) _currentStamina += staminaDiff;
                    _currentStamina = Mathf.Min(_currentStamina, newValue);
                    break;

                case StatType.MaxMana:
                    float manaDiff = newValue - oldValue;
                    if (manaDiff > 0) _currentMana += manaDiff;
                    _currentMana = Mathf.Min(_currentMana, newValue);
                    break;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            // Restore resources on level up
            _currentHealth = MaxHealth;
            _currentStamina = MaxStamina;
            _currentMana = MaxMana;

            Debug.Log($"Level Up! Now level {newLevel}");
        }

        /// <summary>
        /// Take damage from an attack
        /// </summary>
        public void TakeDamage(float rawDamage, bool isMagic = false)
        {
            float defense = isMagic ? MagicDefense : PhysicalDefense;
            
            // Simple damage formula - adjust as needed
            float damageReduction = defense / (defense + 100f);
            float finalDamage = rawDamage * (1f - damageReduction);
            
            _currentHealth -= finalDamage;

            Debug.Log($"Took {finalDamage:F1} damage ({rawDamage:F1} raw, {damageReduction:P0} reduced)");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Deal physical damage
        /// </summary>
        public float CalculatePhysicalDamage(float weaponDamage)
        {
            float baseDamage = weaponDamage + PhysicalAttack;
            
            // Check for critical hit
            float critChance = _stats.GetStatValue(StatType.CriticalChance);
            float critDamage = _stats.GetStatValue(StatType.CriticalDamage);
            
            if (Random.value < critChance)
            {
                baseDamage *= critDamage;
                Debug.Log("Critical Hit!");
            }

            return baseDamage;
        }

        /// <summary>
        /// Deal magic damage
        /// </summary>
        public float CalculateMagicDamage(float spellBaseDamage)
        {
            return spellBaseDamage + MagicAttack;
        }

        /// <summary>
        /// Try to consume stamina for an action
        /// </summary>
        public bool TryConsumeStamina(float amount)
        {
            if (_currentStamina >= amount)
            {
                _currentStamina -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to consume mana for a spell
        /// </summary>
        public bool TryConsumeMana(float amount)
        {
            if (_currentMana >= amount)
            {
                _currentMana -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Heal the character
        /// </summary>
        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
        }

        /// <summary>
        /// Restore stamina
        /// </summary>
        public void RestoreStamina(float amount)
        {
            _currentStamina = Mathf.Min(_currentStamina + amount, MaxStamina);
        }

        /// <summary>
        /// Restore mana
        /// </summary>
        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(_currentMana + amount, MaxMana);
        }

        private void Die()
        {
            Debug.Log("Player died!");
            // Implement death logic
        }

        /// <summary>
        /// Get save data for the entire character
        /// </summary>
        public CharacterSaveData GetSaveData()
        {
            return new CharacterSaveData
            {
                Level = _stats.Level,
                AvailableStatPoints = _stats.AvailableStatPoints,
                PrimaryStats = new System.Collections.Generic.Dictionary<StatType, float>
                {
                    { StatType.Vigor, _stats.GetStatBaseValue(StatType.Vigor) },
                    { StatType.Endurance, _stats.GetStatBaseValue(StatType.Endurance) },
                    { StatType.Strength, _stats.GetStatBaseValue(StatType.Strength) },
                    { StatType.Dexterity, _stats.GetStatBaseValue(StatType.Dexterity) },
                    { StatType.Intelligence, _stats.GetStatBaseValue(StatType.Intelligence) },
                    { StatType.Faith, _stats.GetStatBaseValue(StatType.Faith) },
                    { StatType.Arcane, _stats.GetStatBaseValue(StatType.Arcane) }
                },
                CurrentHealth = _currentHealth,
                CurrentStamina = _currentStamina,
                CurrentMana = _currentMana,
                SkillTreeData = _skillTree.GetSaveData(),
                ArtifactData = _artifacts.GetSaveData()
            };
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Level Up")]
        private void DebugLevelUp()
        {
            _stats.LevelUp(3); // 3 stat points per level
            _skillTree.AddSkillPoints(1);
        }

        [ContextMenu("Debug - Print All Stats")]
        private void DebugPrintStats()
        {
            Debug.Log($"=== {gameObject.name} Stats ===");
            Debug.Log($"Level: {_stats.Level}");
            Debug.Log($"HP: {_currentHealth:F0}/{MaxHealth:F0}");
            Debug.Log($"Stamina: {_currentStamina:F0}/{MaxStamina:F0}");
            Debug.Log($"Mana: {_currentMana:F0}/{MaxMana:F0}");
            Debug.Log($"Physical Attack: {PhysicalAttack:F1}");
            Debug.Log($"Magic Attack: {MagicAttack:F1}");
            Debug.Log($"Physical Defense: {PhysicalDefense:F1}");
            Debug.Log($"Magic Defense: {MagicDefense:F1}");
            Debug.Log($"Crit Chance: {_stats.GetStatValue(StatType.CriticalChance):P1}");
            Debug.Log($"Crit Damage: {_stats.GetStatValue(StatType.CriticalDamage):F2}x");
        }

        [ContextMenu("Debug - Take 50 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(50f, false);
        }
#endif
    }

    /// <summary>
    /// Complete save data for a character
    /// </summary>
    [System.Serializable]
    public class CharacterSaveData
    {
        public int Level;
        public int AvailableStatPoints;
        public System.Collections.Generic.Dictionary<StatType, float> PrimaryStats;
        public float CurrentHealth;
        public float CurrentStamina;
        public float CurrentMana;
        public SkillTreeSaveData SkillTreeData;
        public ArtifactSaveData ArtifactData;
    }
}
