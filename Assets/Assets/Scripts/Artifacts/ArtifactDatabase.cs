using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Artifacts
{
    /// <summary>
    /// Database of all artifacts in the game
    /// Use this to lookup artifacts by ID or filter by criteria
    /// </summary>
    [CreateAssetMenu(fileName = "ArtifactDatabase", menuName = "RPG Framework/Artifacts/Artifact Database")]
    public class ArtifactDatabase : ScriptableObject
    {
        [Header("All Artifacts")]
        [SerializeField] private List<ArtifactData> _allArtifacts = new List<ArtifactData>();

        private Dictionary<string, ArtifactData> _artifactLookup;
        private bool _initialized = false;

        public IReadOnlyList<ArtifactData> AllArtifacts => _allArtifacts;

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;

            _artifactLookup = new Dictionary<string, ArtifactData>();
            foreach (var artifact in _allArtifacts)
            {
                if (artifact != null && !string.IsNullOrEmpty(artifact.ArtifactId))
                {
                    if (!_artifactLookup.ContainsKey(artifact.ArtifactId))
                    {
                        _artifactLookup[artifact.ArtifactId] = artifact;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate artifact ID: {artifact.ArtifactId}");
                    }
                }
            }

            _initialized = true;
        }

        /// <summary>
        /// Get artifact by ID
        /// </summary>
        public ArtifactData GetArtifactById(string id)
        {
            if (!_initialized) Initialize();
            return _artifactLookup.TryGetValue(id, out var artifact) ? artifact : null;
        }

        /// <summary>
        /// Get all artifacts of a specific rarity
        /// </summary>
        public List<ArtifactData> GetArtifactsByRarity(ArtifactRarity rarity)
        {
            return _allArtifacts.FindAll(a => a != null && a.Rarity == rarity);
        }

        /// <summary>
        /// Get all artifacts for a specific slot
        /// </summary>
        public List<ArtifactData> GetArtifactsBySlot(ArtifactSlot slot)
        {
            return _allArtifacts.FindAll(a => a != null && (a.Slot == slot || a.Slot == ArtifactSlot.Any));
        }

        /// <summary>
        /// Get all artifacts that a character can equip
        /// </summary>
        public List<ArtifactData> GetEquippableArtifacts(Stats.StatContainer stats)
        {
            return _allArtifacts.FindAll(a => a != null && a.CanEquip(stats));
        }

        /// <summary>
        /// Get random artifact of specified rarity
        /// </summary>
        public ArtifactData GetRandomArtifact(ArtifactRarity rarity)
        {
            var artifacts = GetArtifactsByRarity(rarity);
            if (artifacts.Count == 0) return null;
            return artifacts[Random.Range(0, artifacts.Count)];
        }

        /// <summary>
        /// Get random artifact with weighted rarity
        /// </summary>
        public ArtifactData GetRandomArtifactWeighted(Dictionary<ArtifactRarity, float> rarityWeights)
        {
            float totalWeight = 0f;
            foreach (var weight in rarityWeights.Values)
            {
                totalWeight += weight;
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var kvp in rarityWeights)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                {
                    return GetRandomArtifact(kvp.Key);
                }
            }

            return GetRandomArtifact(ArtifactRarity.Common);
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Database")]
        private void RefreshDatabase()
        {
            _allArtifacts.Clear();
            
            // Find all ArtifactData assets in the project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ArtifactData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var artifact = UnityEditor.AssetDatabase.LoadAssetAtPath<ArtifactData>(path);
                if (artifact != null)
                {
                    _allArtifacts.Add(artifact);
                }
            }

            _initialized = false;
            Initialize();
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Artifact database refreshed. Found {_allArtifacts.Count} artifacts.");
        }

        [ContextMenu("Validate All Artifacts")]
        private void ValidateArtifacts()
        {
            int issues = 0;
            foreach (var artifact in _allArtifacts)
            {
                if (artifact == null)
                {
                    Debug.LogError("Null artifact reference in database");
                    issues++;
                    continue;
                }

                if (string.IsNullOrEmpty(artifact.ArtifactId))
                {
                    Debug.LogWarning($"Artifact '{artifact.name}' has no ArtifactId");
                    issues++;
                }

                if (string.IsNullOrEmpty(artifact.ArtifactName))
                {
                    Debug.LogWarning($"Artifact '{artifact.name}' has no ArtifactName");
                    issues++;
                }

                if (artifact.Icon == null)
                {
                    Debug.LogWarning($"Artifact '{artifact.ArtifactName}' has no icon");
                }
            }

            if (issues == 0)
            {
                Debug.Log("All artifacts validated successfully!");
            }
            else
            {
                Debug.LogWarning($"Found {issues} issues in artifact database");
            }
        }
#endif
    }
}
