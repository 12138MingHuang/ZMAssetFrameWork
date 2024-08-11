using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

/// <summary>
/// AssetBundle热更模式
/// </summary>
public enum BundleHotEnum
{
    /// <summary>
    /// 不热更
    /// </summary>
    NoHot,
    
    /// <summary>
    /// 热更
    /// </summary>
    Hot,
}

[CreateAssetMenu(menuName = "AssetBundleSettings", fileName = "AssetBundleSettings", order = 0)]
public class BundleSettings : ScriptableObject
{
    private static BundleSettings _instance;
    public static BundleSettings Instance
    {
        get => _instance ?? (_instance = Resources.Load<BundleSettings>("AssetBundleSettings"));
    }

    [TitleGroup("AssetBundle打包设置")]
    [LabelText("AssetBundle下载地址")]
    public string AssetBundleDownLoadURL;

    [TitleGroup("AssetBundle打包设置")]
    [LabelText("是否加密AssetBundle")]
    public BundleEncryptToggle bundleEncrypt = new BundleEncryptToggle();

    [TitleGroup("AssetBundle打包设置")]
    [LabelText("资源压缩格式")]
    public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;

    [TitleGroup("AssetBundle打包设置")]
    [LabelText("资源打包平台")]
    public BuildTarget buildTarget = BuildTarget.Android;
    
    [TitleGroup("AssetBundle打包设置")]
    [LabelText("资源热更模式")]
    public BundleHotEnum bundleHotType = BundleHotEnum.Hot;
    
    [TitleGroup("AssetBundle打包设置")]
    [LabelText("最大下载线程数量")]
    public int MAX_THREAD_COUNT;
    
    [Title("AssetBundle热更文件存储路径")]
    private string HotAssetsPath
    {
        get { return Application.persistentDataPath + "/HotAssets/"; }
    }

    [Title("AssetBundle解压文件存储路径")]
    private string BundleDecompressPath

    {
        get { return Application.persistentDataPath + "/DecompressAssets/"; }
    }
    
    [Title("AssetBundle内嵌文件存储路径")]
    private string BuiltinAssetsPath
    {
        get { return Application.streamingAssetsPath + "/AssetBundle/"; }
    }
    
    /// <summary>
    /// 获取热更文件存储路径
    /// </summary>
    /// <param name="bundleModuleEnum">热更资源类型</param>
    /// <returns>热更文件存储路径</returns>
    public string GetHotAssetsPath(BundleModuleEnum bundleModuleEnum)
    {
        return HotAssetsPath + bundleModuleEnum.ToString() + "/";
    }

    /// <summary>
    /// 获取解压文件存储路径
    /// </summary>
    /// <param name="bundleModuleEnum">热更资源类型</param>
    /// <returns>解压文件存储路径</returns>
    public string GetAssetsDecompressPath(BundleModuleEnum bundleModuleEnum)
    {
        return BundleDecompressPath + bundleModuleEnum.ToString() + "/";
    }

    /// <summary>
    /// 获取内嵌文件存储路径
    /// </summary>
    /// <param name="bundleModuleEnum">热更资源类型</param>
    /// <returns>内嵌文件存储路径</returns>
    public string GetAssetsBuiltinBundlePath(BundleModuleEnum bundleModuleEnum)
    {
        return BuiltinAssetsPath + bundleModuleEnum.ToString() + "/";
    }
}

[Serializable]
[Toggle("isEncrypt")]
public class BundleEncryptToggle
{
    //是否加密
    public bool isEncrypt = false;

    [LabelText("加密密钥")]
    public string encryptKey;
}

