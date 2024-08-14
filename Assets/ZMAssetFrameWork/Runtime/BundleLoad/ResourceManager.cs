using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 缓存对象
    /// </summary>
    public class CacheObject
    {
        /// <summary>
        /// 缓存对象的crc
        /// </summary>
        public uint crc;
        
        /// <summary>
        /// 缓存对象的路径
        /// </summary>
        public string path;
        
        /// <summary>
        /// 缓存对象的id
        /// </summary>
        public int insId;
        
        /// <summary>
        /// 缓存对象实体
        /// </summary>
        public UnityEngine.GameObject obj;

        /// <summary>
        /// 释放缓存对象
        /// </summary>
        public void Release()
        {
            crc = 0;
            path = "";
            insId = 0;
            obj = null;
        }
    }

    /// <summary>
    /// 加载对象回调
    /// </summary>
    public class LoadObjectCallBack
    {
        public string path;
        public uint crc;
        public object param1;
        public object param2;
        public System.Action<GameObject, object, object> loadResult;
    }
    
    public class ResourceManager
    {
        /// <summary>
        /// 已经加载的资源 key 未资源路径crc value为资源对象
        /// </summary>
        private Dictionary<uint, BundleItem> _alReadyAssetDic = new Dictionary<uint, BundleItem>();

        /// <summary>
        /// 对象池字典
        /// </summary>
        private Dictionary<uint, List<CacheObject>> _objectPoolDic = new Dictionary<uint, List<CacheObject>>();

        /// <summary>
        /// 所有的对象缓存字典
        /// </summary>
        private Dictionary<int, CacheObject> _allObjectDic = new Dictionary<int, CacheObject>();
        
        /// <summary>
        /// 缓存对象的对象池
        /// </summary>
        private ClassObjectPool<CacheObject> _cacheObjectPool = new ClassObjectPool<CacheObject>(300);

        /// <summary>
        /// 异步加载任务列表
        /// </summary>
        private List<long> _asyncLoadingTaskList = new List<long>();

        /// <summary>
        /// 异步加载任务唯一id
        /// </summary>
        private long _asyncGuid;

        /// <summary>
        /// 异步加载任务唯一id
        /// </summary>
        private long _asyncTaskGuid
        {
            get
            {
                if(_asyncGuid > long.MaxValue)
                {
                    _asyncGuid = 0;
                }
                return _asyncGuid++;
            }
        }
        
        /// <summary>
        /// 加载对象回调字典
        /// </summary>
        private Dictionary<long, LoadObjectCallBack> _loadObjectCallBackDic = new Dictionary<long, LoadObjectCallBack>();
        
        /// <summary>
        /// 等待加载的资源列表
        /// </summary>
        private List<HotFileInfo> _waitLoadAssetsList = new List<HotFileInfo>();

        public void Initlizate()
        {
            HotAssetsManager.DownLoadBundleFinish += AssetsDownLoadFinish;
        }

        private void AssetsDownLoadFinish(HotFileInfo hotFileInfo)
        {
            Debug.Log("ReourceManager AssetsDownLoadFinish: " + hotFileInfo.abName);
            //如果回调字典长度大于0 才需要去处理回调
            if (_loadObjectCallBackDic.Count > 0)
            {
                //根据对象的路径查找对象所在的AB包，以及这个AB包下的所有资源
                List<BundleItem> bundleItemList = AssetBundleManager.Instance.GetBundleItemByABName(hotFileInfo.abName);
                //如果bundleItemList.Count==0则说明配置文件未加载，资源下载是多线程下的
                //有可能会出现AssetBundle下载的速度比AssetBundleConfig配置文件快，这种情况下AB配置文件就处于未加载状态
                if (bundleItemList.Count == 0)
                {
                    for (int i = 0; i < _waitLoadAssetsList.Count; ++i)
                    {
                        if(hotFileInfo.abName == _waitLoadAssetsList[i].abName)
                        {
                            return;
                        }
                    }
                    _waitLoadAssetsList.Add(hotFileInfo);
                    return;
                }
                
                List<long> removeLst = new List<long>();
                //遍历对象回调，触发资源加载
                foreach (var item in _loadObjectCallBackDic)
                {
                    if(ListContainsAsset(bundleItemList, item.Value.crc))
                    {
                        Debug.Log("ReourceManager AssetsDownLoadFinish Load Obj path: " + item.Value.path);
                        item.Value.loadResult?.Invoke(Instantiate(item.Value.path, null, Vector3.zero, Vector3.one, Quaternion.identity), 
                            item.Value.param1, item.Value.param2);
                        
                        removeLst.Add(item.Key);
                    }
                }
                //移除字典中的回调
                for (int i = 0; i < removeLst.Count; i++)
                {
                    _loadObjectCallBackDic.Remove(removeLst[i]);
                }
            }
        }

        private bool ListContainsAsset(List<BundleItem> bundleItemList, uint crc)
        {
            foreach (BundleItem item in bundleItemList)
            {
                if (item.crc == crc)
                {
                    return true;
                }
            }
            return false;
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
        public GameObject Instantiate(string path, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion quaternion)
        {
            path = path.EndsWith(".prefab") ? path : (path + ".prefab");
            //先从对象池中查询这个对象，如果存在就直接使用
            GameObject cacheObj = GetCacheObjFormPools(Crc32.GetCrc32(path));
            if (cacheObj != null)
            {
                cacheObj.transform.SetParent(parent);
                cacheObj.transform.localPosition = localPosition;
                cacheObj.transform.localScale = localScale;
                cacheObj.transform.localRotation = quaternion;
                return cacheObj;
            }
            //加载该对象
            GameObject obj = LoadResource<GameObject>(path);
            if (obj != null)
            {
                GameObject newObj = Instantiate(path, obj, parent);
                newObj.transform.localPosition = localPosition;
                newObj.transform.localScale = localScale;
                newObj.transform.localRotation = quaternion;
                return newObj;
            }
            else
            {
                Debug.LogError("GameObject load failed, path is null");
                return null;
            }
        }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="obj">对象</param>
        /// <param name="parent">物体所在父物体</param>
        /// <returns>克隆所得到的对象</returns>
        private GameObject Instantiate(string path, GameObject obj, Transform parent)
        {
            obj = GameObject.Instantiate(obj, parent, false);
            CacheObject cacheObj = _cacheObjectPool.Spawn();
            cacheObj.obj = obj;
            cacheObj.path = path;
            cacheObj.crc = Crc32.GetCrc32(path);
            cacheObj.insId = obj.GetInstanceID();
            
            //将对象添加到对象池中
            _allObjectDic.Add(cacheObj.insId, cacheObj);
            return obj;
        }
        
        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="crc">对象的路径crc</param>
        /// <returns>获取的对象</returns>
        private GameObject GetCacheObjFormPools(uint crc)
        {
            List<CacheObject> cacheObjectList = null;
            _objectPoolDic.TryGetValue(crc, out cacheObjectList);
            if (cacheObjectList != null && cacheObjectList.Count > 0)
            {
                //直接去对象池中的第0个对象
                CacheObject cacheObj = cacheObjectList[0];
                cacheObjectList.RemoveAt(0);
                return cacheObj.obj;
            }
            return null;
        }

        /// <summary>
        /// 异步克隆对象
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="loadAsync">异步加载回调</param>
        /// <param name="param1">异步加载参数1</param>
        /// <param name="param2">异步加载参数2</param>
        public void InstantiateAsync(string path, System.Action<GameObject, object, object> loadAsync, object param1 = null, object param2 = null)
        {
            path = path.EndsWith(".prefab") ? path : (path + ".prefab");
            //先从对象池中查询这个对象，如果存在就直接使用
            GameObject cacheObj = GetCacheObjFormPools(Crc32.GetCrc32(path));
            if (cacheObj != null)
            {
                loadAsync?.Invoke(null, cacheObj, null);
                return;
            }
            //获取异步加载任务唯一id
            long guid = _asyncTaskGuid;
            _asyncLoadingTaskList.Add(guid);
            //开始异步加载资源
            LoadResourceAsync<GameObject>(path, (obj) =>
            {
                //异步加载完成
                if (obj != null)
                {
                    if(_asyncLoadingTaskList.Contains(guid))
                    {
                        _asyncLoadingTaskList.Remove(guid);
                        GameObject newObj = Instantiate(path, (GameObject)obj, null);
                    }
                }
                else
                {
                    _asyncLoadingTaskList.Remove(guid);
                    Debug.LogError("GameObject Async load failed, Path: " + path);
                }
            });
        }

        public long InstantiateAndLoad(string path, System.Action<GameObject, object, object> loadAsync, System.Action loading, object param1 = null, object param2 = null)
        {
            path = path.EndsWith(".prefab") ? path : (path + ".prefab");
            //先从对象池中查询这个对象，如果存在就直接使用
            GameObject cacheObj = GetCacheObjFormPools(Crc32.GetCrc32(path));
            long loadId = -1;
            if(cacheObj != null)
            {
                loadAsync?.Invoke(null, cacheObj, null);
            }
            GameObject obj = Instantiate(path, null, Vector3.zero, Vector3.one, Quaternion.identity);
            
            if(obj != null)
            {
                loadAsync?.Invoke(null, param1, param2);
            }
            else
            {
                //资源没有下载完成，本地没有这个资源
                loadId = _asyncTaskGuid;
                loading?.Invoke();
                _loadObjectCallBackDic.Add(loadId, new LoadObjectCallBack
                {
                    path = path,
                    crc = Crc32.GetCrc32(path),
                    loadResult = loadAsync,
                    param1 = param1,
                    param2 = param2
                });
            }
            return loadId;
        }
        
        #region 资源加载
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
        /// 异步加载资源，外部直接调用，仅仅加载不需要实例化的资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>资源</returns>
        public void LoadResourceAsync<T>(string path, System.Action<UnityEngine.Object> loadFinish) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                loadFinish?.Invoke(null);
            }
            uint crc = Crc32.GetCrc32(path);
            //从缓存中获取BundleItem
            BundleItem bundleItem = GeCacheItemFormAssetDic(crc);
            
            //如果BundleItem中的对象已经加载过，就直接返回对象
            if(bundleItem.obj != null)
            {
                loadFinish?.Invoke(bundleItem.obj as T);
            }
            
            //声明新对象
            T obj = null;
#if UNITY_EDITOR
            if(BundleSettings.Instance.loadAssetType == LoadAssetEnum.Editor)
            {
                obj = LoadAssetsFormEditor<T>(path);
                loadFinish?.Invoke(obj);
            }
#endif
            if (obj == null)
            {
                //加载该路径对应的AssetBundle
                bundleItem = AssetBundleManager.Instance.LoadAssetBundle(crc);
                if (bundleItem != null)
                {
                    if(bundleItem.obj != null)
                    {
                        loadFinish?.Invoke(bundleItem.obj as T);
                        bundleItem.path = path;
                        bundleItem.crc = crc;
                        _alReadyAssetDic.Add(crc, bundleItem);
                    }
                    else
                    {
                        //通过异步方式加载AssetBundle
                        AssetBundleRequest bundleRequest = bundleItem.assetBundle.LoadAssetAsync<T>(bundleItem.assetName);
                        bundleRequest.completed += (asyncOperation) =>
                        {
                            UnityEngine.Object loadObj = (asyncOperation as AssetBundleRequest).asset;
                            bundleItem.obj = loadObj;
                            bundleItem.path = path;
                            bundleItem.crc = crc;
                            if (!_alReadyAssetDic.ContainsKey(crc))
                            {
                                _alReadyAssetDic.Add(crc, bundleItem);
                            }
                            loadFinish?.Invoke(loadObj);
                        };
                    }
                }
                else
                {
                    Debug.LogError("item is null... Path:" + path);
                    loadFinish?.Invoke(null);
                }
            }
            else
            {
                bundleItem.obj = obj;
                bundleItem.path = path;
                //将对象添加到缓存中
                _alReadyAssetDic.Add(crc, bundleItem);
            }
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

        #endregion
    }
}