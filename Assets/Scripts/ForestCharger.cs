using UnityEngine;

public class ForestCharger : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            triggerHeight = stats.triggerHeight;
            dashLength = stats.dashLength;
            dashSpeed = stats.dashSpeed;
            hitRange = stats.hitRange;
            cooldown = stats.cooldown;
            windUp = stats.windUp;
            damage = stats.damage;
        }
    }

    public ForestChargerStats stats;
    enum State { Idle, WindUp, Dash, Stunned }
    public Transform leftPoint, rightPoint;
    [HideInInspector] public float triggerHeight;
    [HideInInspector] public float dashLength;
    [HideInInspector] public float dashSpeed;
    [HideInInspector] public float hitRange;
    [HideInInspector] public float cooldown;
    [HideInInspector] public float windUp;
    [HideInInspector] public int damage;

    private EnemySight sight;
    private State state;
    private float timer, targetX;

    void Start() { sight = GetComponent<EnemySight>(); }

    void SetState(State next)
    {
        state = next;
        timer = 0;
        sight.visual.color = next == State.WindUp ? sight.warningColor :
            next == State.Stunned ? sight.restColor : next == State.Dash ? Color.yellow : Color.white;
    }

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                sight.Face(sight.Offset.x);
                if (sight.OnScreen && Mathf.Abs(sight.Offset.y) <= triggerHeight && sight.CanSee())
                {
                    targetX = Mathf.Clamp(transform.position.x + sight.direction * dashLength,
                        leftPoint.position.x, rightPoint.position.x);
                    SetState(State.WindUp);
                }
                break;
            case State.WindUp:
                if (!sight.OnScreen) SetState(State.Stunned);
                else if (timer >= windUp) SetState(State.Dash);
                break;
            case State.Dash:
                if (!sight.OnScreen) { SetState(State.Stunned); break; }
                float step = Mathf.Min(dashSpeed * Time.deltaTime, Mathf.Abs(targetX - transform.position.x));
                if (Physics2D.Raycast(transform.position, Vector2.right * sight.direction, step, sight.walls))
                { SetState(State.Stunned); break; }
                sight.MoveX(targetX, dashSpeed);
                if (sight.Offset.magnitude <= hitRange && sight.ClearPath())
                {
                    sight.player.TakeDamage(damage);
                    SetState(State.Stunned);
                }
                else if (Mathf.Approximately(transform.position.x, targetX)) SetState(State.Stunned);
                break;
            case State.Stunned:
                if (timer >= cooldown) SetState(State.Idle);
                break;
        }
    }
}
