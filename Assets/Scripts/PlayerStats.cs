using Unity.Mathematics;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [SerializeField] float PlayerDamage = 10f;
    [SerializeField] float SpeedMultiplier = 1;
    [SerializeField] float DamageMultiplier = 1;
    [SerializeField] public float TotalHealth = 100;
    [SerializeField] float MaxHealth = 100;
    [SerializeField] float HeartValue = 20;
    [SerializeField] float DamagePickUpAdd = 0.2f;
    [SerializeField] float SpeeedPickUpAdd = 0.1f;
    [SerializeField] public bool IsDead = false;


    public float GetDamage()
    {
        return PlayerDamage * DamageMultiplier;
    }


    public void IncreaseHealth()
    {
        TotalHealth = math.min(MaxHealth, TotalHealth +  HeartValue);
    }

    public void DecreaseHealth()
    {
        TotalHealth -= HeartValue;
        if (TotalHealth <= 0)
        {
            IsDead = true;
            Debug.Log("Player Died");
        }
    }

    public void IncreaseDamage()
    {
        DamageMultiplier += DamagePickUpAdd;
    }

    public void IncreaseSpeed()
    {
        SpeedMultiplier += SpeeedPickUpAdd;
    }

}
