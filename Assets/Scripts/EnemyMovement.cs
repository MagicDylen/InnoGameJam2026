using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public int movementSpeed = 1;

    private void FixedUpdate()
    {
        Vector3 direction = ObjectHolder.Player.transform.position - this.transform.position;
        this.transform.position += direction.normalized * (Time.fixedDeltaTime * movementSpeed);
    }
}
