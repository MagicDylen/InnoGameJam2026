using System;
using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter emitter;
    DynamicEnemySpawner spawner;
    int currentIntensity = 0;

    [Header("Player Sounds")]
    [SerializeField] public FMODUnity.EventReference PlayerDeath;
    [SerializeField] public FMODUnity.EventReference PlayerJump;
    [SerializeField] public FMODUnity.EventReference PlayerDamage;
    [SerializeField] public FMODUnity.EventReference PlayerPotion;
    [SerializeField] public FMODUnity.EventReference PlayerSlash;

    [Header("Enemy Sounds")]
    [SerializeField] public FMODUnity.EventReference EnemyHit;
    [SerializeField] public FMODUnity.EventReference EnemyDeath;

    [Header("UI Sounds")]
    [SerializeField] public FMODUnity.EventReference UiClick;
    [SerializeField] public FMODUnity.EventReference UiSelect;

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
            // make sure music is always playing
            if(!emitter.IsPlaying()) emitter.Play();
        }
    }

    public void PlayOneShot(FMODUnity.EventReference reference, Vector3 position)
    {
        FMODUnity.RuntimeManager.PlayOneShot(reference, position);
    }
}
