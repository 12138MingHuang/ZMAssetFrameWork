using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class HallWindow : MonoBehaviour
{
    public Button exShopButton;

    public Button exampleButton;

    private void Start()
    {
        exShopButton.onClick.AddListener(OnExShopButtonClick);
        exampleButton.onClick.AddListener(OnExampleButtonClick);
    }

    private void OnExShopButtonClick()
    {
        ZMAssetsFrame.Instantiate(AssetsPathConfig.HALL_PREFAB_PATH + "ExShopWindow", null);
    }

    private void OnExampleButtonClick()
    {
        ZMAssetsFrame.Instantiate(AssetsPathConfig.HALL_PREFAB_PATH + "ExampleWindow", null);
    }
}
