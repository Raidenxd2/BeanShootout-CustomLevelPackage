using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class GitPackageInstaller : Editor
{
    [InitializeOnLoadMethod]
    public static void InstallPackages()
    {
        if (!CheckPackageInstalled("com.raiden.sharedshaders"))
        {
            AddPackage("com.raiden.sharedshaders", "https://github.com/Raidenxd2/SharedShaders.git#1.1.0");
        }
    }

    public static void AddPackage(string packageName, string gitURL)
    {
        var path = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        var jsonString = File.ReadAllText(path);
        int indexOfLastBracket = jsonString.IndexOf("}");
        string dependenciesSubstring = jsonString.Substring(0, indexOfLastBracket);
        var endOfLastPackage = dependenciesSubstring.LastIndexOf("\"");
        string oldValue = jsonString.Substring(endOfLastPackage, indexOfLastBracket - endOfLastPackage);
        jsonString = jsonString.Insert(endOfLastPackage + 1,
             $", \n \"{packageName}\": \"{gitURL}\"");
        File.WriteAllText(path, jsonString);
        Client.Resolve();
    }

    public static bool CheckPackageInstalled(string packageName)
    {
        var path = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        var jsonString = File.ReadAllText(path);
        return jsonString.Contains(packageName);
    }
}