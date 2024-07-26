using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BuildWindow : OdinMenuEditorWindow
{
    [MenuItem("ZMAssetFrameWork/BuildAssetBundleWindow")]
    public static void ShowAssetBundleWindow()
    {
        BuildWindow window = GetWindow<BuildWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(958, 612);
        window.ForceMenuTreeRebuild();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree menuTree = new OdinMenuTree(supportsMultiSelect: false)
        {
            {
                "Build", null, EditorIcons.House
            },
        };
        return menuTree;
    }
    
}