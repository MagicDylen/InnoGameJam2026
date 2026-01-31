using Mono.Cecil.Cil;
using UnityEngine;

public class EnemyDamagingArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.TryGetComponent<EnemyStats>(out var enemyStats);
        if(!enemyStats) return;
        if(!ObjectHolder.Player) return;
        ObjectHolder.Player.TryGetComponent<PlayerStats>(out var playerStats);
        if(!playerStats) return;
        enemyStats.Healthpoints -= playerStats.GetDamage();
    } 
}
