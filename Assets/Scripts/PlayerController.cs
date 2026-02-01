using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float fallingVelocity = 3f;

    [Header("Spinning State")]
    [Tooltip("If true, after hitting an enemy you enter Spinning: slash stays active until grounded.")]
    public bool enableSpinning = true;

    [Tooltip("Upward velocity applied when you hit an enemy and enter/refresh Spinning.")]
    public float spinUpBoost = 10f;

    [Tooltip("Extra upward velocity added per hit while already spinning (optional; set 0 to just set to spinUpBoost).")]
    public float spinUpBoostAdd = 6f;

    [Tooltip("Clamp the maximum upward velocity while chaining hits.")]
    public float spinMaxUpVelocity = 22f;

    bool isSpinning;
    public bool IsSpinning => isSpinning;

    [Header("Movement")]
    public float moveSpeed = 21.3f;
    public float maxHorizontalSpeed = 21.3f; // clamp ONLY X

    [Header("Jump")]
    public float jumpVelocity = 13f;
    public float jumpBuffer = 0.08f;

    [Header("Double Jump")]
    [Tooltip("Total jumps allowed before touching ground again. 2 = double jump.")]
    public int maxJumps = 2;

    [Header("Second Jump Swoosh Attack")]
    [Tooltip("Child GameObject to enable as the circular swoosh collider/attack.")]
    public GameObject secondJumpSwooshObject;

    [Tooltip("How long the swoosh collider stays enabled (seconds).")]
    public float swooshDuration = 0.12f;

    [Header("Slash Visuals")]
    [Tooltip("SpriteRenderer on the slash/swoosh object (or a child of it). Used to tint/flash.")]
    public SpriteRenderer slashSpriteRenderer;

    [Tooltip("Normal slash tint.")]
    public Color normalSlashColor = Color.white;

    [Tooltip("Slash tint while in Spinning/Juggle state.")]
    public Color juggleSlashColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Tooltip("If true, juggle state will flash between normal and juggle color.")]
    public bool juggleFlash = true;

    [Tooltip("Flashes per second while juggling (only if juggleFlash = true).")]
    public float juggleFlashSpeed = 14f;

    [Tooltip("Scale multiplier for the slash while juggling. (0.9 = smaller, 1.1 = bigger).")]
    public float juggleSlashScaleMultiplier = 0.9f;

    [Header("Visuals")]
    public Transform playerVisuals;
    [Tooltip("Input threshold to prevent flip jitter when input is ~0.")]
    public float flipDeadzone = 0.01f;

    [Tooltip("SpriteRenderer for the 'normal player' visuals to hide during Slash/Swoosh.")]
    public SpriteRenderer normalPlayerSpriteRenderer;

    [Header("Spring Feet (PD Hover)")]
    [Tooltip("Where rays start from (usually around feet/hips). If null, uses transform.")]
    public Transform springOrigin;
    public float raySpacing = 0.25f;
    public float rayLength = 1.2f;
    public float targetHeight = 0.10f;
    public float springStrength = 120f;
    public float springDamping = 18f;
    public float maxUpwardForce = 250f;
    public float stickDownForce = 15f;
    public bool disableSpringWhileRising = true;

    [Header("Hurt / Knockback Lock")]
    public float defaultHurtLockDuration = 0.5f;
    public bool disableJumpDuringHurtLock = true;
    public bool clearJumpBufferOnHit = true;

    [Header("Better Jump")]
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 8f;

    [Header("Ground Mask")]
    public LayerMask groundMask;

    public bool facingRight = true;

    Rigidbody2D rb;

    float moveInput;
    bool jumpHeld;

    float jumpBufferCounter;

    int jumpsRemaining;
    bool wasGrounded;

    float swooshTimer;

    bool grounded;
    float groundDistance;
    RaycastHit2D groundHit;

    float hurtLockTimer;

    // Slash visuals caching
    Vector3 slashBaseLocalScale = Vector3.one;
    Color slashBaseColor = Color.white;
    bool slashCacheReady;

    public float VerticalSpeed => rb.linearVelocity.y;
    public bool IsGrounded => grounded;
    public float MoveInput => moveInput;
    public float HorizontalSpeed => Mathf.Abs(moveInput);
    public bool IsHurtLocked => hurtLockTimer > 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = Mathf.Max(1, maxJumps);

        if (secondJumpSwooshObject != null)
            secondJumpSwooshObject.SetActive(false);

        SetNormalVisualsActive(true);

        CacheSlashDefaultsOrThrow();
        ApplySlashVisuals_Normal();
    }

    void Update()
    {
        if (isDead())
            return;

        if (!IsHurtLocked)
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
                jumpBufferCounter = jumpBuffer;
        }
        else
        {
            moveInput = 0f;
            if (clearJumpBufferOnHit)
                jumpBufferCounter = 0f;
        }

        jumpHeld = Input.GetButton("Jump");
        jumpBufferCounter -= Time.deltaTime;

        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (isDead())
            return;

        if (hurtLockTimer > 0f)
            hurtLockTimer -= Time.fixedDeltaTime;

        UpdateGroundInfo();

        if (grounded && !wasGrounded && rb.linearVelocity.y <= fallingVelocity)
        {
            jumpsRemaining = Mathf.Max(1, maxJumps);

            // Landing ends Spinning and any slash visuals.
            if (isSpinning)
                EndSpin();
            else
                StopSwoosh();
        }

        // If just left ground without jumping, only allow 1 air jump
        if (!grounded && wasGrounded && jumpsRemaining == maxJumps)
        {
            jumpsRemaining = 1;
        }

        wasGrounded = grounded;

        Vector2 v = rb.linearVelocity;

        if (!IsHurtLocked)
        {
            float targetX = moveInput * moveSpeed;
            v.x = Mathf.Clamp(targetX, -maxHorizontalSpeed, maxHorizontalSpeed);
        }

        bool allowJump = !IsHurtLocked || !disableJumpDuringHurtLock;
        bool didJumpThisStep = false;

        if (allowJump && jumpBufferCounter > 0f)
        {
            bool canGroundJump = grounded && v.y <= 0f;
            bool canAirJump = !grounded && jumpsRemaining > 0;

            if (canGroundJump || canAirJump)
            {
                bool isSecondJump = (!canGroundJump && jumpsRemaining == 1 && maxJumps >= 2);

                v.y = jumpVelocity;

                jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);
                jumpBufferCounter = 0f;

                didJumpThisStep = true;

                if (isSecondJump)
                    TriggerSecondJumpSwoosh();
            }
        }

        rb.linearVelocity = v;

        if (!IsHurtLocked && !didJumpThisStep)
            ApplySpringFeet();

        v = rb.linearVelocity;

        if (v.y < 0f)
            v += Vector2.up * Physics2D.gravity * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (v.y > 0f && !jumpHeld)
            v += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        rb.linearVelocity = v;

        UpdateSwooshTimer();

        // Flash slash visuals while spinning/juggling
        if (isSpinning)
            UpdateJuggleSlashFlash();
    }

    /// <summary>
    /// Call this when the player's slash/swoosh successfully hits an enemy.
    /// Starts Spinning if not already spinning, keeps slash active until grounded,
    /// and gives an upward momentum boost. Re-hits refresh the boost.
    /// </summary>
    public void NotifySlashHitEnemy()
    {
        if (!enableSpinning)
            return;

        StartOrRefreshSpin();
        ApplySpinBoost();
    }

    void StartOrRefreshSpin()
    {
        if (secondJumpSwooshObject == null)
            throw new System.Exception($"{nameof(PlayerController)}: secondJumpSwooshObject is not assigned.");

        if (normalPlayerSpriteRenderer == null)
            throw new System.Exception($"{nameof(PlayerController)}: normalPlayerSpriteRenderer is not assigned (drag your normal visuals SpriteRenderer into the Inspector).");

        CacheSlashDefaultsOrThrow();

        isSpinning = true;

        if (!secondJumpSwooshObject.activeSelf)
            secondJumpSwooshObject.SetActive(true);

        SetNormalVisualsActive(false);

        // Ensure the normal swoosh timer can't turn it off while spinning.
        swooshTimer = 0f;

        // Apply juggle visuals immediately
        ApplySlashVisuals_Juggle();
    }

    void ApplySpinBoost()
    {
        Vector2 v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;

        v.y = Mathf.Max(v.y, spinUpBoost);

        if (spinUpBoostAdd > 0f)
            v.y = Mathf.Min(v.y + spinUpBoostAdd, spinMaxUpVelocity);

        rb.linearVelocity = v;
    }

    void EndSpin()
    {
        if (!isSpinning)
            return;

        isSpinning = false;

        // Turn off swoosh and restore normal visuals.
        StopSwoosh();
    }

    public void ApplyKnockbackVelocity(Vector2 knockbackVelocity, float lockDuration = -1f, bool cancelMomentumOnHit = true)
    {
        if (lockDuration < 0f)
            lockDuration = defaultHurtLockDuration;

        hurtLockTimer = Mathf.Max(hurtLockTimer, lockDuration);

        if (clearJumpBufferOnHit)
            jumpBufferCounter = 0f;

        if (cancelMomentumOnHit)
            rb.linearVelocity = Vector2.zero;

        rb.linearVelocity = knockbackVelocity;
    }

    void TriggerSecondJumpSwoosh()
    {
        if (secondJumpSwooshObject == null)
            throw new System.Exception($"{nameof(PlayerController)}: secondJumpSwooshObject is not assigned.");

        if (normalPlayerSpriteRenderer == null)
            throw new System.Exception($"{nameof(PlayerController)}: normalPlayerSpriteRenderer is not assigned (drag your normal visuals SpriteRenderer into the Inspector).");

        CacheSlashDefaultsOrThrow();

        swooshTimer = swooshDuration;
        secondJumpSwooshObject.SetActive(true);

        SetNormalVisualsActive(false);

        // If you’re already spinning, juggle visuals should win.
        if (isSpinning)
            ApplySlashVisuals_Juggle();
        else
            ApplySlashVisuals_Normal();
    }

    void UpdateSwooshTimer()
    {
        if (isSpinning)
            return;

        if (swooshTimer <= 0f)
            return;

        swooshTimer -= Time.fixedDeltaTime;

        if (swooshTimer <= 0f)
            StopSwoosh();
    }

    void StopSwoosh()
    {
        swooshTimer = 0f;

        if (secondJumpSwooshObject != null && secondJumpSwooshObject.activeSelf)
            secondJumpSwooshObject.SetActive(false);

        SetNormalVisualsActive(true);

        // Restore slash visuals (unless spinning is still somehow true)
        if (isSpinning)
            ApplySlashVisuals_Juggle();
        else
            ApplySlashVisuals_Normal();
    }

    void SetNormalVisualsActive(bool active)
    {
        if (normalPlayerSpriteRenderer == null)
            return;

        if (normalPlayerSpriteRenderer.enabled != active)
            normalPlayerSpriteRenderer.enabled = active;
    }

    void CacheSlashDefaultsOrThrow()
    {
        if (slashCacheReady) return;

        if (slashSpriteRenderer == null)
            throw new System.Exception($"{nameof(PlayerController)}: slashSpriteRenderer is not assigned (drag the SpriteRenderer used by the slash into the Inspector).");

        if (secondJumpSwooshObject == null)
            throw new System.Exception($"{nameof(PlayerController)}: secondJumpSwooshObject is not assigned (needed to scale the slash).");

        slashBaseLocalScale = secondJumpSwooshObject.transform.localScale;
        slashBaseColor = slashSpriteRenderer.color;

        // You can choose whether "normal" means base color or the inspector's normalSlashColor.
        // We'll use normalSlashColor as the intended look.
        slashCacheReady = true;
    }

    void ApplySlashVisuals_Normal()
    {
        if (!slashCacheReady) return;

        // Reset scale
        secondJumpSwooshObject.transform.localScale = slashBaseLocalScale;

        // Reset color (use your intended normal color)
        slashSpriteRenderer.color = normalSlashColor;
    }

    void ApplySlashVisuals_Juggle()
    {
        if (!slashCacheReady) return;

        // Scale by multiplier (0.9, 1.1, etc.)
        secondJumpSwooshObject.transform.localScale = slashBaseLocalScale * juggleSlashScaleMultiplier;

        // Color immediately (flash will animate it afterwards if enabled)
        slashSpriteRenderer.color = juggleFlash ? juggleSlashColor : juggleSlashColor;
    }

    void UpdateJuggleSlashFlash()
    {
        if (!slashCacheReady || slashSpriteRenderer == null) return;

        if (!juggleFlash)
        {
            slashSpriteRenderer.color = juggleSlashColor;
            return;
        }

        // Ping-pong between normal and juggle colors.
        // Using fixed time keeps it consistent during physics.
        float t = Mathf.PingPong(Time.time * juggleFlashSpeed, 1f);
        slashSpriteRenderer.color = Color.Lerp(normalSlashColor, juggleSlashColor, t);
    }

    void UpdateFacing()
    {
        float faceX = IsHurtLocked ? rb.linearVelocity.x : moveInput;

        if (faceX > flipDeadzone) SetFacing(true);
        else if (faceX < -flipDeadzone) SetFacing(false);
    }

    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;

        if (!playerVisuals)
            throw new System.Exception($"{nameof(PlayerController)}: playerVisuals is not assigned.");

        Vector3 s = playerVisuals.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
        playerVisuals.localScale = s;
    }

    void UpdateGroundInfo()
    {
        Transform originT = springOrigin ? springOrigin : transform;
        Vector2 origin = originT.position;

        Vector2 oL = origin + Vector2.left * raySpacing;
        Vector2 oC = origin;
        Vector2 oR = origin + Vector2.right * raySpacing;

        RaycastHit2D hL = Physics2D.Raycast(oL, Vector2.down, rayLength, groundMask);
        RaycastHit2D hC = Physics2D.Raycast(oC, Vector2.down, rayLength, groundMask);
        RaycastHit2D hR = Physics2D.Raycast(oR, Vector2.down, rayLength, groundMask);

        groundHit = default;
        float bestDist = float.PositiveInfinity;

        if (hL.collider && hL.distance < bestDist) { bestDist = hL.distance; groundHit = hL; }
        if (hC.collider && hC.distance < bestDist) { bestDist = hC.distance; groundHit = hC; }
        if (hR.collider && hR.distance < bestDist) { bestDist = hR.distance; groundHit = hR; }

        grounded = groundHit.collider != null && bestDist <= rayLength;
        groundDistance = grounded ? bestDist : float.PositiveInfinity;
    }

    void ApplySpringFeet()
    {
        if (!grounded) return;

        if (disableSpringWhileRising && rb.linearVelocity.y > 0.1f)
            return;

        float engageRange = targetHeight + 0.35f;
        if (groundDistance > engageRange) return;

        float error = (targetHeight - groundDistance);
        float velY = rb.linearVelocity.y;

        float forceY = (error * springStrength) - (velY * springDamping);

        if (error < 0f)
            forceY -= stickDownForce;

        forceY = Mathf.Min(forceY, maxUpwardForce);

        rb.AddForce(Vector2.up * forceY, ForceMode2D.Force);
    }

    void OnDrawGizmosSelected()
    {
        Transform originT = springOrigin ? springOrigin : transform;
        Vector2 origin = originT.position;

        Gizmos.color = Color.cyan;
        Vector2 oL = origin + Vector2.left * raySpacing;
        Vector2 oC = origin;
        Vector2 oR = origin + Vector2.right * raySpacing;

        Gizmos.DrawLine(oL, oL + Vector2.down * rayLength);
        Gizmos.DrawLine(oC, oC + Vector2.down * rayLength);
        Gizmos.DrawLine(oR, oR + Vector2.down * rayLength);

        if (grounded && groundHit.collider)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundHit.point, 0.05f);
        }
    }

    bool isDead()
    {
        try
        {
            return gameObject.GetComponent<PlayerStats>().IsDead;
        }
        catch
        {
            return false;
        }
    }
}
