using System;
using System.Collections.Generic;
using UnityEngine;

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
                assetsModule = new HotAssetsModule(bundleModuleEnum, ZMAssetsFrame.Instance);
                _allAssetsModuleDic.Add(bundleModuleEnum, assetsModule);
            }
            return assetsModule;
        }
        
        /// <summary>
        /// 检测资源版本是否需要热更
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        /// <param name="callBack">热更回调</param>
        public void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack)
        {
            HotAssetsModule assetsModule = GetOrNewAssetsModule(bundleModuleEnum);
            assetsModule.CheckAssetsVersion(callBack);
        }
        
        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <returns>热更模块</returns>
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            if(_allAssetsModuleDic.ContainsKey(bundleModuleEnum))
            {
                return _allAssetsModuleDic[bundleModuleEnum];
            }
            return null;
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
            //获取当前正在下载热更资源模块的一个长度个数
            int count = _downLoadingAssetsModuleDic.Count;
            //计算多线程均衡后的线程分配个数
            //以最大下载线程个数3 举例子
            //1. 3/1=3 最大并发下载线程个数是3 （偶数）
            //2. 3/2=1.5 向上取整 最大并发下载线程个数是2 （奇数）
            //3. 3/3=1 每一个模块都拥有一个下载线程
            float threadCount = MAX_THREAD_COUNT / 1.0f / count;
            //主下载线程个数
            int mainThreadCount = 0;
            //通过(int)进行强转 (int)强转：向下取整
            int threadBalancingCount = (int)threadCount;

            if ((int)threadCount < threadCount)
            {
                //向上取整
                mainThreadCount = Mathf.CeilToInt(threadCount);
                //向下取整
                threadBalancingCount = Mathf.FloorToInt(threadCount);
            }
            //多线程均衡
            int i = 0;
            foreach(HotAssetsModule item in _downLoadingAssetsModuleDic.Values)
            {
                //如果当前下载线程个数小于主下载线程个数，则把当前下载线程个数+1
                if (mainThreadCount != 0 && i == 0)
                {
                    item.SetDownLoadThreadCount(mainThreadCount);//设置主线程下载个数
                }
                else
                {
                    item.SetDownLoadThreadCount(threadBalancingCount);
                }
                i++;
            }
        }
        
        /// <summary>
        /// 主线程更新
        /// </summary>
        public void OnMainThreadUpdate()
        {
            for (int i = 0; i < _downLoadAssetsModuleList.Count; i++)
            {
                _downLoadAssetsModuleList[i].OnMainThreadUpdate();
            }
        }
    }
}
