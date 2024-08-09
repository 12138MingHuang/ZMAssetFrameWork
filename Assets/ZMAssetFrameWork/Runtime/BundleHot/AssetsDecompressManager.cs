using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        
        public override IDecompressAssets startDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callback)
        {
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
        
        public override float GetDecompressProgress()
        {
            return 0;
        }
    }
}
