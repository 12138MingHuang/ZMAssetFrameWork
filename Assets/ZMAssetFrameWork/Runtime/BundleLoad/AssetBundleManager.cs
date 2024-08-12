using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZMAssetFrameWork
{

    public class BundleItem
    {
        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string path;
        
        /// <summary>
        /// 文件加载crc
        /// </summary>
        public uint crc;
        
        /// <summary>
        /// AssetBundle名称
        /// </summary>
        public string bundleName;
        
        /// <summary>
        /// 资源名称
        /// </summary>
        public string assetName;
        
        /// <summary>
        /// AssetBundle所属的模块
        /// </summary>
        public BundleModuleEnum bundleModuleType;
        
        /// <summary>
        /// AssetBundle依赖项列表
        /// </summary>
        public List<string> bundleDependenceList;
        
        /// <summary>
        /// AssetBundle
        /// </summary>
        public AssetBundle assetBundle;
        
        /// <summary>
        /// 通过AssetBundle加载的对象
        /// </summary>
        public UnityEngine.Object obj;
    }
    
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        /// <summary>
        /// 所有模块的AssetBundle的资源对象字典
        /// </summary>
        private Dictionary<uint, BundleItem> _allBundleAssetDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// AssetBundle配置文件加载路径
        /// </summary>
        private string _bundleConfigPath;

        /// <summary>
        /// AssetBundle配置文件名称
        /// </summary>
        private string _bundleConfigName;
        
        /// <summary>
        ///  AssetBundle配置文件名称
        /// </summary>
        private string _assetsBundleConfigName;

        /// <summary>
        /// 生成AssetBundleConfig配置文件路径
        /// </summary>
        /// <param name="bundleModuleEnum">资源类型</param>
        /// <returns>是否生成路径成功</returns>
        public bool GeneratorBundleConfigPath(BundleModuleEnum bundleModuleEnum)
        {
            _assetsBundleConfigName = bundleModuleEnum.ToString() + "AssetBundleConfig";
            //这里的文件读取失败是AssetBundle后缀引起的，高版本Unity自动会自动读取,unity文件,导致文件大小异常，解决方案:建议不要AssetBundle后缀,或更换后缀
            // _bundleConfigName = bundleModuleEnum.ToString().ToLower() + "bundleconfig.unity";
            _bundleConfigName = bundleModuleEnum.ToString().ToLower() + "bundleconfig.ab";
            _bundleConfigPath = BundleSettings.Instance.GetHotAssetsPath(bundleModuleEnum) + _bundleConfigName;
            //如果配置文件存在，return true 如果不存在就直接从内嵌解压的资源中去加载
            //如果内嵌解压的文件不存在，说明网络有问题
            if(!File.Exists(_bundleConfigPath))
            {
                _bundleConfigPath = BundleSettings.Instance.GetAssetsDecompressPath(bundleModuleEnum) + _bundleConfigName;
                if (!File.Exists(_bundleConfigPath))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 加载AssetBundle配置
        /// </summary>
        /// <param name="bundleModuleEnum">资源类型</param>
        public void LoadAssetBundleConfig(BundleModuleEnum bundleModuleEnum)
        {
            try
            {
                //获取当前模块配置文件所在路径
                if (GeneratorBundleConfigPath(bundleModuleEnum))
                {
                    AssetBundle bundleConfig = AssetBundle.LoadFromFile(_bundleConfigPath);
                    string bundleConfigJson = bundleConfig.LoadAsset<TextAsset>(_assetsBundleConfigName).text;
                    BundleConfig bundleManifest = JsonConvert.DeserializeObject<BundleConfig>(bundleConfigJson);
                    //把所有的AssetBundle信息存放在字典中，管理起来
                    foreach (BundleInfo item in bundleManifest.bundleInfoList)
                    {
                        if (!_allBundleAssetDic.ContainsKey(item.crc))
                        {
                            BundleItem bundleItem = new BundleItem();
                            bundleItem.path = item.path;
                            bundleItem.crc = item.crc;
                            bundleItem.assetName = item.assetName;
                            bundleItem.bundleModuleType = bundleModuleEnum;
                            bundleItem.bundleDependenceList = item.bundleDependce;
                            bundleItem.bundleName = item.bundleName;
                            _allBundleAssetDic.Add(item.crc, bundleItem);
                        }
                        else
                        {
                            Debug.LogError("AssetBundleManager:LoadAssetBundleConfig:当前模块的AssetBundle配置文件中有重复的AssetBundle信息: " + item.bundleName);
                        }
                    }
                    //释放AssetBundle配置
                    bundleConfig.Unload(false);
                }
                else
                {
                    Debug.LogError("AssetBundleManager:LoadAssetBundleConfig:当前模块的AssetBundle配置文件不存在");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("AssetBundleManager:LoadAssetBundleConfig:加载配置文件失败" + e);
                throw;
            }
        }
    }   
}