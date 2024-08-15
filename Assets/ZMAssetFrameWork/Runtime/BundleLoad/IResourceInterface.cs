using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ZMAssetFrameWork
{
    public interface IResourceInterface
    {
        void Initlizate();
        
        void PreLoadObj(string path, int count = 1);

        void PreLoadResource<T>(string path) where T : UnityEngine.Object;
        
        GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quaternion);
        
        void InstantiateAsync(string path, System.Action<GameObject, object, object> loadAsync, object param1 = null, object param2 = null);
        
        long InstantiateAndLoad(string path, System.Action<GameObject, object, object> loadAsync, System.Action loading, object param1 = null, object param2 = null);
        
        void RemoveObjectLoadCallBack(long loadId);
        
        void Release(GameObject obj, bool isDestroy = false);
        
        void Release(Texture texture);
        
        Sprite LoadSprite(string path);
        
        Texture LoadTexture(string path);
        
        AudioClip LoadAudio(string path);
        
        TextAsset LoadTextAsset(string path);
        
        Sprite LoadAtlasSprite(string atlasPath, string spriteName);
        
        long LoadTextureAsync(string path, Action<Texture, object> loadAsync, object param = null);
        
        long LoadSpriteAsync(string path, Image image, bool setNativeSize = false, Action<Sprite> loadAsync = null);
        
        void ClearAllAsyncLoadTask();

        /// <summary>
        /// 是否深度清理
        /// </summary>
        /// <param name="abSoluteCleaning"></param>
        void ClearResourcesAssets(bool abSoluteCleaning);
    }
}
