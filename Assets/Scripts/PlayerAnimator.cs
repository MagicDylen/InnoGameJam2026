using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerController controller;

    public float fallThreshold = -1f; // prevents jitter at apex
    public float groundedCoyoteTime = 0.15f;

    private float airborneTimer = 0f;
    private bool wasGroundedLastFrame = true;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (!animator) Debug.LogError("No Animator found!");
        if (!controller) Debug.LogError("No PlayerController found!");
    }

    void Update()
    {
        // Feed movement speed into Animator
        animator.SetFloat("movement", controller.HorizontalSpeed);
        animator.SetBool("hurt", controller.IsHurtLocked);

        // Grounded state with coyote time to prevent animation flickering
        bool actuallyGrounded = controller.IsGrounded;
        bool animatorGrounded;

        if (actuallyGrounded)
        {
            // Immediately set grounded when landing (no delay for responsiveness)
            animatorGrounded = true;
            airborneTimer = 0f;
        }
        else
        {
            // When leaving ground, start/continue timer
            if (wasGroundedLastFrame)
            {
                // Just left ground, reset timer
                airborneTimer = 0f;
            }

            airborneTimer += Time.deltaTime;

            // Only consider airborne for animation after coyote time has elapsed
            // This prevents brief spring bounces from triggering the "up" animation
            animatorGrounded = airborneTimer < groundedCoyoteTime;
        }

        wasGroundedLastFrame = actuallyGrounded;
        animator.SetBool("grounded", animatorGrounded);

        // Falling detection
        bool falling = !controller.IsGrounded && controller.VerticalSpeed < fallThreshold;
        animator.SetBool("falling", falling);
    }
}
