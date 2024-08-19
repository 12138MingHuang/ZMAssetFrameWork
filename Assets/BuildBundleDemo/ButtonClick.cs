using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class ButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        ZMAssetsFrame.Release(transform.parent.gameObject, true);
        ZMAssetsFrame.ClearResourcesAssets(true);
        ZMAssetsFrame.Instantiate("Assets/BuildBundleDemo/Hall/Prefab/HallWindow", null, Vector3.zero, Vector3.one, Quaternion.identity);
    }
}
