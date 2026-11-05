
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "Level2";

    private bool isLoading = false;
    private float hintUntil;

    private void OnTriggerStay2D(Collider2D other) => OnTriggerEnter2D(other);

    private void OnGUI()
    {
        if (Time.time < hintUntil)
            GUI.Box(new Rect(Screen.width / 2f - 200, 30, 400, 40), "Собери все монеты, чтобы пройти уровень!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (other.GetComponentInParent<PlayerController>() != null)
        {
            if (!CoinManager.AllCoinsCollected)
            {
                hintUntil = Time.time + 2f;
                return;
            }
            isLoading = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
