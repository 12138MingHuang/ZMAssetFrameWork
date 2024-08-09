using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public partial class ZMAssetsFrame
    {
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        /// <param name="startHotCallBack">开始热更回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载的回调</param>
        /// <param name="isCheckAssetsVersion">是否需要检测资源版本</param>
        public static void HotAssets(BundleModuleEnum bundleModuleEnum, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true)
        {
            Instance._hotAssets.HotAssets(bundleModuleEnum, startHotCallBack, hotFinish, waiteDownLoad, isCheckAssetsVersion);
        }
        
        /// <summary>
        /// 检测资源版本是否需要热更，获取需要热更资源大小
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <param name="callBack">开始热更资源回调</param>
        public static void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack)
        {
            Instance._hotAssets.CheckAssetsVersion(bundleModuleEnum, callBack);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <returns>热更模块</returns>
        public static HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            return Instance._hotAssets.GetHotAssetsModule(bundleModuleEnum);
        }

        /// <summary>
        /// 开始解压内嵌资源
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <param name="callBack">开始解压资源回调</param>
        /// <returns>热更模块</returns>
        public static IDecompressAssets StartDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callBack)
        {
            return Instance._decompressAssets.StartDecompressBuiltinFile(bundleModuleEnum, callBack);
        }
        
        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns>解压进度</returns>
        public static float GetDecompressProgress()
        {
            return Instance._decompressAssets.GetDecompressProgress();
        }
    }

}