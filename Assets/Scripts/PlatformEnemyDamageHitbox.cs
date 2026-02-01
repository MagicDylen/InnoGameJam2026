using UnityEngine;

public class PlatformEnemyDamageHitbox : MonoBehaviour
{
    private PlatformEnemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<PlatformEnemy>();
        if (enemy == null)
            throw new MissingComponentException("DamageHitbox must be a child of PlatformEnemy.");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        enemy.TryDamagePlayer(other);
    }
}
