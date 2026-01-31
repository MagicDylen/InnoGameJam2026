using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Analytics;

public class ColorComboEffect : MonoBehaviour
{

    public enum MaskType
    {
        Inactive = 0,
        Spike, 
        Explosive,
        Standard,
        Sticky,
    }

    Renderer ren;
    public MaskType AssignedType = MaskType.Inactive;
    public float ExplosionRadius = 3;
    public float ExplosionForce = 300;
    private bool triggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ren = gameObject.GetComponent<Renderer>();
        if(AssignedType == MaskType.Inactive) AsssignRandomComboColor();
    }

    void AsssignRandomComboColor()
    {
        var v = System.Enum.GetValues (typeof (MaskType));
        AssignedType = (MaskType) v.GetValue((int)Random.Range(1, v.Length));
        SetRenderColor();
    }

    public void SetRenderColor()
    {
        Color randColor = AssignedType switch {
            MaskType.Spike => Color.yellow,
            MaskType.Explosive => Color.red,
            MaskType.Standard => Color.blue,
            MaskType.Sticky => Color.green,
            MaskType.Inactive => Color.black,
            _ => Color.gray,
        };
        if(!ren) return;
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
        
        if(otherCombo && otherCombo.AssignedType == AssignedType)
        {
            TriggerCollisionEffect(gameObject.transform.position);
            SetRenderColor();
            otherCombo.SetRenderColor();
        }
    }

    private void TriggerCollisionEffect(Vector2 origin)
    {
        if(triggered) return;
        triggered = true;

        if (AssignedType == MaskType.Explosive)
        {
            Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            var layerMask = LayerMask.GetMask("Ground");

            var inRange = Physics2D.OverlapCircleAll(pos, ExplosionRadius, layerMask);
            Debug.Log($"With {inRange.Length} Boom");
            foreach (var mask in inRange)
            {
                mask.gameObject.TryGetComponent<ColorComboEffect>(out var otherCombo);
                if(!otherCombo)
                {
                    continue;
                }
                otherCombo.TriggerCollisionEffect(pos);
                Destroy(gameObject);
            }
        } 
        else if (AssignedType == MaskType.Standard)
        {
            Destroy(gameObject);
        }
        
        else // all other types
        {
            var rb = gameObject.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(((Vector2)gameObject.transform.position - origin) * ExplosionForce, ForceMode2D.Impulse);
            gameObject.TryGetComponent<BecomeStaticAfterSeconds>(out var staticComponent);
            if(staticComponent)
            {
                staticComponent.BecomeStaticWithDelay(10f);
            }
        }
    }
}
