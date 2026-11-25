using UnityEngine;
using UnityEngine.SceneManagement;

// Music only: sound effects moved to SoundManager (Assignment 12).
public class GameAudio : MonoBehaviour
{
    public const float MenuVolume = 0.32f;
    public const float GameVolume = 0.20f;
    private static GameAudio instance;
    private AudioSource music;
    private AudioClip menuLoop, gameLoop;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance == null) new GameObject("Game Audio").AddComponent<GameAudio>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = 0;
        menuLoop = MakeMusic("Forest menu", new[] { 72, 76, 79, 83, 79, 76, 74, 71, 69, 72, 76, 79, 76, 72, 67, 71 }, 0.45f);
        gameLoop = MakeMusic("Forest adventure", new[] { 69, 72, 76, 72, 67, 71, 74, 71, 65, 69, 72, 69, 67, 71, 74, 76 }, 0.30f);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool menu = scene.name == "MainMenu";
        AudioClip next = menu ? menuLoop : gameLoop;
        music.volume = (menu ? MenuVolume : GameVolume) * (SaveManager.Instance == null ? 1 : SaveManager.Instance.Data.musicVolume);
        if (music.clip == next && music.isPlaying) return;
        music.clip = next;
        music.Play();
    }

    public static void RefreshVolume()
    {
        if (instance != null) instance.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static float Frequency(int note) => 440f * Mathf.Pow(2f, (note - 69) / 12f);

    static AudioClip MakeMusic(string title, int[] melody, float beat)
    {
        const int rate = 22050;
        int step = Mathf.RoundToInt(rate * beat);
        float[] samples = new float[step * melody.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            int note = i / step;
            float t = (i % step) / (float)rate;
            float envelope = Mathf.Min(t / 0.015f, 1f) * Mathf.Pow(1f - t / beat, 2);
            float phase = 2f * Mathf.PI * Frequency(melody[note % melody.Length]) * t;
            float lead = Mathf.Sin(phase) + 0.18f * Mathf.Sin(phase * 2);
            float bassPhase = 2f * Mathf.PI * Frequency(melody[(note / 4 * 4) % melody.Length] - 24) * t;
            samples[i] = envelope * (0.33f * lead + 0.18f * Mathf.Sin(bassPhase));
        }
        return Clip(title, samples, rate);
    }

    static AudioClip Clip(string title, float[] samples, int rate)
    {
        var clip = AudioClip.Create(title, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
        Destroy(menuLoop); Destroy(gameLoop);
    }
}
