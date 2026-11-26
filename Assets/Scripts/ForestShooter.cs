using UnityEngine;

public class ForestShooter : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            moveSpeed = stats.moveSpeed;
            preferredDistance = stats.preferredDistance;
            moveTime = stats.moveTime;
            aimTime = stats.aimTime;
            cooldown = stats.cooldown;
            shotSpeed = stats.shotSpeed;
            shotLife = stats.shotLife;
            shotScale = stats.shotScale;
            damage = stats.damage;
        }
    }

    public ForestShooterStats stats;
    enum State { Idle, KeepDistance, Aim, Rest }
    public Transform leftPoint, rightPoint;
    public Sprite shotSprite;
    public LineRenderer aimLine;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float preferredDistance;
    [HideInInspector] public float moveTime;
    [HideInInspector] public float aimTime;
    [HideInInspector] public float cooldown;
    [HideInInspector] public float shotSpeed;
    [HideInInspector] public float shotLife;
    [HideInInspector] public float shotScale;
    [HideInInspector] public int damage;

    private EnemySight sight;
    private State state;
    private float timer;
    private Vector2 shotDirection;

    void Start() { sight = GetComponent<EnemySight>(); }

    void SetState(State next)
    {
        state = next;
        timer = 0;
        aimLine.enabled = next == State.Aim;
        if (next == State.Aim) SoundManager.Play(Sfx.WindupShooter);
        sight.visual.color = next == State.Aim ? sight.warningColor :
            next == State.Rest ? sight.restColor : Color.white;
        if (next == State.Aim)
        {
            // Запоминаем цель: во время предупреждения ещё можно увернуться.
            shotDirection = sight.Offset.normalized;
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, (Vector2)transform.position + shotDirection * sight.viewRange);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                sight.Face(sight.Offset.x);
                if (sight.OnScreen && sight.CanSee()) SetState(State.KeepDistance);
                break;
            case State.KeepDistance:
                float goal = sight.player.transform.position.x - Mathf.Sign(sight.Offset.x) * preferredDistance;
                sight.MoveX(Mathf.Clamp(goal, leftPoint.position.x, rightPoint.position.x), moveSpeed);
                sight.Face(sight.Offset.x);
                if (!sight.OnScreen || !sight.CanSee()) SetState(State.Idle);
                else if (timer >= moveTime) SetState(State.Aim);
                break;
            case State.Aim:
                if (!sight.OnScreen) { SetState(State.Rest); break; }
                if (timer < aimTime) break;
                var shot = new GameObject("Spore");
                shot.transform.position = transform.position;
                var sprite = shot.AddComponent<SpriteRenderer>();
                sprite.sprite = shotSprite;
                sprite.color = Color.yellow;
                sprite.sortingOrder = sight.visual.sortingOrder + 1;
                shot.transform.localScale = Vector3.one * shotScale / shotSprite.bounds.size.x;
                shot.AddComponent<ForestSpore>().Launch(sight, shotDirection, shotSpeed, shotLife, damage);
                SetState(State.Rest);
                break;
            case State.Rest:
                if (timer >= cooldown) SetState(State.Idle);
                break;
        }
    }
}
