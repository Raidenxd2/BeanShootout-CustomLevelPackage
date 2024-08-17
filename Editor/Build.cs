using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using Debug = UnityEngine.Debug;
public class Build : EditorWindow
{
    CompressionType ct = CompressionType.LZMA;

    [MenuItem("Bean Shootout/Build Level")]
    public static void ShowWindow()
    {
        EditorWindow win = GetWindow(typeof(Build));
        win.titleContent = new GUIContent("Build Level");
    }

    private void OnGUI()
    {
        ct = (CompressionType)EditorGUI.EnumPopup(new Rect(3, 3, position.width - 6, 15), new GUIContent("Compression Type", "None: Fastest to load, biggest file size\nLZ4: Fast to load, medium file size\nLZMA (Default): Slowest to load, smallest file size"), ct);

        GUILayout.Space(50);

        GUILayout.Label("The built files will be under Assets/Levels/<LEVEL NAME>/Build/level");

        if (GUILayout.Button("Build for Windows"))
        {
            Debug.Log("(BeanShootout) Building level");

            BuildLevel(ct, BuildTarget.StandaloneWindows64, "WindowsBuild");
        }

        if (GUILayout.Button("Build and Run for Windows"))
        {
            BeanShootoutConfigSO config = AssetDatabase.LoadAssetAtPath<BeanShootoutConfigSO>("Assets/BeanShootoutConfig.asset");

            if (string.IsNullOrEmpty(config.GamePath))
            {
                EditorUtility.DisplayDialog("Error", "Please set a Game Path in the config!", "OK");
                return;
            }

            if (!config.IsValid)
            {
                EditorUtility.DisplayDialog("Error", "Game Path isn't valid.\n" + config.ValidReason, "OK");
                return;
            }

            BuildLevel(ct, BuildTarget.StandaloneWindows64, "WindowsBuild");

            string SceneName = EditorSceneManager.GetActiveScene().name;

            Debug.Log("(BeanShootout) Running level");

            EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Running level...", 0);

            string LevelLocalBuildFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "OneWing", "The Great Bean Shootout", "Mods", "LevelLocalBuild") + "\\";
            Debug.Log("(BeanShootout) LevelLocalBuildFolder: "+ LevelLocalBuildFolder);

            File.Copy("Assets/Levels/" + SceneName + "/WindowsBuild/level", LevelLocalBuildFolder + "level", true);
            File.Copy("Assets/Levels/" + SceneName + "/WindowsBuild/image.png", LevelLocalBuildFolder + "image.png", true);
            File.Copy("Assets/Levels/" + SceneName + "/WindowsBuild/name.txt", LevelLocalBuildFolder + "name.txt", true);

            Process GameProcess = new();
            GameProcess.StartInfo.FileName = config.GamePath + "/BeanShootout.exe";
            GameProcess.StartInfo.Arguments = "-loadlevellocalbuild -gs_fnop " + config.FullscreenWhenTheresNoOtherPlayers + " -gs_sm " + config.ShowMinimap + " -gs_ma " + config.MaxAmmo + " -ga_mp " + config.MaxPlayers;
            GameProcess.Start();

            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("Build for Mac"))
        {
            Debug.Log("(BeanShootout) Building level");

            BuildLevel(ct, BuildTarget.StandaloneOSX, "MacBuild");
        }

        if (GUILayout.Button("Build for Linux"))
        {
            Debug.Log("(BeanShootout) Building level");

            BuildLevel(ct, BuildTarget.StandaloneLinux64, "LinuxBuild");
        }

        if (GUILayout.Button("Build for Android"))
        {
            Debug.Log("(BeanShootout) Building level");

            BuildLevel(ct, BuildTarget.Android, "AndroidBuild");
        }
    }

    private void BuildLevel(CompressionType ct, BuildTarget target, string BuildPathName)
    {
        EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Preparing build...", 0);

        string SceneName = EditorSceneManager.GetActiveScene().name;

        if (Directory.Exists("Assets/Levels/" + SceneName + "/" + BuildPathName))
        {
            Directory.Delete("Assets/Levels/" + SceneName + "/" + BuildPathName, true);
            AssetDatabase.Refresh();
        }

        Directory.CreateDirectory("Assets/Levels/" + SceneName + "/" + BuildPathName);
        AssetDatabase.Refresh();

        AssetBundleUtils.BuildAssetBundlesByName(new[] { SceneName + "_ab" }, "Assets/Levels/" + SceneName + "/" + BuildPathName, target, ct);

        EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Finishing build...", 0);

        File.Move("Assets/Levels/" + SceneName + "/" + BuildPathName + "/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level");
        AssetDatabase.Refresh();

        Directory.Delete("Assets/Levels/" + SceneName + "/" + BuildPathName, true);
        AssetDatabase.Refresh();

        Directory.CreateDirectory("Assets/Levels/" + SceneName + "/" + BuildPathName);
        AssetDatabase.Refresh();

        File.Move("Assets/Levels/" + SceneName + "/level", "Assets/Levels/" + SceneName + "/" + BuildPathName + "/level");
        File.Copy("Assets/Levels/" + SceneName + "/image.png", "Assets/Levels/" + SceneName + "/" + BuildPathName + "/image.png");
        File.Copy("Assets/Levels/" + SceneName + "/name.txt", "Assets/Levels/" + SceneName + "/" + BuildPathName + "/name.txt");
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/" + BuildPathName + "/level", "OK");
    }

    private void BuildAssetBundle(CompressionType ct, BuildTarget target, string SceneName, string BuildPathName)
    {
        switch (ct)
        {
            case CompressionType.None:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/" + BuildPathName, BuildAssetBundleOptions.UncompressedAssetBundle, target);
                break;

            case CompressionType.LZ4:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/" + BuildPathName, BuildAssetBundleOptions.ChunkBasedCompression, target);
                break;

            case CompressionType.LZMA:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/" + BuildPathName, BuildAssetBundleOptions.None, target);
                break;
        }

        AssetDatabase.Refresh();
    }
}

public enum CompressionType
{
    None,
    LZ4,
    LZMA
}