using System;

namespace ZMAssetFrameWork
{
    public class HotAssetsManager : IHotAssets
    {

        public void HotAssets(BundleModuleEnum bundleModuleEnum, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true)
        {
            throw new NotImplementedException();
        }
        public void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack)
        {
            throw new NotImplementedException();
        }
        public HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            throw new NotImplementedException();
        }
        public void OnMainThreadUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
