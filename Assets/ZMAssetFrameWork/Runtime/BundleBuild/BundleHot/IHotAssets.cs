using System;

namespace ZMAssetFrameWork
{
    public interface IHotAssets
    {
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        /// <param name="startHotCallBack">开始热更回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载的回调</param>
        /// <param name="isCheckAssetsVersion">是否需要检测资源版本</param>
        void HotAssets(BundleModuleEnum bundleModuleEnum, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true);
        
        /// <summary>
        /// 检测资源版本是否需要热更，获取需要热更资源大小
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <param name="callBack"></param>
        void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack);

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <returns>热更模块</returns>
        HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum);
        
        /// <summary>
        /// 主线程更新
        /// </summary>
        void OnMainThreadUpdate();
    }
}
