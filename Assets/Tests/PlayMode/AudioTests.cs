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
