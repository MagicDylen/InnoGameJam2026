using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    public static GameObject EnemyPrefab;
    public static GameObject Player;

    [SerializeField] private GameObject enemyPrefab;

    private void Start()
    {
        EnemyPrefab = enemyPrefab;
        Player = FindAnyObjectByType<PlayerController>().gameObject;
    }
}