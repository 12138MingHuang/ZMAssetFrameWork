using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 热更资源清单
    /// </summary>
    public class HotAssetsMainfest
    {
        /// <summary>
        /// 热更公告
        /// </summary>
        public string updateNotice;

        /// <summary>
        /// 下载地址
        /// </summary>
        public string downLoadURL;
        
        /// <summary>
        /// 热更资源补丁列表
        /// </summary>
        public List<HotAssetsPatch> hotAssetsPatchList = new List<HotAssetsPatch>();
    }

    public class HotAssetsPatch
    {
        /// <summary>
        /// 补丁版本
        /// </summary>
        public string patchVesion;
        
        /// <summary>
        /// 热更资源信息列表
        /// </summary>
        public List<HotFileInfo> hotAssetsList = new List<HotFileInfo>();
    }

    public class HotFileInfo
    {
        /// <summary>
        /// ab包名
        /// </summary>
        public string abName;

        /// <summary>
        /// 文件的MD5
        /// </summary>
        public string md5;

        /// <summary>
        /// 文件的大小
        /// </summary>
        public float size;
    }
}
