using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private readonly Vector3 _offset = new Vector2(0.5f, 0.5f);
    private readonly Cooldown _attackCooldown = new Cooldown(0.3f);

    public Animator animator;


    void Update()
    {
        if (!ShouldAttack()) return;
        Attack();
    }

    private bool ShouldAttack()
    {
        return ObjectHolder.ActionMap["Attack"].WasPerformedThisFrame()
               && _attackCooldown.HasFinished();
    }

    private void Attack()
    {
        // 🔥 Trigger animation
        animator.SetTrigger("attack");

        Instantiate(
            ObjectHolder.AttackPrefab,
            transform.position + _offset,
            transform.rotation
        );
        Debug.Log("Attacked");
        var am = FindFirstObjectByType<AudioManager>();
        am?.PlayOneShot(am.PlayerSlash, ObjectHolder.Player.transform.position);

        _attackCooldown.StartCooldown();
    }
}
