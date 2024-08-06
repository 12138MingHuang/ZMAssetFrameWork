using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// 资源下载线程
    /// </summary>
    public class DownLoadThread
    {
        /// <summary>
        /// 下载完成回调
        /// </summary>
        private Action<DownLoadThread, HotFileInfo> OnDownLoadSuccess;
        
        /// <summary>
        /// 下载失败回调
        /// </summary>
        private Action<DownLoadThread, HotFileInfo> OnDownLoadFailed;
        
        /// <summary>
        /// 当前热更的资源模块
        /// </summary>
        private HotAssetsModule _curHotAssetsModule;

        /// <summary>
        /// 当前热更的文件信息
        /// </summary>
        private HotFileInfo _hotFileInfo;

        /// <summary>
        /// 文件下载的地址
        /// </summary>
        private string _downLoadURL;

        /// <summary>
        /// 下载的文件保存路径
        /// </summary>
        private string _fileSavePath;

        /// <summary>
        /// 下载的文件大小
        /// </summary>
        private float _downLoadSizeKb;

        /// <summary>
        /// 当前下载次数
        /// </summary>
        private int _curDownLoadCount = 0;

        /// <summary>
        /// 最大尝试下载次数
        /// </summary>
        private const int MAX_TRY_DOWNLOAD_COUNT = 3;

        /// <summary>
        /// 资源下载线程构造函数
        /// </summary>
        /// <param name="assetsModule">资源模块</param>
        /// <param name="hotFileInfo">热更文件信息</param>
        /// <param name="downLoadURL">下载地址</param>
        /// <param name="fileSavePath">下载文件存储路径</param>
        public DownLoadThread(HotAssetsModule assetsModule, HotFileInfo hotFileInfo, string downLoadURL, string fileSavePath)
        {
            _curHotAssetsModule = assetsModule;
            _hotFileInfo = hotFileInfo;
            _downLoadURL = downLoadURL + "/" + hotFileInfo.abName;
            _fileSavePath = fileSavePath + "/" + hotFileInfo.abName;
        }

        /// <summary>
        /// 开始下载资源
        /// </summary>
        /// <param name="downLoadSuccess">下载成功回调</param>
        /// <param name="downLoadFailed">下载失败回调</param>
        public void StartDownLoad(Action<DownLoadThread, HotFileInfo> downLoadSuccess, Action<DownLoadThread, HotFileInfo> downLoadFailed)
        {
            OnDownLoadSuccess = downLoadSuccess;
            OnDownLoadFailed = downLoadFailed;

            //这里的代码在子线程中执行
            Task.Run(() =>
            {
                try
                {
                    Debug.Log("StartDownLoad ModuleEnum:" + _curHotAssetsModule.CurBundleModuleEnum + " AssetBundle URL:" + _downLoadURL);
                    
                    HttpWebRequest request = WebRequest.Create(_downLoadURL) as HttpWebRequest;
                    request.Method = "GET";
                    //发起请求
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    
                    //创建本地文件流
                    FileStream fileStream = File.Create(_fileSavePath);
                    using (Stream stream = response.GetResponseStream())
                    {
                        //文件下载异常
                        if (stream == null || stream.Length == 0)
                        {
                            Debug.LogError("File DownLoad exception please check file fileName:" + _hotFileInfo.abName + "fileURL:" + _downLoadURL);
                        }
                        byte[] buffer = new byte[512];
                        //从字节流中读取字节，读取到buff数组中
                        int size = stream.Read(buffer, 0, buffer.Length);

                        while (size > 0)
                        {
                            fileStream.Write(buffer, 0, buffer.Length);
                            size = stream.Read(buffer, 0, buffer.Length);
                            //1mb=1024kb 1kb=1024字节
                            _downLoadSizeKb += size;
                            //计算以m为单位的大小
                            _curHotAssetsModule.assetsDownLoadSizeM += (size / 1024f / 1024f);
                        }
                        fileStream.Dispose();
                        fileStream.Close();
                        Debug.Log("OnDownLoadSuccess ModuleEnum:" + _curHotAssetsModule.CurBundleModuleEnum + " AssetBundle URL:" + _downLoadURL + " FileSavePath:" + _fileSavePath);
                        OnDownLoadSuccess?.Invoke(this, _hotFileInfo);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("DownLoad AssetBundle Error URL:" + _downLoadURL + " Exception:" + e);
                    if (_curDownLoadCount > MAX_TRY_DOWNLOAD_COUNT)
                    {
                        OnDownLoadFailed?.Invoke(this, _hotFileInfo);
                    }
                    else
                    {
                        Debug.LogError("文件下载失败，正在进行重新下载， 下载次数：" + _curDownLoadCount);
                        StartDownLoad(OnDownLoadSuccess, OnDownLoadFailed);
                        _curDownLoadCount++;
                    }
                    throw;
                }
            });
        }
    }
}

