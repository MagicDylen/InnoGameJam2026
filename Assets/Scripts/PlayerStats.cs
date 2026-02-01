using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStats : MonoBehaviour
{
    public GameObject playerVisuals;
    public GameObject deathParticlesPrefab;

    [Header("Combat Stats")]
    [SerializeField] float PlayerDamage = 10f;
    [SerializeField] float SpeedMultiplier = 1;
    [SerializeField] float DamageMultiplier = 1;

    [Header("Health")]
    [SerializeField] public float TotalHealth = 100;
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float HeartValue = 20;
    [SerializeField] public bool IsDead = false;

    [Header("Pickups")]
    [SerializeField] float DamagePickUpAdd = 0.2f;
    [SerializeField] float SpeeedPickUpAdd = 0.1f;

    [Header("Damage Cooldown (i-frames)")]
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime = -Mathf.Infinity;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 25f;
    [SerializeField] private float hurtLockDuration = 0.5f;
    [SerializeField] private bool cancelMomentumOnHit = true;


    [Header("Damage Particles")]
    [Tooltip("Particle prefab to spawn when damaged.")]
    [SerializeField] private GameObject damageParticlesPrefab;

    [Tooltip("Local offset from player position to spawn particles (e.g. chest height).")]
    [SerializeField] private Vector3 damageParticlesOffset = new Vector3(0f, 0.3f, 0f);

    [Header("Hitstop")]
    [Tooltip("Freeze time for this many seconds (real time). Big values feel crunchy.")]
    [SerializeField] private float hitstopDuration = 0.12f;

    [Tooltip("Optional: during hitstop set timescale to this. 0 = full freeze.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float hitstopTimeScale = 0f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // debug keys
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(HeartValue, transform.position + Vector3.right); // pretend hit from right
            Debug.Log("Health: " + TotalHealth);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            IncreaseHealth(HeartValue);
            Debug.Log("Health: " + TotalHealth);
        }
    }

    public float GetDamage()
    {
        return PlayerDamage * DamageMultiplier;
    }

    public void IncreaseHealth(float amount)
    {
        if (IsDead) return;
        TotalHealth = math.min(MaxHealth, TotalHealth + amount);
    }

    /// <summary>
    /// Call this when the player gets hit.
    /// damageSourcePosition = position of enemy / projectile / hazard that caused damage.
    /// </summary>
    public void TakeDamage(float amount, Vector2 damageSourcePosition)
    {
        if (IsDead) return;

        // i-frames / cooldown
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        // Apply health first (so death can short-circuit if you want)
        TotalHealth -= amount;

        // Feedback (even if you die, these feel good; tweak if you want)
        ApplyKnockback(damageSourcePosition);
        SpawnDamageParticles();
        DoHitstop();

        if (TotalHealth <= 0)
        {
            IsDead = true;
            //Disable Player visuals
            playerVisuals.SetActive(false);
            // Spawn death particles
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

            ObjectHolder objHolder = FindAnyObjectByType<ObjectHolder>();
            SceneManager.LoadScene("GameOverScene", LoadSceneMode.Additive);
            objHolder?.endGame();
        }
    }

    // Backwards-compatible wrapper if other code still calls DecreaseHealth(amount)
    public void DecreaseHealth(float amount)
    {
        // If nobody provides a source, treat it as "hit from left" (push right)
        Vector2 fakeSource = (Vector2)transform.position + Vector2.left;
        TakeDamage(amount, fakeSource);
    }

    private void ApplyKnockback(Vector2 damageSourcePosition)
    {
        var controller = GetComponent<PlayerController>();
        if (controller == null)
            throw new System.Exception($"{nameof(PlayerStats)}: PlayerController not found on player.");

        Vector2 away = (Vector2)transform.position - damageSourcePosition;

        if (away.sqrMagnitude < 0.0001f)
            away = Vector2.right; // stable fallback

        away.Normalize();

        Vector2 kbVel = away * knockbackStrength;

        controller.ApplyKnockbackVelocity(kbVel, hurtLockDuration, cancelMomentumOnHit);
    }



    private void SpawnDamageParticles()
    {
        if (damageParticlesPrefab == null)
            return;

        Instantiate(damageParticlesPrefab, transform.position + damageParticlesOffset, Quaternion.identity);
    }

    private void DoHitstop()
    {
        if (hitstopDuration <= 0f)
            return;

        HitStop.Do(hitstopDuration, hitstopTimeScale);
    }


    public void IncreaseDamage()
    {
        DamageMultiplier += DamagePickUpAdd;
    }

    public void IncreaseSpeed()
    {
        SpeedMultiplier += SpeeedPickUpAdd;
    }
}
