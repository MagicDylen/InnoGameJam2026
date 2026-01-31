using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimatorDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController controller;

    [Header("Tuning")]
    [Tooltip("How fast you must be moving (x) to count as 'moving'.")]
    public float moveEpsilon = 0.05f;

    [Tooltip("How fast (y) you must be moving to count as rising/falling.")]
    public float verticalEpsilon = 0.10f;

    [Tooltip("Used for the 'float' apex window: |vy| below this means float.")]
    public float floatEpsilon = 0.08f;

    // Animator parameter hashes (faster + typo-proof)
    static readonly int Grounded = Animator.StringToHash("grounded");
    static readonly int Falling = Animator.StringToHash("falling");
    static readonly int Movement = Animator.StringToHash("movement");
    static readonly int Attack = Animator.StringToHash("attack");

    void Awake()
    {
        if (!controller) controller = GetComponent<PlayerController>();

        if (!animator)
        {
            // Try same object first, then children
            animator = GetComponent<Animator>();
            if (!animator) animator = GetComponentInChildren<Animator>();
        }

        // Your preference: loud failures > silent nothing
        if (!controller) throw new MissingComponentException("PlayerAnimatorDriver needs PlayerController.");
        if (!animator) throw new MissingComponentException("PlayerAnimatorDriver could not find an Animator. Assign it in the Inspector.");
    }

    void Update()
    {
        // Basic facts
        bool grounded = controller.IsGrounded;
        Vector2 v = controller.Velocity;

        float speedX = Mathf.Abs(v.x);
        float vy = v.y;

        // Movement parameter (0..whatever)
        animator.SetFloat(Movement, speedX);

        // Grounded
        animator.SetBool(Grounded, grounded);

        // Falling bool: true only when airborne and going down noticeably
        bool falling = !grounded && (vy < -verticalEpsilon);
        animator.SetBool(Falling, falling);

        // Optional: if you want a clean "float apex" moment,
        // you can force Falling false while |vy| is tiny (still airborne).
        // This makes it easier to transition JumpUp -> Float -> FallDown.
        if (!grounded && Mathf.Abs(vy) <= floatEpsilon)
        {
            animator.SetBool(Falling, false);
        }

        // Attack input -> trigger
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger(Attack);
        }
    }
}
