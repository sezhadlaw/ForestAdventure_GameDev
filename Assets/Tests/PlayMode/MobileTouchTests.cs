using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Phase = UnityEngine.InputSystem.TouchPhase;

public class MobileTouchTests
{
    static Vector2 Center(Component component)
    {
        var rect = component.GetComponent<RectTransform>();
        return RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
    }

    static void Touch(Touchscreen screen, int id, Phase phase, Vector2 position)
    {
        InputSystem.QueueStateEvent(screen, new TouchState { touchId = id, phase = phase, position = position });
    }

    [UnityTest]
    public IEnumerator TouchOpensLevelAndJoystickWorksWithSecondFinger()
    {
        var settings = InputSystem.settings;
        var oldBackground = settings.backgroundBehavior;
        var oldFocus = settings.editorInputBehaviorInPlayMode;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var screen = InputSystem.AddDevice<Touchscreen>();
        try
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            Canvas.ForceUpdateCanvases();
            var play = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(button => button.name == "PLAY" && button.interactable);
            Vector2 position = Center(play);
            Touch(screen, 1, Phase.Began, position);
            yield return null;
            yield return null;
            Touch(screen, 1, Phase.Ended, position);
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual("Game", SceneManager.GetActiveScene().name);
            var stick = Object.FindFirstObjectByType<OnScreenStick>();
            var jump = Object.FindObjectsByType<OnScreenButton>(FindObjectsSortMode.None).First(button => button.controlPath.Contains("buttonSouth"));
            Vector2 start = Center(stick);
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            var es = UnityEngine.EventSystems.EventSystem.current;
            es.RaycastAll(new UnityEngine.EventSystems.PointerEventData(es) { position = start }, hits);
            Debug.Log("TOUCH_DIAGNOSTIC " + start + " hits " + string.Join(",", hits.Select(hit => hit.gameObject.name)));
            Touch(screen, 1, Phase.Began, start);
            yield return null;
            Touch(screen, 1, Phase.Moved, start + Vector2.right * 80);
            yield return null;
            yield return null;
            var type = Assembly.Load("Assembly-CSharp").GetType("PlayerController", true);
            var player = (Component)Object.FindFirstObjectByType(type);
            Debug.Log("TOUCH_DIAGNOSTIC stick=" + stick.control.ReadValueAsObject() + " gamepad=" + Gamepad.current.leftStick.ReadValue() + " map=" + player.GetComponent<PlayerInput>().currentActionMap.name + " move=" + player.GetComponent<PlayerInput>().actions["Move"].ReadValue<Vector2>());
            Assert.Greater((float)type.GetProperty("HorizontalInput").GetValue(player), 0.1f);
            Touch(screen, 2, Phase.Began, Center(jump));
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.Greater(player.GetComponent<Rigidbody2D>().linearVelocity.y, 0, "Second finger must jump");
            Assert.Greater((float)type.GetProperty("HorizontalInput").GetValue(player), 0.1f);
            Touch(screen, 2, Phase.Ended, Center(jump));
            Touch(screen, 1, Phase.Ended, start + Vector2.right * 80);
            yield return null;
            yield return null;
            Assert.AreEqual(0f, (float)type.GetProperty("HorizontalInput").GetValue(player), 0.01f);
        }
        finally
        {
            InputSystem.RemoveDevice(screen);
            settings.backgroundBehavior = oldBackground;
            settings.editorInputBehaviorInPlayMode = oldFocus;
        }
    }
}
