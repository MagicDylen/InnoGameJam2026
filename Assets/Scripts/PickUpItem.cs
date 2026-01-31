using UnityEngine;

public class PickUpItem : MonoBehaviour
{

    public enum Type
    {
        Undefined,
        Health,
        Damage,
        Speed
    }

    [SerializeField] Type ItemType = Type.Undefined;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        PlayerStats stats;
        collision.gameObject.TryGetComponent(out stats);
        if (!stats)
        {
            return;
        }
        
        switch (ItemType)
        {
            case Type.Undefined:
                break;
            case Type.Health:
                stats.IncreaseHealth(stats.HeartValue);
                break;
            case Type.Damage:
                stats.IncreaseDamage();
                break;
            case Type.Speed:
                stats.IncreaseSpeed();
                break;
        }
        Destroy(gameObject);

        
    }


}
