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
    }

    private void Start()
    {
        HotUpdateManager.Instance.CheckAssetsVersion(BundleModuleEnum.Game);
    }
}
