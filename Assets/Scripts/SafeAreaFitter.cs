using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastArea;
    private Vector2Int lastSize;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect area = Screen.safeArea;
        Vector2Int size = new(Screen.width, Screen.height);

        if (area == lastArea && size == lastSize)
            return;

        panel.anchorMin = new Vector2(
            area.xMin / Screen.width,
            area.yMin / Screen.height);

        panel.anchorMax = new Vector2(
            area.xMax / Screen.width,
            area.yMax / Screen.height);

        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        lastArea = area;
        lastSize = size;
    }
}