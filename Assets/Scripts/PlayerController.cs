using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 21.3f;
    public float maxHorizontalSpeed = 21.3f; // clamp ONLY X

    [Header("Jump")]
    public float jumpVelocity = 13f;
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.08f;

    [Header("Better Jump")]
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 8f;

    [Header("Ground Mask")]
    public LayerMask groundMask;

    [Header("Visuals")]
    public Transform playerVisuals;
    [Tooltip("Input threshold to prevent flip jitter when input is ~0.")]
    public float flipDeadzone = 0.01f;

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

    public bool facingRight = true;

    Rigidbody2D rb;

    float moveInput;
    bool jumpHeld;

    float coyoteCounter;
    float jumpBufferCounter;


    // Cached ground info from last FixedUpdate spring cast
    bool grounded;
    float groundDistance; // distance from origin to ground hit
    RaycastHit2D groundHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead())
        {
            return;
        }
        
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBuffer;

        jumpHeld = Input.GetButton("Jump");

        jumpBufferCounter -= Time.deltaTime;

        // coyote counter is based on "grounded" computed in FixedUpdate.
        // That's fine: it will lag by at most one physics step, which is stable.
        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        UpdateFacing(); // <-- flip visuals here

    }

    void FixedUpdate()
    {
        if (isDead())
        {
            return; 
        }
        
        // 1) Read ground via rays (stable on moving rigidbodies)
        UpdateGroundInfo();

        // 2) Apply spring stabilization BEFORE we finalize velocity
        ApplySpringFeet();

        // 3) Horizontal movement: set X, keep Y untouched
        Vector2 v = rb.linearVelocity;
        float targetX = moveInput * moveSpeed;
        v.x = Mathf.Clamp(targetX, -maxHorizontalSpeed, maxHorizontalSpeed);

        // 4) Jump: set Y once when conditions are met
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            v.y = jumpVelocity;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        rb.linearVelocity = v;

        // 5) Better jump shaping (affects Y only)
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
    }

    void UpdateFacing()
    {
        // Use input to decide facing (most platformers do this).
        // If you prefer velocity-based facing, replace moveInput with rb.linearVelocity.x.
        if (moveInput > flipDeadzone) SetFacing(true);
        else if (moveInput < -flipDeadzone) SetFacing(false);
    }

    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;

        Vector3 s = playerVisuals.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
        playerVisuals.localScale = s;
    }

    void UpdateGroundInfo()
    {
        Transform originT = springOrigin ? springOrigin : transform;
        Vector2 origin = originT.position;

        // Three ray origins: left, center, right
        Vector2 oL = origin + Vector2.left * raySpacing;
        Vector2 oC = origin;
        Vector2 oR = origin + Vector2.right * raySpacing;

        RaycastHit2D hL = Physics2D.Raycast(oL, Vector2.down, rayLength, groundMask);
        RaycastHit2D hC = Physics2D.Raycast(oC, Vector2.down, rayLength, groundMask);
        RaycastHit2D hR = Physics2D.Raycast(oR, Vector2.down, rayLength, groundMask);

        // Pick the closest valid hit (smallest distance)
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

        // Optional: don't let spring fight you while you are moving upward from a jump
        if (disableSpringWhileRising && rb.linearVelocity.y > 0.1f)
            return;

        // Only engage when we're within reasonable range of target height
        // (If we're far above, don't "yoink" downward weirdly.)
        float engageRange = targetHeight + 0.35f;
        if (groundDistance > engageRange) return;

        // PD controller trying to keep origin at targetHeight above ground
        float error = (targetHeight - groundDistance); // positive => below desired height (need push up)
        float velY = rb.linearVelocity.y;

        float forceY = (error * springStrength) - (velY * springDamping);

        // Extra "stick" so we settle instead of hovering
        if (error < 0f) // we're above target height
            forceY -= stickDownForce;

        // Clamp upward force to prevent launch spikes
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
            // PlayerStats component doesn't exist, assume not dead
            return false;
        }
    }
}
