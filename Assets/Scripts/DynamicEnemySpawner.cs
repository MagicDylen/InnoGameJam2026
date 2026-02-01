using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DynamicEnemySpawner : MonoBehaviour
{
    public enum EnemyType { Normal, Spikey, Explosive }
    public enum EnemyTier { Tier1 = 1, Tier2 = 2, Tier3 = 3 }
    private float lastXSpwanPos = 0;


    [Serializable]
    public class TypeConfig
    {
        public EnemyType type;

        [Tooltip("Relative weight for selecting this enemy type (e.g., 70, 20, 10). Can be any non-negative numbers.")]
        [Min(0f)] public float weight = 1f;

        [Header("Prefabs (must be assigned)")]
        public GameObject tier1Prefab;
        public GameObject tier2Prefab;
        public GameObject tier3Prefab;

        public GameObject GetPrefab(EnemyTier tier)
        {
            return tier switch
            {
                EnemyTier.Tier1 => tier1Prefab,
                EnemyTier.Tier2 => tier2Prefab,
                EnemyTier.Tier3 => tier3Prefab,
                _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported tier")
            };
        }
    }

    [Serializable]
    public struct LevelCooldowns
    {
        [Tooltip("Cooldown (seconds) between spawns before 1/3 progress.")]
        [Min(0.01f)] public float level1Cooldown;

        [Tooltip("Cooldown (seconds) between spawns after 1/3 progress, before 2/3.")]
        [Min(0.01f)] public float level2Cooldown;

        [Tooltip("Cooldown (seconds) between spawns after 2/3 progress.")]
        [Min(0.01f)] public float level3Cooldown;

        public float GetCooldown(int level)
        {
            return level switch
            {
                1 => level1Cooldown,
                2 => level2Cooldown,
                3 => level3Cooldown,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be 1..3")
            };
        }
    }

    private const string PLAYER_TAG = "Player";
    private const string WINCONDITION_TAG = "WinCondition";

    [Header("References (auto-found by Tag if left empty)")]
    [Tooltip("Player Transform. If null, will auto-find by tag 'Player'.")]
    [SerializeField] private Transform player;

    [Tooltip("Winning condition Transform placed at the top. If null, will auto-find by tag 'WinCondition'.")]
    [SerializeField] private Transform winConditionTop;

    [Header("Spawn Space")]
    [Tooltip("How far above the player (in world Y) the spawn plane is.")]
    [Min(0f)]
    [SerializeField] private float heightBuffer = 20f;

    [Tooltip("Square half-extent. X and Z offsets will be in [-horizontalRange, +horizontalRange].")]
    [Min(0f)]
    [SerializeField] private float horizontalRange = 10f;

    [Tooltip("Spawns the next enemy a guaranteed distance from the last, so they are less clustered")]
    [Min(0f)]
    [SerializeField] private float minDistanceFromLastSpawn = 1f;

    [Header("Spawn Timing")]
    [SerializeField]
    private LevelCooldowns cooldowns = new LevelCooldowns
    {
        level1Cooldown = 1.2f,
        level2Cooldown = 0.9f,
        level3Cooldown = 0.7f
    };

    [Header("Type Weights + Prefabs (3 Types, each 3 Tiers)")]
    [Tooltip("Must contain exactly 3 entries (Normal, Spikey, Explosive), each with 3 tier prefabs assigned.")]
    [SerializeField] private List<TypeConfig> typeConfigs = new List<TypeConfig>(3);

    [Header("Runtime")]
    [SerializeField] private bool isSpawning = true;

    private float _nextSpawnTime;

    private const float ONE_THIRD = 1f / 3f;
    private const float TWO_THIRDS = 2f / 3f;

    private void Awake()
    {
        AutoFindReferencesOrThrow();
        ValidateOrThrow();

        _nextSpawnTime = Time.time + cooldowns.level1Cooldown;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Try to auto-find in editor too (no throwing here � just log loudly).
        try
        {
            AutoFindReferencesOrThrow();
            ValidateOrThrow();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EnemySpawner] Configuration error on '{name}': {ex.Message}", this);
        }
    }
