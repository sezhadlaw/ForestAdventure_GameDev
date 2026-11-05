using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class GameplayTests
{
    static Type GameType(string name) => Assembly.Load("Assembly-CSharp").GetType(name, true);
    static Component Find(string name) => (Component)Object.FindFirstObjectByType(GameType(name));
    static object Field(Component component, string name) => component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(component);
    static void Call(Component component, string name, params object[] args) => component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(component, args);

    [UnitySetUp]
    public IEnumerator Setup()
    {
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync("Level3");
        ((Behaviour)Find("BossAttack")).enabled = false;
        var player = Find("PlayerController");
        player.GetComponent<Rigidbody2D>().simulated = false;
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        Time.timeScale = 1;
        yield return null;
    }

    [UnityTest]
    public IEnumerator FastBulletDamagesBossExactlyOnce()
    {
        Component boss = Find("BossHealth");
        int before = (int)Field(boss, "currentHealth");
        var bounds = boss.GetComponent<Collider2D>().bounds;
        var prefab = (GameObject)Field(Find("PlayerShooting"), "bulletPrefab");
        var bullet = Object.Instantiate(prefab, bounds.center + Vector3.left * (bounds.extents.x + 1), Quaternion.identity);
        var script = bullet.GetComponent(GameType("Bullet"));
        script.GetType().GetField("speed").SetValue(script, 300f);
        Call(script, "SetDirection", Vector2.right);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return null;
        Assert.AreEqual(before - 1, Field(boss, "currentHealth"));
        Assert.IsTrue(bullet == null);
    }

    [UnityTest]
    public IEnumerator VictoryWaitsForLastCoinAndPickupCannotCountTwice()
    {
        // Add a coin even if the boss scene has none.
        var coin = new GameObject("Test coin").AddComponent(GameType("Coin"));
        var win = Find("WinScreen");
        Call(win, "ShowWin");
        Assert.AreEqual(1f, Time.timeScale);
        var playerCollider = Find("PlayerController").GetComponent<Collider2D>();
        foreach (Component item in Object.FindObjectsByType(GameType("Coin"), FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Call(item, "OnTriggerEnter2D", playerCollider);
            Call(item, "OnTriggerEnter2D", playerCollider);
        }
        yield return null;
        yield return null;
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsTrue(((GameObject)Field(win, "winPanel")).activeSelf);
    }

    [UnityTest]
    public IEnumerator ExitRequiresEveryCoin()
    {
        yield return SceneManager.LoadSceneAsync("Level1");
        var player = Find("PlayerController");
        player.GetComponent<Rigidbody2D>().simulated = false;
        var exit = Find("LevelExit");
        var collider = player.GetComponent<Collider2D>();
        Call(exit, "OnTriggerEnter2D", collider);
        yield return null;
        Assert.AreEqual("Level1", SceneManager.GetActiveScene().name);
        int count = 0;
        foreach (Component coin in Object.FindObjectsByType(GameType("Coin"), FindObjectsSortMode.None))
        {
            Call(coin, "OnTriggerEnter2D", collider);
            Call(coin, "OnTriggerEnter2D", collider);
            count++;
        }
        Assert.Greater(count, 0);
        Assert.AreEqual(count, Field(Find("CoinManager"), "coinsCollected"));
        Call(exit, "OnTriggerStay2D", collider);
        yield return null;
        yield return null;
        Assert.AreEqual("Level2", SceneManager.GetActiveScene().name);
    }

    [UnityTest]
    public IEnumerator RunningAndShootingAnimateTheVisualOnly()
    {
        var player = Find("PlayerController");
        var animation = player.GetComponent(GameType("PlayerAnimation"));
        var animator = player.GetComponentInChildren<Animator>();
        var visual = animator.transform;
        Vector3 original = visual.localPosition;
        player.GetType().GetField("movement", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, Vector2.right);
        yield return null;
        yield return null;
        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Run"));
        Call(animation, "PlayShot", 1);
        yield return null;
        Assert.AreNotEqual(original, visual.localPosition);
        yield return new WaitForSeconds(0.25f);
        Assert.AreEqual(original, visual.localPosition);
    }

    [UnityTest]
    public IEnumerator MusicChangesAndGameplayIsQuieter()
    {
        var audio = Find("GameAudio");
        var source = (AudioSource)Field(audio, "music");
        var gameClip = source.clip;
        float gameVolume = source.volume;
        yield return SceneManager.LoadSceneAsync("MainMenu");
        Assert.AreNotEqual(gameClip, source.clip);
        Assert.Greater(source.volume, gameVolume);
        Assert.AreEqual(1, Object.FindObjectsByType(GameType("GameAudio"), FindObjectsSortMode.None).Length);
        Assert.Greater(((AudioClip)Field(audio, "coinSound")).samples, 0);
        Assert.Greater(((AudioClip)Field(audio, "damageSound")).samples, 0);
    }
}
