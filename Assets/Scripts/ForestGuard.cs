using UnityEngine;

public class ForestGuard : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            patrolSpeed = stats.patrolSpeed;
            chaseSpeed = stats.chaseSpeed;
            hitRange = stats.hitRange;
            loseTime = stats.loseTime;
            windUp = stats.windUp;
            cooldown = stats.cooldown;
            damage = stats.damage;
        }
    }

    public ForestGuardStats stats;
    enum State { Patrol, Chase, WindUp, Rest }
    public Transform leftPoint, rightPoint;
    [HideInInspector] public float patrolSpeed;
    [HideInInspector] public float chaseSpeed;
    [HideInInspector] public float hitRange;
    [HideInInspector] public float loseTime;
    [HideInInspector] public float windUp;
    [HideInInspector] public float cooldown;
    [HideInInspector] public int damage;

    private EnemySight sight;
    private State state;
    private float timer, lost;
    private bool goingRight = true;

    void Start() { sight = GetComponent<EnemySight>(); }

    void SetState(State next)
    {
        state = next;
        timer = 0;
        lost = 0;
        if (next == State.WindUp) SoundManager.Play(Sfx.WindupGuard);
        sight.visual.color = next == State.WindUp ? sight.warningColor :
            next == State.Rest ? sight.restColor : next == State.Chase ? Color.yellow : Color.white;
    }

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Patrol:
                float goal = (goingRight ? rightPoint : leftPoint).position.x;
                sight.MoveX(goal, patrolSpeed);
                if (Mathf.Approximately(transform.position.x, goal)) goingRight = !goingRight;
                if (sight.OnScreen && sight.CanSee()) SetState(State.Chase);
                break;
            case State.Chase:
                lost = sight.CanSee() ? 0 : lost + Time.deltaTime;
                if (!sight.OnScreen || lost >= loseTime) { SetState(State.Patrol); break; }
                if (sight.Offset.magnitude <= hitRange && sight.ClearPath()) SetState(State.WindUp);
                else sight.MoveX(Mathf.Clamp(sight.player.transform.position.x,
                    leftPoint.position.x, rightPoint.position.x), chaseSpeed);
                break;
            case State.WindUp:
                if (!sight.OnScreen) { SetState(State.Rest); break; }
                if (timer < windUp) break;
                if (sight.Offset.magnitude <= hitRange && sight.ClearPath()) sight.player.TakeDamage(damage);
                SetState(State.Rest);
                break;
            case State.Rest:
                if (timer >= cooldown) SetState(State.Patrol);
                break;
        }
    }
}
