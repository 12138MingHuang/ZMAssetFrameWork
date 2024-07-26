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
                _instance = AssetDatabase.LoadAssetAtPath<BuildBundleConfigura>("Assets/ZMAssetsFrameWork/Config/BuildBundleConfigura.asset");
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 模块资源配置
    /// </summary>
    [SerializeField]
    public List<BundleModuleData> AssetBundleConfig = new List<BundleModuleData>();
}
