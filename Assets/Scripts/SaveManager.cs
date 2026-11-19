using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData Data { get; private set; }
    public bool Saved { get; private set; } = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (Instance == null) new GameObject("Save Manager").AddComponent<SaveManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Data = SaveSystem.Load();
    }

    public void Save() => Saved = SaveSystem.Save(Data);

    public void CompleteLevel(int level, int score, int coins)
    {
        int oldScore = Data.levelScores[level - 1];
        Data.levelScores[level - 1] = Mathf.Max(oldScore, score);
        Data.bestScore = Mathf.Max(Data.bestScore, score);
        if (!Data.completedLevels.Contains(level))
        {
            Data.completedLevels.Add(level);
            Data.totalCoins += coins;
        }
        if (level < LevelData.Count && !Data.unlockedLevels.Contains(level + 1)) Data.unlockedLevels.Add(level + 1);
        Data.lastLevel = Mathf.Min(level + 1, LevelData.Count);
        Save();
    }

    public void SetVolume(float music, float effects)
    {
        Data.musicVolume = Mathf.Clamp01(music);
        Data.effectsVolume = Mathf.Clamp01(effects);
        PlayerPrefs.SetFloat("Forest.Music", Data.musicVolume);
        PlayerPrefs.SetFloat("Forest.Effects", Data.effectsVolume);
        PlayerPrefs.Save();
        Save();
        GameAudio.RefreshVolume();
    }

    public bool ResetProgress()
    {
        // Сначала сохраняем пустой прогресс, потом обновляем данные в памяти.
        var defaults = SaveSystem.Defaults();
        if (!SaveSystem.Delete()) { Saved = false; return false; }
        Data = defaults;
        SaveSystem.UsedDefaults = false;
        PlayerPrefs.DeleteKey("Forest.Music");
        PlayerPrefs.DeleteKey("Forest.Effects");
        PlayerPrefs.Save();
        Saved = true;
        GameAudio.RefreshVolume();
        return true;
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationFocus(bool focused) { if (!focused) Save(); }
    void OnApplicationQuit() => Save();
}
