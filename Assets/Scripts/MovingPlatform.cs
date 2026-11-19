
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            moveSpeed = stats.moveSpeed;
            moveDistance = stats.moveDistance;
        }
    }

    public MovingPlatformStats stats;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float moveDistance;

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