[Flags]
public enum BuildAssetBundleOptions
{
    /// <summary>
    /// 无任何特殊选项地构建 AssetBundle。
    /// </summary>
    None = 0,
    /// <summary>
    /// 在创建 AssetBundle 时不压缩数据。
    /// </summary>
    UncompressedAssetBundle = 1,
    /// <summary>
    /// [已弃用] 包含所有依赖项。在 Unity 5.0 引入的新 AssetBundle 构建系统中，此选项始终启用。
    /// </summary>
    [Obsolete("此选项已被弃用。在 Unity 5.0 引入的新 AssetBundle 构建系统中，它始终启用。")]
    CollectDependencies = 2,
    /// <summary>
    /// [已弃用] 强制包含整个资源。在 Unity 5.0 引入的新 AssetBundle 构建系统中，此选项始终禁用。
    /// </summary>
    [Obsolete("此选项已被弃用。在 Unity 5.0 引入的新 AssetBundle 构建系统中，它始终禁用。")]
    CompleteAssets = 4,
    /// <summary>
    /// 不在 AssetBundle 中包含类型信息。
    /// </summary>
    DisableWriteTypeTree = 8,
    /// <summary>
    /// [已弃用] 使用存储在 AssetBundle 中的对象的哈希值作为 ID 构建 AssetBundle。在 Unity 5.0 引入的新 AssetBundle 构建系统中，此选项始终启用。
    /// </summary>
    [Obsolete("此选项已被弃用。在 Unity 5.0 引入的新 AssetBundle 构建系统中，它始终启用。")]
    DeterministicAssetBundle = 16, // 0x00000010
    /// <summary>
    /// 强制重新构建 AssetBundles。
    /// </summary>
    ForceRebuildAssetBundle = 32, // 0x00000020
    /// <summary>
    /// 在执行增量构建检查时忽略类型树更改。
    /// </summary>
    IgnoreTypeTreeChanges = 64, // 0x00000040
    /// <summary>
    /// 将哈希值追加到 AssetBundle 名称中。
    /// </summary>
    AppendHashToAssetBundleName = 128, // 0x00000080
    /// <summary>
    /// 在创建 AssetBundle 时使用基于块的 LZ4 压缩。
    /// </summary>
    ChunkBasedCompression = 256, // 0x00000100
    /// <summary>
    /// 如果在构建过程中报告任何错误，则不允许构建成功。
    /// </summary>
    StrictMode = 512, // 0x00000200
    /// <summary>
    /// 执行干运行构建。
    /// </summary>
    DryRunBuild = 1024, // 0x00000400
    /// <summary>
    /// 禁用通过文件名加载 AssetBundle 中的资源。
    /// </summary>
    DisableLoadAssetByFileName = 4096, // 0x00001000
    /// <summary>
    /// 禁用通过带扩展名的文件名加载 AssetBundle 中的资源。
    /// </summary>
    DisableLoadAssetByFileNameWithExtension = 8192, // 0x00002000
    /// <summary>
    /// 在构建过程中从归档文件和序列化文件头中移除 Unity 版本号。
    /// </summary>
    AssetBundleStripUnityVersion = 32768, // 0x00008000
    /// <summary>
    /// 使用 AssetBundle 的内容来计算哈希值。启用此标志可改善增量构建结果，但会强制重建所有之前未启用此标志而构建的现有 AssetBundles。
    /// </summary>
    UseContentHash = 65536, // 0x00010000
    /// <summary>
    /// 当需要递归计算 AssetBundle 依赖项时使用，例如当您具有匹配类型的 Scriptable Objects 的依赖链时。
    /// </summary>
    RecurseDependencies = 131072, // 0x00020000
}

