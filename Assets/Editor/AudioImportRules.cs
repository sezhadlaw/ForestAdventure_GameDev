using UnityEditor;
using UnityEngine;

// Mobile import settings from lecture 12, section 7. Applied automatically to every clip in Resources/Audio.
// Short SFX: Force To Mono, Decompress On Load, ADPCM. Music: stereo, Streaming, Vorbis.
public class AudioImportRules : AssetPostprocessor
{
    const string SfxFolder = "Assets/Resources/Audio/Sfx/";
    const string MusicFolder = "Assets/Resources/Audio/Music/";

    void OnPreprocessAudio()
    {
        bool sfx = assetPath.StartsWith(SfxFolder);
        bool music = assetPath.StartsWith(MusicFolder);
        if (!sfx && !music) return;

        var importer = (AudioImporter)assetImporter;
        var settings = importer.defaultSampleSettings;
        importer.forceToMono = sfx;
        importer.loadInBackground = music;
        if (sfx)
        {
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
        }
        else
        {
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
        }
        importer.defaultSampleSettings = settings;
    }
}
