using UnityEngine;

public enum LevelGoal { CollectCoinsAndExit, CollectCoinsAndDefeatBoss }

[CreateAssetMenu(menuName = "Forest Adventure/Level Data")]
public class LevelData : ScriptableObject
{
    public static int Count => Resources.LoadAll<LevelData>("Levels").Length;
    public int levelNumber;
    public string title;
    public string description;
    public GameObject worldPrefab;
    public Vector2 playerSpawn;
    public LevelGoal goal;
    public int goalAmount;
    public float timeLimit;
    public int completionScore;
    public int coinScore;
    public int secondScore;
    public EnemySpawn[] enemies;
}

[System.Serializable]
public class EnemySpawn
{
    public GameObject prefab;
    public Vector3 position;
}
