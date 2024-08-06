using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 下载事件
    /// </summary>
    public delegate void DownLoadEvent(HotFileInfo hotFileInfo);
    
    /// <summary>
    /// 资源下载器
    /// </summary>
    public class AssetsDownLoader
    {
        /// <summary>
        /// 资源文件下载地址
        /// </summary>
        private string _assetsDownLoadURL;
        
        /// <summary>
        /// 热更文件存储路径
        /// </summary>
        private string _hotAssetsSavePath;
        
        /// <summary>
        /// 当前热更的资源模块
        /// </summary>
        private HotAssetsManifest _currentHotAssetsManifest;
        
        /// <summary>
        /// 文件下载队列
        /// </summary>
        private Queue<HotFileInfo> _downLoadQueue;
        
        /// <summary>
        /// 文件下载成功回调
        /// </summary>
        private DownLoadEvent OnDownLoadSuccess;
        
        /// <summary>
        /// 文件下载失败回调
        /// </summary>
        private DownLoadEvent OnDownLoadFailed;
        
        /// <summary>
        /// 文件下载完成回调
        /// </summary>
        private DownLoadEvent OnDownLoadFinish;
        
        /// <summary>
        /// 资源下载器构造函数
        /// </summary>
        /// <param name="assetModule">资源下载模块</param>
        /// <param name="downLoadQueue">资源下载队列</param>
        /// <param name="downLoadURL">资源下载地址</param>
        /// <param name="hotAssetsSavePath">热更文件存储路径</param>
        /// <param name="downLoadSuccess">文件下载成功回调</param>
        /// <param name="downLoadFailed">文件下载失败回调</param>
        /// <param name="downLoadFinish">文件下载完成回调</param>
        public AssetsDownLoader(HotAssetsManifest assetModule, Queue<HotFileInfo> downLoadQueue, string downLoadURL, string hotAssetsSavePath,
            DownLoadEvent downLoadSuccess, DownLoadEvent downLoadFailed, DownLoadEvent downLoadFinish)
        {
            _currentHotAssetsManifest = assetModule;
            _hotAssetsSavePath = hotAssetsSavePath;
            _downLoadQueue = downLoadQueue;
            _assetsDownLoadURL = downLoadURL;
            OnDownLoadSuccess = downLoadSuccess;
            OnDownLoadFailed = downLoadFailed;
            OnDownLoadFinish = downLoadFinish;
        }
    }
}