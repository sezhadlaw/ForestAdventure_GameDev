using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public SaveData Data { get; private set; }
    public bool Saved { get; private set; } = true;
    private float volumeSaveAt = -1;

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

    // Called on every slider move: the mixer changes at once, the file is written
    // once the slider stops for half a second (and on Back, pause and quit), not on every frame of a drag.
    public void SetVolume(float master, float music, float effects)
    {
        Data.masterVolume = Mathf.Clamp01(master);
        Data.musicVolume = Mathf.Clamp01(music);
        Data.effectsVolume = Mathf.Clamp01(effects);
        MusicManager.ApplyVolumes();
        volumeSaveAt = Time.unscaledTime + 0.5f;
    }

    public void SetVolume(float music, float effects) => SetVolume(Data.masterVolume, music, effects);

    // Writes pending volume changes now.
    public void SaveVolume()
    {
        volumeSaveAt = -1;
        PlayerPrefs.SetFloat("Forest.Master", Data.masterVolume);
        PlayerPrefs.SetFloat("Forest.Music", Data.musicVolume);
        PlayerPrefs.SetFloat("Forest.Effects", Data.effectsVolume);
        PlayerPrefs.Save();
        Save();
    }

    void Update()
    {
        if (volumeSaveAt > 0 && Time.unscaledTime >= volumeSaveAt) SaveVolume();
    }

    public bool ResetProgress()
    {
        // Сначала сохраняем пустой прогресс, потом обновляем данные в памяти.
        var defaults = SaveSystem.Defaults();
        if (!SaveSystem.Delete()) { Saved = false; return false; }
        Data = defaults;
        SaveSystem.UsedDefaults = false;
        volumeSaveAt = -1;
        PlayerPrefs.DeleteKey("Forest.Master");
        PlayerPrefs.DeleteKey("Forest.Music");
        PlayerPrefs.DeleteKey("Forest.Effects");
        PlayerPrefs.Save();
        Saved = true;
        MusicManager.ApplyVolumes();
        return true;
    }

    void OnApplicationPause(bool paused) { if (paused) SaveVolume(); }
    void OnApplicationFocus(bool focused) { if (!focused) SaveVolume(); }
    void OnApplicationQuit() => SaveVolume();
}
