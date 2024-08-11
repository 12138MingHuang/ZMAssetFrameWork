using System.Collections.Generic;
using UnityEngine;

namespace ZMAssetFrameWork
{
    public class ClassObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 存放类的一个对象池，偏底层的东西 尽量别用List
        /// </summary>
        protected Stack<T> pool = new Stack<T>();
        
        /// <summary>
        /// 最大缓存对象个数，小于等于0表示不限制个数
        /// </summary>
        protected int maxCount = 0;
        
        /// <summary>
        /// 类对象池构造函数
        /// </summary>
        /// <param name="maxCount">最大缓存个数</param>
        public ClassObjectPool(int maxCount)
        {
            this.maxCount = maxCount;
            for (int i = 0; i < maxCount; i++)
            {
                pool.Push(new T());
            }
        }
        
        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns>类对象</returns>
        public T Spawn()
        {
            if (pool.Count > 0)
                return pool.Pop();
            else
                return new T();
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj">类对象</param>
        public void Recycle(T obj)
        {
            if(obj == null)
            {
                Debug.LogError("Recycle Obj failed, obj is null");
                return;
            }
            pool.Push(obj);
        }
    }
}
