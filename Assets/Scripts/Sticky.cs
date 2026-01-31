using System.Collections;
using UnityEngine;

public class Sticky : MonoBehaviour
{

    public float Delay = 0.7f;
    bool stickyStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (stickyStarted)
        {
            return;
        }
        coll.gameObject.TryGetComponent<Sticky>(out var otherSticky);
        if(otherSticky)
        {
            StartCoroutine("GetSticky");
        }
    }

    IEnumerator GetSticky()
    {
        yield return new WaitForSeconds(Delay);
        Destroy(gameObject.GetComponent<Rigidbody2D>());
    }
}
