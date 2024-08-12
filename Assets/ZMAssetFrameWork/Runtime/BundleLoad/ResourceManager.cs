using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public class ResourceManager
    {
        /// <summary>
        /// 已经加载的资源
        /// </summary>
        private Dictionary<uint, BundleItem> _alReadyAssetDic = new Dictionary<uint, BundleItem>();
        
        /// <summary>
        /// 同步加载资源，外部直接调用，仅仅加载不需要实例化的资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>资源</returns>
        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("path is Null, return null");
                return null;
            }
            uint crc = Crc32.GetCrc32(path);
            //从缓存中获取BundleItem
            BundleItem bundleItem = GeCacheItemFormAssetDic(crc);
            
            //如果BundleItem中的对象已经加载过，就直接返回对象
            if(bundleItem.obj != null)
            {
                return bundleItem.obj as T;
            }
            
            //声明新对象
            T obj = null;
#if UNITY_EDITOR
            if(BundleSettings.Instance.loadAssetType == LoadAssetEnum.Editor)
            {
                obj = LoadAssetsFormEditor<T>(path);
            }
#endif
            if (obj == null)
            {
                //加载该路径对应的AssetBundle
                bundleItem = AssetBundleManager.Instance.LoadAssetBundle(crc);
                if (bundleItem != null)
                {
                    if(bundleItem.assetBundle != null)
                    {
                        obj = bundleItem.obj != null ? bundleItem.obj as T : bundleItem.assetBundle.LoadAsset<T>(bundleItem.assetName);
                    }
                    else
                    {
                        Debug.LogError("item.assetBundle is null");
                    }
                }
                else
                {
                    Debug.LogError("item is null... Path:" + path);
                    return null;
                }
            }
            
            bundleItem.obj = obj;
            bundleItem.path = path;
            //将对象添加到缓存中
            _alReadyAssetDic.Add(crc, bundleItem);
            return obj;
        }
        
        /// <summary>
        /// 从缓存中获取BundleItem
        /// </summary>
        /// <param name="crc">crc</param>
        /// <returns>BundleItem</returns>
        public BundleItem GeCacheItemFormAssetDic(uint crc)
        {
            BundleItem bundleItem = null;
            _alReadyAssetDic.TryGetValue(crc, out bundleItem);
            return bundleItem != null ? bundleItem : new BundleItem { crc = crc };
        }

#if UNITY_EDITOR

        /// <summary>
        /// 加载资源，仅在编辑器下使用
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>资源</returns>
        public T LoadAssetsFormEditor<T>(string path) where T : UnityEngine.Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}