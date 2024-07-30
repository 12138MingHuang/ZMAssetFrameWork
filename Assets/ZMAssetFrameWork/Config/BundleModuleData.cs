using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BundleModuleData
{
    /// <summary>
    /// AssetBundle模块id
    /// </summary>
    public long bundleId;
    
    /// <summary>
    /// AssetBundle模块名称
    /// </summary>
    public string moduleName;
    
    /// <summary>
    /// 是否打包
    /// </summary>
    public bool isBuild;
    
    /// <summary>
    /// 上一次点击按钮的时间
    /// </summary>
    public float lastClickBtnTime;
    
    /// <summary>
    /// 预制体资源路径配置
    /// </summary>
    public string[] prefabPathArr;
    
    /// <summary>
    /// 文件夹子包资源路径配置
    /// </summary>
    public string[] rootFolderPathArr;
    
    /// <summary>
    /// 单个补丁包资源路径配置
    /// </summary>
    public BundleFileInfo[] signFolderPathArr;

}

[System.Serializable]
public class BundleFileInfo
{
    [HideLabel]
    public string abName = "AB Name...";
    
    [HideLabel]
    [FolderPath]
    public string bundlePath = "BundlePath...";
}