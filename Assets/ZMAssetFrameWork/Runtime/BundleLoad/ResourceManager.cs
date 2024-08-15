using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    
    public class ResourceManager : IResourceInterface
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
        
        /// <summary>
        /// 预加载对象
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="count">数量</param>
        public void PreLoadObj(string path, int count = 1)
        {
            List<GameObject> preLoadObjList = new List<GameObject>();
            for (int i = 0; i < count; ++i)
            {
                preLoadObjList.Add(Instantiate(path, null, Vector3.zero, Vector3.one, Quaternion.identity));
            }
            //回收对象到对象池
            foreach (GameObject obj in preLoadObjList)
            {
                Release(obj);
            }
        }
        
        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">类型</typeparam>
        public void PreLoadResource<T>(string path) where T : Object
        {
            LoadResource<T>(path);
        }
        
        /// <summary>
        /// 移除对象加载回调
        /// </summary>
        /// <param name="loadId"></param>
        public void RemoveObjectLoadCallBack(long loadId)
        {
            if (loadId == -1)
            {
                return;
            }
            if(_loadObjectCallBackDic.ContainsKey(loadId))
            {
                _loadObjectCallBackDic.Remove(loadId);
            }
        }
        
        /// <summary>
        /// 释放对象占用内存
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isDestroy">是否销毁</param>
        public void Release(GameObject obj, bool isDestroy = false)
        {
            CacheObject cacheObject = null;
            int insId = obj.GetInstanceID();
            _allObjectDic.TryGetValue(insId, out cacheObject);
            //通过GameObject.Instantiate创建的对象 不支持回收，因为对象池中没有记录
            if (cacheObject == null)
            {
                Debug.LogError("Recycle obj failed,obj is GameObject.Instantiate create");
                return;
            }

            if (isDestroy)
            {
                GameObject.Destroy(obj);
                if(_allObjectDic.ContainsKey(insId))
                {
                    _allObjectDic.Remove(insId);
                }
                //获取该物体所在的对象池
                List<CacheObject> objectPoolList = null;
                _objectPoolDic.TryGetValue(cacheObject.crc, out objectPoolList);
                if (objectPoolList != null)
                {
                    //从对象池中移除缓存对象
                    if(objectPoolList.Contains(cacheObject))
                    {
                        objectPoolList.Remove(cacheObject);
                    }
                    cacheObject.Release();
                    _cacheObjectPool.Recycle(cacheObject);
                }
                //如果该对象不在对象池，或者已经全部释放了，就卸载该对象AssetBundle的资源占用
                if(objectPoolList == null || objectPoolList.Count == 0)
                {
                    BundleItem bundleItem = null;
                    if (_alReadyAssetDic.TryGetValue(cacheObject.crc, out bundleItem))
                    {
                        AssetBundleManager.Instance.ReleaseAssets(bundleItem, true);
                    }
                    else
                    {
                        Debug.LogError("_alreadyLoadAssetsDic not find BundleItem Path: " + cacheObject.path);
                    }
                }
            }
            else
            {
                //回收到对象池
                List<CacheObject> objectList = null;
                _objectPoolDic.TryGetValue(cacheObject.crc, out objectList);
                //字典中没有该对象池
                if (objectList == null)
                {
                    //创建对象池
                    objectList = new List<CacheObject>();
                    objectList.Add(cacheObject);
                    _objectPoolDic.Add(cacheObject.crc, objectList);
                }
                else
                {
                    //回收到对象池
                    objectList.Add(cacheObject);
                }
                //回收到对象会节点下
                if(cacheObject.obj != null)
                {
                    cacheObject.obj.transform.SetParent(ZMAssetsFrame.RecycleObjRoot);
                }
                else
                {
                    Debug.LogError("cacheObject.obj is null,Release failed");
                }
            }
        }

        /// <summary>
        /// 释放图片所占用的内存
        /// </summary>
        /// <param name="texture">图片</param>
        public void Release(Texture texture)
        {
            Resources.UnloadAsset(texture);
        }
        
        /// <summary>
        /// 加载图片资源
        /// </summary>
        /// <param name="path">Sprite图片路径</param>
        /// <returns>Sprite</returns>
        public Sprite LoadSprite(string path)
        {
            if(!path.EndsWith(".png")) path += ".png";
            return Resources.Load<Sprite>(path);
        }
        
        /// <summary>
        /// 加载Texture图片资源
        /// </summary>
        /// <param name="path">Texture图片路径</param>
        /// <returns>Texture</returns>
        public Texture LoadTexture(string path)
        {
            if(!path.EndsWith(".jpg")) path += ".jpg";
            return Resources.Load<Texture>(path);
        }
        
        /// <summary>
        /// 加载音频资源
        /// </summary>
        /// <param name="path">音频路径</param>
        /// <returns>音频资源</returns>
        public AudioClip LoadAudio(string path)
        {
            return LoadResource<AudioClip>(path);
        }
        
        /// <summary>
        /// 加载Text文本资源
        /// </summary>
        /// <param name="path">Text文本路径</param>
        /// <returns>Text文本</returns>
        public TextAsset LoadTextAsset(string path)
        {
            return LoadResource<TextAsset>(path);
        }
        
        /// <summary>
        /// 从图集中加载指定名称的图片
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        /// <param name="spriteName">图片名称</param>
        /// <returns>图片</returns>
        public Sprite LoadAtlasSprite(string atlasPath, string spriteName)
        {
            if (!atlasPath.EndsWith(".spriteatlas")) atlasPath += ".spriteatlas";
            return LoadSpriteFormAtlas(LoadResource<SpriteAtlas>(atlasPath), spriteName);
        }
        
        /// <summary>
        /// 从图集中加载指定名称的图片
        /// </summary>
        /// <param name="spriteAtlas">图集</param>
        /// <param name="spriteName">指定图片名称</param>
        /// <returns>图片</returns>
        private Sprite LoadSpriteFormAtlas(SpriteAtlas spriteAtlas, string spriteName)
        {
            if (spriteAtlas == null)
            {
                Debug.LogError("Not find spriteAtlas Name:" + spriteName);
                return null;
            }
            //从图集中获取指定名称的图片
            Sprite sprite = spriteAtlas.GetSprite(spriteName);
            if (sprite != null)
            {
                return sprite;
            }
            Debug.LogError("Not find sprite Name:" + spriteName);
            return null;
        }
        
        /// <summary>
        /// 异步加载Texture图片资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="loadAsync">异步加载回调</param>
        /// <param name="param">可选参数</param>
        /// <returns>Texture图片</returns>
        public long LoadTextureAsync(string path, Action<Texture, object> loadAsync, object param = null)
        {
            if (!path.EndsWith(".jpg")) path += ".jpg";

            long guid = _asyncTaskGuid;
            _asyncLoadingTaskList.Add(guid);
            LoadResourceAsync<Texture>(path, (texture) =>
            {
                if (loadAsync != null)
                {
                    if (_asyncLoadingTaskList.Contains(guid))
                    {
                        _asyncLoadingTaskList.Remove(guid);
                        loadAsync?.Invoke(texture as Texture, param);
                    }
                }
                else
                {
                    _asyncLoadingTaskList.Remove(guid);
                    Debug.LogError("Async Load texture is null, Path:" + path);
                }
            });
            return guid;
        }
        
        /// <summary>
        /// 异步加载Sprite图片资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="image">Image组件</param>
        /// <param name="setNativeSize">是否设置为美术图的原始尺寸</param>
        /// <param name="loadAsync">加载完成回调</param>
        /// <returns>Sprite图片</returns>
        public long LoadSpriteAsync(string path, Image image, bool setNativeSize = false, Action<Sprite> loadAsync = null)
        {
            if (!path.EndsWith(".png")) path += ".png";
            
            long guid = _asyncTaskGuid;
            _asyncLoadingTaskList.Add(guid);
            LoadResourceAsync<Sprite>(path, (obj) =>
            {
                if (obj != null)
                {
                    if (_asyncLoadingTaskList.Contains(guid))
                    {
                        Sprite sprite = obj as Sprite;
                        if(image != null)
                        {
                            image.sprite = sprite;
                            if (setNativeSize) image.SetNativeSize();
                        }
                        _asyncLoadingTaskList.Remove(guid);
                        loadAsync?.Invoke(sprite);
                    }
                }
                else
                {
                    _asyncLoadingTaskList.Remove(guid);
                    Debug.LogError("Async Load sprite is null, Path:" + path);
                }
            });
            return guid;
        }
        
        /// <summary>
        /// 清理所有异步加载任务
        /// </summary>
        public void ClearAllAsyncLoadTask()
        {
            _asyncLoadingTaskList.Clear();
        }
        
        /// <summary>
        /// 清理加载的资源，释放内存
        /// </summary>
        /// <param name="abSoluteCleaning">深度清理: true: 销毁所有由AssetBundle加载和生成的对象，彻底释放内存占用
        /// 深度清理: false: 销毁对象池中的对象，但不销毁由AssetBundle克隆出并在使用的对象，具体内存释放根据内存引用计数选择性释放</param>
        public void ClearResourcesAssets(bool abSoluteCleaning)
        {
            if (abSoluteCleaning)
            {
                foreach (var item in _allObjectDic)
                {
                    if (item.Value.obj != null)
                    {
                        //销毁GameObject对象，回收缓存类对象，等待下次复用
                        GameObject.Destroy(item.Value.obj);
                        item.Value.Release();
                        _cacheObjectPool.Recycle(item.Value);
                    }
                }
                //清理列表
                _allObjectDic.Clear();
                _objectPoolDic.Clear();
                ClearAllAsyncLoadTask();
            }
            else
            {
                foreach (List<CacheObject> objList in _objectPoolDic.Values)
                {
                    if (objList != null)
                    {
                        foreach (CacheObject cacheObject in objList)
                        {
                            if(cacheObject.obj != null)
                            {
                                //销毁GameObject对象，回收缓存类对象，等待下次复用
                                GameObject.Destroy(cacheObject.obj);
                                cacheObject.Release();
                                _cacheObjectPool.Recycle(cacheObject);
                            }
                        }
                    }
                }
                _objectPoolDic.Clear();
            }
            
            //释放AssetBundle以及里面的资源所占用的内存
            foreach (var item in _alReadyAssetDic)
            {
                AssetBundleManager.Instance.ReleaseAssets(item.Value, abSoluteCleaning);
            }
            
            //清理列表
            _loadObjectCallBackDic.Clear();
            _alReadyAssetDic.Clear();
            //释放未使用的资源(未使用的资源值得是 没有被引用的资源)
            Resources.UnloadUnusedAssets();
            //触发GC垃圾回收
            System.GC.Collect();
        }

        #region 对象加载
        
        /// <summary>
        /// AssetBundle资源下载完成回调
        /// </summary>
        /// <param name="hotFileInfo">热更文件信息</param>
        private void AssetsDownLoadFinish(HotFileInfo hotFileInfo)
        {
            Debug.Log("ReourceManager AssetsDownLoadFinish: " + hotFileInfo.abName);
            //处理比AssetBundle配置文件先下载下来的AssetBundle的加载
            if(hotFileInfo.abName == "AssetBundleConfig")
            {
                Debug.Log("Handler waitLoadList Count: " + _waitLoadAssetsList.Count);
                HotFileInfo[] hotFileArray = _waitLoadAssetsList.ToArray();
                _waitLoadAssetsList.Clear();
                foreach (HotFileInfo item in hotFileArray)
                {
                    AssetsDownLoadFinish(item);
                }
                return;
            }
            
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
                        //去重
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
        
        #endregion
        
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