using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// Forest Adventure → Validate Audio (or batch mode: -executeMethod AudioValidation.Validate).
// Checks the mixer groups, exposed parameters, snapshots and the mobile import settings of every clip.
public static class AudioValidation
{
    [MenuItem("Forest Adventure/Validate Audio")]
    public static void Validate()
    {
        AssetDatabase.Refresh();
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Resources/Audio/GameMix.mixer");
        Require(mixer != null, "GameMix.mixer loads");
        foreach (string group in new[] { "Master", "Music", "Tracks", "SFX", "UI" })
            Require(mixer.FindMatchingGroups(group).Length > 0, "mixer group " + group);
        Require(mixer.FindSnapshot("Normal") != null, "Normal snapshot");
        Require(mixer.FindSnapshot("Paused") != null, "Paused snapshot");
        Require(mixer.updateMode == AudioMixerUpdateMode.UnscaledTime, "mixer runs on unscaled time");
        var exposed = new SerializedObject(mixer).FindProperty("m_ExposedParameters");
        string names = "";
        for (int i = 0; i < exposed.arraySize; i++) names += exposed.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue + " ";
        foreach (string parameter in new[] { "MasterVol", "MusicVol", "SFXVol" })
            Require(names.Contains(parameter + " "), "exposed parameter " + parameter);

        int clips = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/Audio" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            bool music = path.Contains("/Music/");
            Require(importer.forceToMono != music, path + " mono setting");
            Require(settings.loadType == (music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad), path + " load type");
            Require(settings.compressionFormat == (music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM), path + " compression");
            clips++;
        }
        Require(clips >= 20, "at least 20 clips imported (found " + clips + ")");
        Debug.Log("AUDIO_VALIDATION: passed, " + clips + " clips, mixer " + mixer.name);
    }

    static void Require(bool condition, string label)
    {
        if (!condition) throw new Exception("Audio validation failed: " + label);
    }
}
