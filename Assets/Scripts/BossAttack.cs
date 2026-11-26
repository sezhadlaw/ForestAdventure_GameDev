
using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            attackInterval = stats.attackInterval;
        }
    }

    public BossAttackStats stats;
    public GameObject bossBulletPrefab;
    public Transform player;
    [HideInInspector] public float attackInterval;

    private Coroutine attackRoutine;
    private SpriteRenderer visual;
    private Color normalColor = Color.white;

    // The wind-up sound and a red tint start this long before every shot, so the player hears the attack first.
    public const float WindUpTime = 0.6f;
    static readonly Color WarningColor = new Color(1f, 0.35f, 0.25f);

    void OnEnable()
    {
        if (visual == null)
        {
            visual = GetComponentInChildren<SpriteRenderer>();
            if (visual != null) normalColor = visual.color;
        }
        attackRoutine = StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
        if (visual != null) visual.color = normalColor;
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(stats.initialDelay);

        while (true)
        {
            // Wind-up: sound and tint first, the shot comes WindUpTime later.
            SoundManager.Play(Sfx.WindupBoss);
            if (visual != null) visual.color = WarningColor;
            yield return new WaitForSeconds(WindUpTime);

            if (visual != null) visual.color = normalColor;
            Shoot();

            yield return new WaitForSeconds(Mathf.Max(0.1f, attackInterval - WindUpTime));
        }
    }

    void Shoot()
    {
        if (bossBulletPrefab == null || player == null)
            return;

        Vector3 spawnPosition = transform.position;

        // Снаряд появляется со стороны игрока
        float side = player.position.x < transform.position.x
            ? -1f : 1f;

        spawnPosition.x += side * stats.spawnOffset;

        GameObject newBullet = Instantiate(
            bossBulletPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // Не даём снаряду задевать самого босса
        Collider2D bulletCollider =
            newBullet.GetComponent<Collider2D>();

        Collider2D bossCollider =
            GetComponent<Collider2D>();

        if (bulletCollider != null && bossCollider != null)
        {
            Physics2D.IgnoreCollision(
                bulletCollider,
                bossCollider
            );
        }

        BossBullet bullet = newBullet.GetComponent<BossBullet>();

        if (bullet != null)
        {
            Vector2 direction =
                (player.position - spawnPosition).normalized;

            bullet.SetDirection(direction);
        }
    }
}
