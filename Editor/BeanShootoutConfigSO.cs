using System.IO;
using UnityEditor;
using UnityEngine;

public class BeanShootoutConfigSO : ScriptableObject
{
    [Tooltip("Used for build and run.")]
    public string GamePath;

    public bool IsValid;
    public string ValidReason;
}

[CustomEditor(typeof(BeanShootoutConfigSO))]
public class BeanShootoutConfigSO_Inspector : Editor
{
    private BeanShootoutConfigSO config;

    private SerializedProperty GamePath;

    private void OnEnable()
    {
        config = (BeanShootoutConfigSO)target;

        GamePath = serializedObject.FindProperty("GamePath");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(GamePath);
        if (GUILayout.Button("...", GUILayout.Width(25)))
        {
            config.GamePath = EditorUtility.OpenFolderPanel("Select folder", "", "");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update"))
        {
            serializedObject.ApplyModifiedProperties();

            bool DataPathExists = false;
            bool SupportFileExists = false;

            if (Directory.Exists(config.GamePath + "/BeanShootout_Data"))
            {
                DataPathExists = true;
            }

            if (!DataPathExists && !Directory.Exists(config.GamePath + "/BeanShootout_VALIDATION_Data"))
            {
                config.IsValid = false;
                config.ValidReason = "Game Path doesn't contain 'BeanShootout_Data', you may not have selected where the game is located.";
                EditorUtility.DisplayDialog("Error", "Game Path doesn't contain 'BeanShootout_Data', you may not have selected where the game is located.", "OK");

                return;
            }

            if (File.Exists(config.GamePath + "/BeanShootout_Data/SUPPORTS_CLP_BAR"))
            {
                SupportFileExists = true;
            }

            if (!SupportFileExists && !File.Exists(config.GamePath + "/BeanShootout_VALIDATION_Data/SUPPORTS_CLP_BAR"))
            {
                config.IsValid = false;
                config.ValidReason = "This game version doesn't support Build and Run, please make sure you have game version 1.0.0-PublicRelease or higher.";
                EditorUtility.DisplayDialog("Error", "This game version doesn't support Build and Run, please make sure you have game version 1.0.0-PublicRelease or higher.", "OK");
                
                return;
            }

            config.IsValid = true;
            config.ValidReason = "";
        }

        serializedObject.ApplyModifiedProperties();
    }
}