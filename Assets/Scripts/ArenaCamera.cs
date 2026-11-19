using UnityEngine;

public class ArenaCamera : MonoBehaviour
{
    public float arenaWidth = 18f;
    public float minimumHeight = 5f;
    private Camera arena;

    void Awake() { arena = GetComponent<Camera>(); }

    void LateUpdate()
    {
        // На узком экране оставляем в кадре и старт, и выход.
        arena.orthographicSize = Mathf.Max(minimumHeight, arenaWidth / (2f * arena.aspect));
    }
}
