
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerControllerStats stats;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float jumpForce;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Vector2 movement;
    private bool wasGrounded = true;
    private float fallSpeed;
    public float HorizontalInput => movement.x;

    void Awake()
    {
        if (stats != null)
        {
            moveSpeed = stats.moveSpeed;
            jumpForce = stats.jumpForce;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
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
            SoundManager.Play(Sfx.Jump);
        }
    }

    void FixedUpdate()
    {
        // Land: was in the air and falling, now touches the ground.
        bool grounded = playerCollider.IsTouchingLayers(groundLayer);
        if (grounded && !wasGrounded && fallSpeed < -3f) SoundManager.Play(Sfx.Land, 0.6f);
        wasGrounded = grounded;
        fallSpeed = rb.linearVelocity.y;

        rb.linearVelocity = new Vector2(
            movement.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
}
