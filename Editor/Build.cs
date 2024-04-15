using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Build : EditorWindow
{
    CompressionType ct = CompressionType.LZ4;

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

        if (GUILayout.Button("Build for Windows"))
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

            BuildAssetBundle(ct, BuildTarget.StandaloneWindows64, SceneName);

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
    }

    private void BuildAssetBundle(CompressionType ct, BuildTarget target, string SceneName)
    {
        switch (ct)
        {
            case CompressionType.None:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/WindowsBuild", BuildAssetBundleOptions.UncompressedAssetBundle, target);
                break;

            case CompressionType.LZ4:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/WindowsBuild", BuildAssetBundleOptions.ChunkBasedCompression, target);
                break;

            case CompressionType.LZMA:
                BuildPipeline.BuildAssetBundles("Assets/Levels/" + SceneName + "/WindowsBuild", BuildAssetBundleOptions.None, target);
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