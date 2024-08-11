using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class HotAssetsWindow : MonoBehaviour
{
    public Slider progressSlider;
    public Text progressText;
    public Text rateText;
    public GameObject updateNoticeObj;//更新公告总结点
    public Text updateNoticeText;//更新公告文本
    
    private HotAssetsModule _hotAssetsModule;
    private IDecompressAssets _decompressAssets;

    /// <summary>
    /// 显示解压文件进度
    /// </summary>
    /// <param name="decompressAssets">解压资源</param>
    public void ShowDecompressProgress(IDecompressAssets decompressAssets)
    {
        _decompressAssets = decompressAssets;
        progressText.text = "";
        progressSlider.value = 0;
    }

    /// <summary>
    /// 显示热更资源进度
    /// </summary>
    /// <param name="assetsModule">热更资源</param>
    public void ShowHotAssetsProgress(HotAssetsModule assetsModule)
    {
        _decompressAssets = null;
        progressText.text = "";
        progressSlider.value = 0;
        _hotAssetsModule = assetsModule;
        updateNoticeObj.SetActive(true);
        updateNoticeText.text = assetsModule.UpdateNoticeContent.Replace("\\n", "\n");
    }
    
    private void Update()
    {
        if (_decompressAssets != null && progressSlider.value != 1.0f)
        {
            // Debug.Log("mDecompressAssets.GetDecompressProgress():"+ _decompressAssets.GetDecompressProgress());
            progressText.text = "资源解压中,过程中不消耗流量...";
            progressSlider.value = _decompressAssets.GetDecompressProgress();
        }

        if (_hotAssetsModule != null && progressSlider.value != 1.0f)
        {
            // Debug.Log("AssetsDownLoadSizeM:" + _hotAssetsModule.assetsDownLoadSizeM + " AssetsMaxSizeM:"+ _hotAssetsModule.AssetsMaxSizeM);
            progressText.text = $"资源下载中...{_hotAssetsModule.assetsDownLoadSizeM:F1}M/{_hotAssetsModule.AssetsMaxSizeM:F1}M";
            progressSlider.value = _hotAssetsModule.assetsDownLoadSizeM / _hotAssetsModule.AssetsMaxSizeM;
        }
    }
}
