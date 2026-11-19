
using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            damage = stats.damage;
        }
    }

    public SpikeDamageStats stats;
    [HideInInspector] public int damage;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
