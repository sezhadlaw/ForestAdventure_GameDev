using UnityEngine;

public class ForestSpore : MonoBehaviour
{
    private EnemySight shooter;
    private Vector2 direction;
    private float speed;
    private int damage;
    private Collider2D playerBox;

    public void Launch(EnemySight owner, Vector2 aim, float flySpeed, float life, int hitDamage)
    {
        shooter = owner;
        direction = aim;
        speed = flySpeed;
        damage = hitDamage;
        playerBox = owner.player.GetComponent<Collider2D>();
        Destroy(gameObject, life);
    }

    void FixedUpdate()
    {
        if (!shooter || !shooter.OnScreen) { Destroy(gameObject); return; }
        Vector2 start = transform.position;
        float step = speed * Time.fixedDeltaTime;
        RaycastHit2D wall = Physics2D.Raycast(start, direction, step, shooter.walls);
        float travel = wall ? wall.distance : step;
        // Проверяем отрезок до стены, чтобы быстрый снаряд не пролетал сквозь игрока.
        Vector2 nearest = playerBox.ClosestPoint(start);
        float along = Mathf.Clamp(Vector2.Dot(nearest - start, direction), 0, travel);
        if (playerBox.OverlapPoint(start + direction * along) || playerBox.OverlapPoint(start + direction * travel))
        {
            shooter.player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (wall) Destroy(gameObject);
        else transform.position = start + direction * step;
    }
}
