using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    ///   <para>Build assetBundle without any special option.</para>
    /// </summary>
    None = 0,
    /// <summary>
    ///   <para>Don't compress the data when creating the AssetBundle.</para>
    /// </summary>
    UncompressedAssetBundle = 1,
    /// <summary>
    ///   <para>Includes all dependencies.</para>
    /// </summary>
    [Obsolete("This has been made obsolete. It is always enabled in the new AssetBundle build system introduced in 5.0.")]
    CollectDependencies = 2,
    /// <summary>
    ///   <para>Forces inclusion of the entire asset.</para>
    /// </summary>
    [Obsolete("This has been made obsolete. It is always disabled in the new AssetBundle build system introduced in 5.0.")]
    CompleteAssets = 4,
    /// <summary>
    ///   <para>Do not include type information within the AssetBundle.</para>
    /// </summary>
    DisableWriteTypeTree = 8,
    /// <summary>
    ///   <para>Builds an asset bundle using a hash for the id of the object stored in the asset bundle.</para>
    /// </summary>
    [Obsolete("This has been made obsolete. It is always enabled in the new AssetBundle build system introduced in 5.0.")]
    DeterministicAssetBundle = 16, // 0x00000010
    /// <summary>
    ///   <para>Force rebuild the assetBundles.</para>
    /// </summary>
    ForceRebuildAssetBundle = 32, // 0x00000020
    /// <summary>
    ///   <para>Ignore the type tree changes when doing the incremental build check.</para>
    /// </summary>
    IgnoreTypeTreeChanges = 64, // 0x00000040
    /// <summary>
    ///   <para>Append the hash to the assetBundle name.</para>
    /// </summary>
    AppendHashToAssetBundleName = 128, // 0x00000080
    /// <summary>
    ///   <para>Use chunk-based LZ4 compression when creating the AssetBundle.</para>
    /// </summary>
    ChunkBasedCompression = 256, // 0x00000100
    /// <summary>
    ///   <para>Do not allow the build to succeed if any errors are reporting during it.</para>
    /// </summary>
    StrictMode = 512, // 0x00000200
    /// <summary>
    ///   <para>Do a dry run build.</para>
    /// </summary>
    DryRunBuild = 1024, // 0x00000400
    /// <summary>
    ///   <para>Disables Asset Bundle LoadAsset by file name.</para>
    /// </summary>
    DisableLoadAssetByFileName = 4096, // 0x00001000
    /// <summary>
    ///   <para>Disables Asset Bundle LoadAsset by file name with extension.</para>
    /// </summary>
    DisableLoadAssetByFileNameWithExtension = 8192, // 0x00002000
    /// <summary>
    ///   <para>Removes the Unity Version number in the Archive File &amp; Serialized File headers during the build.</para>
    /// </summary>
    AssetBundleStripUnityVersion = 32768, // 0x00008000
    /// <summary>
    ///   <para>Use the content of the asset bundle to calculate the hash. Enabling this flag is recommended to improve incremental build results, but it will force a rebuild of all existing AssetBundles that have been built without the flag.</para>
    /// </summary>
    UseContentHash = 65536, // 0x00010000
    /// <summary>
    ///   <para>Use when AssetBundle dependencies need to be calculated recursively, such as when you have a dependency chain of matching typed Scriptable Objects.</para>
    /// </summary>
    RecurseDependencies = 131072, // 0x00020000
}

