using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static int SelectedLevel = 1;
    public static LevelLoader Instance { get; private set; }
    public LevelData[] levels;
    public LevelData Current { get; private set; }
    public float Remaining { get; private set; }
    public int Coins { get; private set; }
    public bool Finished { get; private set; }
    public bool BossDefeated { get; private set; }
    public int Score { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1;
        SelectedLevel = Mathf.Clamp(SelectedLevel, 1, levels.Length);
        if (!SaveManager.Instance.Data.unlockedLevels.Contains(SelectedLevel)) SelectedLevel = 1;
        Current = levels[SelectedLevel - 1];
        SaveManager.Instance.Data.lastLevel = SelectedLevel;
        Remaining = Current.timeLimit;
        var world = Instantiate(Current.worldPrefab);
        var player = world.GetComponentInChildren<PlayerController>();
        player.transform.position = Current.playerSpawn;
        var camera = world.GetComponentInChildren<Camera>();
        foreach (var spawn in Current.enemies)
        {
            var enemy = Instantiate(spawn.prefab, spawn.position, Quaternion.identity);
            var sight = enemy.GetComponentInChildren<EnemySight>();
            if (sight != null) { sight.player = player.GetComponent<PlayerHealth>(); sight.gameCamera = camera; }
            var boss = enemy.GetComponentInChildren<BossAttack>();
            if (boss != null) boss.player = player.transform;
        }
        gameObject.AddComponent<AdventureUI>();
    }

    void Update()
    {
        if (Finished) return;
        Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
        if (Remaining <= 0) Fail("Time is up");
    }

    public void AddCoin() => Coins++;

    public void DefeatBoss()
    {
        BossDefeated = true;
        TryComplete();
    }

    public void TryComplete()
    {
        if (Finished) return;
        if (Coins < Current.goalAmount || !CoinManager.AllCoinsCollected)
        { AdventureUI.Instance.Hint("Collect all coins to finish"); return; }
        if (Current.goal == LevelGoal.CollectCoinsAndDefeatBoss && !BossDefeated) return;
        Finished = true;
        Score = Current.completionScore + Coins * Current.coinScore + Mathf.FloorToInt(Remaining) * Current.secondScore;
        SaveManager.Instance.CompleteLevel(Current.levelNumber, Score, Coins);
        Time.timeScale = 0;
        SoundManager.Play(Sfx.Win);
        AdventureUI.Instance.ShowResult(true, "Level complete");
    }

    public void Fail(string reason, bool playerDied = false)
    {
        if (Finished) return;
        Finished = true;
        Time.timeScale = 0;
        // After a death the lose jingle waits for the death sound to fade.
        SoundManager.Play(Sfx.Lose, 1f, playerDied ? 0.6f : 0f);
        AdventureUI.Instance.ShowResult(false, reason);
    }

    public static void Play(int level)
    {
        if (!SaveManager.Instance.Data.unlockedLevels.Contains(level)) return;
        SelectedLevel = level;
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
