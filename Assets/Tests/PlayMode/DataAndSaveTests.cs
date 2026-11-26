using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class DataAndSaveTests
{
    static Type TypeOf(string name) => Assembly.Load("Assembly-CSharp").GetType(name, true);
    static object Get(object obj, string field) => obj.GetType().GetField(field).GetValue(obj);

    [Test]
    public void JsonRoundTripAndBrokenFile()
    {
        string folder = Path.Combine(Path.GetTempPath(), "ForestSaveTest_" + Guid.NewGuid());
        string path = Path.Combine(folder, "save.json");
        var system = TypeOf("SaveSystem");
        var load = system.GetMethod("Load");
        var save = system.GetMethod("Save");
        try
        {
            var data = load.Invoke(null, new object[] { path });
            Assert.AreEqual(0, Get(data, "bestScore"));
            data.GetType().GetField("bestScore").SetValue(data, 1234);
            data.GetType().GetField("musicVolume").SetValue(data, 0.35f);
            Assert.IsTrue((bool)save.Invoke(null, new[] { data, path }));
            var restored = load.Invoke(null, new object[] { path });
            Assert.AreEqual(1234, Get(restored, "bestScore"));
            Assert.AreEqual(0.35f, Get(restored, "musicVolume"));
            Assert.IsTrue((bool)save.Invoke(null, new[] { restored, path }));
            foreach (string broken in new[] { "broken {", "", "null", "{\"version\":999}" })
            {
                File.WriteAllText(path, broken);
                Assert.AreEqual(0, Get(load.Invoke(null, new object[] { path }), "bestScore"));
            }
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [UnityTest]
    public IEnumerator CompletionSettingsAndResetSurviveReload()
    {
        yield return SceneManager.LoadSceneAsync("MainMenu");
        var managerType = TypeOf("SaveManager");
        var manager = managerType.GetProperty("Instance").GetValue(null);
        Assert.IsTrue((bool)managerType.GetMethod("ResetProgress").Invoke(manager, null));
        managerType.GetMethod("CompleteLevel").Invoke(manager, new object[] { 1, 1200, 4 });
        managerType.GetMethod("CompleteLevel").Invoke(manager, new object[] { 1, 900, 4 });
        managerType.GetMethod("SetVolume", new[] { typeof(float), typeof(float), typeof(float) }).Invoke(manager, new object[] { 0.9f, 0.3f, 0.6f });
        managerType.GetMethod("SaveVolume").Invoke(manager, null);
        var data = TypeOf("SaveSystem").GetMethod("Load").Invoke(null, new object[] { null });
        Assert.AreEqual(1200, Get(data, "bestScore"));
        Assert.AreEqual(4, Get(data, "totalCoins"));
        CollectionAssert.Contains((IEnumerable)Get(data, "unlockedLevels"), 2);
        Assert.AreEqual(0.3f, Get(data, "musicVolume"));
        Assert.AreEqual(0.9f, Get(data, "masterVolume"));
        Assert.IsTrue((bool)managerType.GetMethod("ResetProgress").Invoke(manager, null));
        managerType.GetMethod("Save").Invoke(manager, null);
        data = TypeOf("SaveSystem").GetMethod("Load").Invoke(null, new object[] { null });
        Assert.AreEqual(0, Get(data, "bestScore"));
        Assert.AreEqual(1, ((IList)Get(data, "unlockedLevels")).Count);
        Assert.IsFalse(PlayerPrefs.HasKey("Forest.Music"));
    }

    [UnityTest]
    public IEnumerator ThreeLevelsUseOneLoaderAndTheirData()
    {
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return null;
        Directory.CreateDirectory("Proof");
        if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot("Proof/menu.png");
        yield return null;
        yield return null;
        var manager = TypeOf("SaveManager").GetProperty("Instance").GetValue(null);
        var data = manager.GetType().GetProperty("Data").GetValue(manager);
        var unlocked = (System.Collections.Generic.List<int>)Get(data, "unlockedLevels");
        unlocked.Clear();
        unlocked.AddRange(new[] { 1, 2, 3 });
        var loaderType = TypeOf("LevelLoader");
        for (int level = 1; level <= 3; level++)
        {
            loaderType.GetField("SelectedLevel").SetValue(null, level);
            yield return SceneManager.LoadSceneAsync("Game");
            yield return new WaitForSecondsRealtime(0.5f);
            var loader = Object.FindFirstObjectByType(loaderType);
            var current = loaderType.GetProperty("Current").GetValue(loader);
            Assert.AreEqual(level, Get(current, "levelNumber"));
            Assert.Greater((float)Get(current, "timeLimit"), 0);
            Assert.AreEqual(Get(current, "goalAmount"), Object.FindObjectsByType(TypeOf("Coin"), FindObjectsSortMode.None).Length);
            var player = Object.FindFirstObjectByType(TypeOf("PlayerHealth"));
            Assert.Greater((int)Get(player, "maxHealth"), 0);
            Assert.NotNull(Get(player, "stats"));
            Assert.NotNull(Object.FindFirstObjectByType(TypeOf("AdventureUI")));
            if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("Proof/level" + level + ".png");
            yield return null;
            yield return null;
        }
        yield return SceneManager.LoadSceneAsync("MainMenu");
        Assert.IsTrue((bool)manager.GetType().GetMethod("ResetProgress").Invoke(manager, null));
    }
}
