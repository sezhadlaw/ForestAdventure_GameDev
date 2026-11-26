using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class AudioTests
{
    static Type GameType(string name) => Assembly.Load("Assembly-CSharp").GetType(name, true);
    static Component Sounds() => (Component)Object.FindFirstObjectByType(GameType("SoundManager"));

    static object SfxValue(string name) => Enum.Parse(GameType("Sfx"), name);

    static void Play(string sound)
    {
        GameType("SoundManager").GetMethod("Play").Invoke(null, new[] { SfxValue(sound), 1f, 0f });
    }

    [UnitySetUp]
    public IEnumerator Setup()
    {
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync("MainMenu");
    }

    [Test]
    public void EverySoundEventHasClips()
    {
        var manager = Sounds();
        Assert.IsNotNull(manager, "SoundManager is created before the first scene");
        var count = manager.GetType().GetMethod("ClipCount");
        foreach (var value in Enum.GetValues(GameType("Sfx")))
            Assert.GreaterOrEqual((int)count.Invoke(manager, new[] { value }), 1, value + " has a clip");
        foreach (string repeated in new[] { "Jump", "Shoot", "Hurt", "EnemyHit", "Coin" })
            Assert.GreaterOrEqual((int)count.Invoke(manager, new[] { SfxValue(repeated) }), 2, repeated + " has alternative clips");
    }

    [Test]
    public void EveryEffectSourceIsRoutedToTheMixer()
    {
        var sources = Sounds().GetComponents<AudioSource>();
        Assert.Greater(sources.Length, 1);
        foreach (var source in sources)
        {
            Assert.IsNotNull(source.outputAudioMixerGroup, "AudioSource has an Output group");
            StringAssert.IsMatch("^(SFX|UI)$", source.outputAudioMixerGroup.name);
            Assert.AreEqual(0f, source.spatialBlend);
            Assert.IsFalse(source.playOnAwake);
        }
    }

    [UnityTest]
    public IEnumerator VolumeSlidersSetDecibelsAndSurviveRestart()
    {
        yield return null; // SetFloat works from Start on
        var managerType = GameType("SaveManager");
        var manager = managerType.GetProperty("Instance").GetValue(null);
        var data = managerType.GetProperty("Data").GetValue(manager);
        var old = new[] { "masterVolume", "musicVolume", "effectsVolume" };
        var saved = new object[3];
        for (int i = 0; i < 3; i++) saved[i] = data.GetType().GetField(old[i]).GetValue(data);
        try
        {
            managerType.GetMethod("SetVolume", new[] { typeof(float), typeof(float), typeof(float) })
                .Invoke(manager, new object[] { 1f, 0.5f, 0.1f });
            var mixer = (UnityEngine.Audio.AudioMixer)Sounds().GetType().GetProperty("Mixer").GetValue(Sounds());
            Assert.IsTrue(mixer.GetFloat("MusicVol", out float music));
            Assert.IsTrue(mixer.GetFloat("SFXVol", out float sfx));
            Assert.IsTrue(mixer.GetFloat("MasterVol", out float master));
            Assert.AreEqual(-6.02f, music, 0.05f, "0.5 → −6 dB (Log10 × 20), not linear");
            Assert.AreEqual(-20f, sfx, 0.05f, "0.1 → −20 dB");
            Assert.AreEqual(0f, master, 0.05f);

            // The file is written half a second after the slider stops: simulate closing and opening the app.
            yield return new WaitForSecondsRealtime(0.7f);
            var restored = GameType("SaveSystem").GetMethod("Load").Invoke(null, new object[] { null });
            Assert.AreEqual(0.5f, restored.GetType().GetField("musicVolume").GetValue(restored));
            Assert.AreEqual(0.1f, restored.GetType().GetField("effectsVolume").GetValue(restored));
            Assert.AreEqual(0.5f, PlayerPrefs.GetFloat("Forest.Music"));
        }
        finally
        {
            managerType.GetMethod("SetVolume", new[] { typeof(float), typeof(float), typeof(float) }).Invoke(manager, saved);
            managerType.GetMethod("SaveVolume").Invoke(manager, null);
        }
    }

    [Test]
    public void OldSaveWithoutMasterVolumeKeepsFullVolume()
    {
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ForestOldSave_" + Guid.NewGuid() + ".json");
        try
        {
            System.IO.File.WriteAllText(path, "{\"version\":1,\"lastLevel\":1,\"unlockedLevels\":[1],\"completedLevels\":[],\"levelScores\":[0,0,0],\"musicVolume\":0.4,\"effectsVolume\":0.7}");
            var data = GameType("SaveSystem").GetMethod("Load").Invoke(null, new object[] { path });
            Assert.AreEqual(1f, data.GetType().GetField("masterVolume").GetValue(data));
            Assert.AreEqual(0.4f, data.GetType().GetField("musicVolume").GetValue(data));
        }
        finally { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
    }

    [UnityTest]
    public IEnumerator PauseSnapshotSoftensMusicAndResumeRestoresIt()
    {
        GameType("LevelLoader").GetField("SelectedLevel").SetValue(null, 1);
        yield return SceneManager.LoadSceneAsync("Game");
        yield return new WaitForSecondsRealtime(0.4f);
        var mixer = (UnityEngine.Audio.AudioMixer)Sounds().GetType().GetProperty("Mixer").GetValue(Sounds());
        Assert.IsTrue(mixer.GetFloat("TracksVol", out float volume));
        Assert.AreEqual(-8f, volume, 0.1f, "Normal snapshot");
        var ui = (Component)Object.FindFirstObjectByType(GameType("AdventureUI"));
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        ui.GetType().GetMethod("ShowPause", flags).Invoke(ui, null);
        Assert.AreEqual(0f, Time.timeScale);
        yield return new WaitForSecondsRealtime(0.5f); // 0.3 s transition on unscaled time
        mixer.GetFloat("TracksVol", out volume);
        mixer.GetFloat("TracksCutoff", out float cutoff);
        Assert.AreEqual(-14f, volume, 0.1f, "paused music is 6 dB quieter");
        Assert.AreEqual(800f, cutoff, 1f, "paused music is muffled by the lowpass");
        ui.GetType().GetMethod("Resume", flags).Invoke(ui, null);
        yield return new WaitForSecondsRealtime(0.5f);
        mixer.GetFloat("TracksVol", out volume);
        mixer.GetFloat("TracksCutoff", out cutoff);
        Assert.AreEqual(-8f, volume, 0.1f);
        Assert.AreEqual(22000f, cutoff, 1f);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [UnityTest]
    public IEnumerator RepeatedSoundsVaryInPitchAndClip()
    {
        var pitches = new HashSet<float>();
        var clips = new HashSet<AudioClip>();
        AudioClip previous = null;
        for (int i = 0; i < 6; i++)
        {
            Play("Hurt");
            var played = (AudioSource)Sounds().GetType().GetProperty("LastSource").GetValue(Sounds());
            Assert.IsNotNull(played);
            Assert.AreEqual("SFX", played.outputAudioMixerGroup.name);
            Assert.That(played.pitch, Is.InRange(0.92f, 1.08f));
            Assert.AreNotEqual(previous, played.clip, "the same clip never plays twice in a row");
            previous = played.clip;
            pitches.Add(played.pitch);
            clips.Add(played.clip);
            yield return null;
        }
        Assert.Greater(pitches.Count, 1, "pitch changes between repeats");
        Assert.Greater(clips.Count, 1, "alternative clips are used");
    }
}
