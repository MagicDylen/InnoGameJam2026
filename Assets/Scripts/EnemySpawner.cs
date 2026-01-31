using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private readonly float heightMax = 6f;
    private readonly float heightBorder = 5f;
    private readonly float heightMin = 0f;
    private readonly float widthMax = 13f;
    private readonly float widthBorder = 11f;
    private readonly float widthMin = 0f;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = DetermineRandomPosition();
        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        EnemyMovement enemyMovement = enemyInstance.GetComponent<EnemyMovement>();
        enemyMovement.SetTarget(GameController.Player);
    }

    private Vector2 DetermineRandomPosition()
    {
        float minWidth = Random.value > 0.5f ? widthMin : 0;
        float x = Random.Range(minWidth, widthMax);
        x *= Random.value > 0.5f ? 1 : -1;

        float minHeight = x > widthBorder ? heightMin : heightBorder;
        float y = Random.Range(minHeight, heightMax);

        return new Vector2(x, y);
    }
}