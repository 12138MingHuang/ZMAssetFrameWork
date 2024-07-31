using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BuildWindow : OdinMenuEditorWindow
{
    [SerializeField]
    public BuildBundleWindow buildBundleWindow = new BuildBundleWindow();
    
    [SerializeField]
    public BuildHotPatchWindow buildHotPatchWindow = new BuildHotPatchWindow();
    
    [MenuItem("ZMAssetFrameWork/BuildAssetBundleWindow")]
    public static void ShowAssetBundleWindow()
    {
        BuildWindow window = GetWindow<BuildWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(985, 612);
        window.ForceMenuTreeRebuild();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        buildBundleWindow.Initialization();
        buildHotPatchWindow.Initialization();
        OdinMenuTree menuTree = new OdinMenuTree(supportsMultiSelect: false)
        {
            {
                "Build", null, EditorIcons.House
            },
            {
                "Build/AssetBundle", buildBundleWindow, EditorIcons.UnityLogo
            },
            {
                "Build/HotPatch", buildHotPatchWindow, EditorIcons.UnityLogo
            },
            {
                "BuildSetting", BundleSettings.Instance, EditorIcons.SettingsCog
            }
        };
        return menuTree;
    }
    
}