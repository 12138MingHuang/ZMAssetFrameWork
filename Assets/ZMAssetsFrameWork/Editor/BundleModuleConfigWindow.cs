using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleModuleConfigWindow : OdinEditorWindow
{
    [PropertySpace(spaceBefore: 5f, spaceAfter: 5f)]
    [Required("请输入资源模块名称")]
    [GUIColor(.3f, .8f, .8f, 1f)]
    [LabelText("资源模块名称：")]
    public string moduleName;

    [ReadOnly]
    [HideLabel]
    [TabGroup("预制体包")]
    [DisplayAsString]
    public string prefabTabel = "该文件夹下的所有预制体都会单独打成一个AssetBundle";

    [ReadOnly]
    [HideLabel]
    [TabGroup("文件夹子包")]
    [DisplayAsString]
    public string rootFolderSubBundle = "该文件夹下的所有子文件夹都会单独打成一个AssetBundle";
    
    [ReadOnly]
    [HideLabel]
    [TabGroup("单个补丁包")]
    [DisplayAsString]
    public string signBundle = "指定文件夹会单独打成一个AssetBundle";

    [FolderPath]
    [TabGroup("预制体包")]
    [LabelText("预制体资源路径配置")]
    public string[] prefabPathArr = new string[] { "Path..." };
    
    [FolderPath]
    [TabGroup("文件夹子包")]
    [LabelText("文件夹子包资源路径配置")]
    public string[] rootFolderPathArr = new string[] { };
    
    [TabGroup("单个补丁包")]
    [LabelText("单个补丁包资源路径配置")]
    public BundleFileInfo[] signFolderPathArr = new BundleFileInfo[] { };
    
    public static void ShowWindow(string moduleName)
    {
        BundleModuleConfigWindow window = GetWindowWithRect<BundleModuleConfigWindow>(new Rect(0, 0, 600, 600));
        window.Show();
        //更新窗口数据
    }
}
