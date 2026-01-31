using UnityEngine;

public class BecomeStaticAfterSeconds : MonoBehaviour
{
    [SerializeField] public float SecondsBeforeStatic = 2f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void Start()
    {
        //Invoke(nameof(MakeStatic), secondsBeforeStatic);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
           BecomeStaticWithDelay(SecondsBeforeStatic);
        }
    }

    public void BecomeStaticWithDelay(float delay)
    {
        Invoke(nameof(MakeStatic), delay);
    }

    void MakeStatic()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
    }
}
