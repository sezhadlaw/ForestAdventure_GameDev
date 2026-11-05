
using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public GameObject bossBulletPrefab;
    public Transform player;
    public float attackInterval = 2f;

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
        yield return new WaitForSeconds(1f);

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

        spawnPosition.x += side * 1.5f;

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
