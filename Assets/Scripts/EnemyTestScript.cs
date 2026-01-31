using UnityEngine;

public class EnemyTestScript : MonoBehaviour
{
    private GameObject _enemy;
    public float distance = 1f;
    public float speed = 2f;

    private Vector3 _startPos;

    void Start()
    {
        _enemy = GameObject.Find("Enemy");
        _startPos = _enemy.transform.position;
    }

    void Update()
    {
        _enemy.transform.position = _startPos + Vector3.right * Mathf.Sin(Time.time * speed) * distance;
    }
}
