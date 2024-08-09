using System;
using System.Collections.Generic;
using System.IO;

namespace ZMAssetFrameWork
{
    public class AssetsDecompressManager : IDecompressAssets
    {

        /// <summary>
        /// 资源解压路径
        /// </summary>
        private string _decompressPath;

        /// <summary>
        /// 资源内嵌路径
        /// </summary>
        private string _streamingAssetsBundlePath;
        
        
        public override IDecompressAssets startDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callback)
        {
            return null;
        }

        /// <summary>
        /// 传入的文件类型是否需要解压
        /// </summary>
        /// <param name="bundleModuleEnum">解压文件类型</param>
        /// <returns>是否需要解压</returns>
        private bool ComputeDecompressFile(BundleModuleEnum bundleModuleEnum)
        {
            _streamingAssetsBundlePath = BundleSettings.Instance.GetAssetsBuiltinBundlePath(bundleModuleEnum);
            _decompressPath = BundleSettings.Instance.GetAssetsDecompressPath(bundleModuleEnum);
            return false;
        }
        
        public override float GetDecompressProgress()
        {
            return 0;
        }
    }
}
