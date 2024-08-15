using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public partial class ZMAssetsFrame : ZMFrameBase
    {
        public static Transform RecycleObjRoot
        {
            get;
            private set;
        }
        
        private IHotAssets _hotAssets = null;
        
        private IDecompressAssets _decompressAssets = null;

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            GameObject recycleObjectRoot = new GameObject("RecycleObjRoot");
            RecycleObjRoot = recycleObjectRoot.transform;
            DontDestroyOnLoad(recycleObjectRoot);
            _hotAssets = new HotAssetsManager();
            _decompressAssets = new AssetsDecompressManager();
        }

        public void Update()
        {
            _hotAssets?.OnMainThreadUpdate();
        }
    }
}

