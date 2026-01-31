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

    public GameObject particlesExplosionPrefab;
    public GameObject damageParticlesPrefab;
    public GameObject littleParticlesPrefab;
    public GameObject enemyKillParticlesPrefab;


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
        collision.gameObject.TryGetComponent<ColorComboEffect>(out var otherCombo);
        collision.gameObject.TryGetComponent<Rigidbody2D>(out var otherRb);
        gameObject.TryGetComponent<Rigidbody2D>(out var rb);

        if (!otherCombo || otherCombo.enabled == false) return;
        
        if(otherCombo && otherCombo.AssignedType == AssignedType)
        {
            TriggerCollisionEffect(gameObject.transform.position);
            SetRenderColor();
            otherCombo.SetRenderColor();
        }
        else if(AssignedType == MaskType.Sticky && (otherCombo.AssignedType == MaskType.Standard || otherCombo.AssignedType == MaskType.Sticky))
        {
            if(collision.gameObject.transform.parent == null)
            {
                collision.gameObject.transform.parent = gameObject.transform;
                otherRb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    private void TriggerCollisionEffect(Vector2 origin)
    {
        if(triggered) return;
        if (AssignedType == MaskType.Explosive)
        {
            triggered = true;
            Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            var layerMask = LayerMask.GetMask("Ground");

            var inRange = Physics2D.OverlapCircleAll(pos, ExplosionRadius, layerMask);
            foreach (var mask in inRange)
            {
                mask.gameObject.TryGetComponent<ColorComboEffect>(out var otherCombo);
                if(!otherCombo)
                {
                    continue;
                }
                switch (otherCombo.AssignedType)
                {
                    case MaskType.Explosive:
                        otherCombo.TriggerCollisionEffect(pos);
                        break;
                    case MaskType.Standard:
                        // unparent first
                        if(mask.transform.parent != null)
                        {
                            mask.transform.parent = null;
                        } 
                        Destroy(mask.gameObject);
                        Instantiate(littleParticlesPrefab, gameObject.transform.position, Quaternion.identity);

                        break;
                    case MaskType.Sticky:
                        var rb = mask.gameObject.GetComponent<Rigidbody2D>();
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.AddForce(((Vector2)mask.gameObject.transform.position - origin) * ExplosionForce, ForceMode2D.Impulse);
                        mask.gameObject.TryGetComponent<BecomeStaticAfterSeconds>(out var staticComponent);
                        if(staticComponent)
                        {
                            staticComponent.BecomeStaticWithDelay(10f);
                        }
                        break;
                }

                // Instantiate Explosion Particles
                Instantiate(particlesExplosionPrefab, gameObject.transform.position, Quaternion.identity);

                Destroy(gameObject);
              
            }
        } 
    }
}
