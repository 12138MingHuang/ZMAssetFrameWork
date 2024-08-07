using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class Test : MonoBehaviour
{
    private HotAssetsModule assetsModule;
    private void Start()
    {
        // HotAssetsModule assetsModule = new HotAssetsModule(BundleModuleEnum.Game, this);
        assetsModule = new HotAssetsModule(BundleModuleEnum.Game, this);
        assetsModule.StartHotAssets(StartDownLoadAsset, DownLoadFinish);
    }

    private void Update()
    {
        if (assetsModule != null)
        {
            assetsModule.OnMainThreadUpdate();
        }
    }

    private void StartDownLoadAsset()
    {
        Debug.Log("StartDownLoadAsset...");
    }

    private void DownLoadFinish(BundleModuleEnum moduleEnum)
    {
        Debug.Log("DownLoadFinish moduleEnum" + moduleEnum);
    }
}
