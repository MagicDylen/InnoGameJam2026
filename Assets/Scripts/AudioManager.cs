using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter emitter;
    DynamicEnemySpawner spawner;
    int currentIntensity = 0;

    void Awake()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.SetParameter("Music", currentIntensity);
                
    }

    void Update()
    {
        if(!spawner)
        {
            spawner = FindFirstObjectByType<DynamicEnemySpawner>();
            if(!spawner) return;
        }
        if (emitter != null)
        {
            // max is 8 for the parameter, progress is between 0 and 1, 1 being done
            int intensity = math.min((int)(spawner.GetProgress01() * 9), 8);
            if(intensity != currentIntensity)
            {
                currentIntensity = intensity;
                emitter.SetParameter("Music", intensity);
            }
        }
    }
}
