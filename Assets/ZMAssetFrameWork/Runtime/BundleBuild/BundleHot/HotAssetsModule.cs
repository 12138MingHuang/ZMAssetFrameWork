using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public HotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            CurBundleModuleEnum = bundleModuleEnum;
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
