using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AudioAssetBundleBuilder
{
    /// <summary>
    /// Các định dạng file audio hỗ trợ
    /// </summary>
    private static readonly string[] audioExtensions = { ".mp3", ".wav" };

    /// <summary>
    /// Output path cho Asset Bundle
    /// </summary>
    private static readonly string outputPath = "Assets/NPCSound";

    /// <summary>
    /// Build tất cả audio asset bundles trong project
    /// </summary>
    [MenuItem("AssetBundles/Build All Audio Asset Bundles")]
    public static void BuildAllAudioAssetBundles()
    {
        List<string> audioFiles = FindAllAudioFiles();

        if (audioFiles.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy file audio nào trong project!");
            return;
        }

        CreateOutputDirectory();

        foreach (string audioFile in audioFiles)
        {
            BuildAudioBundle(audioFile);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Build hoàn tất {audioFiles.Count} sounds!");
    }

    /// <summary>
    /// Build một audio thành AssetBundle
    /// </summary>
    /// <param name="audioFile"></param>
    private static void BuildAudioBundle(string audioFile)
    {
        try
        {
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioFile);
            if (audioClip == null)
            {
                Debug.LogError($"Không thể load AudioClip: {audioFile}");
                return;
            }

            SetAudioImportSettings(audioFile);

            string fileName = Path.GetFileNameWithoutExtension(audioFile);
            string bundleName = $"{fileName}.unity3d";
            string fullOutputPath = Path.Combine(outputPath, bundleName);

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = bundleName;
            buildMap[0].assetNames = new string[] { audioFile };

            BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression |
                                              BuildAssetBundleOptions.ForceRebuildAssetBundle;

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                Path.GetDirectoryName(fullOutputPath),
                buildMap,
                options,
                BuildTarget.StandaloneWindows64
            );

            if (manifest != null)
            {
                Debug.Log($"Build thành công: {audioFile} -> {fullOutputPath}");
            }
            else
            {
                Debug.LogError($"Build thất bại: {audioFile}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi build {audioFile}: {ex.Message}");
        }
    }

    /// <summary>
    /// Cấu hình import settings cho AudioClip
    /// </summary>
    /// <param name="audioFile"></param>
    private static void SetAudioImportSettings(string audioFile)
    {
        AudioImporter audioImporter = AssetImporter.GetAtPath(audioFile) as AudioImporter;
        if (audioImporter != null)
        {
            // Cấu hình cho AssetBundle
            AudioImporterSampleSettings settings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.7f,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate,
                preloadAudioData = true 
            };

            audioImporter.defaultSampleSettings = settings;
            audioImporter.loadInBackground = false;
            audioImporter.SaveAndReimport();

            Debug.Log($"Cập nhật import settings cho: {audioFile}");
        }
    }

    /// <summary>
    /// Tạo thư mục Asset Bundle mới theo outputPath
    /// </summary>
    private static void CreateOutputDirectory()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Debug.Log($"Tạo thư mục: {outputPath}");
        }
    }

    /// <summary>
    /// Lấy tất cả file audio trong dự án
    /// </summary>
    /// <returns></returns>
    private static List<string> FindAllAudioFiles()
    {
        List<string> audioFiles = new List<string>();
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssets)
        {
            if (IsAudioFile(assetPath) && assetPath.StartsWith("Assets/"))
            {
                audioFiles.Add(assetPath);
            }
        }
        return audioFiles;
    }

    /// <summary>
    /// Kiểm tra xem có phải file audio không
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static bool IsAudioFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return audioExtensions.Contains(extension);
    }

    /// <summary>
    /// Build audio file cụ thể
    /// </summary>
    [MenuItem("AssetBundles/Build Selected Audio")]
    public static void BuildSelectedAudio()
    {
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (IsAudioFile(assetPath))
            {
                CreateOutputDirectory();
                BuildAudioBundle(assetPath);
            }
        }

        AssetDatabase.Refresh();
    }
}