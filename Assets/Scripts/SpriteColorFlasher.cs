using UnityEngine;

public class SpriteColorFlasher : MonoBehaviour
{
    [Header("Target")]
    public SpriteRenderer spriteRenderer;

    [Header("Rainbow Chaos")]
    public Color[] colors;
    public float flashSpeed = 10f; // colors per second

    private int currentIndex = 0;
    private float timer = 0f;

    void Update()
    {
        if (spriteRenderer == null || colors == null || colors.Length == 0)
            return;

        timer += Time.deltaTime * flashSpeed;

        if (timer >= 1f)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % colors.Length;
            spriteRenderer.color = colors[currentIndex];
        }
    }
}
