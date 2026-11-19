using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// PlayMode tests live in a separate assembly; the scene scripts are in Assembly-CSharp.
public class ForestEncounterTests
{
    Component guard, shooter, slime, player;
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static Component Script(string name) => (Component)UnityEngine.Object.FindFirstObjectByType(Type.GetType(name + ", Assembly-CSharp", true));
    static object Read(Component c, string field) => c.GetType().GetField(field, Fields).GetValue(c);
    static string State(Component c) => Read(c, "state").ToString();

    [UnitySetUp]
    public IEnumerator Load()
    {
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync("Level1");
        Camera.main.aspect = 16f/9f;
        guard = Script("ForestGuard"); shooter = Script("ForestShooter"); slime = Script("ForestCharger");
        player = Script("PlayerHealth");
        player.GetComponent<Rigidbody2D>().simulated = false;
        yield return null;
    }

    [UnityTest]
    public IEnumerator GuardWarnsAndMissesWhenPlayerLeaves()
    {
        ((Behaviour)shooter).enabled = ((Behaviour)slime).enabled = false;
        player.transform.position = guard.transform.position + Vector3.right * 0.7f;
        yield return new WaitForSeconds(0.25f);
        Assert.AreEqual("WindUp", State(guard));
        Assert.AreEqual(Read(player,"maxHealth"), Read(player,"currentHealth"));
        player.transform.position += Vector3.up * 2;
        yield return new WaitForSeconds(0.6f);
        Assert.AreEqual("Rest", State(guard));
        Assert.AreEqual(Read(player,"maxHealth"), Read(player,"currentHealth"));
    }

    [UnityTest]
    public IEnumerator WallsBlockSightAndOffscreenSlimeCannotDash()
    {
        var sight = guard.GetComponent("EnemySight");
        guard.transform.position = new Vector3(-2.8f,-2.5f);
        player.transform.position = new Vector3(-0.6f,-2.5f);
        Physics2D.SyncTransforms();
        Assert.IsFalse((bool)sight.GetType().GetMethod("ClearPath").Invoke(sight,null));
        Camera.main.transform.position += Vector3.left * 30;
        player.transform.position = slime.transform.position + Vector3.left;
        yield return new WaitForSeconds(1);
        Assert.AreEqual("Idle",State(slime));
    }

    [UnityTest]
    public IEnumerator MushroomFiresAlongItsWarningLine()
    {
        ((Behaviour)guard).enabled = ((Behaviour)slime).enabled = false;
        player.transform.position = shooter.transform.position + Vector3.left * 1.5f;
        yield return new WaitForSeconds(0.58f);
        Assert.AreEqual("Aim",State(shooter));
        var line = (LineRenderer)Read(shooter,"aimLine");
        Vector3 target = line.GetPosition(1);
        player.transform.position += Vector3.up * 2;
        yield return new WaitForSeconds(0.16f);
        Assert.AreEqual(target,line.GetPosition(1));
        yield return new WaitForSeconds(0.6f);
        Assert.AreEqual("Rest",State(shooter));
        Assert.IsFalse(line.enabled);
        Assert.AreEqual(Read(player,"maxHealth"), Read(player,"currentHealth"));
    }

    [UnityTest]
    public IEnumerator SlimeCommitsToOneDirectionThenStops()
    {
        ((Behaviour)guard).enabled = ((Behaviour)shooter).enabled = false;
        player.transform.position = slime.transform.position + Vector3.left;
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual("WindUp",State(slime));
        float start = slime.transform.position.x;
        player.transform.position += Vector3.up * 2;
        yield return new WaitForSeconds(1.1f);
        Assert.AreEqual("Stunned",State(slime));
        Assert.Less(slime.transform.position.x,start);
        Assert.AreEqual(Read(player,"maxHealth"), Read(player,"currentHealth"));
    }
}
