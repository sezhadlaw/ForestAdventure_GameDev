
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 18f;
    public float lifetime = 3f;
    public int damage = 1;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private Collider2D hitbox;
    private bool spent;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[32];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
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
        if (spent) return;
        float distance = speed * Time.fixedDeltaTime;
        if (hitbox != null)
        {
            var filter = new ContactFilter2D();
            filter.NoFilter();
            filter.useTriggers = true;
            int count = hitbox.Cast(direction, filter, hits, distance);
            for (int i = 0; i < count; i++)
                if (TryHit(hits[i].collider)) return;
        }
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
        TryHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision.collider);
    }

    bool TryHit(Collider2D other)
    {
        if (spent || other == null) return false;
        BossHealth boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            spent = true;
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
