using UnityEngine;

// Animate only the visual child: recoil must never move the physics collider.
public class PlayerAnimation : MonoBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private SpriteRenderer visual;
    private Vector3 restPosition;
    private Vector3 restScale;
    private float shotRemaining;
    private int facing = 1;
    private int currentState;
    private static readonly int Idle = Animator.StringToHash("Player_Idle");
    private static readonly int Run = Animator.StringToHash("Player_Run");

    void Start()
    {
        controller = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
        visual = animator != null ? animator.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
        if (visual != null)
        {
            restPosition = visual.transform.localPosition;
            restScale = visual.transform.localScale;
        }
    }

    public void PlayShot(int direction)
    {
        facing = direction;
        shotRemaining = 0.18f;
    }

    void LateUpdate()
    {
        if (visual == null) return;
        float movement = controller.HorizontalInput;
        if (Mathf.Abs(movement) > 0.01f) facing = movement > 0 ? 1 : -1;
        visual.flipX = facing < 0;
        int state = Mathf.Abs(movement) > 0.01f ? Run : Idle;
        if (animator != null && animator.runtimeAnimatorController != null && currentState != state && animator.HasState(0, state))
        {
            animator.Play(state);
            currentState = state;
        }
        shotRemaining = Mathf.Max(0, shotRemaining - Time.deltaTime);
        float recoil = Mathf.Sin(shotRemaining / 0.18f * Mathf.PI);
        if (visual.transform != transform)
        {
            visual.transform.localPosition = restPosition + Vector3.left * (facing * recoil * 0.09f);
            visual.transform.localScale = Vector3.Scale(restScale, new Vector3(1 + recoil * 0.12f, 1 - recoil * 0.08f, 1));
        }
    }
}
