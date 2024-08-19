using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class ExShopWindow : MonoBehaviour
{
    public Transform itemParent;
    
    //游戏道具ID列表
    private List<int> itemIdList = new List<int>();
    
    private List<ExShopItem> exShopItemList = new List<ExShopItem>();

    private void Awake()
    {
        ZMAssetsFrame.HotAssets(BundleModuleEnum.GameItem, null, null, null);
    }

    private void Start()
    {
        for (int i = 0; i < 15; i++)
        {
            itemIdList.Add(i + 6000 + 1);
        }
        
        //生成兑换道具列表
        foreach (int id in itemIdList)
        {
            GameObject itemObj = ZMAssetsFrame.Instantiate(AssetsPathConfig.HALL_PREFAB_PATH + "ExShopItem", itemParent);
            itemObj.SetActive(true);
            ExShopItem exShopItem = itemObj.GetComponent<ExShopItem>();
            exShopItem.SetData(id);
            exShopItemList.Add(exShopItem);
        }
    }

    private void OnDestroy()
    {
        foreach (ExShopItem exShopItem in exShopItemList)
        {
            exShopItem.Release();
        }
    }
}
