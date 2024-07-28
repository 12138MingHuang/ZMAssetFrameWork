using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 用于处理模块配置列表和行配置列表，并在Unity编辑器中展示这些配置。
/// </summary>
public class BundleBehaviour
{
    /// <summary>
    /// 模块配置列表。
    /// </summary>
    protected List<BundleModuleData> moduleDataList;

    /// <summary>
    /// 行配置列表，用于在Unity编辑器中按行展示模块配置。
    /// </summary>
    protected List<List<BundleModuleData>> rowModuleDataList;

    /// <summary>
    /// 当前平台。
    /// </summary>
    protected string curPlatfam;

    /// <summary>
    /// 初始化方法，用于获取模块配置列表并构建行配置列表。
    /// </summary>
    public virtual void Initialization()
    {
        // 获取多模块配置列表
        moduleDataList = BuildBundleConfigura.Instance.AssetBundleConfig;
        rowModuleDataList = new List<List<BundleModuleData>>();

        // 遍历模块配置列表，计算模块绘制行数索引，并创建行配置列表
        for (int i = 0; i < moduleDataList.Count; i++)
        {
            int rowIndex = Mathf.FloorToInt(i / 6);
            if (rowIndex + 1 > rowModuleDataList.Count)
            {
                rowModuleDataList.Add(new List<BundleModuleData>());
            }
            rowModuleDataList[rowIndex].Add(moduleDataList[i]);
        }

#if UNITY_IOS
    curPlatfam = "BuildSettings.iPhone";
#elif UNITY_ANDROID
    curPlatfam = "BuildSettings.Android";
#endif
    }

    /// <summary>
    /// 在Unity编辑器中绘制UI界面，展示模块配置列表。
    /// </summary>
    [OnInspectorGUI]
    public virtual void OnGUI()
    {
        if (rowModuleDataList == null)
        {
            return;
        }

        //获取Unity Logo
        GUIContent content = EditorGUIUtility.IconContent("SceneAsset Icon".Trim(), "测试文字显示");
        content.tooltip = "单击可选中火取消\n快速点击可打开配置窗口";

        // 遍历行配置列表，并绘制按钮展示模块配置
        for (int i = 0; i < rowModuleDataList.Count; i++)
        {
            // 开始横向绘制
            GUILayout.BeginHorizontal();
            for (int j = 0; j < rowModuleDataList[i].Count; j++)
            {
                BundleModuleData bundleModuleData = rowModuleDataList[i][j];
                bool isClick = GUILayout.Button(content, GUILayout.Width(130), GUILayout.Height(170));
                if (isClick)
                {
                    // 在此处添加按钮点击后的处理逻辑
                    bundleModuleData.isBuild = !bundleModuleData.isBuild;
                    //检测按钮是否双击
                    if (Time.realtimeSinceStartup - bundleModuleData.lastClickBtnTime <= .18f)
                    {
                        //双击处理逻辑
                        Debug.Log("双击");
                        // bundleModuleData.isBuild = !bundleModuleData.isBuild;
                        // bundleModuleData.lastClickBtnTime = Time.realtimeSinceStartup;
                    }
                }
                GUI.Label(new Rect((j + 1) * 20 + (j * 112), 150 * (i + 1) + (i * 20), 115, 20), bundleModuleData.moduleName, new GUIStyle() { alignment = TextAnchor.MiddleCenter });

                //绘制按钮选中的高亮效果
                if (bundleModuleData.isBuild)
                {
                    GUIStyle guiStyle = UnityEditorUility.GetGUIStyle("LightmapEditorSelectedHighlight");
                    guiStyle.contentOffset = new Vector2(100, -70);
                    GUI.Toggle(new Rect(10 + (j * 133), -160 + 1 * (i + 1) + ((i + 1) * 170), 120, 160), true, EditorGUIUtility.IconContent("Collab"), guiStyle);
                }
            }
            
            //绘制添加资源模块按钮
            if(rowModuleDataList.Count - 1 == i)
            {
                this.DrawAddModuleButton();
            }
            
            // 结束横向绘制
            GUILayout.EndHorizontal();
        }
        
        //绘制添加资源模块按钮
        if (rowModuleDataList.Count == 0)
        {
            this.DrawAddModuleButton();
        }
        
        //绘制打包按钮
        this.DrawBuildButtons();
    }

    /// <summary>
    /// 绘制打包按钮
    /// </summary>
    public virtual void DrawBuildButtons()
    {
        
    }
    
    /// <summary>
    /// 打包资源
    /// </summary>
    public virtual void BuildBundle()
    {
        
    }
    
    /// <summary>
    /// 绘制添加资源模块按钮
    /// </summary>
    public virtual void DrawAddModuleButton()
    {
        
    }
}
