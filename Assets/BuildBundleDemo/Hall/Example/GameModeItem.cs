using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class GameModeItem : MonoBehaviour
{
    public Button button;
    public Image downSliderImage;
    public Text downLoadSpeedText;//下载速度 3m/s
    public Text downLoadRatioText;//下载百分比进度 60%
    public Text downLoadProgressText;//下载进度 30m/100m
    public Text downLoadTips;//开始下载提示
    public GameObject updateRoot; //热更总节点

    public BundleModuleEnum gameType;

    private HotAssetsModule _hotModule;
    private float _lastTime;
    private float _lastDownLoadSize;
    
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnGameButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (_hotModule != null)
        {
            downLoadProgressText.text = $"{_hotModule.assetsDownLoadSizeM:F1}M/{_hotModule.AssetsMaxSizeM:F1}M";
            downLoadRatioText.text = $"{(_hotModule.assetsDownLoadSizeM / _hotModule.AssetsMaxSizeM * 100):F1}%";
            if (Time.realtimeSinceStartup - _lastTime > 1f)
            {
                downLoadSpeedText.text = $"{(_hotModule.assetsDownLoadSizeM - _lastDownLoadSize):F1}M/S";
                _lastTime = Time.realtimeSinceStartup;
                _lastDownLoadSize = _hotModule.assetsDownLoadSizeM;
            }
        }
    }

    public void OnGameButtonClick()
    {
        ZMAssetsFrame.CheckAssetsVersion(gameType, CheckAssetCallBack);
    }

    public void CheckAssetCallBack(bool isHot, float sizeM)
    {
        //如果说需要热更，就下载该模块的热更资源
        if (isHot)
        {
            ZMAssetsFrame.HotAssets(gameType, OnStartHotAssets, OnFinishHotAssets, OnWaitHotAssets);
        }
        else
        {
            ZMAssetsFrame.Release(transform.parent.parent.parent.parent.gameObject);
            //如果不需要更新，就直接加载对应模块资源进入游戏
            ZMAssetsFrame.ClearResourcesAssets(true);
            ZMAssetsFrame.Instantiate("Assets/BuildBundleDemo/" + gameType + "/Prefab/" + gameType + "Window", null, Vector3.zero, Vector3.one, Quaternion.identity);
        }
    }

    public void OnStartHotAssets(BundleModuleEnum moduleType)
    {
        updateRoot.SetActive(true);
        downLoadTips.text = "正在更新";
        _hotModule = ZMAssetsFrame.GetHotAssetsModule(moduleType);
    }
    
    public void OnFinishHotAssets(BundleModuleEnum moduleType)
    {
        _hotModule = null;
        updateRoot.SetActive(false);
        downLoadTips.text = "更新完成";
        Debug.Log("热更完成: " + moduleType);
    }
    
    public void OnWaitHotAssets(BundleModuleEnum moduleType)
    {
        updateRoot.SetActive(true);
        downLoadTips.text = "等待更新";
    }
}
