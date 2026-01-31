using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameObject Player;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject world;

    void Start()
    {
        Instantiate(world, transform.position - new Vector3(0, 5), Quaternion.identity);
        Player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
    }
}
