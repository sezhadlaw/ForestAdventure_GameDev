
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealth;
        }
    }

    public BossHealthStats stats;
    [HideInInspector] public int maxHealth;
    public Slider healthBar;
    public WinScreen winScreen;

    private int currentHealth;
    public int Health => currentHealth;

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

        SoundManager.Play(currentHealth > 0 ? Sfx.EnemyHit : Sfx.BossDeath);

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Boss defeated!");

        if (LevelLoader.Instance != null) LevelLoader.Instance.DefeatBoss();
        else if (winScreen != null) winScreen.ShowWin();

        Destroy(gameObject);
    }
}