#endif

    private void Update()
    {
        if (!isSpawning) return;

        if (Time.time >= _nextSpawnTime)
        {
            SpawnOne();

            int level = GetLevelFromProgress(GetProgress01());
            float cd = cooldowns.GetCooldown(level);
            _nextSpawnTime = Time.time + cd;
        }
    }

    public void SetSpawning(bool enabled) => isSpawning = enabled;

    public void ForceSpawnOne()
    {
        AutoFindReferencesOrThrow();
        ValidateOrThrow();
        SpawnOne();
    }

    private void AutoFindReferencesOrThrow()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (p == null)
                throw new InvalidOperationException($"Player reference is null and no GameObject with tag '{PLAYER_TAG}' was found.");

            player = p.transform;
        }

        if (winConditionTop == null)
        {
            GameObject w = GameObject.FindGameObjectWithTag(WINCONDITION_TAG);
            if (w == null)
                throw new InvalidOperationException($"WinConditionTop reference is null and no GameObject with tag '{WINCONDITION_TAG}' was found.");

            winConditionTop = w.transform;
        }
    }

    private void SpawnOne()
    {
        float progress01 = GetProgress01();
        int level = GetLevelFromProgress(progress01);

        EnemyType chosenType = ChooseTypeByWeight();
        EnemyTier chosenTier = ChooseTierByLevel(level);

        GameObject prefab = GetPrefab(chosenType, chosenTier);
        Vector3 pos = GetSpawnPosition();

        Instantiate(prefab, pos, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 basePos = new Vector3(0, player.position.y, player.position.z) + Vector3.up * heightBuffer;

        float x = UnityEngine.Random.Range(-horizontalRange, horizontalRange);
        float z = UnityEngine.Random.Range(-horizontalRange, horizontalRange);

        // check if too narrow to last spawn
        if(x >= lastXSpwanPos - minDistanceFromLastSpawn && x <= lastXSpwanPos + minDistanceFromLastSpawn)
        {
            return GetSpawnPosition();
        }
        lastXSpwanPos = x;

        return basePos + new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Progress is computed relative to World Y=0 and the WinConditionTop's Y.
    /// progress01 = clamp(playerY / topY, 0..1).
    /// </summary>
    private float GetProgress01()
    {
        float topY = winConditionTop.position.y;
        if (topY <= 0f)
            throw new InvalidOperationException($"WinConditionTop Y must be > 0 (currently {topY}). World Y=0 is the base.");

        float playerY = player.position.y;
        float progress = playerY / topY;
        return Mathf.Clamp01(progress);
    }

    private static int GetLevelFromProgress(float progress01)
    {
        if (progress01 < ONE_THIRD) return 1;
        if (progress01 < TWO_THIRDS) return 2;
        return 3;
    }

    private EnemyType ChooseTypeByWeight()
    {
        float total = 0f;
        for (int i = 0; i < typeConfigs.Count; i++)
            total += Mathf.Max(0f, typeConfigs[i].weight);

        if (total <= 0f)
            throw new InvalidOperationException("Total type weight is 0. Set at least one TypeConfig weight > 0.");

        float roll = UnityEngine.Random.Range(0f, total);
        float running = 0f;

        for (int i = 0; i < typeConfigs.Count; i++)
        {
            float w = Mathf.Max(0f, typeConfigs[i].weight);
            running += w;
            if (roll <= running)
                return typeConfigs[i].type;
        }

        return typeConfigs[typeConfigs.Count - 1].type;
    }

    private static EnemyTier ChooseTierByLevel(int level)
    {
        float r = UnityEngine.Random.value;

        return level switch
        {
            1 => (r < 0.90f) ? EnemyTier.Tier1 : EnemyTier.Tier2,

            2 => (r < 0.50f) ? EnemyTier.Tier1
               : (r < 0.90f) ? EnemyTier.Tier2
               : EnemyTier.Tier3,

            3 => (r < (1f / 3f)) ? EnemyTier.Tier1
               : (r < (2f / 3f)) ? EnemyTier.Tier2
               : EnemyTier.Tier3,

            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be 1..3")
        };
    }

    private GameObject GetPrefab(EnemyType type, EnemyTier tier)
    {
        for (int i = 0; i < typeConfigs.Count; i++)
        {
            if (typeConfigs[i].type == type)
            {
                GameObject prefab = typeConfigs[i].GetPrefab(tier);
                if (prefab == null)
                    throw new InvalidOperationException($"Missing prefab for {type} {tier}.");
                return prefab;
            }
        }

        throw new InvalidOperationException($"No TypeConfig found for type '{type}'.");
    }

    private void ValidateOrThrow()
    {
        if (player == null)
            throw new InvalidOperationException("Player reference is not assigned and could not be auto-found.");

        if (winConditionTop == null)
            throw new InvalidOperationException("WinConditionTop reference is not assigned and could not be auto-found.");

        if (typeConfigs == null)
            throw new InvalidOperationException("TypeConfigs list is null.");

        if (typeConfigs.Count != 3)
            throw new InvalidOperationException($"TypeConfigs must contain exactly 3 entries (one per type). Current count: {typeConfigs.Count}");

        var seen = new HashSet<EnemyType>();
        for (int i = 0; i < typeConfigs.Count; i++)
        {
            TypeConfig cfg = typeConfigs[i];
            if (!seen.Add(cfg.type))
                throw new InvalidOperationException($"Duplicate TypeConfig for type '{cfg.type}'. Each type must appear once.");

            if (cfg.tier1Prefab == null) throw new InvalidOperationException($"Type '{cfg.type}' is missing Tier1 prefab.");
            if (cfg.tier2Prefab == null) throw new InvalidOperationException($"Type '{cfg.type}' is missing Tier2 prefab.");
            if (cfg.tier3Prefab == null) throw new InvalidOperationException($"Type '{cfg.type}' is missing Tier3 prefab.");
        }

        if (cooldowns.level1Cooldown <= 0f || cooldowns.level2Cooldown <= 0f || cooldowns.level3Cooldown <= 0f)
            throw new InvalidOperationException("All cooldowns must be > 0.");

        if (heightBuffer < 0f || horizontalRange < 0f)
            throw new InvalidOperationException("heightBuffer and horizontalRange must be >= 0.");
    }
}
