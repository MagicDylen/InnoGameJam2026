using UnityEngine;

public class SelfDestruction : MonoBehaviour
{
    [SerializeField] private float selfDestructTimer = 1f;

    private void Start()
    {
        Destroy(gameObject, selfDestructTimer);
    }
}