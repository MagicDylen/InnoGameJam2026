using UnityEngine;

public class Slash : MonoBehaviour
{
    PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null)
            throw new System.Exception($"{nameof(Slash)}: couldn't find PlayerController in parents. Make sure Slash is under the Player.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlatformEnemy platformEnemy = other.GetComponent<PlatformEnemy>();
        if (platformEnemy == null)
            platformEnemy = other.GetComponentInParent<PlatformEnemy>();

        if (platformEnemy == null)
            return;

        // ✅ Reset slash if alive OR dead-but-springy
        if (platformEnemy.ShouldResetSlashOnHit())
            player.NotifySlashHitEnemy();

        // ✅ Still kill if not dead yet (Kill() already early-outs if dead)
        platformEnemy.Kill();
    }
}
