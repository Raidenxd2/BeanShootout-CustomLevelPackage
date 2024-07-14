using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Setup : EditorWindow
{
    static Setup()
    {
        Debug.Log("(BeanShootout) Startup");

        if (!File.Exists("Assets/DO_NOT_DELETE_THIS_BeanShootout"))
        {
            ShowWindow();
        }

        if (!File.Exists("Assets/BeanShootoutConfig.asset"))
        {
            CreateConfig();
        }
    }

    [MenuItem("Bean Shootout/Setup...")]
    public static void ShowWindow()
    {
        GetWindow(typeof(Setup));
    }

    private void OnGUI()
    {
        GUILayout.Label("This seems to be your first time using this package. In order to properly use this package, you must setup your project for it. Press the 'Setup/Update' button to setup your project", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("Setup/Update"))
        {
            if (EditorUtility.DisplayDialog("Question", "Are you sure you want to setup your project for this package? WARNING: This will overwrite most Project Settings and the Editor will restart.", "Yes", "No"))
            {
                EditorUtility.DisplayProgressBar("The Great Bean Shootout Custom Level Package", "Setting up...", 0);
                
                Directory.CreateDirectory("Assets/Levels");

                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/DynamicsManager.asset", Application.dataPath + "/../ProjectSettings/DynamicsManager.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/GraphicsSettings.asset", Application.dataPath + "/../ProjectSettings/GraphicsSettings.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/InputManager.asset", Application.dataPath + "/../ProjectSettings/InputManager.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/Physics2DSettings.asset", Application.dataPath + "/../ProjectSettings/Physics2DSettings.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/TagManager.asset", Application.dataPath + "/../ProjectSettings/TagManager.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/TimelineSettings.asset", Application.dataPath + "/../ProjectSettings/TimelineSettings.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/TimeManager.asset", Application.dataPath + "/../ProjectSettings/TimeManager.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/URPProjectSettings.asset", Application.dataPath + "/../ProjectSettings/URPProjectSettings.asset", true);
                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/ProjectSettings~/QualitySettings.asset", Application.dataPath + "/../ProjectSettings/QualitySettings.asset", true);

                File.Copy("Packages/com.onewing.beanshootout-customlevels/Editor/Setup/UniversalRenderPipelineGlobalSettings.asset~", Application.dataPath + "/UniversalRenderPipelineGlobalSettings.asset", true);

                EditorUtility.DisplayDialog("Message", "If a dialog says something about the Input System, press Yes on it.", "OK");

                File.WriteAllText(Application.dataPath + "/DO_NOT_DELETE_THIS_BeanShootout", "");

                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
            }
        }
    }

    public static void CreateConfig()
    {
        Debug.Log("(BeanShootout) Creating config");
        BeanShootoutConfigSO config = ScriptableObject.CreateInstance<BeanShootoutConfigSO>();

        AssetDatabase.CreateAsset(config, "Assets/BeanShootoutConfig.asset");
        AssetDatabase.SaveAssets();
    }
}