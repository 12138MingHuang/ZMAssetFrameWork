using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BundleConfig
{
    /// <summary>
    /// 所有AssetsBundle 配置信息列表
    /// </summary>
    public List<BundleInfo> bundleInfoList;
}

/// <summary>
/// AssetsBundle 配置信息
/// </summary>
[System.Serializable]
public class BundleInfo
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public string path;
    
    /// <summary>
    /// crc
    /// </summary>
    public string crc;
    
    /// <summary>
    /// AssetsBundle 名称
    /// </summary>
    public string bundleName;
    
    /// <summary>
    /// 资源名称
    /// </summary>
    public string assetName;
    
    /// <summary>
    /// 依赖项
    /// </summary>
    public List<string> bundleDependce;
}
