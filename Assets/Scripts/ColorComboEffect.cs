using UnityEngine;

public class ColorComboEffect : MonoBehaviour
{

    public enum ComboColor
    {
        Inactive = 0,
        Yellow, 
        Red,
        Blue,
        Green,
    }

    Renderer ren;
    public ComboColor AssignedColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ren = gameObject.GetComponent<Renderer>();
        AsssignRandomComboColor();
        
    }

    void AsssignRandomComboColor()
    {
        var v = System.Enum.GetValues (typeof (ComboColor));
        AssignedColor = (ComboColor) v.GetValue((int)Random.Range(1, v.Length));
        SetRenderColor();
    }

    public void SetRenderColor()
    {
        Color randColor = AssignedColor switch {
            ComboColor.Yellow => Color.yellow,
            ComboColor.Red => Color.red,
            ComboColor.Blue => Color.blue,
            ComboColor.Green => Color.green,
            ComboColor.Inactive => Color.black,
            _ => Color.gray,
        };

        ren.material.SetColor("_Color", randColor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ColorComboEffect otherCombo;
        collision.gameObject.TryGetComponent<ColorComboEffect>(out otherCombo);
        Rigidbody2D otherRb;
        collision.gameObject.TryGetComponent<Rigidbody2D>(out otherRb);
        Rigidbody2D rb;
        gameObject.TryGetComponent<Rigidbody2D>(out rb);
        
        if(otherCombo && otherCombo.AssignedColor == AssignedColor) //&& rb.bodyType == RigidbodyType2D.Static && otherRb.bodyType == RigidbodyType2D.Static)
        {
            Debug.Log("crashed with another!");
            AssignedColor = ComboColor.Inactive;
            SetRenderColor();
            otherCombo.AssignedColor = ComboColor.Inactive;
            otherCombo.SetRenderColor();
        }
    }
}
