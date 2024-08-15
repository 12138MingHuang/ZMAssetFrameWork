using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZMAssetFrameWork
{
    public partial class ZMAssetsFrame
    {
        /// <summary>
        /// 同步克隆物体
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="parent">父物体</param>
        /// <returns>克隆得到物体</returns>
        public static GameObject Instantiate(string path, Transform parent)
        {
            return Instance._resource.Instantiate(path, parent, Vector3.zero, Vector3.one, Quaternion.identity);
        }
        
        /// <summary>
        /// 同步克隆物体
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="parent">物体所在父物体</param>
        /// <param name="localPosition">局部位置</param>
        /// <param name="localScale">局部缩放</param>
        /// <param name="quaternion">旋转</param>
        /// <returns>克隆得到物体</returns>
        public static GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quaternion)
        {
            return Instance._resource.Instantiate(path, parent, localPosition, localScale, quaternion);
        }
        
        /// <summary>
        /// 异步克隆对象
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="loadAsync">异步加载回调</param>
        /// <param name="param1">异步加载参数1</param>
        /// <param name="param2">异步加载参数2</param>
        public static void InstantiateAsync(string path, System.Action<GameObject, object, object> loadAsync, object param1 = null, object param2 = null)
        {
            Instance._resource.InstantiateAsync(path,loadAsync,param1,param2);
        }
        
        /// <summary>
        /// 克隆并且等待资源下载完成克隆
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="loadAsync">异步加载回调</param>
        /// <param name="loading">加载中回调</param>
        /// <param name="param1">异步加载参数1</param>
        /// <param name="param2">异步加载参数2</param>
        /// <returns>资源id</returns>
        public static long InstantiateAndLoad(string path, System.Action<GameObject, object, object> loadAsync, System.Action loading, object param1 = null, object param2 = null)
        {
            return Instance._resource.InstantiateAndLoad(path, loadAsync, loading, param1, param2);
        }

        /// <summary>
        /// 预加载对象
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="count">数量</param>
        public static void PreLoadObj(string path, int count = 1)
        {
             Instance._resource.PreLoadObj(path,count);
        }
        
        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">类型</typeparam>
        public static void PreLoadResource<T>(string path) where T : UnityEngine.Object
        {
            Instance._resource.PreLoadResource<T>(path);
        }
        
        /// <summary>
        /// 移除对象加载回调
        /// </summary>
        /// <param name="loadId"></param>
        public static void RemoveObjectLoadCallBack(long loadId)
        {
            Instance._resource.RemoveObjectLoadCallBack(loadId);
        }
        
        /// <summary>
        /// 释放对象占用内存
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isDestroy">是否销毁</param>
        public static void Release(GameObject obj, bool isDestroy = false)
        {
            Instance._resource.Release(obj,isDestroy);
        }
        
        /// <summary>
        /// 释放图片所占用的内存
        /// </summary>
        /// <param name="texture">图片</param>
        public static void Release(Texture texture)
        {
            Instance._resource.Release(texture);
        }
        
        /// <summary>
        /// 加载图片资源
        /// </summary>
        /// <param name="path">Sprite图片路径</param>
        /// <returns>Sprite</returns>
        public static Sprite LoadSprite(string path)
        {
            return Instance._resource.LoadSprite(path);
        }
        
        /// <summary>
        /// 加载Texture图片资源
        /// </summary>
        /// <param name="path">Texture图片路径</param>
        /// <returns>Texture</returns>
        public static Texture LoadTexture(string path)
        {
            return Instance._resource.LoadTexture(path);
        }
        
        /// <summary>
        /// 加载音频资源
        /// </summary>
        /// <param name="path">音频路径</param>
        /// <returns>音频资源</returns>
        public static AudioClip LoadAudio(string path)
        {
            return Instance._resource.LoadAudio(path);
        }
        
        /// <summary>
        /// 加载Text文本资源
        /// </summary>
        /// <param name="path">Text文本路径</param>
        /// <returns>Text文本</returns>
        public static TextAsset LoadTextAsset(string path)
        {
            return Instance._resource.LoadTextAsset(path);
        }
        
        /// <summary>
        /// 从图集中加载指定名称的图片
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        /// <param name="spriteName">图片名称</param>
        /// <returns>图片</returns>
        public static Sprite LoadAtlasSprite(string atlasPath, string spriteName)
        {
           return Instance._resource.LoadAtlasSprite(atlasPath, spriteName);
        }

        /// <summary>
        /// 异步加载Texture图片资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="loadAsync">异步加载回调</param>
        /// <param name="param">可选参数</param>
        /// <returns>Texture图片</returns>
        public static long LoadTextureAsync(string path, Action<Texture, object> loadAsync, object param = null)
        {
            return Instance._resource.LoadTextureAsync(path,loadAsync,param);
        }
        
        /// <summary>
        /// 异步加载Sprite图片资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="image">Image组件</param>
        /// <param name="setNativeSize">是否设置为美术图的原始尺寸</param>
        /// <param name="loadAsync">加载完成回调</param>
        /// <returns>Sprite图片</returns>
        public static long LoadSpriteAsync(string path, Image image, bool setNativeSize = false, Action<Sprite> loadAsync = null)
        {
            return Instance._resource.LoadSpriteAsync(path, image, setNativeSize,loadAsync);
        }
        
        /// <summary>
        /// 清理所有异步加载任务
        /// </summary>
        public static void ClearAllAsyncLoadTask()
        {
            Instance._resource.ClearAllAsyncLoadTask();
        }
        
        /// <summary>
        /// 清理加载的资源，释放内存
        /// </summary>
        /// <param name="absoluteCleaning">深度清理：true：销毁所有由AssetBundle加载和生成的对象，彻底释放内存占用
        /// 深度清理 false：销毁对象池中的对象，但不销毁由AssetBundle克隆出并在使用的对象，具体的内存释放根据内存引用计数选择性释放</param>
        public static void ClearResourcesAssets(bool absoluteCleaning)
        {
            Instance._resource.ClearResourcesAssets(absoluteCleaning);
        }
        
        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        /// <param name="startHotCallBack">开始热更回调</param>
        /// <param name="hotFinish">热更完成回调</param>
        /// <param name="waiteDownLoad">等待下载的回调</param>
        /// <param name="isCheckAssetsVersion">是否需要检测资源版本</param>
        public static void HotAssets(BundleModuleEnum bundleModuleEnum, Action<BundleModuleEnum> startHotCallBack, Action<BundleModuleEnum> hotFinish, Action<BundleModuleEnum> waiteDownLoad, bool isCheckAssetsVersion = true)
        {
            Instance._hotAssets.HotAssets(bundleModuleEnum, startHotCallBack, hotFinish, waiteDownLoad, isCheckAssetsVersion);
        }
        
        /// <summary>
        /// 检测资源版本是否需要热更，获取需要热更资源大小
        /// </summary>
        /// <param name="bundleModuleEnum"></param>
        /// <param name="callBack">开始热更资源回调</param>
        public static void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum, Action<bool, float> callBack)
        {
            Instance._hotAssets.CheckAssetsVersion(bundleModuleEnum, callBack);
        }

        /// <summary>
        /// 获取热更模块
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <returns>热更模块</returns>
        public static HotAssetsModule GetHotAssetsModule(BundleModuleEnum bundleModuleEnum)
        {
            return Instance._hotAssets.GetHotAssetsModule(bundleModuleEnum);
        }

        /// <summary>
        /// 开始解压内嵌资源
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块类型</param>
        /// <param name="callBack">开始解压资源回调</param>
        /// <returns>热更模块</returns>
        public static IDecompressAssets StartDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callBack)
        {
            return Instance._decompressAssets.StartDecompressBuiltinFile(bundleModuleEnum, callBack);
        }
        
        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns>解压进度</returns>
        public static float GetDecompressProgress()
        {
            return Instance._decompressAssets.GetDecompressProgress();
        }
    }

}