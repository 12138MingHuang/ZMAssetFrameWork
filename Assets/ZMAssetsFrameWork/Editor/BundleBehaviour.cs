using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// 在Unity编辑器中绘制UI界面，展示模块配置列表。
    /// </summary>
    [OnInspectorGUI]
    public virtual void OnGUI()
    {
        // 遍历行配置列表，并绘制按钮展示模块配置
        for (int i = 0; i < rowModuleDataList.Count; i++)
        {
            // 开始横向绘制
            GUILayout.BeginHorizontal();
            for (int j = 0; j < rowModuleDataList[i].Count; j++)
            {
                BundleModuleData bundleModuleData = rowModuleDataList[i][j];
                bool isClick = GUILayout.Button("123123", GUILayout.Width(130), GUILayout.Height(170));
                if (isClick)
                {
                    // 在此处添加按钮点击后的处理逻辑
                }
            }
        }

        // 结束横向绘制
        GUILayout.EndHorizontal();
    }
}


