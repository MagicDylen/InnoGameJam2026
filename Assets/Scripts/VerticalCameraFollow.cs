using UnityEngine;

public class VerticalCameraFollow : MonoBehaviour
{
    public float yOffset = 0f;
    public float smoothSpeed = 5f;

    public float minY = 0f; // lowest Y the camera is allowed to reach

    Transform player;

    void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p == null)
        {
            Debug.LogError("VerticalCameraFollow: No GameObject with tag 'Player' found.");
            enabled = false;
            return;
        }

        player = p.transform;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        float targetY = player.position.y + yOffset;
        float smoothedY = Mathf.Lerp(pos.y, targetY, smoothSpeed * Time.deltaTime);

        pos.y = Mathf.Max(smoothedY, minY);

        transform.position = pos;
    }

}
