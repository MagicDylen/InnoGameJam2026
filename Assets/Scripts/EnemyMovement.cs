using Unity.Mathematics;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float movementSpeed = 1f;
    Rigidbody2D rb;

    void Start()
    {
        TryGetComponent<EnemyStats>(out var stats);
        if(stats)
        {
            movementSpeed = stats.Speed;
        }
        TryGetComponent<Rigidbody2D>(out rb);
    }

    private void FixedUpdate()
    {
        Vector3 direction = ObjectHolder.Player.transform.position - this.transform.position;
        if(rb)
        {
            rb.linearVelocity = direction.normalized * movementSpeed;
            // masks should not be used like elevators, so if the mask is below the player, the y direction should be 0.
            rb.linearVelocityY = math.min(0, rb.linearVelocityY);
        } else
        {
            this.transform.position += direction.normalized * (Time.fixedDeltaTime * movementSpeed);
        }
    }
}
