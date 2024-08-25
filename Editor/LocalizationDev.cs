using UnityEditor;
using UnityEngine;

public class LocalizationDev : Editor
{
    [MenuItem("Bean Shootout/Dev/Localization/Set language to en")]
    public static void LanguageEn()
    {
        PlayerPrefs.SetString("selected-locale", "en");
    }

    [MenuItem("Bean Shootout/Dev/Localization/Set language to es")]
    public static void LanguageEs()
    {
        PlayerPrefs.SetString("selected-locale", "es");
    }

    [MenuItem("Bean Shootout/Dev/Localization/Set language to ja")]
    public static void LanguageJa()
    {
        PlayerPrefs.SetString("selected-locale", "ja");
    }
}