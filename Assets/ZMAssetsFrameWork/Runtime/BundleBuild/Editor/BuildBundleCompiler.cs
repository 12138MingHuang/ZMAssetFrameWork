using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

namespace ZMAssetsFrameWork
{
    public enum BuildType
    {
        /// <summary>
        /// AssetsBundle资源包
        /// </summary>
        AssetsBundle,
    
        /// <summary>
        /// 热更补丁
        /// </summary>
        HotPatch,
    }

    public class BuildBundleCompiler
    {
        /// <summary>
        /// 更新公告
        /// </summary>
        private static string _updateNotice;
        
        /// <summary>
        /// 热更补丁版本
        /// </summary>
        private static int _hotPatchVersion;
        
        /// <summary>
        /// 打包类型
        /// </summary>
        private static BuildType _buildType;
        
        /// <summary>
        /// 资源模块配置数据
        /// </summary>
        private static BundleModuleData _bundleModuleData;
        
        /// <summary>
        /// 打包模块类型
        /// </summary>
        private static BundleModuleEnum _bundleModuleEnum;
        
        /// <summary>
        /// 所有AssetBundle文件路径列表
        /// </summary>
        private static List<string> _allBundlePathList = new List<string>();
        
        /// <summary>
        /// 所有的文件夹的Bundle字典
        /// </summary>
        private static Dictionary<string, List<string>> _allFolderBundleDic = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// 所有的预制体的Bundle字典
        /// </summary>
        private static Dictionary<string, List<string>> _allPrefabsBundleDic = new Dictionary<string, List<string>>();
        
        
        /// <summary>
        /// 打包AssetBundle资源包
        /// </summary>
        /// <param name="bundleModuleData">资源模块配置数据</param>
        /// <param name="buildType">打包类型</param>
        /// <param name="hotPatchVersion">热更补丁版本</param>
        /// <param name="updateNotice">更新公告</param>
        public static void BuildAssetBundle(BundleModuleData bundleModuleData, BuildType buildType, int hotPatchVersion = 0, string updateNotice = "")
        {
            //初始化打包数据
            Initlization(bundleModuleData, buildType, hotPatchVersion, updateNotice);
            //打包所有的文件夹
            BuildAllFolder();
            //打包父文件夹下的所有子文件夹
            BuildRootSubFolder();
            //打包指定文件夹下的所有预制体
            BuildAllPrefabs();
            //开始调用UnityAPI进行打包AssetBundle资源包
            BuildAllAssetBundle();
        }
        
        /// <summary>
        /// 初始化AssetBundle资源包
        /// </summary>
        /// <param name="bundleModuleData">资源模块配置数据</param>
        /// <param name="buildType">打包类型</param>
        /// <param name="hotPatchVersion">热更补丁版本</param>
        /// <param name="updateNotice">更新公告</param>
        private static void Initlization(BundleModuleData bundleModuleData, BuildType buildType, int hotPatchVersion, string updateNotice)
        {
            //清理数据以防下次打包时有数据残留
            _allBundlePathList.Clear();
            _allFolderBundleDic.Clear();
            _allPrefabsBundleDic.Clear();
            
            _buildType = buildType;
            _hotPatchVersion = hotPatchVersion;
            _updateNotice = updateNotice;
            _bundleModuleData = bundleModuleData;
            _bundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), bundleModuleData.moduleName);
        }

        /// <summary>
        /// 打包所有文件夹AssetsBundle资源包
        /// </summary>
        private static void BuildAllFolder()
        {
            if(_bundleModuleData.signFolderPathArr == null || _bundleModuleData.signFolderPathArr.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _bundleModuleData.signFolderPathArr.Length; i++)
            {
                //获取文件夹路径
                string folderPath = _bundleModuleData.signFolderPathArr[i].bundlePath.Replace(@"\", "/");
                if (!IsRepeatBundleFile(folderPath))
                {
                    _allBundlePathList.Add(folderPath);
                    //获取以模块名+_+ABName的格式的AssetBundle包名
                    string bundleName = GenerateBundleName(_bundleModuleData.signFolderPathArr[i].abName);
                    if (!_allFolderBundleDic.ContainsKey(folderPath))
                    {
                        _allFolderBundleDic.Add(bundleName, new List<string>() { folderPath });
                    }
                    else
                    {
                        _allFolderBundleDic[bundleName].Add(folderPath);
                    }
                }
                else
                {
                    Debug.LogError("重复的Bundle文件：" + folderPath);
                }
            }
        }

        /// <summary>
        /// 打包父文件夹下的所有子文件夹
        /// </summary>
        private static void BuildRootSubFolder()
        {
            
        }

        /// <summary>
        /// 打包指定文件夹下的所有预制体
        /// </summary>
        private static void BuildAllPrefabs()
        {
            
        }

        /// <summary>
        /// 打包AssetBundle资源包
        /// </summary>
        private static void BuildAllAssetBundle()
        {
            
        }

        /// <summary>
        /// 是否重复的Bundle文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否重复</returns>
        private static bool IsRepeatBundleFile(string path)
        {
            foreach (string item in _allBundlePathList)
            {
                if (string.Equals(item, path) || item.Contains(path) || path.EndsWith(".cs"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 生成Bundle文件名
        /// </summary>
        /// <param name="abName">ab包名称</param>
        /// <returns>返回完整名称</returns>
        private static string GenerateBundleName(string abName)
        {
            return _bundleModuleEnum.ToString() + "_" + abName;
        }
    }   
}
