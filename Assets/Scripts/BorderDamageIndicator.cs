using UnityEngine;
using UnityEngine.UI;

public class PlayerBorderImage : MonoBehaviour
{
    private Image image;
    private PlayerStats playerStats;
    private Texture2D vignetteTexture;
    
    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 256;
    [SerializeField] private Color vignetteColor = Color.red;
    [SerializeField] private float borderSize = 0.3f;
    
    void Start()
    {
        image = GetComponent<Image>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        
        CreateBorderVignetteTexture();
    }
    
    void CreateBorderVignetteTexture()
    {
        vignetteTexture = new Texture2D(textureWidth, textureHeight);
        
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                // Normalize coordinates to 0-1
                float u = (float)x / textureWidth;
                float v = (float)y / textureHeight;
                
                // Calculate distance from each edge (0 at edge, 0.5 at center)
                float distLeft = u;
                float distRight = 1f - u;
                float distBottom = v;
                float distTop = 1f - v;
                
                // Find minimum distance to any edge
                float minDist = Mathf.Min(Mathf.Min(distLeft, distRight), Mathf.Min(distBottom, distTop));
                
                // Calculate alpha: 1 at edge, fading to 0 at borderSize distance
                float alpha = 1f - Mathf.Clamp01(minDist / borderSize);
                
                // Smooth the falloff (quadratic)
                alpha = alpha * alpha;
                
                Color pixel = vignetteColor;
                pixel.a = alpha;
                vignetteTexture.SetPixel(x, y, pixel);
            }
        }
        
        vignetteTexture.Apply();
        
        Sprite sprite = Sprite.Create(
            vignetteTexture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f)
        );
        image.sprite = sprite;
    }

    void Update()
    {
        float healthPercent = playerStats.TotalHealth / playerStats.MaxHealth;
        Debug.Log("Health Percent: " + healthPercent);
        
        Color col = image.color;
        float targetAlpha = 1f - healthPercent;
        col.a = Mathf.Lerp(col.a, targetAlpha, Time.deltaTime * 5f);
        image.color = col;
    }
}
