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

            // Delete the zip if it exists
            if (File.Exists("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip"))
            {
                File.Delete("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip");
                File.Delete("Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip.meta");
                AssetDatabase.Refresh();
            }

            ZipUtility.CompressFolderToZip("Assets/Levels/" + SceneName + "-win64.zip", null, "Assets/Levels/" + SceneName + "/WindowsBuild");

            File.Move("Assets/Levels/" + SceneName + "-win64.zip", "Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip");

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Message", "Created Zip file under Assets/Levels/" + SceneName + "/WindowsBuild/" + SceneName + "-win64.zip", "OK");

            EditorUtility.ClearProgressBar();
        }
    }
}