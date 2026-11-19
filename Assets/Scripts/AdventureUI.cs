using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdventureUI : MonoBehaviour
{
    public static AdventureUI Instance { get; private set; }
    private RectTransform root;
    private GameObject panel;
    private TextMeshProUGUI status;
    private TextMeshProUGUI hint;
    private float hintUntil;
    private PlayerHealth player;
    private BossHealth boss;
    private TextMeshProUGUI bossLabel;
    private static readonly Color Ink = new Color(0.05f, 0.12f, 0.13f, 0.97f);
    private static readonly Color Gold = new Color(1f, 0.77f, 0.31f);
    private static readonly Color Green = new Color(0.16f, 0.36f, 0.29f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= MenuLoaded;
        SceneManager.sceneLoaded += MenuLoaded;
    }

    static void MenuLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu") return;
        foreach (var button in FindObjectsByType<Button>(FindObjectsSortMode.None))
            button.gameObject.SetActive(false);
        foreach (var label in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None)) label.gameObject.SetActive(false);
        new GameObject("Adventure Menu").AddComponent<AdventureUI>();
    }

    void Awake()
    {
        Instance = this;
        var canvasObject = new GameObject("Adventure Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        root = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(canvasObject.transform, false);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.gameObject.AddComponent<SafeAreaFitter>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") ShowLevels();
        else
        {
            player = FindFirstObjectByType<PlayerHealth>();
            var bar = Box(root, "Journey HUD", new Vector2(0, 296), new Vector2(1130, 74), Ink);
            status = Label(bar, "", new Vector2(-65, 0), new Vector2(920, 62), 25);
            Button(bar, "II", new Vector2(490, 0), new Vector2(95, 54), ShowPause);
            hint = Label(root, "", new Vector2(0, 225), new Vector2(1050, 58), 25);
            Hint(LevelLoader.Instance.Current.description);
            boss = FindFirstObjectByType<BossHealth>();
            if (boss != null) bossLabel = Label(root, "", new Vector2(0, 175), new Vector2(660, 46), 24);
        }
    }

    void Update()
    {
        var level = LevelLoader.Instance;
        if (status != null && level != null && player != null)
        {
            int seconds = Mathf.CeilToInt(level.Remaining);
            status.text = "HP " + player.Health + "/" + player.maxHealth + "    •    COINS " + level.Coins + "/" + level.Current.goalAmount +
                "    •    " + seconds / 60 + ":" + (seconds % 60).ToString("00") + "    •    " + level.Current.title;
        }
        if (bossLabel != null) bossLabel.text = boss == null ? "GUARDIAN DEFEATED" : "GUARDIAN  " + boss.Health + " / " + boss.maxHealth;
        if (hint != null && Time.unscaledTime > hintUntil) hint.text = "";
    }

    public void Hint(string message)
    {
        if (hint == null) return;
        hint.text = message;
        hintUntil = Time.unscaledTime + 3;
    }

    RectTransform OpenPanel(string title, string subtitle)
    {
        if (panel != null) { panel.SetActive(false); Destroy(panel); }
        var shade = Box(root, "Modal", Vector2.zero, new Vector2(1280, 720), new Color(0.025f, 0.06f, 0.07f, 0.88f));
        shade.anchorMin = Vector2.zero;
        shade.anchorMax = Vector2.one;
        shade.offsetMin = shade.offsetMax = Vector2.zero;
        panel = shade.gameObject;
        Label(shade, title, new Vector2(0, 248), new Vector2(1100, 75), 48).color = Gold;
        Label(shade, subtitle, new Vector2(0, 174), new Vector2(1080, 62), 23);
        return shade;
    }

    void ShowLevels()
    {
        Time.timeScale = 1;
        var data = SaveManager.Instance.Data;
        var p = OpenPanel("FOREST ADVENTURE", "Collect every coin to unlock the next trail.");
        var levels = Resources.LoadAll<LevelData>("Levels");
        System.Array.Sort(levels, (a, b) => a.levelNumber.CompareTo(b.levelNumber));
        for (int i = 0; i < levels.Length; i++)
        {
            var level = levels[i];
            bool unlocked = data.unlockedLevels.Contains(level.levelNumber);
            float x = (i - 1) * 354;
            var card = Box(p, level.title, new Vector2(x, 5), new Vector2(330, 240), Green);
            Label(card, level.title, new Vector2(0, 72), new Vector2(305, 55), 29).color = Gold;
            string detail = unlocked ? level.description : "Complete the previous trail to unlock";
            Label(card, detail, new Vector2(0, 10), new Vector2(295, 70), 21);
            var button = Button(card, unlocked ? "PLAY" : "LOCKED", new Vector2(0, -73), new Vector2(278, 62), () => LevelLoader.Play(level.levelNumber));
            button.interactable = unlocked;
        }
        Label(p, "BEST SCORE  " + data.bestScore + "     •     TRAILS CLEARED  " + data.completedLevels.Count + "/" + LevelData.Count, new Vector2(0, -153), new Vector2(1000, 45), 23);
        Button(p, "SETTINGS", new Vector2(0, -230), new Vector2(310, 64), ShowSettings);
        if (SaveSystem.UsedDefaults) Label(p, "Your save could not be read. A fresh adventure is ready.", new Vector2(0, -294), new Vector2(1100, 45), 20);
    }

    void ShowPause()
    {
        if (LevelLoader.Instance.Finished) return;
        Time.timeScale = 0;
        SaveManager.Instance.Save();
        var p = OpenPanel("TAKE A BREATH", LevelLoader.Instance.Current.title);
        Button(p, "RESUME", new Vector2(0, 72), new Vector2(400, 68), Resume);
        Button(p, "SETTINGS", new Vector2(0, -15), new Vector2(400, 68), ShowSettings);
        Button(p, "RETRY", new Vector2(0, -102), new Vector2(400, 68), () => LevelLoader.Play(LevelLoader.SelectedLevel));
        Button(p, "TRAIL MAP", new Vector2(0, -189), new Vector2(400, 68), Menu);
    }

    void Resume()
    {
        panel.SetActive(false);
        Destroy(panel);
        Time.timeScale = 1;
    }

    void ShowSettings()
    {
        var p = OpenPanel("SETTINGS", "Sound and saved progress");
        var data = SaveManager.Instance.Data;
        Label(p, "MUSIC", new Vector2(-310, 70), new Vector2(240, 50), 26);
        Slider music = Volume(p, new Vector2(100, 70), data.musicVolume);
        Label(p, "EFFECTS", new Vector2(-310, -20), new Vector2(240, 50), 26);
        Slider effects = Volume(p, new Vector2(100, -20), data.effectsVolume);
        music.onValueChanged.AddListener(value => SaveManager.Instance.SetVolume(value, effects.value));
        effects.onValueChanged.AddListener(value => SaveManager.Instance.SetVolume(music.value, value));
        Button(p, "RESET PROGRESS", new Vector2(0, -132), new Vector2(430, 66), AskReset);
        Button(p, "BACK", new Vector2(0, -225), new Vector2(280, 66), Back);
    }

    void AskReset()
    {
        var data = SaveManager.Instance.Data;
        var p = OpenPanel("START OVER?", "Best score " + data.bestScore + ", unlocked trails and sound settings will be reset.");
        Label(p, "Your next adventure begins at the first trail.", Vector2.zero, new Vector2(1000, 70), 26);
        Button(p, "CANCEL", new Vector2(-230, -145), new Vector2(400, 72), ShowSettings);
        var delete = Button(p, "RESET", new Vector2(230, -145), new Vector2(400, 72), () =>
        {
            if (SaveManager.Instance.ResetProgress()) Menu();
            else Label(p, "Reset failed. Please try again.", new Vector2(0, -250), new Vector2(1000, 60), 24);
        });
        delete.GetComponent<Image>().color = new Color(0.72f, 0.22f, 0.18f);
    }

    void Back()
    {
        if (LevelLoader.Instance == null) ShowLevels();
        else ShowPause();
    }

    public void ShowResult(bool won, string title)
    {
        var level = LevelLoader.Instance;
        var p = OpenPanel(title.ToUpper(), won ? "Score " + level.Score + "    •    Coins " + level.Coins : "Learn the timing and try the trail again.");
        Label(p, won ? "A little further into the forest." : "Watch for the enemy's warning before an attack.", new Vector2(0, 40), new Vector2(1000, 80), 29);
        if (!SaveManager.Instance.Saved)
            Label(p, "Progress could not be written. Free storage and retry saving.", new Vector2(0, -40), new Vector2(1100, 60), 22);
        string action = won && level.Current.levelNumber < LevelData.Count ? "NEXT TRAIL" : "PLAY AGAIN";
        Button(p, action, new Vector2(0, -120), new Vector2(420, 75), () =>
        {
            SaveManager.Instance.Save();
            LevelLoader.Play(won ? Mathf.Min(level.Current.levelNumber + 1, LevelData.Count) : level.Current.levelNumber);
        });
        Button(p, "TRAIL MAP", new Vector2(0, -222), new Vector2(420, 70), Menu);
    }

    static void Menu()
    {
        SaveManager.Instance.Save();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        obj.GetComponent<Image>().color = color;
        return rect;
    }

    static TextMeshProUGUI Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        var obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        var label = obj.GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(parent, false);
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = size;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.93f, 0.94f, 0.85f);
        label.raycastTarget = false;
        return label;
    }

    static Button Button(Transform parent, string text, Vector2 position, Vector2 size, UnityAction action)
    {
        var rect = Box(parent, text, position, size, Gold);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);
        var label = Label(rect, text, Vector2.zero, size, 25);
        label.color = Ink;
        label.fontStyle = FontStyles.Bold;
        return button;
    }

    static Slider Volume(Transform parent, Vector2 position, float value)
    {
        var rect = Box(parent, "Volume", position, new Vector2(540, 58), Green);
        var handle = Box(rect, "Handle", Vector2.zero, new Vector2(40, 6), Gold);
        var slider = rect.gameObject.AddComponent<Slider>();
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.SetValueWithoutNotify(value);
        return slider;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
