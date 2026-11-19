using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class Assignment11Setup
{
    [MenuItem("Forest Adventure/Build Assignment 11 Data")]
    public static void BuildData()
    {
        Directory.CreateDirectory("Assets/Data/Balance");
        Directory.CreateDirectory("Assets/Data/Worlds");
        Directory.CreateDirectory("Assets/Data/Enemies");
        Directory.CreateDirectory("Assets/Resources/Levels");
        AssetDatabase.Refresh();
        // Переносим сохранённые значения из компонентов в assets.
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            Balance(root, Path.GetFileNameWithoutExtension(path));
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        var levels = new List<LevelData>();
        string[] titles = { "Whispering Woods", "Ancient Crossing", "The Forest Guardian" };
        string[] descriptions = { "Collect every coin and reach the cabin.", "Cross the platforms and find the exit.", "Collect every coin and defeat the guardian." };
        float[] times = { 180, 210, 240 };
        for (int i = 1; i <= 3; i++)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Level" + i + ".unity");
            foreach (var root in scene.GetRootGameObjects()) Balance(root, "Level" + i + "_" + root.name);
            EditorSceneManager.SaveScene(scene);
            var player = Object.FindFirstObjectByType<PlayerController>();
            var data = ScriptableObject.CreateInstance<LevelData>();
            data.name = "Level_0" + i;
            data.levelNumber = i;
            data.title = titles[i - 1];
            data.description = descriptions[i - 1];
            data.playerSpawn = player.transform.position;
            data.goal = i == 3 ? LevelGoal.CollectCoinsAndDefeatBoss : LevelGoal.CollectCoinsAndExit;
            data.goalAmount = Object.FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            data.timeLimit = times[i - 1];
            data.completionScore = 500;
            data.coinScore = 100;
            data.secondScore = 5;
            var spawns = new List<EnemySpawn>();
            var enemies = new List<GameObject>();
            foreach (var sight in Object.FindObjectsByType<EnemySight>(FindObjectsSortMode.None)) enemies.Add(sight.gameObject);
            foreach (var boss in Object.FindObjectsByType<BossHealth>(FindObjectsSortMode.None)) enemies.Add(boss.gameObject);
            foreach (var patrol in Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None))
                if (!enemies.Contains(patrol.gameObject)) enemies.Add(patrol.gameObject);
            enemies.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            foreach (var enemy in enemies)
            {
                var group = new GameObject(enemy.name + " Spawn");
                group.transform.position = enemy.transform.position;
                foreach (var behaviour in enemy.GetComponents<MonoBehaviour>())
                {
                    foreach (string fieldName in new[] { "leftPoint", "rightPoint" })
                    {
                        var field = behaviour.GetType().GetField(fieldName);
                        if (field?.GetValue(behaviour) is Transform point) point.SetParent(group.transform, true);
                    }
                }
                enemy.transform.SetParent(group.transform, true);
                var shooter = enemy.GetComponent<ForestShooter>();
                if (shooter != null && shooter.aimLine != null && !shooter.aimLine.transform.IsChildOf(group.transform))
                    shooter.aimLine.transform.SetParent(group.transform, true);
                var sight = enemy.GetComponent<EnemySight>();
                if (sight != null) { sight.player = null; sight.gameCamera = null; }
                var attack = enemy.GetComponent<BossAttack>();
                if (attack != null) attack.player = null;
                var boss = enemy.GetComponent<BossHealth>();
                if (boss != null) { boss.winScreen = null; boss.healthBar = null; }
                string path = "Assets/Data/Enemies/Level" + i + "_" + Clean(enemy.name) + "_" + spawns.Count + ".prefab";
                spawns.Add(new EnemySpawn { prefab = PrefabUtility.SaveAsPrefabAsset(group, path), position = group.transform.position });
                Object.DestroyImmediate(group);
            }
            data.enemies = spawns.ToArray();
            foreach (var stick in Object.FindObjectsByType<OnScreenStick>(FindObjectsSortMode.None)) stick.useIsolatedInputActions = true;
            foreach (var control in Object.FindObjectsByType<OnScreenButton>(FindObjectsSortMode.None))
            {
                foreach (var oldLabel in control.GetComponentsInChildren<TextMeshProUGUI>(true)) oldLabel.gameObject.SetActive(false);
                var image = control.GetComponent<Image>();
                if (image != null) image.color = new Color(1f, 0.77f, 0.31f, 0.95f);
                var labelObject = new GameObject("Action Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(control.transform, false);
                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                label.text = control.controlPath.Contains("buttonSouth") ? "JUMP" : "FIRE";
                label.fontSize = 28;
                label.alignment = TextAlignmentOptions.Center;
                label.fontStyle = FontStyles.Bold;
                label.color = new Color(0.05f, 0.12f, 0.13f);
                label.raycastTarget = false;
            }
            foreach (var pause in Object.FindObjectsByType<PauseMenu>(FindObjectsSortMode.None)) pause.enabled = false;
            foreach (var win in Object.FindObjectsByType<WinScreen>(FindObjectsSortMode.None))
            {
                if (win.winPanel != null) win.winPanel.SetActive(false);
                if (win.bossHealthBar != null) win.bossHealthBar.SetActive(false);
                win.enabled = false;
            }
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                bool pause = false;
                for (int n = 0; n < button.onClick.GetPersistentEventCount(); n++)
                    pause |= button.onClick.GetPersistentMethodName(n) == "Pause";
                if (pause) button.gameObject.SetActive(false);
            }
            foreach (var label in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
                if (label.text.StartsWith("HP") || label.text.StartsWith("Coins")) label.gameObject.SetActive(false);
            foreach (var rect in Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None))
                if (rect.name == "Vitals" || rect.name == "Wallet") rect.gameObject.SetActive(false);
            var world = new GameObject("World " + i);
            foreach (var root in scene.GetRootGameObjects()) if (root != world) root.transform.SetParent(world.transform, true);
            data.worldPrefab = PrefabUtility.SaveAsPrefabAsset(world, "Assets/Data/Worlds/World" + i + ".prefab");
            string assetPath = "Assets/Resources/Levels/Level_0" + i + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
            if (existing != null) { EditorUtility.CopySerialized(data, existing); Object.DestroyImmediate(data); data = existing; }
            else AssetDatabase.CreateAsset(data, assetPath);
            levels.Add(data);
        }
        var game = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Level Loader").AddComponent<LevelLoader>().levels = levels.ToArray();
        EditorSceneManager.SaveScene(game, "Assets/Game.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/MainMenu.unity", true), new EditorBuildSettingsScene("Assets/Game.unity", true), new EditorBuildSettingsScene("Assets/Level1.unity", true), new EditorBuildSettingsScene("Assets/Level2.unity", true), new EditorBuildSettingsScene("Assets/Level3.unity", true) };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/MainMenu.unity");
        Debug.Log("ASSIGNMENT11_SETUP_OK");
    }

    static string Clean(string text)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) text = text.Replace(c, '_');
        return text.Replace(' ', '_');
    }

    static void Balance(GameObject root, string prefix)
    {
        int index = 0;
        foreach (var script in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (script == null) continue;
            var statsField = script.GetType().GetField("stats");
            if (statsField == null || !typeof(ScriptableObject).IsAssignableFrom(statsField.FieldType)) continue;
            if (statsField.GetValue(script) != null) continue;
            var stats = ScriptableObject.CreateInstance(statsField.FieldType);
            foreach (var field in statsField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var source = script.GetType().GetField(field.Name);
                if (source != null) field.SetValue(stats, source.GetValue(script));
            }
            if (stats is BossAttackStats boss) { boss.initialDelay = 1; boss.spawnOffset = 1.5f; }
            if (stats is PlayerShootingStats shooting) shooting.spawnOffset = 0.8f;
            string path = "Assets/Data/Balance/" + Clean(prefix) + "_" + script.GetType().Name + "_" + index++ + ".asset";
            AssetDatabase.CreateAsset(stats, path);
            statsField.SetValue(script, stats);
            EditorUtility.SetDirty(script);
        }
    }
}
