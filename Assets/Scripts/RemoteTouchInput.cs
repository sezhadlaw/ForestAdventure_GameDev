#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using NewPhase = UnityEngine.InputSystem.TouchPhase;

public class RemoteTouchInput : MonoBehaviour
{
    private Touchscreen phone;
    private bool connected;
    private InputSettings.EditorInputBehaviorInPlayMode previousFocus;
    private InputSettings.BackgroundBehavior previousBackground;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartRemoteInput()
    {
        if (Application.isBatchMode) return;
        var helper = new GameObject("Unity Remote Touch");
        DontDestroyOnLoad(helper);
        helper.AddComponent<RemoteTouchInput>();
    }

    private void Update()
    {
        if (!EditorApplication.isRemoteConnected)
        {
            EndRemote();
            return;
        }

        if (!connected) BeginRemote();

        // Когда Unity сама подключила экран телефона, второй экран не нужен.
        foreach (var device in InputSystem.devices)
        {
            if (device is Touchscreen && device.remote)
            {
                RemovePhone();
                return;
            }
        }

        for (int i = 0; i < Input.touchCount; i++)
            SendTouch(Input.GetTouch(i));
    }

    private void SendTouch(UnityEngine.Touch touch)
    {
        NewPhase phase;
        switch (touch.phase)
        {
            case UnityEngine.TouchPhase.Began: phase = NewPhase.Began; break;
            case UnityEngine.TouchPhase.Moved: phase = NewPhase.Moved; break;
            case UnityEngine.TouchPhase.Ended: phase = NewPhase.Ended; break;
            case UnityEngine.TouchPhase.Canceled: phase = NewPhase.Canceled; break;
            default: return;
        }

        if (phone == null)
        {
            if (phase != NewPhase.Began) return;
            phone = InputSystem.AddDevice<Touchscreen>("UnityRemoteTouch");
        }

        InputSystem.QueueStateEvent(phone, new TouchState
        {
            touchId = touch.fingerId + 1,
            phase = phase,
            position = touch.position,
            pressure = touch.pressure
        });
    }

    private void BeginRemote()
    {
        var settings = InputSystem.settings;
        previousFocus = settings.editorInputBehaviorInPlayMode;
        previousBackground = settings.backgroundBehavior;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        connected = true;
    }

    private void EndRemote()
    {
        if (!connected) return;
        connected = false;
        RemovePhone();
        InputSystem.settings.editorInputBehaviorInPlayMode = previousFocus;
        InputSystem.settings.backgroundBehavior = previousBackground;
    }

    private void OnDisable()
    {
        EndRemote();
        RemovePhone();
    }

    private void RemovePhone()
    {
        if (phone != null && phone.added)
            InputSystem.RemoveDevice(phone);
        phone = null;
    }
}
#endif

