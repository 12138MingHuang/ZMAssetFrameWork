using UnityEngine;

namespace ZMAssetFrameWork
{
    public class HotUpdateManager : Singleton<HotUpdateManager>
    {
        /// <summary>
        /// 热更且解压游戏内嵌资源
        /// </summary>
        /// <param name="bundleModuleEnum">热更资源类型</param>
        public void HotAndUnPackAssets(BundleModuleEnum bundleModuleEnum)
        {
            //开始解压游戏内嵌资源
            ZMAssetsFrame.StartDecompressBuiltinFile(bundleModuleEnum, () =>
            {
                //说明资源开启解压了
                if(Application.internetReachability == NetworkReachability.NotReachable)
                {
                    InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow").InitView("当前没有网络，请检测网络重试？", () =>
                    {
                        NotNetButtonClick(bundleModuleEnum);
                    }, () =>
                    {
                        NotNetButtonClick(bundleModuleEnum);
                    });
                    return;
                }
                else
                {
                    CheckAssetsVersion(bundleModuleEnum);
                }
            });
        }
        
        /// <summary>
        /// 没有网络情况更新
        /// </summary>
        /// <param name="bundleModuleEnum">热更资源类型</param>
        public void NotNetButtonClick(BundleModuleEnum bundleModuleEnum)
        {
            //如果没有网络，弹出弹窗提示，提示用户没有网络
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                CheckAssetsVersion(bundleModuleEnum);
            }
            else
            {
                
            }
        }
        
        /// <summary>
        /// 开始热更检测
        /// </summary>
        /// <param name="bundleModuleEnum">热更资源类型</param>
        public void CheckAssetsVersion(BundleModuleEnum bundleModuleEnum)
        {
            ZMAssetsFrame.CheckAssetsVersion(bundleModuleEnum, (isHot, sizeM) =>
            {
                if (isHot)
                {
                    //当用户使用的是流量的时候，需要询问用户是否需要更新资源
                    if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork
                        || Application.platform == RuntimePlatform.WindowsEditor
                        || Application.platform == RuntimePlatform.OSXEditor)
                    {
                        //弹出选择弹窗，让用户决定是否需要更新
                        InstantiateResourcesObj<UpdateTipsWindow>("UpdateTipsWindow").InitView("当前有" + sizeM.ToString("F2") + "M的资源更新，是否需要更新？", () =>
                        {
                            //确认更新回调
                            StartHotAssets(bundleModuleEnum);
                        }, () =>
                        {
                            //退出游戏回调
                            Application.Quit();
                        });
                    }
                    else
                    {
                        StartHotAssets(bundleModuleEnum);
                    }
                }
                else
                {
                    //如果不需要热更，说明用户已经热更过了，资源是最新的，直接进入游戏
                    OnHotFinishCallBack(bundleModuleEnum);
                }
            });
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="prefabName">预制体名字</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>生成的物体</returns>
        public T InstantiateResourcesObj<T>(string prefabName)
        {
            return GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(prefabName)).GetComponent<T>();
        }

        /// <summary>
        /// 开始热更
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        public void StartHotAssets(BundleModuleEnum bundleModuleEnum)
        {
            ZMAssetsFrame.HotAssets(bundleModuleEnum, OnStartHotAssetsCallBack, OnHotFinishCallBack, null, false);
        }

        /// <summary>
        /// 开始热更回调
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        public void OnStartHotAssetsCallBack(BundleModuleEnum bundleModuleEnum)
        {
            
        }

        /// <summary>
        /// 热更完成回调
        /// </summary>
        /// <param name="bundleModuleEnum">热更模块</param>
        public void OnHotFinishCallBack(BundleModuleEnum bundleModuleEnum)
        {
            Debug.Log("OnHotFinishCallBack.....");
        }
    }
}
