using UnityEngine;
using UnityEditor;
using Unity.SharpZipLib.Utils;
using UnityEditor.SceneManagement;
using System.IO;

public class Publishing : EditorWindow
{
    [MenuItem("Bean Shootout/Publishing")]
    public static void ShowWindow()
    {
        EditorWindow win = GetWindow(typeof(Publishing));
        win.titleContent = new GUIContent("Publishing");
    }

    private void OnGUI()
    {
        GUILayout.Label("The Zip will be created under Assets/Levels/<LEVEL NAME>/Build/<LEVEL NAME>.zip");

        if (GUILayout.Button("Create Zip for Windows x64"))
        {
            EditorUtility.DisplayProgressBar("Bean Shootout Custom Level Package", "Creating Zip file...", 0);

            string SceneName = EditorSceneManager.GetActiveScene().name;

            // Delete the zip and meta files if it exists
            if (File.Exists("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip"))
            {
                File.Delete("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip");
                File.Delete("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip.meta");
                AssetDatabase.Refresh();
            }

            ZipUtility.CompressFolderToZip("Assets/Levels/" + SceneName + "-win64.zip", null, "Assets/Levels/" + SceneName + "/WindowsBuild");

            File.Move("Assets/Levels/" + SceneName + "-win64.zip", "Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Message", "Created Zip file under 'Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip'", "OK");

            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("Create Zip for Mac"))
        {
            EditorUtility.DisplayProgressBar("Bean Shootout Custom Level Package", "Creating Zip file...", 0);

            string SceneName = EditorSceneManager.GetActiveScene().name;

            // Delete the zip and meta files if it exists
            if (File.Exists("Assets/Levels/" + SceneName + "/MacBuild/" + SceneName + "-mac.zip"))
            {
                File.Delete("Assets/Levels/" + SceneName + "/MacBuild/" + SceneName + "-mac.zip");
                File.Delete("Assets/Levels/" + SceneName + "/MacBuild/" + SceneName + "-mac.zip.meta");
                AssetDatabase.Refresh();
            }

            ZipUtility.CompressFolderToZip("Assets/Levels/" + SceneName + "-mac.zip", null, "Assets/Levels/" + SceneName + "/MacBuild");

            File.Move("Assets/Levels/" + SceneName + "-mac.zip", "Assets/Levels/" + SceneName + "/MacBuild/" + SceneName + "-mac.zip");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Message", "Created Zip file under 'Assets/Levels/" + SceneName + "/MacBuild/" + SceneName + "-mac.zip'", "OK");

            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("Create Zip for Linux x64"))
        {
            EditorUtility.DisplayProgressBar("Bean Shootout Custom Level Package", "Creating Zip file...", 0);

            string SceneName = EditorSceneManager.GetActiveScene().name;

            // Delete the zip and meta files if it exists
            if (File.Exists("Assets/Levels/" + SceneName + "/LinuxBuild/" + SceneName + "-linux.zip"))
            {
                File.Delete("Assets/Levels/" + SceneName + "/LinuxBuild/" + SceneName + "-linux.zip");
                File.Delete("Assets/Levels/" + SceneName + "/LinuxBuild/" + SceneName + "-linux.zip.meta");
                AssetDatabase.Refresh();
            }

            ZipUtility.CompressFolderToZip("Assets/Levels/" + SceneName + "-linux.zip", null, "Assets/Levels/" + SceneName + "/LinuxBuild");

            File.Move("Assets/Levels/" + SceneName + "-linux.zip", "Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-linux.zip");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Message", "Created Zip file under 'Assets/Levels/" + SceneName + "/LinuxBuild/" + SceneName + "-linux.zip'", "OK");

            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("Create Zip for Android"))
        {
            EditorUtility.DisplayProgressBar("Bean Shootout Custom Level Package", "Creating Zip file...", 0);

            string SceneName = EditorSceneManager.GetActiveScene().name;

            // Delete the zip and meta files if it exists
            if (File.Exists("Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName + "-android.zip"))
            {
                File.Delete("Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName + "-android.zip");
                File.Delete("Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName + "-android.zip.meta");
                AssetDatabase.Refresh();
            }

            ZipUtility.CompressFolderToZip("Assets/Levels/" + SceneName + "-android.zip", null, "Assets/Levels/" + SceneName + "/AndroidBuild");

            File.Move("Assets/Levels/" + SceneName + "-android.zip", "Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName + "-android.zip");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Message", "Created Zip file under 'Assets/Levels/" + SceneName + "/AndroidBuild/" + SceneName + "-android.zip'", "OK");

            EditorUtility.ClearProgressBar();
        }
    }
}