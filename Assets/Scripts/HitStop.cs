using UnityEngine;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;

    private float baseTimeScale = 1f;
    private float baseFixedDeltaTime;

    private float stopEndUnscaledTime = -1f;
    private float activeTimeScale = 1f;
    private bool isStopping = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        baseFixedDeltaTime = Time.fixedDeltaTime;
        baseTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
    }

    public static void Do(float duration, float timeScale)
    {
        if (duration <= 0f) return;

        if (instance == null)
        {
            var go = new GameObject("HitStop");
            instance = go.AddComponent<HitStop>();
            DontDestroyOnLoad(go);
            Debug.LogWarning("HitStop object was missing. Auto-created one.");
        }

        instance.ApplyStop(duration, timeScale);
    }

    private void ApplyStop(float duration, float timeScale)
    {
        float now = Time.unscaledTime;

        // If no stop is active, capture baseline once.
        if (!isStopping)
        {
            baseTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            baseFixedDeltaTime = Time.fixedDeltaTime;
            activeTimeScale = Mathf.Clamp01(timeScale);
            isStopping = true;
        }
        else
        {
            // While already stopping:
            // - extend end time
            // - keep the "most frozen" timescale (minimum)
            activeTimeScale = Mathf.Min(activeTimeScale, Mathf.Clamp01(timeScale));
        }

        stopEndUnscaledTime = Mathf.Max(stopEndUnscaledTime, now + duration);

        // Apply immediately
        Time.timeScale = activeTimeScale;

        // Keep physics step consistent
        // (if activeTimeScale == 0, physics won't tick anyway)
        Time.fixedDeltaTime = baseFixedDeltaTime * Mathf.Max(Time.timeScale, 0.0001f);
    }

    private void Update()
    {
        if (!isStopping) return;

        if (Time.unscaledTime >= stopEndUnscaledTime)
        {
            // Restore baseline (NOT "whatever it was mid-hitstop")
            Time.timeScale = baseTimeScale;
            Time.fixedDeltaTime = baseFixedDeltaTime;

            isStopping = false;
            stopEndUnscaledTime = -1f;
            activeTimeScale = 1f;
        }
    }
}
