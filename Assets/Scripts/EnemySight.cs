using UnityEngine;

public class EnemySight : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            viewRange = stats.viewRange;
            viewAngle = stats.viewAngle;
            checkInterval = stats.checkInterval;
        }
    }

    public EnemySightStats stats;
    public PlayerHealth player;
    public SpriteRenderer visual;
    public Camera gameCamera;
    public LayerMask walls;
    [HideInInspector] public float viewRange;
    [HideInInspector] public float viewAngle;
    [HideInInspector] public float checkInterval;
    public int direction = 1;
    public Color warningColor = new Color(1f, 0.25f, 0.12f);
    public Color restColor = new Color(0.55f, 0.75f, 1f);

    private float nextCheck;
    private bool seen;

    public Vector2 Offset => player.transform.position - transform.position;
    public bool OnScreen
    {
        get
        {
            Vector3 p = gameCamera.WorldToViewportPoint(transform.position);
            return p.z > 0 && p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1;
        }
    }

    public void Face(float x)
    {
        if (x == 0) return;
        direction = x > 0 ? 1 : -1;
        visual.flipX = direction < 0;
    }

    public bool ClearPath()
    {
        Vector2 delta = Offset;
        return !Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, walls);
    }

    // Сначала расстояние и угол, только потом луч до игрока.
    public bool CanSee()
    {
        if (Time.time < nextCheck) return seen;
        nextCheck = Time.time + checkInterval;
        Vector2 delta = Offset;
        seen = delta.magnitude <= viewRange &&
            Vector2.Angle(Vector2.right * direction, delta) <= viewAngle / 2 && ClearPath();
        Debug.DrawRay(transform.position, delta, seen ? Color.green : Color.red, checkInterval);
        return seen;
    }

    public void MoveX(float x, float speed)
    {
        float next = Mathf.MoveTowards(transform.position.x, x, speed * Time.deltaTime);
        Face(next - transform.position.x);
        Vector3 p = transform.position;
        p.x = next;
        transform.position = p;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 forward = Vector3.right * direction;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);
        foreach (int side in new[] { -1, 1 })
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, side * viewAngle / 2) * forward * viewRange);
    }
}
