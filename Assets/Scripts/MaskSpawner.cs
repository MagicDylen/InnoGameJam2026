using System.Collections;
using UnityEngine;



public class MaskSpawner : MonoBehaviour
{
    public GameObject[] MaskShapes;
    public float SpawnRadius;
    public float SpawnInterval;
    public float stickyDelay;

    IEnumerator SpawnMasks()
    {
        while(true)
        {            
            yield return new WaitForSeconds(SpawnInterval);
            var offset = (Random.value > 0.5 ? -1 : 1) * new Vector3(Random.value * SpawnRadius, Random.value * SpawnRadius);
            Vector3 spawnPos = gameObject.transform.position + offset;
            int index = (int)(Random.value * MaskShapes.Length);

            var mask = Instantiate(MaskShapes[index], spawnPos, Quaternion.Euler(0, 0, Random.value * 360));
            mask.AddComponent<Sticky>();
            var stickyComponent = mask.GetComponent<Sticky>();
            stickyComponent.Delay = stickyDelay;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("SpawnMasks");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
