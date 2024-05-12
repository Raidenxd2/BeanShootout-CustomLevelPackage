using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PlayModeManager : Editor
{
    static PlayModeManager()
    {
        EditorApplication.playModeStateChanged += PlayModeState;
    }

    [MenuItem("Bean Shootout/_Enter Playmode on level")]
    public static void EnterPlaymode()
    {
        EnterPlaymode2();
    }

    public async static void EnterPlaymode2()
    {
        // Reload Domain must be disabled or else this wont work
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        // Wait 100 ms then enter playmode then wait another 100 ms and load the prefab
        await Task.Delay(100);

        EditorApplication.EnterPlaymode();

        await Task.Delay(100);

        GameObject levelBase = AssetDatabase.LoadMainAssetAtPath("Packages/com.onewing.beanshootout-customlevels/Playmode/Prefabs/LevelBase/LevelBase_LocalLevel.prefab") as GameObject;
        Instantiate(levelBase);
    }

    private static void PlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Enable Reload Domain
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        }
    }
}