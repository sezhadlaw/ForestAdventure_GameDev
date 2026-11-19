using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class Assignment11Build
{
    [MenuItem("Forest Adventure/Build Android APK")]
    public static void Android()
    {
        Directory.CreateDirectory("Builds");
        PlayerSettings.productName = "ForestAdventure";
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/MainMenu.unity", "Assets/Game.unity" },
            locationPathName = "Builds/ForestAdventure_Assignment11.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Android build failed: " + report.summary.result);
        UnityEngine.Debug.Log("ASSIGNMENT11_ANDROID_OK");
    }
}
