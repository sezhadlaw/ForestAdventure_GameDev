using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");
    public static bool UsedDefaults { get; internal set; }

    public static SaveData Defaults()
    {
        var data = new SaveData();
        for (int i = 0; i < LevelData.Count; i++) data.levelScores.Add(0);
        return data;
    }

    public static SaveData Load(string path = null)
    {
        path = path ?? FilePath;
        UsedDefaults = false;
        if (!File.Exists(path)) return Defaults();
        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                throw new InvalidDataException("Empty save");
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null || data.version != 1 || data.unlockedLevels == null ||
                !data.unlockedLevels.Contains(1) || data.levelScores == null || data.levelScores.Count != LevelData.Count ||
                data.completedLevels == null || data.bestScore < 0 || data.totalCoins < 0 ||
                !float.IsFinite(data.musicVolume) || !float.IsFinite(data.effectsVolume))
                throw new InvalidDataException("Invalid save data");
            data.unlockedLevels.RemoveAll(level => level < 1 || level > LevelData.Count);
            data.completedLevels.RemoveAll(level => level < 1 || level > LevelData.Count);
            data.lastLevel = Mathf.Clamp(data.lastLevel, 1, LevelData.Count);
            data.musicVolume = Mathf.Clamp01(data.musicVolume);
            data.effectsVolume = Mathf.Clamp01(data.effectsVolume);
            return data;
        }
        catch (Exception e)
        {
            UsedDefaults = true;
            Debug.LogWarning("Save reset to defaults: " + e.Message);
            return Defaults();
        }
    }

    public static bool Delete()
    {
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
            string temp = Path.ChangeExtension(FilePath, ".tmp");
            if (File.Exists(temp)) File.Delete(temp);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not reset progress: " + e.Message);
            return false;
        }
    }

    public static bool Save(SaveData data, string path = null)
    {
        try
        {
            path = path ?? FilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = Path.ChangeExtension(path, ".tmp");
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not save progress: " + e.Message);
            return false;
        }
    }
}
