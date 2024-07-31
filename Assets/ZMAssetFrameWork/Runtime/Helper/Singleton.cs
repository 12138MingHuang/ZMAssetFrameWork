namespace ZMAssetFrameWork
{

    /// <summary>
    /// 泛型单例类，用于创建指定类型的单例对象。
    /// </summary>
    /// <typeparam name="T">需要创建单例的类型，该类型必须拥有一个无参构造函数。</typeparam>
    public class Singleton<T> where T : new()
    {
        /// <summary>
        /// 泛型单例类的私有静态成员变量，用于存储单例对象。
        /// </summary>
        private static T _instance;

        /// <summary>
        /// 泛型单例类的公共静态属性，用于获取单例对象。
        /// </summary>
        public static T Instance
        {
            get
            {
                // 如果单例对象尚未创建，则创建之
                if (_instance == null)
                {
                    _instance = new T();
                }

                // 返回单例对象
                return _instance;
            }
        }
    }
}
