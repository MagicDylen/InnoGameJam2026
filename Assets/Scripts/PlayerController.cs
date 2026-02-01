using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Spinning State")]
    [Tooltip("If true, after hitting an enemy you enter Spinning: slash stays active until grounded.")]
    public bool enableSpinning = true;

    [Tooltip("Upward velocity applied when you hit an enemy and enter/refresh Spinning.")]
    public float spinUpBoost = 10f;

    [Tooltip("Extra upward velocity added per hit while already spinning (optional; set 0 to just set to spinUpBoost).")]
    public float spinUpBoostAdd = 6f;

    [Tooltip("Clamp the maximum upward velocity while chaining hits.")]
    public float spinMaxUpVelocity = 22f;

    // State
    bool isSpinning;
    public bool IsSpinning => isSpinning;



    [Header("Movement")]
    public float moveSpeed = 21.3f;
    public float maxHorizontalSpeed = 21.3f; // clamp ONLY X

    [Header("Jump")]
    public float jumpVelocity = 13f;
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.08f;

    [Header("Double Jump")]
    [Tooltip("Total jumps allowed before touching ground again. 2 = double jump.")]
    public int maxJumps = 2;

    [Header("Second Jump Swoosh Attack")]
    [Tooltip("Child GameObject to enable as the circular swoosh collider/attack.")]
    public GameObject secondJumpSwooshObject;
    [Tooltip("How long the swoosh collider stays enabled (seconds).")]
    public float swooshDuration = 0.12f;

    [Header("Visuals")]
    public Transform playerVisuals;
    [Tooltip("Input threshold to prevent flip jitter when input is ~0.")]
    public float flipDeadzone = 0.01f;

    [Tooltip("SpriteRenderer for the 'normal player' visuals to hide during Slash/Swoosh.")]
    public SpriteRenderer normalPlayerSpriteRenderer;

    [Header("Spring Feet (PD Hover)")]
    [Tooltip("Where rays start from (usually around feet/hips). If null, uses transform.")]
    public Transform springOrigin;
    [Tooltip("Horizontal spacing for left/right rays from origin.")]
    public float raySpacing = 0.25f;
    [Tooltip("How far down we search for ground.")]
    public float rayLength = 1.2f;
    [Tooltip("Desired hover height above ground when grounded. Small value = 'standing' feel.")]
    public float targetHeight = 0.10f;
    [Tooltip("Spring strength (K). Higher = stiffer.")]
    public float springStrength = 120f;
    [Tooltip("Damping (D). Higher = less bounce.")]
    public float springDamping = 18f;
    [Tooltip("Clamp upward spring force to avoid rocket launches from chaotic piles.")]
    public float maxUpwardForce = 250f;
    [Tooltip("Extra downward bias to help settle onto ground instead of hovering jittery.")]
    public float stickDownForce = 15f;
    [Tooltip("Disable spring while rising after a jump (prevents spring fighting jump).")]
    public bool disableSpringWhileRising = true;

    [Header("Hurt / Knockback Lock")]
    [Tooltip("Default duration to disable player input after knockback.")]
    public float defaultHurtLockDuration = 0.5f;

    [Tooltip("If true, disables jumping during hurt lock too.")]
    public bool disableJumpDuringHurtLock = true;

    [Tooltip("If true, clears buffered jump when hit so you can't immediately jump-cancel knockback.")]
    public bool clearJumpBufferOnHit = true;

    public bool facingRight = true;

    Rigidbody2D rb;

    float moveInput;
    bool jumpHeld;

    float coyoteCounter;
    float jumpBufferCounter;

    int jumpsRemaining;
    bool wasGrounded;

    // Swoosh timer
    float swooshTimer;

    // Cached ground info
    bool grounded;
    float groundDistance;
    RaycastHit2D groundHit;

    // Hurt lock state
    float hurtLockTimer;

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

        // Ensure normal visuals are on at start (if assigned)
        SetNormalVisualsActive(true);
    }

    void Update()
    {
        if (isDead())
            return;

        // If locked, ignore player horizontal input (but we still update timers & facing based on velocity)
        if (!IsHurtLocked)
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
                jumpBufferCounter = jumpBuffer;
        }
        else
        {
            moveInput = 0f;

            // Optional: prevent input from queueing during lock
            if (clearJumpBufferOnHit)
                jumpBufferCounter = 0f;
        }

        jumpHeld = Input.GetButton("Jump");

        jumpBufferCounter -= Time.deltaTime;

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (isDead())
            return;

        if (hurtLockTimer > 0f)
            hurtLockTimer -= Time.fixedDeltaTime;

        UpdateGroundInfo();

        if (grounded && !wasGrounded)
        {
            jumpsRemaining = Mathf.Max(1, maxJumps);

            // Landing ends Spinning and any slash visuals.
            if (isSpinning)
                EndSpin();
            else
                StopSwoosh();
        }

        wasGrounded = grounded;

        Vector2 v = rb.linearVelocity;

        // Horizontal movement ONLY when not hurt-locked
        if (!IsHurtLocked)
        {
            float targetX = moveInput * moveSpeed;
            v.x = Mathf.Clamp(targetX, -maxHorizontalSpeed, maxHorizontalSpeed);
        }

        // Jump handling (optional disable while hurt-locked)
        bool allowJump = !IsHurtLocked || !disableJumpDuringHurtLock;

        bool didJumpThisStep = false;

        if (allowJump && jumpBufferCounter > 0f)
        {
            bool canGroundJump = (coyoteCounter > 0f);
            bool canAirJump = (!canGroundJump && jumpsRemaining > 0);

            if (canGroundJump || canAirJump)
            {
                bool isSecondJump = (!canGroundJump && jumpsRemaining == 1 && maxJumps >= 2);

                // Optional: normalize takeoff so downward spring motion can't reduce jump
                if (v.y < 0f) v.y = 0f;

                v.y = jumpVelocity;

                jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);

                jumpBufferCounter = 0f;
                coyoteCounter = 0f;

                didJumpThisStep = true;

                if (isSecondJump)
                    TriggerSecondJumpSwoosh();
            }
        }

        rb.linearVelocity = v;

        // ✅ Apply spring AFTER jump, and skip it on the jump frame
        if (!IsHurtLocked && !didJumpThisStep)
            ApplySpringFeet();

        // Better jump shaping (Y only)
        v = rb.linearVelocity;

        if (v.y < 0f)
        {
            v += Vector2.up * Physics2D.gravity * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (v.y > 0f && !jumpHeld)
        {
            v += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        rb.linearVelocity = v;

        UpdateSwooshTimer();
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

        // If you're hurt-locked, you can decide whether spin should be allowed.
        // Current behavior: allow it (feels juicy), but you can block it if desired.
        // if (IsHurtLocked) return;

        StartOrRefreshSpin();
        ApplySpinBoost();
    }

    void StartOrRefreshSpin()
    {
        if (secondJumpSwooshObject == null)
            throw new System.Exception($"{nameof(PlayerController)}: secondJumpSwooshObject is not assigned.");

        if (normalPlayerSpriteRenderer == null)
            throw new System.Exception($"{nameof(PlayerController)}: normalPlayerSpriteRenderer is not assigned (drag your normal visuals SpriteRenderer into the Inspector).");

        isSpinning = true;

        // Force slash/swoosh to remain on visually & mechanically.
        if (!secondJumpSwooshObject.activeSelf)
            secondJumpSwooshObject.SetActive(true);

        SetNormalVisualsActive(false);

        // Ensure the normal swoosh timer can't turn it off while spinning.
        swooshTimer = 0f;
    }

    void ApplySpinBoost()
    {
        Vector2 v = rb.linearVelocity;

        // Optional: if falling, cancel downwards first so the boost feels consistent.
        if (v.y < 0f) v.y = 0f;

        // Two styles:
        // 1) "Set" style: v.y = max(v.y, spinUpBoost)
        // 2) "Add" style: v.y += spinUpBoostAdd (with clamp)
        //
        // We do both: ensure at least spinUpBoost, then add extra per hit.
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

        // lock first so the same FixedUpdate doesn't instantly overwrite v.x
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

        // Optional loud error if you intended to use this and forgot to assign it
        if (normalPlayerSpriteRenderer == null)
            throw new System.Exception($"{nameof(PlayerController)}: normalPlayerSpriteRenderer is not assigned (drag your normal visuals SpriteRenderer into the Inspector).");

        swooshTimer = swooshDuration;
        secondJumpSwooshObject.SetActive(true);

        // Hide normal visuals while Slash/Swoosh is active
        SetNormalVisualsActive(false);
    }

    void UpdateSwooshTimer()
    {
        if (isSpinning)
            return; // spinning owns the swoosh lifetime

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

        // Re-show normal visuals when Slash/Swoosh ends
        SetNormalVisualsActive(true);
    }

    void SetNormalVisualsActive(bool active)
    {
        if (normalPlayerSpriteRenderer == null)
            return; // keep this quiet outside of the swoosh call; swoosh will throw if missing

        if (normalPlayerSpriteRenderer.enabled != active)
            normalPlayerSpriteRenderer.enabled = active;
    }

    void UpdateFacing()
    {
        // During hurt lock, face based on velocity to avoid "input says 0 so I never flip"
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

    [Header("Better Jump")]
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 8f;

    [Header("Ground Mask")]
    public LayerMask groundMask;
}