public enum BuildTarget
{
    NoTarget = -2, // 0xFFFFFFFE
    [Obsolete("BlackBerry 已在 5.4 中移除")]
    BB10 = -1, // 0xFFFFFFFF
    [Obsolete("请使用 WSAPlayer 代替 (UnityUpgradable) -> WSAPlayer", true)]
    MetroPlayer = -1, // 0xFFFFFFFF
    /// <summary>
    ///   <para>已过时：请使用 iOS。构建一个 iOS 玩家。</para>
    /// </summary>
    [Obsolete("请使用 iOS 代替 (UnityUpgradable) -> iOS", true)]
    iPhone = -1, // 0xFFFFFFFF
    /// <summary>
    ///   <para>构建一个 macOS 独立（Intel 64-bit）。</para>
    /// </summary>
    StandaloneOSX = 2,
    [Obsolete("请使用 StandaloneOSX 代替 (UnityUpgradable) -> StandaloneOSX", true)]
    StandaloneOSXUniversal = 3,
    /// <summary>
    ///   <para>构建一个 macOS Intel 32-bit 独立。（此构建目标已弃用）</para>
    /// </summary>
    [Obsolete("StandaloneOSXIntel 已在 2017.3 中移除")]
    StandaloneOSXIntel = 4,
    /// <summary>
    ///   <para>构建一个 Windows 独立版本。</para>
    /// </summary>
    StandaloneWindows = 5,
    /// <summary>
    ///   <para>构建一个网页播放器。（此构建目标已弃用，将不再支持构建网页播放器。）</para>
    /// </summary>
    [Obsolete("WebPlayer 已在 5.4 中移除", true)]
    WebPlayer = 6,
    /// <summary>
    ///   <para>构建一个流式网页播放器。</para>
    /// </summary>
    [Obsolete("WebPlayerStreamed 已在 5.4 中移除", true)]
    WebPlayerStreamed = 7,
    /// <summary>
    ///   <para>构建一个 iOS 玩家。</para>
    /// </summary>
    iOS = 9,
    [Obsolete("PS3 已在 >=5.5 中移除")]
    PS3 = 10, // 0x0000000A
    [Obsolete("XBOX360 已在 5.5 中移除")]
    XBOX360 = 11, // 0x0000000B
    /// <summary>
    ///   <para>构建一个 Android .apk 独立应用。</para>
    /// </summary>
    Android = 13, // 0x0000000D
    /// <summary>
    ///   <para>构建一个 Linux 独立版本。</para>
    /// </summary>
    [Obsolete("StandaloneLinux 已在 2019.2 中移除")]
    StandaloneLinux = 17, // 0x00000011
    /// <summary>
    ///   <para>构建一个 Windows 64-bit 独立版本。</para>
    /// </summary>
    StandaloneWindows64 = 19, // 0x00000013
    /// <summary>
    ///   <para>构建到 WebGL 平台。</para>
    /// </summary>
    WebGL = 20, // 0x00000014
    /// <summary>
    ///   <para>构建一个 Windows Store Apps 玩家。</para>
    /// </summary>
    WSAPlayer = 21, // 0x00000015
    /// <summary>
    ///   <para>构建一个 Linux 64-bit 独立版本。</para>
    /// </summary>
    StandaloneLinux64 = 24, // 0x00000018
    /// <summary>
    ///   <para>构建一个 Linux 通用独立版本。</para>
    /// </summary>
    [Obsolete("StandaloneLinuxUniversal 已在 2019.2 中移除")]
    StandaloneLinuxUniversal = 25, // 0x00000019
    [Obsolete("请使用 WSAPlayer 并选择 Windows Phone 8.1")]
    WP8Player = 26, // 0x0000001A
    /// <summary>
    ///   <para>构建一个 macOS Intel 64-bit 独立版本。（此构建目标已弃用）</para>
    /// </summary>
    [Obsolete("StandaloneOSXIntel64 已在 2017.3 中移除")]
    StandaloneOSXIntel64 = 27, // 0x0000001B
    [Obsolete("BlackBerry 已在 5.4 中移除")]
    BlackBerry = 28, // 0x0000001C
    [Obsolete("Tizen 已在 2017.3 中移除")]
    Tizen = 29, // 0x0000001D
    [Obsolete("PSP2 自 2018.3 起不再支持")]
    PSP2 = 30, // 0x0000001E
    /// <summary>
    ///   <para>构建一个 PS4 独立版本。</para>
    /// </summary>
    PS4 = 31, // 0x0000001F
    [Obsolete("PSM 已在 >= 5.3 中移除")]
    PSM = 32, // 0x00000020
    /// <summary>
    ///   <para>构建一个 Xbox One 独立版本。</para>
    /// </summary>
    XboxOne = 33, // 0x00000021
    [Obsolete("SamsungTV 已在 2017.3 中移除")]
    SamsungTV = 34, // 0x00000022
    /// <summary>
    ///   <para>构建到 Nintendo 3DS 平台。</para>
    /// </summary>
    [Obsolete("Nintendo 3DS 自 2018.1 起不再支持")]
    N3DS = 35, // 0x00000023
    [Obsolete("Wii U 支持已在 2018.1 中移除")]
    WiiU = 36, // 0x00000024
    /// <summary>
    ///   <para>构建到 Apple 的 tvOS 平台。</para>
    /// </summary>
    tvOS = 37, // 0x00000025
    /// <summary>
    ///   <para>构建一个 Nintendo Switch 玩家。</para>
    /// </summary>
    Switch = 38, // 0x00000026
    [Obsolete("Lumin 已在 2022.2 中移除")]
    Lumin = 39, // 0x00000027
    /// <summary>
    ///   <para>构建一个 Stadia 独立版本。</para>
    /// </summary>
    Stadia = 40, // 0x00000028
    /// <summary>
    ///   <para>构建一个 CloudRendering 独立版本。</para>
    /// </summary>
    [Obsolete("CloudRendering 已弃用，请使用 LinuxHeadlessSimulation (UnityUpgradable) -> LinuxHeadlessSimulation", false)]
    CloudRendering = 41, // 0x00000029
    /// <summary>
    ///   <para>构建一个 LinuxHeadlessSimulation 独立版本。</para>
    /// </summary>
    LinuxHeadlessSimulation = 41, // 0x00000029
    [Obsolete("GameCoreScarlett 已弃用，请使用 GameCoreXboxSeries (UnityUpgradable) -> GameCoreXboxSeries", false)]
    GameCoreScarlett = 42, // 0x0000002A
    GameCoreXboxSeries = 42, // 0x0000002A
    GameCoreXboxOne = 43, // 0x0000002B
    /// <summary>
    ///   <para>构建到 PlayStation 5 平台。</para>
    /// </summary>
    PS5 = 44, // 0x0000002C
    EmbeddedLinux = 45, // 0x0000002D
    QNX = 46, // 0x0000002E
    /// <summary>
    ///   <para>构建一个 visionOS 玩家。</para>
    /// </summary>
    VisionOS = 47, // 0x0000002F
}
