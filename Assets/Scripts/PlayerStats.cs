using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{

    [SerializeField] float PlayerDamage = 10f;
    [SerializeField] float SpeedMultiplier = 1;
    [SerializeField] float DamageMultiplier = 1;
    [SerializeField] public float TotalHealth = 100;
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float HeartValue = 20;
    [SerializeField] float DamagePickUpAdd = 0.2f;
    [SerializeField] float SpeeedPickUpAdd = 0.1f;
    [SerializeField] public bool IsDead = false;
    [SerializeField] private float damageCooldown = 1f;
    
    private float lastDamageTime = -Mathf.Infinity;
        
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DecreaseHealth(HeartValue);
            Debug.Log("Health: " + TotalHealth);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            IncreaseHealth(HeartValue);
            Debug.Log("Health: " + TotalHealth);
        }
    }
    

    public float GetDamage()
    {
        return PlayerDamage * DamageMultiplier;
    }


    public void IncreaseHealth(float amount)
    {
        if (IsDead) return;

        TotalHealth = math.min(MaxHealth, TotalHealth +  amount);
    }

    public void DecreaseHealth(float amount)
    {
        if (IsDead) return;
        
        if (Time.time - lastDamageTime < damageCooldown)
            return;
            
        lastDamageTime = Time.time;
        TotalHealth -= amount;

        if (TotalHealth <= 0)
        {
            IsDead = true;
            ObjectHolder objHolder = FindAnyObjectByType<ObjectHolder>();
            SceneManager.LoadScene("GameOverScene", LoadSceneMode.Additive);
            objHolder?.endGame();
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