public enum BuildTarget
{
    NoTarget = -2, // 0xFFFFFFFE
    [Obsolete("BlackBerry has been removed in 5.4")]
    BB10 = -1, // 0xFFFFFFFF
    [Obsolete("Use WSAPlayer instead (UnityUpgradable) -> WSAPlayer", true)]
    MetroPlayer = -1, // 0xFFFFFFFF
    /// <summary>
    ///   <para>OBSOLETE: Use iOS. Build an iOS player.</para>
    /// </summary>
    [Obsolete("Use iOS instead (UnityUpgradable) -> iOS", true)]
    iPhone = -1, // 0xFFFFFFFF
    /// <summary>
    ///   <para>Build a macOS standalone (Intel 64-bit).</para>
    /// </summary>
    StandaloneOSX = 2, [Obsolete("Use StandaloneOSX instead (UnityUpgradable) -> StandaloneOSX", true)]
    StandaloneOSXUniversal = 3,
    /// <summary>
    ///   <para>Build a macOS Intel 32-bit standalone. (This build target is deprecated)</para>
    /// </summary>
    [Obsolete("StandaloneOSXIntel has been removed in 2017.3")]
    StandaloneOSXIntel = 4,
    /// <summary>
    ///   <para>Build a Windows standalone.</para>
    /// </summary>
    StandaloneWindows = 5,
    /// <summary>
    ///   <para>Build a web player. (This build target is deprecated. Building for web player will no longer be supported in future versions of Unity.)</para>
    /// </summary>
    [Obsolete("WebPlayer has been removed in 5.4", true)]
    WebPlayer = 6,
    /// <summary>
    ///   <para>Build a streamed web player.</para>
    /// </summary>
    [Obsolete("WebPlayerStreamed has been removed in 5.4", true)]
    WebPlayerStreamed = 7,
    /// <summary>
    ///   <para>Build an iOS player.</para>
    /// </summary>
    iOS = 9, [Obsolete("PS3 has been removed in >=5.5")]
    PS3 = 10, // 0x0000000A
    [Obsolete("XBOX360 has been removed in 5.5")]
    XBOX360 = 11, // 0x0000000B
    /// <summary>
    ///   <para>Build an Android .apk standalone app.</para>
    /// </summary>
    Android = 13, // 0x0000000D
    /// <summary>
    ///   <para>Build a Linux standalone.</para>
    /// </summary>
    [Obsolete("StandaloneLinux has been removed in 2019.2")]
    StandaloneLinux = 17, // 0x00000011
    /// <summary>
    ///   <para>Build a Windows 64-bit standalone.</para>
    /// </summary>
    StandaloneWindows64 = 19, // 0x00000013
    /// <summary>
    ///   <para>Build to WebGL platform.</para>
    /// </summary>
    WebGL = 20, // 0x00000014
    /// <summary>
    ///   <para>Build an Windows Store Apps player.</para>
    /// </summary>
    WSAPlayer = 21, // 0x00000015
    /// <summary>
    ///   <para>Build a Linux 64-bit standalone.</para>
    /// </summary>
    StandaloneLinux64 = 24, // 0x00000018
    /// <summary>
    ///   <para>Build a Linux universal standalone.</para>
    /// </summary>
    [Obsolete("StandaloneLinuxUniversal has been removed in 2019.2")]
    StandaloneLinuxUniversal = 25, // 0x00000019
    [Obsolete("Use WSAPlayer with Windows Phone 8.1 selected")]
    WP8Player = 26, // 0x0000001A
    /// <summary>
    ///   <para>Build a macOS Intel 64-bit standalone. (This build target is deprecated)</para>
    /// </summary>
    [Obsolete("StandaloneOSXIntel64 has been removed in 2017.3")]
    StandaloneOSXIntel64 = 27, // 0x0000001B
    [Obsolete("BlackBerry has been removed in 5.4")]
    BlackBerry = 28, // 0x0000001C
    [Obsolete("Tizen has been removed in 2017.3")]
    Tizen = 29, // 0x0000001D
    [Obsolete("PSP2 is no longer supported as of Unity 2018.3")]
    PSP2 = 30, // 0x0000001E
    /// <summary>
    ///   <para>Build a PS4 Standalone.</para>
    /// </summary>
    PS4 = 31, // 0x0000001F
    [Obsolete("PSM has been removed in >= 5.3")]
    PSM = 32, // 0x00000020
    /// <summary>
    ///   <para>Build a Xbox One Standalone.</para>
    /// </summary>
    XboxOne = 33, // 0x00000021
    [Obsolete("SamsungTV has been removed in 2017.3")]
    SamsungTV = 34, // 0x00000022
    /// <summary>
    ///   <para>Build to Nintendo 3DS platform.</para>
    /// </summary>
    [Obsolete("Nintendo 3DS support is unavailable since 2018.1")]
    N3DS = 35, // 0x00000023
    [Obsolete("Wii U support was removed in 2018.1")]
    WiiU = 36, // 0x00000024
    /// <summary>
    ///   <para>Build to Apple's tvOS platform.</para>
    /// </summary>
    tvOS = 37, // 0x00000025
    /// <summary>
    ///   <para>Build a Nintendo Switch player.</para>
    /// </summary>
    Switch = 38, // 0x00000026
    [Obsolete("Lumin has been removed in 2022.2")]
    Lumin = 39, // 0x00000027
    /// <summary>
    ///   <para>Build a Stadia standalone.</para>
    /// </summary>
    Stadia = 40, // 0x00000028
    /// <summary>
    ///   <para>Build a CloudRendering standalone.</para>
    /// </summary>
    [Obsolete("CloudRendering is deprecated, please use LinuxHeadlessSimulation (UnityUpgradable) -> LinuxHeadlessSimulation", false)]
    CloudRendering = 41, // 0x00000029
    /// <summary>
    ///   <para>Build a LinuxHeadlessSimulation standalone.</para>
    /// </summary>
    LinuxHeadlessSimulation = 41, // 0x00000029
    [Obsolete("GameCoreScarlett is deprecated, please use GameCoreXboxSeries (UnityUpgradable) -> GameCoreXboxSeries", false)]
    GameCoreScarlett = 42, // 0x0000002A
    GameCoreXboxSeries = 42, // 0x0000002A
    GameCoreXboxOne = 43, // 0x0000002B
    /// <summary>
    ///   <para>Build to PlayStation 5 platform.</para>
    /// </summary>
    PS5 = 44, // 0x0000002C
    EmbeddedLinux = 45, // 0x0000002D
    QNX = 46, // 0x0000002E
    /// <summary>
    ///   <para>Build a visionOS player.</para>
    /// </summary>
    VisionOS = 47, // 0x0000002F
}
