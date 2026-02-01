using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator animator;
    public PlayerController controller;

    public float fallThreshold = -0.1f; // prevents jitter at apex


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
        animator.SetBool("grounded", controller.IsGrounded);
        animator.SetBool("hurt", controller.IsHurtLocked);


        // Falling detection
        bool falling = !controller.IsGrounded && controller.VerticalSpeed < fallThreshold;
        animator.SetBool("falling", falling);
    }
}
