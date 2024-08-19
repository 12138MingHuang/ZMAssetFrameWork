using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class HallWindow : MonoBehaviour
{
    public Button exShopButton;

    private void Start()
    {
        exShopButton.onClick.AddListener(OnExShopButtonClick);
    }

    private void OnExShopButtonClick()
    {
        ZMAssetsFrame.Instantiate(AssetsPathConfig.HALL_PREFAB_PATH + "ExShopWindow", null);
    }
}
