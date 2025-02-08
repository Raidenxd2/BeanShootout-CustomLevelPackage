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
        ct = (CompressionType)EditorGUI.EnumPopup(new Rect(3, 3, position.width - 6, 15), new GUIContent("Compression Type", "None: Fastest to load, biggest file size\nLZ4: Fast to load, medium file size\nLZMA (Default): Slowest to load, smallest file size\n\nInternally, the game will save an uncompressed version of the mod to the cache to improve load times."), ct);

        GUILayout.Space(50);

        GUILayout.Label("The built files will be under Assets/Levels/<LEVEL NAME>/Build/level", EditorStyles.wordWrappedLabel);

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

        GUILayout.Space(25);

        if (GUILayout.Button("Cleanup build folders for all levels"))
        {
            if (EditorUtility.DisplayDialog("Question", "Are you sure you want to cleanup build folders for all levels?\nThis will delete all built files.", "Yes", "No"))
            {
                Debug.Log("(BeanShootout) Cleaning build folders");
                EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Cleaning build folders...", 0);

                string[] levelDirs = Directory.GetDirectories("Assets/Levels");
                foreach (var item in levelDirs)
                {
                    if (Directory.Exists(item + "/WindowsBuild"))
                    {
                        File.Delete(item + "/WindowsBuild.meta");
                        Directory.Delete(item + "/WindowsBuild", true);
                    }
                    if (Directory.Exists(item + "/MacBuild"))
                    {
                        File.Delete(item + "/MacBuild.meta");
                        Directory.Delete(item + "/MacBuild", true);
                    }
                    if (Directory.Exists(item + "/LinuxBuild"))
                    {
                        File.Delete(item + "/LinuxBuild.meta");
                        Directory.Delete(item + "/LinuxBuild", true);
                    }
                    if (Directory.Exists(item + "/AndroidBuild"))
                    {
                        File.Delete(item + "/AndroidBuild.meta");
                        Directory.Delete(item + "/AndroidBuild", true);
                    }

                    AssetDatabase.Refresh();
                }

                EditorUtility.ClearProgressBar();
            }
        }

        if (GUILayout.Button("Cleanup build folders for the current level"))
        {
            if (EditorUtility.DisplayDialog("Question", "Are you sure you want to cleanup build folders for the current level?\nThis will delete all built files.", "Yes", "No"))
            {
                Debug.Log("(BeanShootout) Cleaning build folders");
                EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Cleaning build folders...", 0);

                string item = "Assets/Levels/" + EditorSceneManager.GetActiveScene().name;

                if (Directory.Exists(item + "/WindowsBuild"))
                {
                    File.Delete(item + "/WindowsBuild.meta");
                    Directory.Delete(item + "/WindowsBuild", true);
                }
                if (Directory.Exists(item + "/MacBuild"))
                {
                    File.Delete(item + "/MacBuild.meta");
                    Directory.Delete(item + "/MacBuild", true);
                }
                if (Directory.Exists(item + "/LinuxBuild"))
                {
                    File.Delete(item + "/LinuxBuild.meta");
                    Directory.Delete(item + "/LinuxBuild", true);
                }
                if (Directory.Exists(item + "/AndroidBuild"))
                {
                    File.Delete(item + "/AndroidBuild.meta");
                    Directory.Delete(item + "/AndroidBuild", true);
                }

                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
            }
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

        if (AssetImporter.GetAtPath("Assets/Levels/" + SceneName + "/image.png").assetBundleName != SceneName.ToLower() + "_info")
        {
            AssetImporter.GetAtPath("Assets/Levels/" + SceneName + "/image.png").assetBundleName = SceneName.ToLower() + "_info";
            AssetImporter.GetAtPath("Assets/Levels/" + SceneName + "/name.txt").assetBundleName = SceneName.ToLower() + "_info";
        }

        Directory.CreateDirectory("Assets/Levels/" + SceneName + "/" + BuildPathName);
        AssetDatabase.Refresh();

        AssetBundleUtils.BuildAssetBundlesByName(new[] { SceneName.ToLower() + "_ab", SceneName.ToLower() + "_info" }, "Assets/Levels/" + SceneName + "/" + BuildPathName, target, ct);

        EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Finishing build...", 0);

        File.Move("Assets/Levels/" + SceneName + "/" + BuildPathName + "/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level_data.bundle");
        File.Move("Assets/Levels/" + SceneName + "/" + BuildPathName + "/" + SceneName.ToLower() + "_info", "Assets/Levels/" + SceneName + "/level_info.bundle");
        AssetDatabase.Refresh();

        Directory.Delete("Assets/Levels/" + SceneName + "/" + BuildPathName, true);
        AssetDatabase.Refresh();

        Directory.CreateDirectory("Assets/Levels/" + SceneName + "/" + BuildPathName);
        AssetDatabase.Refresh();

        File.Move("Assets/Levels/" + SceneName + "/level_data.bundle", "Assets/Levels/" + SceneName + "/" + BuildPathName + "/level_data.bundle");
        File.Move("Assets/Levels/" + SceneName + "/level_info.bundle", "Assets/Levels/" + SceneName + "/" + BuildPathName + "/level_info.bundle");
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/" + BuildPathName + "/", "OK");
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