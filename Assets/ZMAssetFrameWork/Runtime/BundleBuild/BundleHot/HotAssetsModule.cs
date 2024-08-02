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
        /// 当前下载的资源模块类型
        /// </summary>
        public BundleModuleEnum CurBundleModuleEnum { get; set; }
        
        /// <summary>
        /// 下载所有资源完成的回调
        /// </summary>
        public Action<BundleModuleEnum> onDownLoadAllAssetsFinish;

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
            this.onDownLoadAllAssetsFinish = hotFinish;

            if (isCheckAssetsVersion)
            {
                
            }
        }

        /// <summary>
        /// 检测热更资源版本
        /// </summary>
        /// <param name="checkCallBack">检测回调</param>
        public void CheckAssetsVersion(Action<bool, float> checkCallBack)
        {
            GeneratorHotAssetsManifest();

            _monoBehaviour.StartCoroutine(DownLoadHotAssetsManifest(() =>
            {
                //资源下载完成
                
                //1.检测当前版本是否需要热更
                if (CheckModuleAssetsIsHot())
                {
                    
                }
                else
                {
                    
                }
                
                //2.如果需要热更，开始计算需要下载的文件，开始下载文件
                
                //3.如果不需要热更，说明文件是新的，直接热更完成
            }));
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

            if (localHotAssetsPatch != null && serverHotAssetsPatch != null)
            {
                return localHotAssetsPatch.patchVersion != serverHotAssetsPatch.patchVersion;
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
                    Debug.LogError("*** Request AssetBundle HotAssetsManifest Url Finish Module:" + CurBundleModuleEnum + "text:" + webRequest.downloadHandler.text);
                    //写入服务端资源热更清单到本地
                    FileHelper.WriteFile(_serverHotAssetsManifestPath, webRequest.downloadHandler.data);
                    _serverHotAssetsManifest = JsonConvert.DeserializeObject<HotAssetsManifest>(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogError("服务端资源清单配置下载异常，文件不存在或者配置有问题，更新出错，请检查：" + e.ToString());
                }
            }
            downLoadFinish?.Invoke();
        }

        private void GeneratorHotAssetsManifest()
        {
            _serverHotAssetsManifestPath = Application.persistentDataPath + "/Server" + CurBundleModuleEnum + "AssetsHotManifest.json";
            _localHotAssetsManifestPath = Application.persistentDataPath + "/Local" + CurBundleModuleEnum + "AssetsHotManifest.json";
        }
    }
}
