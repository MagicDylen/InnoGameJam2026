using UnityEngine;

public class TransformDeadEnemyToMask : MonoBehaviour
{
    EnemyStats stats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent<EnemyStats>(out stats);
    }

    // Update is called once per frame
    void Update()
    {
     if(!stats) return;
     if(stats.Healthpoints <= 0)
        {
            TryGetComponent<Rigidbody2D>(out var rb);
            if(!rb) return;
            rb.gravityScale = 1;
            TryGetComponent<EnemyMovement>(out var mover);
            if(mover) Destroy(mover);
            TryGetComponent<EnemyStats>(out var stats);
            if(stats) Destroy(stats);
            TryGetComponent<ColorComboEffect>(out var combo);
            combo.active = true;
            var ps = gameObject.GetComponentInChildren<ParticleSystem>();
            if(ps) Destroy(ps);
        }   
    }
}
