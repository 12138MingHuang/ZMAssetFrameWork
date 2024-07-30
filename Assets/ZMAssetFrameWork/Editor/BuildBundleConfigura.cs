#if UNITY_EDITOR //编辑器下才使用,因为该文件所在文件夹不是Editor文件夹，所以需要加#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AssetBundle", fileName = "BuildBundleConfigura", order = 4)]
public class BuildBundleConfigura : ScriptableObject
{
    private static BuildBundleConfigura _instance;
    public static BuildBundleConfigura Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<BuildBundleConfigura>("Assets/ZMAssetFrameWork/Config/BuildBundleConfigura.asset");
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 模块资源配置
    /// </summary>
    [SerializeField]
    public List<BundleModuleData> AssetBundleConfig = new List<BundleModuleData>();

    /// <summary>
    /// 根据模块名称获取模块资源数据
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    /// <returns>模块资源数据</returns>
    public BundleModuleData GetBundleDataByName(string moduleName)
    {
        foreach (var item in AssetBundleConfig)
        {
            if(string.Equals(item.moduleName,moduleName))
            {
                return item;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 通过模块名称移除模块资源数据
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    public void RemoveModuleByName(string moduleName)
    {
        for (int i = 0; i < AssetBundleConfig.Count; i++)
        {
            if(AssetBundleConfig[i].moduleName == moduleName)
            {
                AssetBundleConfig.RemoveAt(i);
                break;
            }
        }
    }
    
    /// <summary>
    /// 存储新的模块数据
    /// </summary>
    /// <param name="moduleData"></param>
    public void SaveModuleData(BundleModuleData moduleData)
    {
        AssetBundleConfig.Add(moduleData);
        Save();
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public void Save()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}
