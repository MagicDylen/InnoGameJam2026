using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    PlayerController player;

    // Prevent multi-triggering the same enemy while the collider overlaps
    // (common when enemy has multiple colliders or physics gets jittery).
    readonly HashSet<int> hitThisActivation = new HashSet<int>();

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null)
            throw new System.Exception($"{nameof(Slash)}: couldn't find PlayerController in parents. Make sure Slash is under the Player.");
    }

    void OnEnable()
    {
        hitThisActivation.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Try find the enemy (child collider case supported)
        PlatformEnemy platformEnemy = other.GetComponent<PlatformEnemy>();
        if (platformEnemy == null)
            platformEnemy = other.GetComponentInParent<PlatformEnemy>();

        if (platformEnemy == null)
            return;

        // De-dupe per activation
        int id = platformEnemy.gameObject.GetInstanceID();
        if (hitThisActivation.Contains(id))
            return;

        hitThisActivation.Add(id);

        if (!platformEnemy.isDead)
        {
            // ✅ Notify player to enter/refresh Spinning and get boost
            player.NotifySlashHitEnemy();
        }

        // ✅ "Successful hit"
        platformEnemy.Kill();

       
    }
}
