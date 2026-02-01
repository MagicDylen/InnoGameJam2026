using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Do(float duration, float timeScale)
    {
        if (instance == null)
        {
            // Auto-create if missing, but loudly.
            var go = new GameObject("HitStop");
            instance = go.AddComponent<HitStop>();
            DontDestroyOnLoad(go);
            Debug.LogWarning("HitStop object was missing. Auto-created one.");
        }

        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.HitStopRoutine(duration, timeScale));
    }

    private IEnumerator HitStopRoutine(float duration, float timeScale)
    {
        float original = Time.timeScale;
        Time.timeScale = timeScale;

        // use unscaled time so it still waits while timeScale is tiny
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = original;
    }
}
