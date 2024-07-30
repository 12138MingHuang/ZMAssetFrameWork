using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    
    // 新增标志位，用于控制是否显示删除按钮
    private bool _isNewModule;

    public static void ShowWindow(string moduleName)
    {
        BundleModuleConfigWindow window = GetWindowWithRect<BundleModuleConfigWindow>(new Rect(0, 0, 600, 600));
        window.Show();
        // 更新窗口数据
        if (string.IsNullOrEmpty(moduleName))
        {
            window._isNewModule = true;
            window.moduleName = string.Empty;
            window.prefabPathArr = new string[] { "Path..." };
            window.rootFolderPathArr = new string[] { };
            window.signFolderPathArr = new BundleFileInfo[] { };
        }
        else
        {
            window._isNewModule = false;
            BundleModuleData moduleData = BuildBundleConfigura.Instance.GetBundleDataByName(moduleName: moduleName);
            if (moduleData != null)
            {
                window.moduleName = moduleName;
                window.prefabPathArr = moduleData.prefabPathArr;
                window.rootFolderPathArr = moduleData.rootFolderPathArr;
                window.signFolderPathArr = moduleData.signFolderPathArr;
            }
        }
    }

    [OnInspectorGUI]
    public void DrawSaveConfiguraButton()
    {
        // 绘制保存配置按钮
        if (_isNewModule) GUILayout.BeginArea(new Rect(0, 555, 600, 200));
        else GUILayout.BeginArea(new Rect(0, 510, 600, 200));
        if (GUILayout.Button("Save Config", GUILayout.Height(47)))
        {
            SaveConfiguration();
        }
        GUILayout.EndArea();

        // 绘制删除配置按钮
        GUILayout.BeginArea(new Rect(0, 555, 600, 200));
        if (!_isNewModule && GUILayout.Button("Delete Config", GUILayout.Height(47)))
        {
            bool confirmDelete = EditorUtility.DisplayDialog("确认删除", "是否确认删除该配置？", "确认", "取消");
            if (confirmDelete)
            {
                DeleteConfiguration();
            }
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 删除资源模块配置
    /// </summary>
    private void DeleteConfiguration()
    {
        BuildBundleConfigura.Instance.RemoveModuleByName(moduleName: moduleName);
        EditorUtility.DisplayDialog("删除成功", "配置已经删除", "确定");
        Close();
        BuildWindow.ShowAssetBundleWindow();
        GUIUtility.ExitGUI(); // 退出当前GUI执行
    }
    
    /// <summary>
    /// 存储资源模块配置
    /// </summary>
    private void SaveConfiguration()
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            EditorUtility.DisplayDialog("保存失败", "资源模块名称不能为空", "确定");
            return;
        }
        
        BundleModuleData moduleData = BuildBundleConfigura.Instance.GetBundleDataByName(moduleName: moduleName);

        if (moduleData == null)
        {
            // 添加新的模块资源
            moduleData = new BundleModuleData
            {
                moduleName = moduleName,
                prefabPathArr = prefabPathArr,
                rootFolderPathArr = rootFolderPathArr,
                signFolderPathArr = signFolderPathArr
            };
            BuildBundleConfigura.Instance.SaveModuleData(moduleData);
        }
        else
        {
            // 更新模块资源
            moduleData.prefabPathArr = prefabPathArr;
            moduleData.rootFolderPathArr = rootFolderPathArr;
            moduleData.signFolderPathArr = signFolderPathArr;
        }
        EditorUtility.DisplayDialog("保存成功!", "配置已经保存", "确定");
        Close();
        BuildWindow.ShowAssetBundleWindow();
        GUIUtility.ExitGUI(); // 退出当前GUI执行
    }
}
