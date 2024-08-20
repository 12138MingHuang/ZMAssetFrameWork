using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class ExShopItem : MonoBehaviour
{
    public Transform gameItemParent;

    public GameObject loadingObj;

    private GameObject _itemObj;
    
    private int _itemId;

    public void SetData(int itemId)
    {
        _itemId = itemId;
        ZMAssetsFrame.InstantiateAndLoad("Assets/BuildBundleDemo/GameItem/" + itemId + "/" + itemId, LoadItemObjComplete, ItemObjLoading);
    }
    
    private void LoadItemObjComplete(GameObject itemObj, object param1, object param2)
    {
        loadingObj.SetActive(false);
        if (itemObj != null)
        {
            itemObj.SetActive(true);
            itemObj.transform.SetParent(gameItemParent);
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;
            itemObj.transform.localRotation = Quaternion.identity;
            _itemObj = itemObj;
        }
        else
        {
            Debug.Log("item obj is Null:" + _itemId);
        }
    }

    private void ItemObjLoading()
    {
        loadingObj.SetActive(true);
    }

    public void Release()
    {
        if (_itemObj != null)
        {
            ZMAssetsFrame.Release(_itemObj, true);
        }
        ZMAssetsFrame.Release(gameObject, true);
    }
}
