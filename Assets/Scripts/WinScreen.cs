
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject bossHealthBar;
    private bool bossDefeated;
    private bool shown;

    void Update()
    {
        if (bossDefeated && !shown && CoinManager.AllCoinsCollected) ShowWin();
    }

    void OnGUI()
    {
        if (bossDefeated && !shown)
            GUI.Box(new Rect(Screen.width / 2f - 220, 30, 440, 40), "Босс побеждён! Собери оставшиеся монеты.");
    }

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void ShowWin()
    {
        bossDefeated = true;
        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);

        if (!CoinManager.AllCoinsCollected || shown) return;
        shown = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
