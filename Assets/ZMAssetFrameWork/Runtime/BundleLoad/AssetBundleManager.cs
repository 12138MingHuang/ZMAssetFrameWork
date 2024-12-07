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
        
        /// <summary>
        /// 通过AssetBundle加载出的对象数组
        /// </summary>
        public UnityEngine.Object[] objArr;
    }

    /// <summary>
    /// AssetBundle缓存
    /// </summary>
    public class AssetBundleCache
    {
        /// <summary>
        /// AssetBundle对象
        /// </summary>
        public AssetBundle assetBundle;

        /// <summary>
        /// AssetBundle引用计数
        /// </summary>
        public int referenceCount;

        /// <summary>
        /// 释放AssetBundle
        /// </summary>
        public void Release()
        {
            assetBundle = null;
            referenceCount = 0;
        }
    }

    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        /// <summary>
        /// 已经加载的资源模块
        /// </summary>
        private List<BundleModuleEnum> _alreadyLoadModuleList = new List<BundleModuleEnum>();
        
        /// <summary>
        /// 所有模块的AssetBundle的资源对象字典
        /// </summary>
        private Dictionary<uint, BundleItem> _allBundleAssetDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// 所有模块的已经加载过的AssetBundle的资源对象字典
        /// </summary>
        private Dictionary<string, AssetBundleCache> _allAlreadyLoadBundleDic = new Dictionary<string, AssetBundleCache>();

        /// <summary>
        /// AssetBundle缓存类对象池
        /// </summary>
        private ClassObjectPool<AssetBundleCache> _bundleCachePool = new ClassObjectPool<AssetBundleCache>(200);

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
            if (!File.Exists(_bundleConfigPath))
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
                if(_alreadyLoadModuleList.Contains(bundleModuleEnum))
                {
                    Debug.Log("AssetBundleManager:LoadAssetBundleConfig:当前模块的AssetBundle配置文件已经加载过了");
                    return;
                }
                
                //获取当前模块配置文件所在路径
                if (GeneratorBundleConfigPath(bundleModuleEnum))
                {
                    AssetBundle bundleConfig = null;
                    //判断AssetBundle是否加密，如果加密，则解密
                    if (BundleSettings.Instance.bundleEncrypt.isEncrypt)
                    {
                        byte[] bytes = AES.AESFileByteDecrypt(_bundleConfigPath, BundleSettings.Instance.bundleEncrypt.encryptKey);
                        bundleConfig = AssetBundle.LoadFromMemory(bytes);
                    }
                    else
                    {
                        //通过LoadFromFile方法加载AssetBundle
                        bundleConfig = AssetBundle.LoadFromFile(_bundleConfigPath);
                    }
                    // AssetBundle bundleConfig = AssetBundle.LoadFromFile(_bundleConfigPath);
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
                            Debug.Log("AssetBundleManager:LoadAssetBundleConfig:当前模块的AssetBundle配置文件中有重复的AssetBundle信息: " + item.bundleName);
                            // Debug.LogError("AssetBundleManager:LoadAssetBundleConfig:当前模块的AssetBundle配置文件中有重复的AssetBundle信息: " + item.bundleName);
                        }
                    }
                    //释放AssetBundle配置
                    bundleConfig.Unload(false);
                    _alreadyLoadModuleList.Add(bundleModuleEnum);
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
        
        /// <summary>
        /// 根据AssetBundle名称查询该AssetBundle中都有那些资源
        /// </summary>
        /// <param name="bundleName">AssetBundle名称</param>
        /// <returns>BundleItem资源列表</returns>
        public List<BundleItem> GetBundleItemByABName(string bundleName)
        {
            List<BundleItem> itemList = new List<BundleItem>();
            foreach (var item in _allBundleAssetDic.Values)
            {
                if (string.Equals(item.bundleName, bundleName))
                {
                    itemList.Add(item);
                }
            }
            return itemList;
        }

        /// <summary>
        /// 通过资源路径的Crc加载该资源所在的AssetBundle
        /// </summary>
        /// <param name="crc">源路径的Crc</param>
        /// <returns>资源所在的AssetBundle</returns>
        public BundleItem LoadAssetBundle(uint crc)
        {
            BundleItem bundleItem = null;
            //先到所有的AssetBundle资源字典中查询一下这个资源存不存在，如果存在说明该资源已经打成了AssetBundle，这种情况下就可以直接加载资源了
            //如果不存在，说明该资源没有打成AssetBundle，给与错误提示
            _allBundleAssetDic.TryGetValue(crc, out bundleItem);

            if (bundleItem != null)
            {
                //如果AssetBundle为空，索命该资源所在的AssetBundle没有加载进内存，这种情况就需要加载该AssetBundle
                if (bundleItem.assetBundle == null)
                {
                    bundleItem.assetBundle = LoadAssetBundle(bundleItem.bundleName, bundleItem.bundleModuleType);
                    //需要加载这个AssetBundle依赖的其他AssetBundle
                    foreach (string bundleName in bundleItem.bundleDependenceList)
                    {
                        if (bundleItem.bundleName != bundleName)
                        {
                            LoadAssetBundle(bundleName, bundleItem.bundleModuleType);
                        }
                    }
                    return bundleItem;
                }
                else
                {
                    return bundleItem;
                }
            }
            else
            {
                Debug.LogError("Assets not exist AssetBundleConfig, LoadAssetBundle failed CRC:" + crc);
                return null;
            }
        }

        /// <summary>
        /// 通过AssetBundle Name加载AssetBundle
        /// </summary>
        /// <param name="bundleName">AssetBundle Name</param>
        /// <param name="bundleModuleEnum">资源类型</param>
        /// <returns>AssetBundle</returns>
        public AssetBundle LoadAssetBundle(string bundleName, BundleModuleEnum bundleModuleEnum)
        {
            AssetBundleCache bundleCache = null;
            _allAlreadyLoadBundleDic.TryGetValue(bundleName, out bundleCache);

            if (bundleCache == null || (bundleCache != null && bundleCache.assetBundle == null))
            {
                //从类对象池中取出一个AssetBundleCahce对象
                bundleCache = _bundleCachePool.Spawn();
                //计算出AssetBundle的加载路径
                string hotFilePath = BundleSettings.Instance.GetHotAssetsPath(bundleModuleEnum) + bundleName;
                //获取热更模块
                HotAssetsModule hotAssetsModule = ZMAssetsFrame.GetHotAssetsModule(bundleModuleEnum);
                bool isHotPath = true;
                if (hotAssetsModule == null)
                {
                    isHotPath = File.Exists(hotFilePath);
                }
                else
                {
                    isHotPath = hotAssetsModule.HotAssetCount == 0 ? File.Exists(hotFilePath) : hotAssetsModule.HotAssetsIsExists(bundleName);
                }
                // if (hotAssetsModule == null)
                // {
                //     if(File.Exists(hotFilePath))
                //     {
                //         isHotPath = true;
                //     }
                //     else
                //     {
                //         isHotPath = false;
                //     }
                // }
                // else
                // {
                //     if (hotAssetsModule.HotAssetCount == 0)
                //     {
                //         if(File.Exists(hotFilePath))
                //         {
                //             isHotPath = true;
                //         }
                //         else
                //         {
                //             isHotPath = false;
                //         }
                //     }
                //     else
                //     {
                //         if (hotAssetsModule.HotAssetsIsExists(bundleName))
                //         {
                //             isHotPath = true;
                //         }
                //         else
                //         {
                //             isHotPath = false;
                //         }
                //     }
                // }

                //通过是否是热更路径 计算出AssetBundle加载路径
                string bundlePath = isHotPath ? hotFilePath : BundleSettings.Instance.GetAssetsDecompressPath(bundleModuleEnum) + bundleName;

                //判断AssetBundle是否加密，如果加密，则解密
                if (BundleSettings.Instance.bundleEncrypt.isEncrypt)
                {
                    byte[] bytes = AES.AESFileByteDecrypt(bundlePath, BundleSettings.Instance.bundleEncrypt.encryptKey);
                    bundleCache.assetBundle = AssetBundle.LoadFromMemory(bytes);
                }
                else
                {
                    //通过LoadFromFile方法加载AssetBundle
                    bundleCache.assetBundle = AssetBundle.LoadFromFile(bundlePath);
                }

                if (bundleCache.assetBundle == null)
                {
                    Debug.LogError("AssetBundleManager:LoadAssetBundle:加载AssetBundle失败，路径：" + bundlePath);
                    return null;
                }

                //AssetBundle引用计数增加
                bundleCache.referenceCount++;
                _allAlreadyLoadBundleDic.Add(bundleName, bundleCache);
            }
            else
            {
                //AssetBundle已经加载过了
                bundleCache.referenceCount++;
            }
            return bundleCache.assetBundle;
        }

        /// <summary>
        /// 释放AssetBundle，并且释放AssetBundle占用的内存资源
        /// </summary>
        /// <param name="assetItem">assetItem资源</param>
        /// <param name="unLoad">是否释放内存资源</param>
        public void ReleaseAssets(BundleItem assetItem, bool unLoad)
        {
            //assetBundle释放策略一般有两种
            //1.第一种
            // 以AssetBundle.UnLoad(false)为主
            // 对于非对象资源，比如Text字体，Texture Audio等等，资源加载完成后，就可以直接通过AssetBundleUnLoad(false)释放AssetBundle的镜像文件
            // 对于对象资源 比如GameObject，Prefab，Mesh等等，需要再上层做一个引用计数的对象池，Obj在加载出来之后就可以使用AssetBundle.UnLoad(false)释放AssetBundle的镜像文件
            // 因为后续访问的对象都是对象池的物体了

            //2.第二种
            // 以AssetBundle.UnLoad(true)为主
            // 在加载AssetBundle时候做一个缓存，后续加载的所有的资源对象全部通过缓存的AssetBundle进行加载
            // 跳转场景的时候，直接通过AssetBundle.UnLoad(true)释放AssetBundle的镜像文件

            if (assetItem != null)
            {
                if (assetItem.obj != null)
                {
                    assetItem.obj = null;
                }
                if (assetItem.objArr!=null)
                {
                    assetItem.objArr = null;
                }

                ReleaseAssetBundle(assetItem, unLoad);

                if (assetItem.bundleDependenceList != null)
                {
                    foreach (string bundleName in assetItem.bundleDependenceList)
                    {
                        //根据内存引用计数释放AssetBundle
                        ReleaseAssetBundle(null, unLoad, bundleName);
                    }
                }
                
            }
            else
            {
                Debug.LogError("assetItem is null, release asset failed");
            }
        }

        /// <summary>
        /// 释放AssetBundle所占用的内存资源
        /// </summary>
        /// <param name="assetItem">资源</param>
        /// <param name="unLoad">是否卸载内存资源</param>
        /// <param name="bundleName">bundleName名称</param>
        public void ReleaseAssetBundle(BundleItem assetItem, bool unLoad, string bundleName = "")
        {
            string assetBundleName = "";
            if (assetItem == null)
            {
                assetBundleName = bundleName;
            }
            else
            {
                assetBundleName = assetItem.bundleName;
            }
            AssetBundleCache bundleCacheItem = null;
            //如果该AssetBundle的名字不为空，这个AssetBundle已经加载过来
            if (!string.IsNullOrEmpty(assetBundleName) && _allAlreadyLoadBundleDic.TryGetValue(assetBundleName, out bundleCacheItem))
            {
                if (bundleCacheItem.assetBundle != null)
                {
                    bundleCacheItem.referenceCount--;
                    //如果该AssetBundle的引用计数为小于等于0，则卸载AssetBundle
                    if (bundleCacheItem.referenceCount <= 0)
                    {
                        bundleCacheItem.assetBundle.Unload(unLoad);
                        _allAlreadyLoadBundleDic.Remove(assetBundleName);
                        //回收BundleCacheItem类对象
                        bundleCacheItem.Release();
                        _bundleCachePool.Recycle(bundleCacheItem);
                    }
                }
            }
        }
    }
}