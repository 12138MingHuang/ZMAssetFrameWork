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
        
        GameObject Instantiate(string path, Action<GameObject, object, object> load, object param1, object param2);
        
        void InstantiateAsync(string path, Action<GameObject, object, object> loadAsync, Action loading, object param1, object param2);
        
        void InstantiateAndLoad(string path, Action<GameObject, object> loadAsync, Action loading, object param1, object param2);
        
        void RemoveObjectLoadCallBack(long lodaId);
        
        void Release(GameObject obj, bool isDestroy = false);
        
        void Release(Texture texture);
        
        Sprite LoadSprite(string path);
        
        Texture LoadTexture(string path);
        
        AudioClip LoadAudio(string path);
        
        TextAsset LoadTextAsset(string path);
        
        Sprite LoadAtlasSprite(string atlasPath, string spriteName);
        
        void LoadTextureAysnc(string path, Action<Texture, object> loadAsync, object param = null);
        
        void LoadSpirteAysnc(string path, Image image, bool setNativeSize = false, Action<Sprite> loadAsync = null);
        
        void ClearAllAsyncLoadTask();

        /// <summary>
        /// 是否深度清理
        /// </summary>
        /// <param name="abSoluteCleaning"></param>
        void ClearResourcesAssets(bool abSoluteCleaning);
    }
}
