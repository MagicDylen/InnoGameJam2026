using UnityEngine;

public class Slash : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kill anything with PlatformEnemy
        var platformEnemy = other.GetComponent<PlatformEnemy>();
        if (platformEnemy != null)
        {
            platformEnemy.Kill();
            return;
        }

        // If your enemy collider is a child and the script is on parent:
        platformEnemy = other.GetComponentInParent<PlatformEnemy>();
        if (platformEnemy != null)
        {
            platformEnemy.Kill();
        }
    }
}
