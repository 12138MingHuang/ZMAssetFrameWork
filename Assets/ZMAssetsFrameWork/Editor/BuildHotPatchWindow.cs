using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZMAssetsFrameWork;

public class BuildHotPatchWindow : BundleBehaviour
{
    protected string[] buildButtonsNameArr = new string[] { "打包热更", "上传资源" };
    
    //热更描述 热更公告
    [HideInInspector]
    public string patchDes = "输入本次热更描述...";
    
    //热更补丁版本号
    [HideInInspector]
    public string hotVersion = "1.0";
    
    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        
        GUILayout.BeginArea(new Rect(0, 400, 800, 600));
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("请输入本次热更公告");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        patchDes = GUILayout.TextField(patchDes, GUILayout.Width(800), GUILayout.Height(80));
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        hotVersion = EditorGUILayout.TextField("热更版本号", hotVersion, GUILayout.Width(800), GUILayout.Height(24));
        GUILayout.EndHorizontal();
        
        GUILayout.EndArea();
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
        GUI.DrawTexture(new Rect(545, 13, 30, 30), EditorGUIUtility.IconContent("CollabPush".Trim()).image);
        
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
                BuildBundleCompiler.BuildAssetBundle(item, BuildType.HotPatch, int.Parse(hotVersion), patchDes);
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
