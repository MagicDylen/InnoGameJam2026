using UnityEngine;

public class PlatformEnemy : MonoBehaviour
{
    [SerializeField] private bool isImmortal = false;

    // NEW: if true, after death it keeps resetting slash (springy corpse)
    [SerializeField] private bool becomesSlashSpringOnDeath = false;

    [Header("Movement")]
    [SerializeField] private float fallSpeed = 1f;

    [Header("Damage")]
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private Collider2D damageTrigger; // assign the child trigger collider

    [Header("Graphics")]
    public GameObject vfxPrefab;
    public Sprite deadSprite;
    [SerializeField] private Sprite aliveSprite;
    private SpriteRenderer _spriteRenderer;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteToTint; // assign in inspector
    [SerializeField] private Color deadTint = new Color(0.6f, 0.9f, 0.6f, 1f);

    [Header("Death")]
    [SerializeField] private ParticleSystem deathParticlesPrefab;
    [SerializeField] private float hitStopDuration = 0.06f;
    [SerializeField] private float hitStopTimeScale = 0.05f;

    public bool isDead;
    private Color originalTint;
    private bool hasOriginalTint;

    private void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        if (damageTrigger == null)
            throw new MissingReferenceException($"{name}: Assign damageTrigger (child trigger collider).");

        if (spriteToTint == null)
            throw new MissingReferenceException($"{name}: Assign spriteToTint (SpriteRenderer to tint).");

        originalTint = spriteToTint.color;
        hasOriginalTint = true;
    }

    private void Update()
    {
        if (!isDead)
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        }
    }

    public void Kill()
    {
        if (isImmortal) return;
        if (isDead) return;

        isDead = true;

        // Stop doing damage
        damageTrigger.enabled = false;

        // Visuals
        spriteToTint.color = deadTint;
        _spriteRenderer.sprite = deadSprite;

        // Particles
        if (deathParticlesPrefab != null)
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        if (vfxPrefab != null)
            vfxPrefab.gameObject.SetActive(false);

        // Hit-stop
        HitStop.Do(hitStopDuration, hitStopTimeScale);
    }

    // Called by your DamageHitbox child script
    public void TryDamagePlayer(Collider2D other)
    {
        if (isDead) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(damageAmount, transform.position);
    }

    public void Revive()
    {
        isDead = false;
        damageTrigger.enabled = true;

        if (hasOriginalTint)
            spriteToTint.color = originalTint;
    }

    // NEW: tiny helper for Slash
    public bool ShouldResetSlashOnHit()
    {
        return (!isDead && !isImmortal) || becomesSlashSpringOnDeath;
    }
}
