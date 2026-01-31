using UnityEngine;

public class GetDamageFromMasks : MonoBehaviour
{
    PlayerStats playerStats;
   
    public void Start()
    {
        TryGetComponent<PlayerStats>(out playerStats);
    }
    private void OnCollisionEnter2D(Collision2D collision)
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
