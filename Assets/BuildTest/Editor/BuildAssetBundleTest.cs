using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundleTest : Editor
{
    public static string BundleOutPutPath
    {
        get
        {
            return Application.streamingAssetsPath + "/../AssetBundle/";
        }
    }

    [MenuItem("ZMAssetFrameWork/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        if (!Directory.Exists(BundleOutPutPath))
        {
            Directory.CreateDirectory(BundleOutPutPath);
        }
        BuildPipeline.BuildAssetBundles(BundleOutPutPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        Debug.Log("BuildAssetBundleTest");
    }
}
