using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BundleModuleData
{
    //AssetBundle模块id
    public long bundleId;
    //AssetBundle模块名称
    public string moduleName;
    //是否打包
    public bool isBuild;
    
    //上一次点击按钮的时间
    public float lastClickBtnTime;

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