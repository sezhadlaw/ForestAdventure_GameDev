
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

    void OnEnable()
    {
        attackRoutine = StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(stats.initialDelay);

        while (true)
        {
            Shoot();

            yield return new WaitForSeconds(attackInterval);
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
