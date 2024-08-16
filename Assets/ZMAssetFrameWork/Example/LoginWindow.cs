using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZMAssetFrameWork;

public class LoginWindow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLoginButtonClick()
    {
        ZMAssetsFrame.Release(gameObject);
        //弹出大厅弹窗
        ZMAssetsFrame.Instantiate("Assets/BuildBundleDemo/Hall/Prefab/HallWindow", null, Vector3.zero, Vector3.one, Quaternion.identity);
    }
}
