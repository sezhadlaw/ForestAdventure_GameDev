using UnityEngine;

public static class MobilePerformance
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Setup()
    {
        if (Application.isMobilePlatform)
            Application.targetFrameRate = 60;
#if UNITY_EDITOR
        if (!Application.isBatchMode)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
#endif
    }
}
