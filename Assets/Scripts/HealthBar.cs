using UnityEngine;

public class HealthBar : MonoBehaviour
{
    UnityEngine.UI.RawImage image;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<UnityEngine.UI.RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        // get player stats and current max health divided by total health
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        float healthPercent = (float)playerStats.TotalHealth / (float)playerStats.MaxHealth;

        transform.localScale = new Vector3(healthPercent, 1, 1);
        image.uvRect = new Rect(0, 0, healthPercent, 1);
        transform.localPosition = new Vector3(-((1 - healthPercent) * 0.5f * 168) + 18.5f, transform.localPosition.y, transform.localPosition.z);
    }
}
