using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public partial class ZMAssetsFrame : ZMFrameBase
    {
        private IHotAssets _hotAssets = null;
        
        private IDecompressAssets _decompressAssets = null;

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            _hotAssets = new HotAssetsManager();
            _decompressAssets = new AssetsDecompressManager();
        }

        public void Update()
        {
            _hotAssets?.OnMainThreadUpdate();
        }
    }
}

