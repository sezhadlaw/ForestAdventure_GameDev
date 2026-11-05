
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Slider healthBar;
    public WinScreen winScreen;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage, 0
        );

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Boss defeated!");

        if (winScreen != null)
            winScreen.ShowWin();

        Destroy(gameObject);
    }
}
