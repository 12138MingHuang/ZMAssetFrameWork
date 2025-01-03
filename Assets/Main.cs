using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class Main : MonoBehaviour
{

    private void Awake()
    {
        ZMAssetsFrame.Instance.InitFrameWork();
        Debug.Log(Application.persistentDataPath);
    }

    private void Start()
    {
        HotUpdateManager.Instance.HotAndUnPackAssets(BundleModuleEnum.Hall, this);
    }
    
    public void StartGame()
    {
        ZMAssetsFrame.Instantiate("Assets/BuildBundleDemo/Hall/Prefab/LoginWindow", null, Vector3.zero, Vector3.one, Quaternion.identity);
        ZMAssetsFrame.HotAssets(BundleModuleEnum.GameItem, null, null, null);
    }
}
