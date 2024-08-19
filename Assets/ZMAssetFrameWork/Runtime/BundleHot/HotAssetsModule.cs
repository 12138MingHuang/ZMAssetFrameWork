using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 热更资源模块
    /// </summary>
    public class HotAssetsModule
    {
        /// <summary>
        /// 热更资源下载存储路径
        /// </summary>
        public string HotAssetsSavePath
        {
            get { return Application.persistentDataPath + "/HotAssets/" + CurBundleModuleEnum + "/"; }
        }

        /// <summary>
        /// 所有热更的资源列表
        /// </summary>
        private List<HotFileInfo> _allHotAssetsList = new List<HotFileInfo>();
        
        /// <summary>
        /// 需要下载的资源列表
        /// </summary>
        private List<HotFileInfo> _needDownLoadAssetsList = new List<HotFileInfo>();

        /// <summary>
        /// 服务端资源清单
        /// </summary>
        private HotAssetsManifest _serverHotAssetsManifest;

        /// <summary>
        /// 本地资源清单
        /// </summary>
        private HotAssetsManifest _localHotAssetsManifest;

        /// <summary>
        /// 服务端资源热更清单存储路径
        /// </summary>
        private string _serverHotAssetsManifestPath;

        /// <summary>
        /// 本地资源热更清单存储路径
        /// </summary>
        private string _localHotAssetsManifestPath;

        /// <summary>
        /// 热更公告
        /// </summary>
        public string UpdateNoticeContent
        {
            get { return _serverHotAssetsManifest.updateNotice; }
        }
        
        /// <summary>
        /// 当前下载的资源模块类型
        /// </summary>
        public BundleModuleEnum CurBundleModuleEnum { get; set; }

        /// <summary>
        /// 下载所有资源完成的回调
        /// </summary>
        public Action<BundleModuleEnum> onDownLoadAllAssetsFinish;
        
        /// <summary>
        /// 最大下载资源大小
        /// </summary>
        public float AssetsMaxSizeM { get; set; }

        /// <summary>
        /// 资源已经下载大小
        /// </summary>
        public float assetsDownLoadSizeM;

        /// <summary>
        /// 资源下载器
        /// </summary>
        private AssetsDownLoader _assetsDownLoader;
        
        /// <summary>
        /// AssetBundle配置文件下载完成监听
        /// </summary>
        public Action<string> OnDownLoadABConfigListener;
        
        /// <summary>
        /// 下载AssetBundle完成的回调
        /// </summary>
        public Action<string> OnDownLoadAssetBundleListener;

        /// <summary>
        /// 所有热更资源的一个长度
        /// </summary>
        public int HotAssetCount
        {
            get { return _allHotAssetsList.Count; }
        }

        /// <summary>
        /// Mono脚本
        /// </summary>
        private MonoBehaviour _monoBehaviour;

        public HotAssetsModule(BundleModuleEnum bundleModuleEnum, MonoBehaviour monoBehaviour)
        {
            CurBundleModuleEnum = bundleModuleEnum;
            _monoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// 开始热更资源
        /// </summary>
        /// <param name="startDownLoadCallBack">开始下载的回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="isCheckAssetsVersion">是否检测资源版本</param>
        public void StartHotAssets(Action startDownLoadCallBack, Action<BundleModuleEnum> hotFinish = null, bool isCheckAssetsVersion = true)
        {
            this.onDownLoadAllAssetsFinish += hotFinish;

            if (isCheckAssetsVersion)
            {
                //检测资源版本是否需要热更
                CheckAssetsVersion((isHot, size) =>
                {
                    if (isHot)
                    {
                        StartDownLoadHotAssets(startDownLoadCallBack);
                    }
                    else
                    {
                        onDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
                    }
                });
            }
        }

        /// <summary>
        /// 开始下载热更资源
        /// </summary>
        /// <param name="startDownLoadCallBack"></param>
        public void StartDownLoadHotAssets(Action startDownLoadCallBack)
        {
            //优先下载AssetBundle配置文件，下载完成后，调用回调，及时加载配置文件
            //热更资源下载完成之后同样给与回调，动态加载刚下载完成的资源
            List<HotFileInfo> downLoadList = new List<HotFileInfo>();
            for (int i = 0; i < _needDownLoadAssetsList.Count; i++)
            {
                HotFileInfo hotFileInfo = _needDownLoadAssetsList[i];
                //如果包含Config说明是配置文件，需要优先下载
                if (hotFileInfo.abName.Contains("config"))
                {
                    downLoadList.Insert(0, hotFileInfo);
                }
                else
                {
                    downLoadList.Add(hotFileInfo);
                }
            }
            //获取资源下载队列
            Queue<HotFileInfo> downloadQueue = new Queue<HotFileInfo>();
            foreach (HotFileInfo item in downLoadList)
            {
                downloadQueue.Enqueue(item);
            }
            //通过资源下载器，开始下载资源
            _assetsDownLoader = new AssetsDownLoader(this, downloadQueue, _serverHotAssetsManifest.downLoadURL, HotAssetsSavePath
                , DownLoadAssetBundleSuccess, DownLoadAssetBundleFailed, DownLoadAssetBundleFinish);
            
            startDownLoadCallBack?.Invoke();
            //开始下载队列中的资源
            _assetsDownLoader.StartDownLoadNextBundle();
        }

        /// <summary>
        /// 检测热更资源版本
        /// </summary>
        /// <param name="checkCallBack">检测回调</param>
        public void CheckAssetsVersion(Action<bool, float> checkCallBack)
        {
            GeneratorHotAssetsManifest();
            _needDownLoadAssetsList.Clear();
            _monoBehaviour.StartCoroutine(DownLoadHotAssetsManifest(() =>
            {
                //资源下载完成
                //1.检测当前版本是否需要热更
                if (CheckModuleAssetsIsHot())
                {
                    HotAssetsPatch serverHotAssetsPatch = _serverHotAssetsManifest.hotAssetsPatchList[_serverHotAssetsManifest.hotAssetsPatchList.Count - 1];
                    bool isNeedHot = ComputeNeedHotAssetsList(serverHotAssetsPatch);
                    if (isNeedHot)
                    {
                        checkCallBack?.Invoke(true, AssetsMaxSizeM);
                    }
                    else
                    {
                        checkCallBack.Invoke(false, 0);
                    }
                }
                else
                {
                    checkCallBack.Invoke(false, 0);
                }

                //2.如果需要热更，开始计算需要下载的文件，开始下载文件

                //3.如果不需要热更，说明文件是新的，直接热更完成
            }));
        }

        /// <summary>
        /// 计算需要热更的文件列表
        /// </summary>
        /// <param name="serverHotAssetsPatch">服务端热更补丁包</param>
        /// <returns>是否需要热更</returns>
        private bool ComputeNeedHotAssetsList(HotAssetsPatch serverHotAssetsPatch)
        {
            if (!Directory.Exists(HotAssetsSavePath))
            {
                Directory.CreateDirectory(HotAssetsSavePath);
            }
            if (File.Exists(_localHotAssetsManifestPath))
            {
                _localHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(_localHotAssetsManifestPath));
            }
            AssetsMaxSizeM = 0;
            foreach (HotFileInfo item in serverHotAssetsPatch.hotAssetsList)
            {
                //获取本地AssetBundle文件路径
                string localFilePath = HotAssetsSavePath + item.abName;
                _allHotAssetsList.Add(item);
                //如果本地文件不存在，或者本地文件与服务端不一致，就需要热更
                string localMD5 = GetLocalFileMD5ByBundleName(item.abName);
                if (!File.Exists(localFilePath) || item.md5 != localMD5)
                {
                    _needDownLoadAssetsList.Add(item);
                    AssetsMaxSizeM += item.size / 1024.0f;
                }
                // if (!File.Exists(localFilePath) || item.md5 != MD5.GetMd5FromFile(localFilePath))
                // {
                //     _needDownLoadAssetsList.Add(item);
                //     AssetsMaxSizeM += item.size / 1024.0f;
                // }
            }
            return _needDownLoadAssetsList.Count > 0;
        }

        /// <summary>
        /// 获取本地文件MD5
        /// </summary>
        /// <param name="bundleName">ab包名称</param>
        /// <returns>MD5字符串</returns>
        private string GetLocalFileMD5ByBundleName(string bundleName)
        {
            if (_localHotAssetsManifest != null && _localHotAssetsManifest.hotAssetsPatchList.Count > 0)
            {
                HotAssetsPatch localPatch = _localHotAssetsManifest.hotAssetsPatchList[_localHotAssetsManifest.hotAssetsPatchList.Count - 1];
                foreach (HotFileInfo item in localPatch.hotAssetsList)
                {
                    if(string.Equals(bundleName, item.abName))
                        return item.md5;
                }
            }
            return "";
        }

        /// <summary>
        /// 检测模块资源是否需要热更
        /// </summary>
        /// <returns>是否需要热更</returns>
        private bool CheckModuleAssetsIsHot()
        {
            //如果服务端资源清单不存在，需需要热更
            if (_serverHotAssetsManifest == null)
            {
                return false;
            }
            //如果本地资源清单文件不存在，说明需要热更
            if (!File.Exists(_localHotAssetsManifestPath))
            {
                return true;
            }
            //判断本地资源清单补丁版本号是否与服务端资源清单补丁版本号一致，如果一致，不需要热更，如果不一致，则需要热更
            HotAssetsManifest locHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(File.ReadAllText(_localHotAssetsManifestPath));
            if (locHotAssetsManifest.hotAssetsPatchList.Count == 0 && _serverHotAssetsManifest.hotAssetsPatchList.Count != 0)
            {
                return true;
            }
            //获取本地热更补丁的最后一个补丁
            HotAssetsPatch localHotAssetsPatch = locHotAssetsManifest.hotAssetsPatchList[locHotAssetsManifest.hotAssetsPatchList.Count - 1];
            //获取服务端热更补丁的最后一个补丁
            HotAssetsPatch serverHotAssetsPatch = _serverHotAssetsManifest.hotAssetsPatchList[_serverHotAssetsManifest.hotAssetsPatchList.Count - 1];

            // if (localHotAssetsPatch != null && serverHotAssetsPatch != null)
            // {
            //     return localHotAssetsPatch.patchVersion != serverHotAssetsPatch.patchVersion;
            // }
            if (localHotAssetsPatch != null && serverHotAssetsPatch != null)
            {
                if(localHotAssetsPatch.patchVersion != serverHotAssetsPatch.patchVersion)
                    return true;
            }

            return serverHotAssetsPatch != null;
        }

        /// <summary>
        /// 下载热更清单
        /// </summary>
        /// <param name="downLoadFinish"></param>
        /// <returns></returns>
        private IEnumerator DownLoadHotAssetsManifest(Action downLoadFinish)
        {
            string url = BundleSettings.Instance.AssetBundleDownLoadURL + "/HotAssets/" + CurBundleModuleEnum + "AssetsHotManifest.json";
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.timeout = 30;

            Debug.Log(("*** Request HotAssetsManifest Url:" + url));

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("DownLoad Error:" + webRequest.error);
            }
            else
            {
                try
                {
                    Debug.Log("*** Request AssetBundle HotAssetsManifest Url Finish Module:" + CurBundleModuleEnum + " text:" + webRequest.downloadHandler.text);
                    //写入服务端资源热更清单到本地
                    FileHelper.WriteFile(_serverHotAssetsManifestPath, webRequest.downloadHandler.data);
                    _serverHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogError("服务端资源清单配置下载异常，文件不存在或者配置有问题，更新出错，请检查：" + e.ToString());
                }
            }
            webRequest.Dispose();
            downLoadFinish?.Invoke();
        }

        private void GeneratorHotAssetsManifest()
        {
            _serverHotAssetsManifestPath = Application.persistentDataPath + "/Server" + CurBundleModuleEnum + "AssetsHotManifest.json";
            _localHotAssetsManifestPath = Application.persistentDataPath + "/Local" + CurBundleModuleEnum + "AssetsHotManifest.json";
        }

        #region 资源下载回调
        
        private void DownLoadAssetBundleSuccess(HotFileInfo hotFileInfo)
        {
            //这里的文件读取失败是AssetBundle后缀引起的，高版本Unity自动会自动读取,unity文件,导致文件大小异常，解决方案:建议不要AssetBundle后缀,或更换后缀
            // string abName = hotFileInfo.abName.Replace(".unity", "");
            string abName = hotFileInfo.abName.Replace(".ab", "");
            if (hotFileInfo.abName.Contains("bundleconfig"))
            {
                OnDownLoadABConfigListener?.Invoke(abName);
                //如果下载成功需要及时去加载配置文件
                // TODO
            }
            else
            {
                OnDownLoadAssetBundleListener?.Invoke(abName);
            }
            HotAssetsManager.DownLoadBundleFinish?.Invoke(hotFileInfo);
        }
        
        private void DownLoadAssetBundleFailed(HotFileInfo hotFileInfo)
        {
            
        }
        
        private void DownLoadAssetBundleFinish(HotFileInfo hotFileInfo)
        {
            if (File.Exists(_localHotAssetsManifestPath))
            {
                File.Delete(_localHotAssetsManifestPath);
            }
            //把服务端热更清单文件拷贝到本地
            File.Copy(_serverHotAssetsManifestPath, _localHotAssetsManifestPath);
            onDownLoadAllAssetsFinish?.Invoke(CurBundleModuleEnum);
        }
        
        #endregion

        public void OnMainThreadUpdate()
        {
            _assetsDownLoader?.OnMainThreadUpdate();
        }

        /// <summary>
        /// 设置下载线程个数
        /// </summary>
        /// <param name="threadCount">下载线程个数</param>
        public void SetDownLoadThreadCount(int threadCount)
        {
            Debug.Log("多线程负载均衡：" + threadCount + " ModuleType:" + CurBundleModuleEnum);
            if(_assetsDownLoader != null)
            {
                _assetsDownLoader.MAX_THREAD_COUNT = threadCount;
            }
        }
        
        /// <summary>
        /// 判断热更文件是否存在
        /// </summary>
        /// <param name="bundleName">热更文件</param>
        /// <returns>是否存在</returns>
        public bool HotAssetsIsExists(string bundleName)
        {
            foreach (HotFileInfo item in _allHotAssetsList)
            {
                if(string.Equals(bundleName, item.abName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
