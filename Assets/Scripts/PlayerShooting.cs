
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireCooldown = 0.3f;

    private float nextFireTime;
    private int facingDirection = 1;

    public void OnMove(InputValue value)
    {
        float horizontal = value.Get<Vector2>().x;

        if (horizontal > 0.01f)
            facingDirection = 1;
        else if (horizontal < -0.01f)
            facingDirection = -1;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (Time.time < nextFireTime)
            return;

        Shoot();
        nextFireTime = Time.time + fireCooldown;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Vector3 spawnPosition = firePoint.position;
        spawnPosition.x = transform.position.x +
                          facingDirection * 0.8f;

        GameObject newBullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.identity
        );
        GetComponent<PlayerAnimation>()?.PlayShot(facingDirection);

        Collider2D bulletCollider =
            newBullet.GetComponent<Collider2D>();

        Collider2D playerCollider =
            GetComponent<Collider2D>();

        if (bulletCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(
                bulletCollider,
                playerCollider
            );
        }

        Bullet bullet = newBullet.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDirection(
                facingDirection == 1
                ? Vector2.right
                : Vector2.left
            );
        }
    }
}
