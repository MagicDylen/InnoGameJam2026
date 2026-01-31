using UnityEngine;

public class GetDamageFromMasks : MonoBehaviour
{
    PlayerStats playerStats;

    public float DetectionRadius = 1f;
   
    public void Start()
    {
        TryGetComponent<PlayerStats>(out playerStats);
    }

    public void Update()
    {
        var layerMask = LayerMask.GetMask("Ground");

        var inRange = Physics2D.OverlapCircleAll(transform.position, DetectionRadius, layerMask);
        foreach (var coll in inRange)
        {
            coll.gameObject.TryGetComponent<ColorComboEffect>(out var combo);
            if(!combo) continue;
            ManualCollide(coll);
        }
    }
    private void ManualCollide(Collider2D collision)
    {
        if(!playerStats) return;
        collision.gameObject.TryGetComponent<ColorComboEffect>(out var combo);
        if(!combo) return;
        collision.gameObject.TryGetComponent<EnemyStats>(out var enemyStats);
        if(!enemyStats) return;

        if(combo.AssignedType == ColorComboEffect.MaskType.Sticky)
        {
            //Spikey in the air
            //Spikey on the ground
            playerStats.DecreaseHealth(enemyStats.Damage);
            

        } else if(combo.AssignedType == ColorComboEffect.MaskType.Explosive)
        {
            // trigger the explosion, player will be damaged from that script
            combo.TriggerCollisionEffect(transform.position);
        } 
        
    }
}
