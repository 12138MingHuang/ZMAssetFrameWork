using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ZMAssetFrameWork
{
    public class AssetsDecompressManager : IDecompressAssets
    {

        /// <summary>
        /// 资源解压路径
        /// </summary>
        private string _decompressPath;

        /// <summary>
        /// 资源内嵌路径
        /// </summary>
        private string _streamingAssetsBundlePath;

        /// <summary>
        /// 需要解压的资源列表
        /// </summary>
        private List<string> _needDecompressFileList = new List<string>();
        
        /// <summary>
        /// 开始解压内嵌文件
        /// </summary>
        /// <param name="bundleModuleEnum">解压资源模块</param>
        /// <param name="callback">解压完成回调</param>
        /// <returns>解压的内嵌文件</returns>
        public override IDecompressAssets StartDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callback)
        {
            if(ComputeDecompressFile(bundleModuleEnum))
            {
                IsStartDecompress = true;
                //开始解压文件
                ZMAssetsFrame.Instance.StartCoroutine(UnPackToPersistentDataPath(bundleModuleEnum, callback));
            }
            else
            {
                Debug.Log("不需要解压文件");
                callback?.Invoke();
            }
            return null;
        }

        /// <summary>
        /// 传入的文件类型是否需要解压
        /// </summary>
        /// <param name="bundleModuleEnum">解压文件类型</param>
        /// <returns>是否需要解压</returns>
        private bool ComputeDecompressFile(BundleModuleEnum bundleModuleEnum)
        {
            _streamingAssetsBundlePath = BundleSettings.Instance.GetAssetsBuiltinBundlePath(bundleModuleEnum);
            _decompressPath = BundleSettings.Instance.GetAssetsDecompressPath(bundleModuleEnum);
            _needDecompressFileList.Clear();
#if UNITY_ANDROID || UNITY_IOS
            //如果文件夹不存在，就进行创建
            if (!Directory.Exists(_decompressPath))
            {
                Directory.CreateDirectory(_decompressPath);
            }
            
            //计算需要解压的文件，以及大小
            TextAsset textAsset = Resources.Load<TextAsset>(bundleModuleEnum + "Info");
            if (textAsset != null)
            {
                List<BuiltinBundleInfo> builtinBundleInfoList = JsonConvert.DeserializeObject<List<BuiltinBundleInfo>>(textAsset.text);
                foreach (BuiltinBundleInfo info in builtinBundleInfoList)
                {
                    //本地文件存储路径
                    string localFilePath = _decompressPath + "/" + info.fileName;
                    if(localFilePath.EndsWith(".meta"))
                    {
                        continue;
                    }
                    //计算出需要解压的文件
                    if (!File.Exists(localFilePath) || MD5.GetMd5FromFile(localFilePath) != info.md5)
                    {
                        _needDecompressFileList.Add(info.fileName);
                        //计算出需要解压的文件大小
                        TotalSizem += info.size / 1024.0f;
                    }
                }
            }
            else
            {
                Debug.LogError(bundleModuleEnum + "Info" + "不存在，请检查内嵌资源是否内嵌");
            }
            return _needDecompressFileList.Count > 0;
#else
            return false;
#endif
        }
        
        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns>解压进度</returns>
        public override float GetDecompressProgress()
        {
            return AlreadyDecompressSize / TotalSizem;
        }

        /// <summary>
        /// 解压文件到持久化目录(协程方法)
        /// </summary>
        /// <param name="bundleModuleEnum">解压的资源类型</param>
        /// <param name="callback">解压完成回调</param>
        /// <returns></returns>
        private IEnumerator  UnPackToPersistentDataPath(BundleModuleEnum bundleModuleEnum, Action callback)
        {
            foreach (string fileName in _needDecompressFileList)
            {
                string filePath = "";
#if UNITY_EDITOR_OSX || UNIITY_IOS
                filePath = "file://" + _streamingAssetsBundlePath + fileName;
#else
                filePath = _streamingAssetsBundlePath + fileName;
#endif
                Debug.Log("Start UnPack AssetBundle filePath: " + filePath + "\r\n UnpackPath:" + _decompressPath);
                //通过UnityWebRequest(Http)访问文件，这个过程不消耗流量，相当于直接读取，所以速度非常快
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(filePath);
                unityWebRequest.timeout = 30;
                yield return unityWebRequest.SendWebRequest();

                if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("UnPack Error: " + unityWebRequest.error);
                }
                else
                {
                    //到了这一步，文件就已经读取完成了
                    byte[] fileBytes = unityWebRequest.downloadHandler.data;
                    FileHelper.WriteFile(_decompressPath + fileName, fileBytes);
                    AlreadyDecompressSize += fileBytes.Length / 1024.0f / 1024.0f;
                    Debug.Log("AlreadyDecompressSize:" + AlreadyDecompressSize + " TotalSizem:" + TotalSizem);
                    Debug.Log("UnPack Finish " + _decompressPath + fileName);
                }
                unityWebRequest.Dispose();
            }
            
            callback?.Invoke();
            IsStartDecompress = false;
        }
    }
}
