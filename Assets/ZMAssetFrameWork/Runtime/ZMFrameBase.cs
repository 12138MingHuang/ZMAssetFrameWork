using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public class ZMFrameBase : MonoBehaviour
    {
        protected static ZMAssetsFrame instance;
    
        public static ZMAssetsFrame Instance
        {
            get
            {
                if(instance == null )
                {
                    instance = FindObjectOfType<ZMAssetsFrame>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(ZMAssetsFrame).Name);
                        //禁止销毁这个物体
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<ZMAssetsFrame>();
                        instance.OnInitlizate();
                    }
                }
                return instance;
            }
        }

        protected virtual void OnInitlizate()
        {
        
        }
    }
}

