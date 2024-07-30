using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BundleTools
{
    public static string _bundleModuleEnumFilePath = Application.dataPath + "/ZMAssetsFrameWork/Config/BundleModuleEnum.cs";

    [MenuItem("ZMAssetFrameWork/Generate BundleModuleEnum")]
    public static void GenerateBundleModuleEnum()
    {
        string namespacename = "ZMAssetFrameWork";
        string eumname = "BundleModuleEnum";
        
        if(File.Exists(_bundleModuleEnumFilePath))
        {
            File.Delete(_bundleModuleEnumFilePath);
            AssetDatabase.Refresh();
        }
        
        StreamWriter writer = File.CreateText(_bundleModuleEnumFilePath);
        writer.WriteLine("/*");
        writer.WriteLine(" *-------------------------");
        writer.WriteLine(" *Title:AssetBundle模块类");
        writer.WriteLine(" *Author:ZHANGBIN");
        writer.WriteLine(" *Date:" + DateTime.Now);
        writer.WriteLine(" *注意：以下文件是自动生成的，再次生成会覆盖原有的代码，若修改尽量不要自动生成");
        writer.WriteLine(" *--------------------------");
        writer.WriteLine(" */");
        
        writer.WriteLine($"namespace {namespacename}");
        writer.WriteLine("{");
        
        List<BundleModuleData> moduleDataList = BuildBundleConfigura.Instance.AssetBundleConfig;

        if (moduleDataList == null || moduleDataList.Count <= 0)
        {
            return;
        }
        writer.WriteLine($"\tpublic enum {eumname}");
        writer.WriteLine("\t{");
        writer.WriteLine("\t\tNone,");

        for (int i = 0; i < moduleDataList.Count; i++)
        {
            writer.WriteLine($"\t\t{moduleDataList[i].moduleName},");
        }
        writer.WriteLine("\t}");
        writer.WriteLine("}");
        writer.Flush();
        writer.Dispose();
        writer.Close();
        
        AssetDatabase.Refresh();
    }
}
