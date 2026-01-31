using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float movementSpeed = 1f;

    void Start()
    {
        TryGetComponent<EnemyStats>(out var stats);
        if(stats)
        {
            movementSpeed = stats.Speed;
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = ObjectHolder.Player.transform.position - this.transform.position;
        this.transform.position += direction.normalized * (Time.fixedDeltaTime * movementSpeed);
    }
}
