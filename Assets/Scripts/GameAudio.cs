using UnityEngine;
using UnityEngine.SceneManagement;

// Original synthesized loops; no external audio files or scene wiring required.
public class GameAudio : MonoBehaviour
{
    public const float MenuVolume = 0.32f;
    public const float GameVolume = 0.20f;
    private static GameAudio instance;
    private AudioSource music;
    private AudioSource effects;
    private AudioClip menuLoop, gameLoop, coinSound, damageSound;

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
        effects = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = effects.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = effects.spatialBlend = 0;
        menuLoop = MakeMusic("Forest menu", new[] { 72, 76, 79, 83, 79, 76, 74, 71, 69, 72, 76, 79, 76, 72, 67, 71 }, 0.45f);
        gameLoop = MakeMusic("Forest adventure", new[] { 69, 72, 76, 72, 67, 71, 74, 71, 65, 69, 72, 69, 67, 71, 74, 76 }, 0.30f);
        coinSound = MakeEffect(true);
        damageSound = MakeEffect(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool menu = scene.name == "MainMenu";
        AudioClip next = menu ? menuLoop : gameLoop;
        music.volume = menu ? MenuVolume : GameVolume;
        if (music.clip == next && music.isPlaying) return;
        music.clip = next;
        music.Play();
    }

    public static void PlayCoin() { if (instance != null) instance.effects.PlayOneShot(instance.coinSound, 0.65f); }
    public static void PlayDamage() { if (instance != null) instance.effects.PlayOneShot(instance.damageSound, 0.65f); }

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

    static AudioClip MakeEffect(bool coin)
    {
        const int rate = 22050;
        float duration = coin ? 0.22f : 0.28f;
        float[] samples = new float[(int)(rate * duration)];
        float phase = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / (float)rate;
            float frequency = coin ? (t < 0.09f ? 988f : 1480f) : Mathf.Lerp(240f, 65f, t / duration);
            phase += 2 * Mathf.PI * frequency / rate;
            float envelope = Mathf.Min(t / 0.005f, 1) * Mathf.Pow(1 - t / duration, 1.5f);
            samples[i] = envelope * (Mathf.Sin(phase) * 0.55f + (coin ? 0 : Mathf.Sin(phase * 1.43f) * 0.2f));
        }
        return Clip(coin ? "Coin pickup" : "Player damage", samples, rate);
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
        Destroy(menuLoop); Destroy(gameLoop); Destroy(coinSound); Destroy(damageSound);
    }
}
