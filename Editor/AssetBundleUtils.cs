using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AssetBundleUtils
{
    // https://discussions.unity.com/t/how-to-build-a-single-asset-bundle/221960/3
    public static void BuildAssetBundlesByName(string[] assetBundleNames, string outputPath, BuildTarget target, CompressionType ct)
    {
        // Argument validation
        if (assetBundleNames == null || assetBundleNames.Length == 0)
        {
            return;
        }

        // Remove duplicates from the input set of asset bundle names to build.
        //assetBundleNames = assetBundleNames.Distinct().ToArray();

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        foreach (string assetBundle in assetBundleNames)
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle);

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetBundle;
            build.assetNames = assetPaths;

            builds.Add(build);
            Debug.Log("assetBundle to build:" + build.assetBundleName);
        }

        switch (ct)
        {
            case CompressionType.None:
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), BuildAssetBundleOptions.UncompressedAssetBundle, target);
                break;

            case CompressionType.LZ4:
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, target);
                break;

            case CompressionType.LZMA:
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), BuildAssetBundleOptions.None, target);
                break;
        }
    }
}