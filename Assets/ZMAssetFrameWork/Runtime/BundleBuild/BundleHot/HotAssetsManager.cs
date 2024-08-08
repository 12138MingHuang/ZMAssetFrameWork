using System;
using System.Collections.Generic;

namespace ZMAssetFrameWork
{

    /// <summary>
    /// 等待下载的模块
    /// </summary>
    public class WaitDownLoadModule
    {
        /// <summary>
        /// 下载的模块
        /// </summary>
        public BundleModuleEnum bundleModule;
        
        /// <summary>
        /// 开始热更回调
        /// </summary>
        public Action<BundleModuleEnum> startHot;
        
        /// <summary>
        /// 热更完成回调
        /// </summary>
        public Action<BundleModuleEnum> hotFinish;
        
        /// <summary>
        /// 热更资源进度回调
        /// </summary>
        public Action<BundleModuleEnum, float> hotAssetsProgressCallBack;
    }
    
    public class HotAssetsManager : IHotAssets
    {

        /// <summary>
        /// 最大并发下载线程个数
        /// </summary>
        private int MAX_THREAD_COUNT = 3;

        /// <summary>
        /// 所有热更资源模块
        /// </summary>
        private Dictionary<BundleModuleEnum, HotAssetsModule> _allAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();
        
        /// <summary>
        /// 正在下载热更资源模块字典
        /// </summary>
        private Dictionary<BundleModuleEnum, HotAssetsModule> _downLoadingAssetsModuleDic = new Dictionary<BundleModuleEnum, HotAssetsModule>();

        /// <summary>
        /// 正在下载热更资源模块列表
        /// </summary>
        private List<HotAssetsModule> _downLoadAssetsModuleList = new List<HotAssetsModule>();

        /// <summary>
        /// 等待下载的模块队列
        /// </summary>
        private Queue<WaitDownLoadModule> _waitDownLoadModuleQueue = new Queue<WaitDownLoadModule>();
        
        public void HotAssets(BundleModuleEnum bundleModuleEnum, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true)
        {
            if (BundleSettings.Instance.bundleHotType == BundleHotEnum.NoHot)
            {
                hotFinish?.Invoke(bundleModuleEnum);
                return;
            }
            
            //读取配置中的最大下载线程个数
            MAX_THREAD_COUNT = BundleSettings.Instance.MAX_THREAD_COUNT;
            
            HotAssetsModule assetsModule = GetOrNewAssetsModule(bundleModuleEnum);
            //判断是否有闲置资源下载线程
            if (_downLoadingAssetsModuleDic.Count < MAX_THREAD_COUNT)
            {
                if (!_downLoadingAssetsModuleDic.ContainsKey(bundleModuleEnum))
                {
                    _downLoadingAssetsModuleDic.Add(bundleModuleEnum, assetsModule);
                }
                if(!_downLoadAssetsModuleList.Contains(assetsModule))
                {
                    _downLoadAssetsModuleList.Add(assetsModule);
                }
                assetsModule.onDownLoadAllAssetsFinish += HotModuleAssetsFinish;
                //开始热更资源
                assetsModule.StartHotAssets(() =>
                {
                    MultipleThreadBalancing();
                    startHotCallBack?.Invoke(bundleModuleEnum);
                }, hotFinish);
            }
            else
            {
                waiteDownLoad?.Invoke(bundleModuleEnum);
                //把热更模块添加到等待下载队列
                _waitDownLoadModuleQueue.Enqueue(new WaitDownLoadModule
                {
                    bundleModule = bundleModuleEnum,
                    startHot = startHotCallBack,
                    hotFinish = hotFinish,
                });
            }
        }
        
        /// <summary>
        /// 获取或者新建一个热更资源模块
        /// </summary>
        /// <param name="bundleModuleEnum">资源模块类型</param>
        /// <returns>热更资源模块</returns>
        public HotAssetsModule GetOrNewAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            HotAssetsModule assetsModule = null;
            if(_allAssetsModuleDic.ContainsKey(bundleModuleEnum))
            {
                assetsModule = _allAssetsModuleDic[bundleModuleEnum];
            }
            else
            {
                assetsModule = new HotAssetsModule(bundleModuleEnum, null);
                _allAssetsModuleDic.Add(bundleModuleEnum, assetsModule);
            }
            return assetsModule;
        }
        
        public void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack)
        {
            throw new NotImplementedException();
        }
        
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 热更资源完成
        /// </summary>
        /// <param name="bundleModuleEnum">资源类型</param>
        public void HotModuleAssetsFinish(BundleModuleEnum bundleModuleEnum)
        {
            //把下载完成的模块从下载中的字典中移除掉
            if(_downLoadingAssetsModuleDic.ContainsKey(bundleModuleEnum))
            {
                HotAssetsModule assetsModule = _downLoadingAssetsModuleDic[bundleModuleEnum];
                if(_downLoadAssetsModuleList.Contains(assetsModule))
                {
                    _downLoadAssetsModuleList.Remove(assetsModule);
                }
                _downLoadingAssetsModuleDic.Remove(bundleModuleEnum);
            }
            
            //判断等待下载队列中是否有资源模块,如果有则开始下载.因为已经有下现场空闲下来
            if (_waitDownLoadModuleQueue.Count > 0)
            {
                WaitDownLoadModule waitDownLoadModule = _waitDownLoadModuleQueue.Dequeue();
                HotAssets(waitDownLoadModule.bundleModule, waitDownLoadModule.startHot, waitDownLoadModule.hotFinish, null);
            }
            else
            {
                //在没有等待热更模块的情况下，并且已经有下载线程空闲下来了
                //就需要把闲置下来的下线线程分配给其他正在热更的模块，增加该模块的热更速度
                MultipleThreadBalancing();
            }
        }

        /// <summary>
        /// 多线程均衡
        /// </summary>
        public void MultipleThreadBalancing()
        {
            
        }
        
        public void OnMainThreadUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
