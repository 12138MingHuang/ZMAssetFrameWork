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

        private IResourceInterface _resource = null;

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFrameWork()
        {
            GameObject recycleObjectRoot = new GameObject("RecycleObjRoot");
            RecycleObjRoot = recycleObjectRoot.transform;
            recycleObjectRoot.SetActive(false);
            DontDestroyOnLoad(recycleObjectRoot);
            _hotAssets = new HotAssetsManager();
            _decompressAssets = new AssetsDecompressManager();
            _resource = new ResourceManager();
            _resource.Initlizate();
        }

        public void Update()
        {
            _hotAssets?.OnMainThreadUpdate();
        }
    }
}

