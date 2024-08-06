using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 下载事件处理器
    /// </summary>
    public class DownLoadEventHandler
    {
        /// <summary>
        /// 资源下载事件
        /// </summary>
        public DownLoadEvent downLoadEvent;

        /// <summary>
        /// 热更文件信息
        /// </summary>
        public HotFileInfo hotFileInfo;
    }
    
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
        /// 最大下载线程个数
        /// </summary>
        public const int MAX_THREAD_COUNT = 3;
        
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
        private HotAssetsModule _currentHotAssetsModule;
        
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
        /// 下载回调的列表
        /// </summary>
        private Queue<DownLoadEventHandler> _downLoadEventQueue = new Queue<DownLoadEventHandler>();
        
        /// <summary>
        /// 当前所有正在下载的线程列表
        /// </summary>
        private List<DownLoadThread> _allDownLoadThreadList = new List<DownLoadThread>();
        
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
        public AssetsDownLoader(HotAssetsModule assetModule, Queue<HotFileInfo> downLoadQueue, string downLoadURL, string hotAssetsSavePath,
            DownLoadEvent downLoadSuccess, DownLoadEvent downLoadFailed, DownLoadEvent downLoadFinish)
        {
            _currentHotAssetsModule = assetModule;
            _hotAssetsSavePath = hotAssetsSavePath;
            _downLoadQueue = downLoadQueue;
            _assetsDownLoadURL = downLoadURL;
            OnDownLoadSuccess = downLoadSuccess;
            OnDownLoadFailed = downLoadFailed;
            OnDownLoadFinish = downLoadFinish;
        }

        /// <summary>
        /// 开始下载资源
        /// </summary>
        public void StartThreadDownLoadQueue()
        {
            //根据最大的线程下载个数，开启基本下载通道
            for (int i = 0; i < MAX_THREAD_COUNT; i++)
            {
                if (_downLoadQueue.Count > 0)
                {
                    Debug.Log("Start DownLoad AssetBundle MAX_THREAD_COUNT:" + MAX_THREAD_COUNT);
                        StartDownLoadNextBundle();
                }
            }
        }

        /// <summary>
        /// 下载下一个AssetBundle
        /// </summary>
        public void DownLoadNextBundle()
        {
            if (_allDownLoadThreadList.Count > MAX_THREAD_COUNT)
            {
                Debug.LogError("DownLoadNextBundle Out MaxThreadCount, Close this DownLoad Channel...");
                return;
            }
            if (_downLoadQueue.Count > 0)
            {
                StartDownLoadNextBundle();
                if (_allDownLoadThreadList.Count < MAX_THREAD_COUNT)
                {
                    //计算出正在待机的线程下载通道，把这些下载通道全部打开
                    int idleThreadCount = MAX_THREAD_COUNT - _allDownLoadThreadList.Count;
                    for (int i = 0; i < idleThreadCount; i++)
                    {
                        if (_downLoadQueue.Count > 0)
                        {
                            StartDownLoadNextBundle();
                        }
                    }
                }
            }
            else
            {
                //如果下载中的文件也没有了，就说明所有文件都下完成了
                if (_allDownLoadThreadList.Count == 0)
                {
                    TriggerCallBackInMainThread(new DownLoadEventHandler
                    {
                        downLoadEvent = OnDownLoadFinish
                    });
                }
            }
        }

        /// <summary>
        /// 开始下载洗衣歌AssetBundle
        /// </summary>
        public void StartDownLoadNextBundle()
        {
            HotFileInfo hotFileInfo = _downLoadQueue.Dequeue();
            DownLoadThread downLoadThread = new DownLoadThread(_currentHotAssetsModule, hotFileInfo, _assetsDownLoadURL, _hotAssetsSavePath);
            downLoadThread.StartDownLoad(DownLoadSuccess, DownLoadFailed);
            _allDownLoadThreadList.Add(downLoadThread);
        }

        /// <summary>
        /// AssetBundle下载成功回调
        /// </summary>
        /// <param name="downLoadThread">下载线程</param>
        /// <param name="hotFileInfo">下载文件信息</param>
        public void DownLoadSuccess(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            RemoveDownLoadThread(downLoadThread);
            //因为我们的文件是在子线程中下载的，所以回调也是在子线程中触发
            //这里需要把下载成功的回调事件放到主线程中触发
            TriggerCallBackInMainThread(new DownLoadEventHandler
            {
                downLoadEvent = OnDownLoadSuccess,
                hotFileInfo = hotFileInfo
            });
            DownLoadNextBundle();
        }
        
        /// <summary>
        /// AssetBundle下载失败回调
        /// </summary>
        /// <param name="downLoadThread">下载线程</param>
        /// <param name="hotFileInfo">下载文件信息</param>
        public void DownLoadFailed(DownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            RemoveDownLoadThread(downLoadThread);
            //因为我们的文件是在子线程中下载的，所以回调也是在子线程中触发
            //这里需要把下载成功的回调事件放到主线程中触发
            TriggerCallBackInMainThread(new DownLoadEventHandler
            {
                downLoadEvent = OnDownLoadFailed,
                hotFileInfo = hotFileInfo
            });
            DownLoadNextBundle();
        }

        /// <summary>
        /// 在主线程中触发回调
        /// </summary>
        /// <param name="downLoadEventHandler">下载回调</param>
        public void TriggerCallBackInMainThread(DownLoadEventHandler downLoadEventHandler)
        {
            lock (_downLoadEventQueue)
            {
                _downLoadEventQueue.Enqueue(downLoadEventHandler);
            }
        }

        /// <summary>
        /// 主线程更新接口
        /// </summary>
        public void OnMainThreadUpdate()
        {
            if (_downLoadEventQueue.Count > 0)
            {
                DownLoadEventHandler downLoadEventHandler = _downLoadEventQueue.Dequeue();
                downLoadEventHandler.downLoadEvent?.Invoke(downLoadEventHandler.hotFileInfo);
            }
        }
        
        /// <summary>
        /// 移除下载线程
        /// </summary>
        /// <param name="downLoadThread">下载线程</param>
        public void RemoveDownLoadThread(DownLoadThread downLoadThread)
        {
            if (_allDownLoadThreadList.Contains(downLoadThread))
            {
                _allDownLoadThreadList.Remove(downLoadThread);
            }
        }
    }
}