
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        LevelLoader.Play(SaveManager.Instance.Data.lastLevel);
    }

    public void QuitGame()
    {
        Debug.Log("Game closed");
        Application.Quit();
    }
}
