using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Two music tracks (menu and level) on two AudioSources routed to the Tracks group.
// Switching scenes crossfades over 1 second. The same track keeps playing on Retry: no restart, no click.
// Created before the first scene and kept with DontDestroyOnLoad; a second copy destroys itself.
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    public const float FadeTime = 1f;
    public const string MenuScene = "MainMenu";

    public AudioClip MenuTrack { get; private set; }
    public AudioClip LevelTrack { get; private set; }
    public AudioSource Current { get; private set; }
    public AudioSource Other => Current == a ? b : a;

    private AudioSource a, b;
    private AudioMixer mixer;
    private Coroutine fade;
    private AudioMixerSnapshot normalSnapshot, pausedSnapshot;
    public const float SnapshotTime = 0.3f;
    public bool Paused { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInstance() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null) new GameObject("Music Manager").AddComponent<MusicManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } // the copy from a reloaded scene
        Instance = this;
        DontDestroyOnLoad(gameObject); // works on a root object only

        mixer = Resources.Load<AudioMixer>(SoundManager.MixerPath);
        AudioMixerGroup tracks = null;
        if (mixer != null)
        {
            var groups = mixer.FindMatchingGroups("Tracks");
            if (groups.Length > 0) tracks = groups[0];
            normalSnapshot = mixer.FindSnapshot("Normal");
            pausedSnapshot = mixer.FindSnapshot("Paused");
            // Pause sets Time.timeScale = 0: snapshot transitions must run on real time.
            mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
        }
        a = CreateSource(tracks);
        b = CreateSource(tracks);
        Current = a;
        MenuTrack = Resources.Load<AudioClip>("Audio/Music/menu_loop");
        LevelTrack = Resources.Load<AudioClip>("Audio/Music/level_loop");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // SetFloat on the mixer does not work in Awake, so the saved volumes are applied in Start.
    void Start() => ApplyVolumes();

    AudioSource CreateSource(AudioMixerGroup group)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0;
        source.volume = 0;
        source.outputAudioMixerGroup = group;
        return source;
    }

    // One rule for every scene: the menu gets the menu track, every level scene gets the level track.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Paused) ApplySnapshot(false, 0f); // Retry or Trail Map from the pause menu: back to Normal at once
        Play(scene.name == MenuScene ? MenuTrack : LevelTrack);
    }

    // Pause menu: the Paused snapshot lowers the Tracks group by 6 dB and closes a lowpass to 800 Hz,
    // so the music sounds like it plays behind a door. The settings sliders live on Music, so they still work.
    public static void SetPaused(bool paused)
    {
        if (Instance != null) Instance.ApplySnapshot(paused, SnapshotTime);
    }

    void ApplySnapshot(bool paused, float time)
    {
        Paused = paused;
        var snapshot = paused ? pausedSnapshot : normalSnapshot;
        if (snapshot != null) snapshot.TransitionTo(time);
    }

    public void Play(AudioClip clip, float time = FadeTime)
    {
        if (clip == null) return;
        if (Current.clip == clip && Current.isPlaying) return; // same track keeps playing

        AudioSource next = Other;
        next.clip = clip;
        next.volume = 0f;
        next.Play();
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(Current, next, time));
        Current = next;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float time)
    {
        float startFrom = from.volume;
        // Unscaled time: the fade also runs on the pause and result screens where Time.timeScale = 0.
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / time)
        {
            from.volume = startFrom * (1f - t);
            to.volume = t;
            yield return null;
        }
        from.volume = 0f;
        from.Stop();
        to.volume = 1f;
        fade = null;
    }

    // Slider value 0..1 → decibels. Log10 because the ear hears volume in ratios: 0.5 → −6 dB, 0.0001 → −80 dB.
    public static float ToDecibels(float value) => Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;

    // Applies the volumes from the save file to the exposed mixer parameters.
    public static void ApplyVolumes()
    {
        if (Instance == null || Instance.mixer == null || SaveManager.Instance == null) return;
        var data = SaveManager.Instance.Data;
        Instance.mixer.SetFloat("MasterVol", ToDecibels(data.masterVolume));
        Instance.mixer.SetFloat("MusicVol", ToDecibels(data.musicVolume));
        Instance.mixer.SetFloat("SFXVol", ToDecibels(data.effectsVolume));
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
