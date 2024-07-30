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
    public uint crc;
    
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

public class BuiltinBundleInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string fileName;

    /// <summary>
    /// 校验本地文件已解压文件是否与包内文件一致，如果不一致，说明本地文件被篡改
    /// 我们需要进行重新解压（需要进行检验校验的前提是 当前解压的模块没有开启热更）
    /// </summary>
    public string md5;

    /// <summary>
    /// 文件大小，用来计算文件解压进度显示
    /// </summary>
    public float size;
}
