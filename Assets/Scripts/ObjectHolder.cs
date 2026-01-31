using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHolder : MonoBehaviour
{
    public static GameObject AttackPrefab;
    public static GameObject EnemyPrefab;
    public static GameObject Player;
    public static InputActionMap ActionMap;

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private InputActionAsset inputActions;                                                                                                                                                                                      

    private void Start()
    {
        Player = FindAnyObjectByType<PlayerController>().gameObject;
        EnemyPrefab = enemyPrefab;
        AttackPrefab = attackPrefab;
        ActionMap = inputActions.FindActionMap("Player");
    }
}