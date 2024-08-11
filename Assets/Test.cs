using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class Test : MonoBehaviour
{

    private void Awake()
    {
        ZMAssetsFrame.Instance.InitFrameWork();
        Debug.Log(Application.persistentDataPath);
    }

    private void Start()
    {
        // HotUpdateManager.Instance.CheckAssetsVersion(BundleModuleEnum.Game);
        HotUpdateManager.Instance.HotAndUnPackAssets(BundleModuleEnum.Game);
    }
}
