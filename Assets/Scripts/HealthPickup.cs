
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            healAmount = stats.healAmount;
        }
    }

    public HealthPickupStats stats;
    [HideInInspector] public int healAmount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            bool healed = playerHealth.Heal(healAmount);

            if (healed)
            {
                Destroy(gameObject);
            }
        }
    }
}
