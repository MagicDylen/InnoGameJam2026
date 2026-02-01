using UnityEngine;

public class EnemyTestScript : MonoBehaviour
{
    public GameObject enemy;
    public float distance = 1f;
    public float speed = 2f;

    private Vector3 _startPos;

    void Start()
    {
        enemy = GameObject.Find("Enemy");
        _startPos = enemy.transform.position;
    }

    void Update()
    {
        enemy.transform.position = _startPos + Vector3.right * Mathf.Sin(Time.time * speed) * distance;
    }
}
