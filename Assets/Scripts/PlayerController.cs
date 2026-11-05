
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 14f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 movement;
    public float HorizontalInput => movement.x;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        if (GetComponent<PlayerAnimation>() == null) gameObject.AddComponent<PlayerAnimation>();
    }

    // Получаем движение от клавиатуры или джойстика
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    // Прыжок
    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        bool isGrounded =
            playerCollider.IsTouchingLayers(groundLayer);

        if (isGrounded)
        {
            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            movement.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
}
