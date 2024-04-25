using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
        ct = (CompressionType)EditorGUI.EnumPopup(new Rect(3, 3, position.width - 6, 15), new GUIContent("Compression Type"), ct);

        GUILayout.Space(50);

        GUILayout.Label("The built files will be under Assets/Levels/<LEVEL NAME>/Build/level");

        if (GUILayout.Button("Build for Windows x64"))
        {
            Debug.Log("(BeanShootout) Building level");

            string SceneName = EditorSceneManager.GetActiveScene().name;

            if (Directory.Exists("Assets/Levels/" + SceneName + "/WindowsBuild"))
            {
                Directory.Delete("Assets/Levels/" + SceneName + "/WindowsBuild", true);
                AssetDatabase.Refresh();
            }

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/WindowsBuild");
            AssetDatabase.Refresh();

            BuildAssetBundle(ct, BuildTarget.StandaloneWindows64, SceneName, "WindowsBuild");

            File.Move("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level");
            AssetDatabase.Refresh();

            Directory.Delete("Assets/Levels/" + SceneName + "/WindowsBuild", true);
            AssetDatabase.Refresh();

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/WindowsBuild");
            AssetDatabase.Refresh();

            File.Move("Assets/Levels/" + SceneName + "/level", "Assets/Levels/" + SceneName + "/WindowsBuild/level");
            File.Copy("Assets/Levels/" + SceneName + "/image.png", "Assets/Levels/" + SceneName + "/WindowsBuild/image.png");
            File.Copy("Assets/Levels/" + SceneName + "/name.txt", "Assets/Levels/" + SceneName + "/WindowsBuild/name.txt");
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/WindowsBuild/level", "OK");
        }

        if (GUILayout.Button("Build for Mac"))
        {
            Debug.Log("(BeanShootout) Building level");

            string SceneName = EditorSceneManager.GetActiveScene().name;

            if (Directory.Exists("Assets/Levels/" + SceneName + "/MacBuild"))
            {
                Directory.Delete("Assets/Levels/" + SceneName + "/MacBuild", true);
                AssetDatabase.Refresh();
            }

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/MacBuild");
            AssetDatabase.Refresh();

            BuildAssetBundle(ct, BuildTarget.StandaloneOSX, SceneName, "MacBuild");

            File.Move("Assets/Levels/" + SceneName + "/MacBuild/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level");
            AssetDatabase.Refresh();

            Directory.Delete("Assets/Levels/" + SceneName + "/MacBuild", true);
            AssetDatabase.Refresh();

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/MacBuild");
            AssetDatabase.Refresh();

            File.Move("Assets/Levels/" + SceneName + "/level", "Assets/Levels/" + SceneName + "/MacBuild/level");
            File.Copy("Assets/Levels/" + SceneName + "/image.png", "Assets/Levels/" + SceneName + "/MacBuild/image.png");
            File.Copy("Assets/Levels/" + SceneName + "/name.txt", "Assets/Levels/" + SceneName + "/MacBuild/name.txt");
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/MacBuild/level", "OK");
        }

        if (GUILayout.Button("Build for Linux x64"))
        {
            Debug.Log("(BeanShootout) Building level");

            string SceneName = EditorSceneManager.GetActiveScene().name;

            if (Directory.Exists("Assets/Levels/" + SceneName + "/LinuxBuild"))
            {
                Directory.Delete("Assets/Levels/" + SceneName + "/LinuxBuild", true);
                AssetDatabase.Refresh();
            }

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/LinuxBuild");
            AssetDatabase.Refresh();

            BuildAssetBundle(ct, BuildTarget.StandaloneLinux64, SceneName, "LinuxBuild");

            File.Move("Assets/Levels/" + SceneName + "/LinuxBuild/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level");
            AssetDatabase.Refresh();

            Directory.Delete("Assets/Levels/" + SceneName + "/LinuxBuild", true);
            AssetDatabase.Refresh();

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/LinuxBuild");
            AssetDatabase.Refresh();

            File.Move("Assets/Levels/" + SceneName + "/level", "Assets/Levels/" + SceneName + "/LinuxBuild/level");
            File.Copy("Assets/Levels/" + SceneName + "/image.png", "Assets/Levels/" + SceneName + "/LinuxBuild/image.png");
            File.Copy("Assets/Levels/" + SceneName + "/name.txt", "Assets/Levels/" + SceneName + "/LinuxBuild/name.txt");
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/LinuxBuild/level", "OK");
        }

        if (GUILayout.Button("Build for Android"))
        {
            Debug.Log("(BeanShootout) Building level");

            string SceneName = EditorSceneManager.GetActiveScene().name;

            if (Directory.Exists("Assets/Levels/" + SceneName + "/AndroidBuild"))
            {
                Directory.Delete("Assets/Levels/" + SceneName + "/AndroidBuild", true);
                AssetDatabase.Refresh();
            }

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/AndroidBuild");
            AssetDatabase.Refresh();

            BuildAssetBundle(ct, BuildTarget.Android, SceneName, "AndroidBuild");

            File.Move("Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName.ToLower() + "_ab", "Assets/Levels/" + SceneName + "/level");
            AssetDatabase.Refresh();

            Directory.Delete("Assets/Levels/" + SceneName + "/AndroidBuild", true);
            AssetDatabase.Refresh();

            Directory.CreateDirectory("Assets/Levels/" + SceneName + "/AndroidBuild");
            AssetDatabase.Refresh();

            File.Move("Assets/Levels/" + SceneName + "/level", "Assets/Levels/" + SceneName + "/AndroidBuild/level");
            File.Copy("Assets/Levels/" + SceneName + "/image.png", "Assets/Levels/" + SceneName + "/AndroidBuild/image.png");
            File.Copy("Assets/Levels/" + SceneName + "/name.txt", "Assets/Levels/" + SceneName + "/AndroidBuild/name.txt");
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Message", "Build created under Assets/Levels/" + SceneName + "/AndroidBuild/level", "OK");
        }
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