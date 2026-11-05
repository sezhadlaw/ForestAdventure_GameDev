
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDistance = 2f;
    public int damage = 1;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += Vector3.right *
                              direction *
                              moveSpeed *
                              Time.deltaTime;

        if (transform.position.x >=
            startPosition.x + patrolDistance)
        {
            direction = -1;
        }
        else if (transform.position.x <=
                 startPosition.x - patrolDistance)
        {
            direction = 1;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
