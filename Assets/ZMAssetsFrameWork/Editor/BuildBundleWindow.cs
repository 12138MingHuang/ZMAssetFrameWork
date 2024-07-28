using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildBundleWindow : BundleBehaviour
{
    protected string[] buildButtonsNameArr = new string[] { "打包资源", "内嵌资源" };
    
    public override void Initialization()
    {
        base.Initialization();
    }

    /// <summary>
    /// 绘制添加资源模块按钮
    /// </summary>
    public override void DrawAddModuleButton()
    {
        base.DrawAddModuleButton();

        GUIContent addContent = EditorGUIUtility.IconContent("CollabCreate Icon".Trim(), "");
        bool isClick = GUILayout.Button(addContent, GUILayout.Width(130), GUILayout.Height(170));
        if (isClick)
        {
            // TODO 编写添加模块代码
        }
    }

    public override void DrawBuildButtons()
    {
        base.DrawBuildButtons();
        
        GUILayout.BeginArea(new Rect(0, 555 ,800, 600));
        
        GUILayout.BeginHorizontal();

        for (int i = 0; i < buildButtonsNameArr.Length; i++)
        {
            GUIStyle guiStyle = UnityEditorUility.GetGUIStyle("PreButtonBlue");
            guiStyle.fixedHeight = 55;
            
            if(GUILayout.Button(buildButtonsNameArr[i], guiStyle, GUILayout.Width(400)))
            {
                if(i == 0)
                {
                    //打包AssetBundle按钮事件
                    this.BuildBundle();
                }
                else
                {
                    CopyBundleToStreamingAssetsPath();
                }
            }
        }
        
        //打包图标绘制
        GUI.DrawTexture(new Rect(130, 13, 30, 30), EditorGUIUtility.IconContent(curPlatfam).image);
        //内嵌资源图标绘制
        GUI.DrawTexture(new Rect(545, 13, 30, 30), EditorGUIUtility.IconContent("SceneSet icon".Trim()).image);
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndArea();
    }

    public override void BuildBundle()
    {
        base.BuildBundle();

        foreach (BundleModuleData item in moduleDataList)
        {
            if (item.isBuild)
            {
                // TODO 编写打包代码
            }
        }
    }
    
    /// <summary>
    /// 内嵌资源
    /// </summary>
    public void CopyBundleToStreamingAssetsPath()
    {
        foreach (BundleModuleData item in moduleDataList)
        {
            if (item.isBuild)
            {
                // TODO 内嵌资源代码
            }
        }
    }
}
