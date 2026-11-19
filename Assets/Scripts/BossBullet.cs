
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public BossBulletStats stats;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;
    [HideInInspector] public int damage;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        if (stats != null)
        {
            speed = stats.speed;
            lifetime = stats.lifetime;
            damage = stats.damage;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(
                rb.position +
                direction * speed * Time.fixedDeltaTime
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
