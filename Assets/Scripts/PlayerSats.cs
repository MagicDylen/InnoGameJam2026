using Unity.Mathematics;
using UnityEngine;

public class PlayerSats : MonoBehaviour
{
    [SerializeField] float SpeedMultiplier = 1;
    [SerializeField] float DamageMultiplier = 1;
    [SerializeField] float TotalHealth = 100;
    [SerializeField] float MaxHealth = 100;
    [SerializeField] float HealthPickUpAdd = 20;
    [SerializeField] float DamagePickUpAdd = 0.2f;
    [SerializeField] float SpeeedPickUpAdd = 0.1f;





    public void IncreaseHealth()
    {
        TotalHealth = math.min(MaxHealth, TotalHealth +  HealthPickUpAdd);
    }

    public void IncreaseDamage()
    {
        DamageMultiplier = DamagePickUpAdd;
    }

    public void IncreaseSpeed()
    {
        SpeedMultiplier += SpeeedPickUpAdd;
    }

}
