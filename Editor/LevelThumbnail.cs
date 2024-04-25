using UnityEngine;
using UnityEditor;

public class LevelThumbnail : EditorWindow
{
    [MenuItem("Bean Shootout/Create Level Thumbnail")]
    public static void ShowWindow()
    {
        EditorWindow win = GetWindow(typeof(LevelThumbnail));
        win.titleContent = new GUIContent("Level Thumbnail Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("a");
    }
}