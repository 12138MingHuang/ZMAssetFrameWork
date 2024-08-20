using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZMAssetFrameWork;

public class ExampleWindow : MonoBehaviour
{
    public Image loadSpriteImage;

    public Image loadSpriteAsyncImage;

    public Image loadAtlasSpriteImage;

    public RawImage loadTexture;
    
    public RawImage loadTextureAsync;

    public Text loadTextAssetText;

    public Transform instantiateRoot;

    public Transform instantiateAsyncRoot;
    
    private void Start()
    {
        //同步加载Sprite
        loadSpriteImage.sprite = ZMAssetsFrame.LoadSprite(AssetsPathConfig.HALL_TXTURE_PATH + "Hall/kou");
        //异步加载Sprite
        ZMAssetsFrame.LoadSpriteAsync(AssetsPathConfig.HALL_TXTURE_PATH + "Hall/majiang", loadSpriteAsyncImage);
        //加载图集中的Sprite
        loadAtlasSpriteImage.sprite = ZMAssetsFrame.LoadAtlasSprite(AssetsPathConfig.HALL_TXTURE_PATH + "Login/Login", "LoginButton");
        //同步加载图片
        loadTexture.texture = ZMAssetsFrame.LoadTexture(AssetsPathConfig.HALL_TXTURE_PATH + "Hall/bg");
        //异步加载图片
        ZMAssetsFrame.LoadTextureAsync(AssetsPathConfig.HALL_TXTURE_PATH + "Hall/bg", (texture, param) =>
        {
            loadTextureAsync.texture = texture;
        });
        //加载文本资源
        loadTextAssetText.text = ZMAssetsFrame.LoadTextAsset(AssetsPathConfig.HALL_DATA_PATH + "PrefabConfig.txt").text;
        //同步克隆对象
        ZMAssetsFrame.Instantiate(AssetsPathConfig.HALL_PREFAB_PATH + "TestObj", instantiateRoot);
        //异步克隆对象
        ZMAssetsFrame.InstantiateAsync(AssetsPathConfig.HALL_PREFAB_PATH + "TestObj", (obj, param1, param2) =>
        {
            Debug.Log("param1:" + param1 + ",param2:" + param2);
            obj.transform.SetParent(instantiateAsyncRoot);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
        }, 123, 456);
        
        //预加载资源
        ZMAssetsFrame.PreLoadObj(AssetsPathConfig.HALL_PREFAB_PATH + "TestObj", 100);
    }

}
