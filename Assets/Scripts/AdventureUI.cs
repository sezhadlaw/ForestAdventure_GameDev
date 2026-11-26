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
    private RectTransform hintBox;
    private float hintUntil;
    private float nextPreview;
    private int lastHealth = -1, lastMaxHealth = -1, lastCoins = -1, lastSeconds = -1;
    private int lastBossHealth = int.MinValue;
    private PlayerHealth player;
    private BossHealth boss;
    private TextMeshProUGUI bossLabel;
    // Forest palette: deep night teal, moss card, warm lantern gold, cream text.
    private static readonly Color Ink = new Color(0.05f, 0.12f, 0.13f, 0.97f);
    private static readonly Color Card = new Color(0.09f, 0.19f, 0.18f, 0.98f);
    private static readonly Color Gold = new Color(1f, 0.77f, 0.31f);
    private static readonly Color Green = new Color(0.16f, 0.36f, 0.29f);
    private static readonly Color Cream = new Color(0.95f, 0.94f, 0.86f);
    private static readonly Color Muted = new Color(0.66f, 0.74f, 0.68f);
    private static readonly Color Danger = new Color(0.78f, 0.27f, 0.21f);
    private static Sprite rounded;

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
            var bar = Box(root, "Journey HUD", new Vector2(0, 300), new Vector2(1130, 70), Ink);
            Outline(bar, Gold, 0.35f);
            status = Label(bar, "", new Vector2(-50, 0), new Vector2(960, 60), 25);
            Button(bar, "II", new Vector2(515, 0), new Vector2(76, 54), ShowPause);
            // Hints sit on a dark pill so they stay readable over the bright sky.
            hintBox = Box(root, "Hint", new Vector2(0, 228), new Vector2(760, 50), new Color(0.05f, 0.12f, 0.13f, 0.78f));
            hintBox.GetComponent<Image>().pixelsPerUnitMultiplier = 1f;
            hint = Label(hintBox, "", Vector2.zero, new Vector2(740, 50), 23);
            Hint(LevelLoader.Instance.Current.description);
            boss = FindFirstObjectByType<BossHealth>();
            if (boss != null)
            {
                var bossBox = Box(root, "Guardian", new Vector2(0, 170), new Vector2(420, 46), new Color(0.05f, 0.12f, 0.13f, 0.78f));
                bossBox.GetComponent<Image>().pixelsPerUnitMultiplier = 1f;
                bossLabel = Label(bossBox, "", Vector2.zero, new Vector2(400, 46), 23);
                bossLabel.color = Gold;
                bossLabel.fontStyle = FontStyles.Bold;
            }
        }
    }

    void Update()
    {
        var level = LevelLoader.Instance;
        if (status != null && level != null && player != null)
        {
            int seconds = Mathf.CeilToInt(level.Remaining);
            if (lastHealth != player.Health || lastMaxHealth != player.maxHealth || lastCoins != level.Coins || lastSeconds != seconds)
            {
                lastHealth = player.Health;
                lastMaxHealth = player.maxHealth;
                lastCoins = level.Coins;
                lastSeconds = seconds;
                status.text = "HP " + player.Health + "/" + player.maxHealth + "    •    COINS " + level.Coins + "/" + level.Current.goalAmount +
                    "    •    " + seconds / 60 + ":" + (seconds % 60).ToString("00") + "    •    " + level.Current.title;
            }
        }
        int bossHealth = boss == null ? -1 : boss.Health;
        if (bossLabel != null && lastBossHealth != bossHealth)
        {
            lastBossHealth = bossHealth;
            bossLabel.text = boss == null ? "GUARDIAN DEFEATED" : "GUARDIAN  " + boss.Health + " / " + boss.maxHealth;
        }
        if (hint != null && hint.text.Length > 0 && Time.unscaledTime > hintUntil)
        {
            hint.text = "";
            hintBox.gameObject.SetActive(false);
        }
    }

    public void Hint(string message)
    {
        if (hint == null) return;
        hint.text = message;
        hintBox.gameObject.SetActive(true);
        hintUntil = Time.unscaledTime + 3;
    }

    RectTransform OpenPanel(string title, string subtitle)
    {
        if (panel != null) { panel.SetActive(false); Destroy(panel); }
        var shade = Box(root, "Modal", Vector2.zero, new Vector2(1280, 720), new Color(0.015f, 0.04f, 0.05f, 0.82f), false);
        shade.anchorMin = Vector2.zero;
        shade.anchorMax = Vector2.one;
        shade.offsetMin = shade.offsetMax = Vector2.zero;
        panel = shade.gameObject;
        // A framed card in the middle of the screen; every screen draws on it.
        var card = Box(shade, "Card", new Vector2(0, -8), new Vector2(1190, 650), Card);
        Outline(card, Gold, 0.55f);
        Shadow(card, 10);
        var heading = Label(card, title, new Vector2(0, 268), new Vector2(1100, 75), 50);
        heading.color = Gold;
        heading.fontStyle = FontStyles.Bold;
        heading.characterSpacing = 6;
        Box(card, "Divider", new Vector2(0, 216), new Vector2(150, 4), Gold, false);
        Label(card, subtitle, new Vector2(0, 180), new Vector2(1080, 56), 23).color = Muted;
        return card;
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
            bool cleared = data.completedLevels.Contains(level.levelNumber);
            var card = Box(p, level.title, new Vector2(x, 0), new Vector2(330, 250), Green);
            Shadow(card, 6);
            if (cleared) Outline(card, Gold, 0.9f);
            var badge = Box(card, "Badge", new Vector2(0, 125), new Vector2(54, 54), unlocked ? Gold : Muted);
            badge.GetComponent<Image>().pixelsPerUnitMultiplier = 0.9f; // corners = half the size: a circle
            var number = Label(badge, cleared ? "OK" : level.levelNumber.ToString(), Vector2.zero, new Vector2(54, 54), cleared ? 20 : 28);
            number.color = Ink;
            number.fontStyle = FontStyles.Bold;
            Label(card, level.title, new Vector2(0, 62), new Vector2(305, 50), 28).color = unlocked ? Gold : Muted;
            string detail = unlocked ? level.description : "Complete the previous trail to unlock";
            Label(card, detail, new Vector2(0, 5), new Vector2(295, 66), 20).color = unlocked ? Cream : Muted;
            var button = Button(card, unlocked ? "PLAY" : "LOCKED", new Vector2(0, -78), new Vector2(278, 60), () => LevelLoader.Play(level.levelNumber), unlocked);
            button.interactable = unlocked;
            if (!unlocked) card.gameObject.AddComponent<CanvasGroup>().alpha = 0.6f;
        }
        Label(p, "BEST SCORE  " + data.bestScore + "     •     TRAILS CLEARED  " + data.completedLevels.Count + "/" + LevelData.Count, new Vector2(0, -165), new Vector2(1000, 45), 23);
        Button(p, "SETTINGS", new Vector2(0, -237), new Vector2(310, 62), ShowSettings, false);
        if (SaveSystem.UsedDefaults) Label(p, "Your save could not be read. A fresh adventure is ready.", new Vector2(0, -292), new Vector2(1100, 36), 20).color = Danger;
    }

    void ShowPause()
    {
        if (LevelLoader.Instance.Finished) return;
        Time.timeScale = 0;
        MusicManager.SetPaused(true); // Paused snapshot: the music gets quieter and muffled
        SaveManager.Instance.Save();
        var p = OpenPanel("TAKE A BREATH", LevelLoader.Instance.Current.title);
        Button(p, "RESUME", new Vector2(0, 90), new Vector2(400, 64), Resume);
        Button(p, "SETTINGS", new Vector2(0, 10), new Vector2(400, 64), ShowSettings, false);
        Button(p, "RETRY", new Vector2(0, -70), new Vector2(400, 64), () => LevelLoader.Play(LevelLoader.SelectedLevel), false);
        Button(p, "TRAIL MAP", new Vector2(0, -150), new Vector2(400, 64), Menu, false);
        Label(p, "The forest grows quiet while you rest.", new Vector2(0, -240), new Vector2(900, 40), 20).color = Muted;
    }

    void Resume()
    {
        panel.SetActive(false);
        Destroy(panel);
        Time.timeScale = 1;
        MusicManager.SetPaused(false);
    }

    void ShowSettings()
    {
        var p = OpenPanel("SETTINGS", "Sound and saved progress");
        var data = SaveManager.Instance.Data;
        Slider master = VolumeRow(p, "MASTER", 95, data.masterVolume);
        Slider music = VolumeRow(p, "MUSIC", 20, data.musicVolume);
        Slider effects = VolumeRow(p, "EFFECTS", -55, data.effectsVolume);
        UnityAction<float> changed = _ => SaveManager.Instance.SetVolume(master.value, music.value, effects.value);
        master.onValueChanged.AddListener(changed);
        music.onValueChanged.AddListener(changed);
        effects.onValueChanged.AddListener(changed);
        // Let the player hear the effects level while dragging.
        effects.onValueChanged.AddListener(_ => { if (Time.unscaledTime > nextPreview) { nextPreview = Time.unscaledTime + 0.25f; SoundManager.Play(Sfx.Coin); } });
        Label(p, "Music sits under the effects. Changes are saved automatically.", new Vector2(0, -118), new Vector2(1000, 36), 19).color = Muted;
        var reset = Button(p, "RESET PROGRESS", new Vector2(-175, -215), new Vector2(330, 62), AskReset, false);
        reset.GetComponent<Image>().color = Danger;
        reset.GetComponentInChildren<TextMeshProUGUI>().color = Cream;
        Button(p, "BACK", new Vector2(175, -215), new Vector2(330, 62), Back);
    }

    void AskReset()
    {
        var data = SaveManager.Instance.Data;
        var p = OpenPanel("START OVER?", "Best score " + data.bestScore + ", unlocked trails and sound settings will be reset.");
        Label(p, "Your next adventure begins at the first trail.", Vector2.zero, new Vector2(1000, 70), 26);
        Button(p, "CANCEL", new Vector2(-230, -145), new Vector2(400, 66), ShowSettings, false);
        var delete = Button(p, "RESET", new Vector2(230, -145), new Vector2(400, 66), () =>
        {
            if (SaveManager.Instance.ResetProgress()) Menu();
            else Label(p, "Reset failed. Please try again.", new Vector2(0, -250), new Vector2(1000, 60), 24);
        });
        delete.GetComponent<Image>().color = Danger;
        delete.GetComponentInChildren<TextMeshProUGUI>().color = Cream;
    }

    // One settings row: name, slider and the decibel value the mixer gets.
    Slider VolumeRow(Transform parent, string title, float y, float value)
    {
        var name = Label(parent, title, new Vector2(-400, y), new Vector2(200, 50), 25);
        name.alignment = TextAlignmentOptions.Left;
        name.fontStyle = FontStyles.Bold;
        Slider slider = Volume(parent, new Vector2(20, y), value);
        var readout = Label(parent, Decibels(value), new Vector2(410, y), new Vector2(150, 50), 24);
        readout.alignment = TextAlignmentOptions.Right;
        readout.color = Gold;
        slider.onValueChanged.AddListener(v => readout.text = Decibels(v));
        return slider;
    }

    static string Decibels(float value)
    {
        float db = MusicManager.ToDecibels(value);
        return db <= -79.5f ? "OFF" : Mathf.RoundToInt(db) + " dB";
    }

    void Back()
    {
        SaveManager.Instance.SaveVolume();
        if (LevelLoader.Instance == null) ShowLevels();
        else ShowPause();
    }

    public void ShowResult(bool won, string title)
    {
        var level = LevelLoader.Instance;
        var p = OpenPanel(title.ToUpper(), won ? "Score " + level.Score + "    •    Coins " + level.Coins : "Learn the timing and try the trail again.");
        Label(p, won ? "A little further into the forest." : "Listen for the enemy's warning before an attack.", new Vector2(0, 50), new Vector2(1000, 80), 29).color = won ? Gold : Cream;
        if (!SaveManager.Instance.Saved)
            Label(p, "Progress could not be written. Free storage and retry saving.", new Vector2(0, -30), new Vector2(1100, 60), 22).color = Danger;
        string action = won && level.Current.levelNumber < LevelData.Count ? "NEXT TRAIL" : "PLAY AGAIN";
        Button(p, action, new Vector2(0, -95), new Vector2(420, 70), () =>
        {
            SaveManager.Instance.Save();
            LevelLoader.Play(won ? Mathf.Min(level.Current.levelNumber + 1, LevelData.Count) : level.Current.levelNumber);
        });
        Button(p, "TRAIL MAP", new Vector2(0, -185), new Vector2(420, 64), Menu, false);
    }

    static void Menu()
    {
        SaveManager.Instance.Save();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    // A white rounded rectangle made once at runtime and 9-sliced, so any box gets soft corners without art files.
    static Sprite Rounded()
    {
        if (rounded != null) return rounded;
        const int size = 64, radius = 24;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x - 0.5f, x + 0.5f - (size - radius), 0);
                float dy = Mathf.Max(radius - y - 0.5f, y + 0.5f - (size - radius), 0);
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        texture.SetPixels32(pixels);
        texture.Apply();
        rounded = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return rounded;
    }

    static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool round = true)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var image = obj.GetComponent<Image>();
        image.color = color;
        if (round)
        {
            image.sprite = Rounded();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1.4f; // corner radius about 17 px
        }
        return rect;
    }

    static void Outline(RectTransform rect, Color color, float alpha)
    {
        var outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, alpha);
        outline.effectDistance = new Vector2(2, -2);
    }

    static void Shadow(RectTransform rect, float depth)
    {
        var shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.45f);
        shadow.effectDistance = new Vector2(0, -depth);
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
        label.color = Cream;
        label.raycastTarget = false;
        return label;
    }

    // Primary buttons are gold; secondary ones are moss green with gold text.
    static Button Button(Transform parent, string text, Vector2 position, Vector2 size, UnityAction action, bool primary = true)
    {
        var rect = Box(parent, text, position, size, primary ? Gold : Green);
        Shadow(rect, 5);
        if (!primary) Outline(rect, Gold, 0.6f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.88f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.78f, 0.74f, 0.66f);
        colors.disabledColor = new Color(0.55f, 0.58f, 0.56f, 1f); // opaque, so the outline does not shine through
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(() => SoundManager.Play(Sfx.UiClick));
        button.onClick.AddListener(action);
        var label = Label(rect, text, Vector2.zero, size, 25);
        label.color = primary ? Ink : Gold;
        label.fontStyle = FontStyles.Bold;
        label.characterSpacing = 3;
        return button;
    }

    // Real slider: dark track, gold fill and a round cream handle; the whole row is the touch area.
    static Slider Volume(Transform parent, Vector2 position, float value)
    {
        var rect = Box(parent, "Volume", position, new Vector2(500, 58), Color.clear, false);
        var track = Box(rect, "Track", Vector2.zero, new Vector2(480, 16), Ink);
        track.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
        Outline(track, Gold, 0.25f);
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
        fillArea.SetParent(rect, false);
        fillArea.anchorMin = new Vector2(0, 0.5f);
        fillArea.anchorMax = new Vector2(1, 0.5f);
        fillArea.offsetMin = new Vector2(10, -8);
        fillArea.offsetMax = new Vector2(-10, 8);
        var fill = Box(fillArea, "Fill", Vector2.zero, Vector2.zero, Gold);
        fill.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0, 1);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
        handleArea.SetParent(rect, false);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(22, 0);
        handleArea.offsetMax = new Vector2(-22, 0);
        var handle = Box(handleArea, "Handle", Vector2.zero, new Vector2(44, 44), Cream);
        handle.GetComponent<Image>().pixelsPerUnitMultiplier = 1.1f; // 44 px box with 22 px corners: a circle
        handle.anchorMin = new Vector2(0, 0.5f);
        handle.anchorMax = new Vector2(0, 0.5f);
        handle.sizeDelta = new Vector2(44, -14); // the slider stretches the handle to the 58 px row: 58 − 14 = 44
        Shadow(handle, 3);
        Outline(handle, Gold, 0.9f);
        var slider = rect.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0.0001f; // Log10(0.0001) × 20 = −80 dB, the mixer's silence
        slider.maxValue = 1;
        slider.SetValueWithoutNotify(value);
        return slider;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
