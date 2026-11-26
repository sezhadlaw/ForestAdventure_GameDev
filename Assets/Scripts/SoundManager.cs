using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum Sfx
{
    Jump, Land, Shoot, Hurt, EnemyHit, Coin, Heal, Death, BossDeath,
    WindupGuard, WindupCharger, WindupShooter, WindupBoss,
    UiClick, Win, Lose
}

// One SoundManager for the whole game: created before the first scene and kept with DontDestroyOnLoad.
// All effects go through a fixed pool of AudioSources routed to the SFX group (UI click → UI group),
// so the mixer sliders control every sound. No AudioSource is created per shot.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public const string MixerPath = "Audio/GameMix";
    const int PoolSize = 8;

    public AudioMixer Mixer { get; private set; }
    public AudioSource LastSource { get; private set; }
    private AudioSource[] pool;
    private AudioSource ui;
    private int next;
    private readonly Dictionary<Sfx, AudioClip[]> clips = new Dictionary<Sfx, AudioClip[]>();
    private readonly Dictionary<Sfx, int> lastClip = new Dictionary<Sfx, int>();

    // Clip file names: "jump_1.wav", "jump_2.wav" → Sfx.Jump with two versions.
    static readonly Dictionary<string, Sfx> FilePrefix = new Dictionary<string, Sfx>
    {
        { "jump", Sfx.Jump }, { "land", Sfx.Land }, { "shoot", Sfx.Shoot }, { "hurt", Sfx.Hurt },
        { "enemy_hit", Sfx.EnemyHit }, { "coin", Sfx.Coin }, { "heal", Sfx.Heal }, { "death", Sfx.Death },
        { "boss_death", Sfx.BossDeath }, { "windup_guard", Sfx.WindupGuard }, { "windup_charger", Sfx.WindupCharger },
        { "windup_shooter", Sfx.WindupShooter }, { "windup_boss", Sfx.WindupBoss }, { "ui_click", Sfx.UiClick },
        { "win", Sfx.Win }, { "lose", Sfx.Lose }
    };

    // Jingles keep their exact melody; everything that repeats gets random pitch ±8%.
    static bool Varies(Sfx sound) => sound != Sfx.Win && sound != Sfx.Lose && sound != Sfx.BossDeath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null) new GameObject("Sound Manager").AddComponent<SoundManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Mixer = Resources.Load<AudioMixer>(MixerPath);
        AudioMixerGroup sfxGroup = Group("SFX"), uiGroup = Group("UI");
        pool = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++) pool[i] = CreateSource(sfxGroup);
        ui = CreateSource(uiGroup);
        LoadClips();
    }

    public AudioMixerGroup Group(string name)
    {
        if (Mixer == null) return null;
        var groups = Mixer.FindMatchingGroups("Master/" + name);
        if (groups.Length == 0) groups = Mixer.FindMatchingGroups(name);
        return groups.Length > 0 ? groups[0] : null;
    }

    AudioSource CreateSource(AudioMixerGroup group)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0; // 2D game: same volume everywhere
        source.outputAudioMixerGroup = group;
        return source;
    }

    void LoadClips()
    {
        var found = new Dictionary<Sfx, List<AudioClip>>();
        foreach (var clip in Resources.LoadAll<AudioClip>("Audio/Sfx"))
        {
            string name = clip.name;
            int underscore = name.LastIndexOf('_');
            if (underscore > 0 && int.TryParse(name.Substring(underscore + 1), out _)) name = name.Substring(0, underscore);
            if (!FilePrefix.TryGetValue(name, out Sfx sound)) continue;
            if (!found.ContainsKey(sound)) found[sound] = new List<AudioClip>();
            found[sound].Add(clip);
        }
        foreach (var pair in found)
        {
            pair.Value.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            clips[pair.Key] = pair.Value.ToArray();
        }
    }

    public int ClipCount(Sfx sound) => clips.TryGetValue(sound, out var list) ? list.Length : 0;

    public static void Play(Sfx sound, float volume = 1f, float delay = 0f)
    {
        if (Instance != null) Instance.PlaySound(sound, volume, delay);
    }

    void PlaySound(Sfx sound, float volume, float delay)
    {
        if (!clips.TryGetValue(sound, out var list) || list.Length == 0) return;

        // Pick a random version, but never the same one twice in a row.
        int index = Random.Range(0, list.Length);
        if (lastClip.TryGetValue(sound, out int last) && index == last && list.Length > 1) index = (index + 1) % list.Length;
        lastClip[sound] = index;

        AudioSource source = sound == Sfx.UiClick ? ui : FreeSource();
        source.pitch = Varies(sound) ? Random.Range(0.92f, 1.08f) : 1f;
        source.volume = volume;
        source.clip = list[index];
        LastSource = source;
        if (delay > 0) source.PlayDelayed(delay);
        else source.Play();
    }

    // Round robin over the pool; a free source is preferred so a ringing sound is not cut.
    AudioSource FreeSource()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            var source = pool[(next + i) % PoolSize];
            if (!source.isPlaying)
            {
                next = (next + i + 1) % PoolSize;
                return source;
            }
        }
        var oldest = pool[next];
        next = (next + 1) % PoolSize;
        return oldest;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
