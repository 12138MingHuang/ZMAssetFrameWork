using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public abstract class IDecompressAssets
    {
        /// <summary>
        /// 需要解压的资源总大小
        /// </summary>
        public float TotalSizem { get; protected set; }

        /// <summary>
        /// 已经解压的资源大小
        /// </summary>
        public float AlreadyDecompressSize { get; protected set; }

        /// <summary>
        /// 是否开始解压
        /// </summary>
        public bool IsStartDecompress { get; protected set; }

        /// <summary>
        /// 开始解压内嵌文件
        /// </summary>
        /// <returns>解压的文件</returns>
        public abstract IDecompressAssets StartDecompressBuiltinFile(BundleModuleEnum bundleModuleEnum, Action callback);


        /// <summary>
        /// 获取解压进度
        /// </summary>
        /// <returns>解压进度</returns>
        public abstract float GetDecompressProgress();

    }
}