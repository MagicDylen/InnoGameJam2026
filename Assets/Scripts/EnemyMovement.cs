using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public int movementSpeed;
    private GameObject _target;

    public void SetTarget(GameObject target)
    {
        _target = target;
    }

    private void FixedUpdate()
    {
        Vector3 direction = _target.transform.position - this.transform.position;
        this.transform.position += direction.normalized * (Time.fixedDeltaTime * movementSpeed);
    }
}
