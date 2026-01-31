using UnityEngine;

public class SpawnerAtPosition : MonoBehaviour
{
    [Header("Spawn Setup")]
    public Transform spawnPoint;
    public GameObject[] prefabs;

    [Header("Timing")]
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {
        if (spawnPoint == null)
        {
            Debug.LogError($"{name}: SpawnPoint is not assigned!");
            return;
        }

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError($"{name}: No prefabs assigned!");
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandom();
            timer = 0f;
        }
    }

    void SpawnRandom()
    {
        int index = Random.Range(0, prefabs.Length);
        Instantiate(prefabs[index], spawnPoint.position, spawnPoint.rotation);
    }
}
