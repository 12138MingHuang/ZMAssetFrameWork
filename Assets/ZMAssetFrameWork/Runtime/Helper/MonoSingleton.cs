using UnityEngine;

namespace ZMAssetFrameWork
{
    /// <summary>
    /// MonoSingleton 抽象类，用于实现单例模式的 MonoBehaviour 组件。
    /// </summary>
    /// <typeparam name="T">继承自 MonoSingleton 的类型。</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        /// <summary>
        /// 单例实例。
        /// </summary>
        private static T _instance = null;

        /// <summary>
        /// 获取单例实例。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                        _instance.OnAwake();
                    }
                }
                return _instance;
            }
        }
        
        protected virtual void OnAwake() { }

        /// <summary>
        /// 销毁单例对象。
        /// </summary>
        public virtual void Dispose()
        {
            Destroy(_instance.gameObject);
        }
    }
}
