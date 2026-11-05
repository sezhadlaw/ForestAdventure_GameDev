
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float moveDistance = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float offset = Mathf.PingPong(
            Time.time * moveSpeed,
            moveDistance * 2f
        ) - moveDistance;

        transform.position = startPosition +
         Vector3.right * offset; 
    }
}
