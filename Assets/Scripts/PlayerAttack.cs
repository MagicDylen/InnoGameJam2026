using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private readonly Vector3 _offset = new Vector2(0.5f, 0.5f);
    private readonly Cooldown _attackCooldown = new Cooldown(1.5f);

    void Update()
    {
        if (!ShouldAttack()) return;
        Attack();
    }

    private bool ShouldAttack()
    {
        return ObjectHolder.ActionMap["Attack"].WasPerformedThisFrame() && _attackCooldown.HasFinished();
    }

    private void Attack()
    {
        Instantiate(ObjectHolder.AttackPrefab, this.transform.position + _offset, this.transform.rotation);

        _attackCooldown.StartCooldown();
    }
}