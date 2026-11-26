
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    void Awake()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealth;
            invincibilityTime = stats.invincibilityTime;
        }
    }

    public PlayerHealthStats stats;
    [HideInInspector] public int maxHealth;
    [HideInInspector] public float invincibilityTime;

    public TextMeshProUGUI healthText;

    private int currentHealth;
    public int Health => currentHealth;
    private bool isInvincible;
    private SpriteRenderer[] spriteRenderers;
    private bool[] initialVisibility;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        initialVisibility = new bool[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            initialVisibility[i] = spriteRenderers[i].enabled;

        UpdateHealthUI();

        Debug.Log("Player HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0 || damage <= 0)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        SoundManager.Play(currentHealth > 0 ? Sfx.Hurt : Sfx.Death);

        UpdateHealthUI();

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageBlink());
        }
    }

    public bool Heal(int amount)
    {
        if (currentHealth <= 0 || currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        SoundManager.Play(Sfx.Heal);

        UpdateHealthUI();

        Debug.Log("Player healed! HP: " + currentHealth);

        return true;
    }

    void UpdateHealthUI()
    {
        if (healthText == null)
            return;

        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
    }

    IEnumerator DamageBlink()
    {
        isInvincible = true;

        float elapsed = 0f;

        while (elapsed < invincibilityTime)
        {
            SetSpritesVisible(false);
            yield return new WaitForSeconds(0.1f);

            SetSpritesVisible(true);
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        SetSpritesVisible(true);
        isInvincible = false;
    }

    void Die()
    {
        if (LevelLoader.Instance != null) LevelLoader.Instance.Fail("Try again", true);
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SetSpritesVisible(bool visible)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            if (spriteRenderers[i] != null)
                spriteRenderers[i].enabled = visible && initialVisibility[i];
    }
}

