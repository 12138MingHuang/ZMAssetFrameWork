/*---------------------------------------------------------------------------------------------------------------------------------------------
*
* Title: AssetBundle 多模块打包工具
*
* Description: 类对象池/资源对象池/替换图片的封装类，所有路径都基于Assets开始
*
* Author: 铸梦xy
*
* Date: 2019.8.29
*
* Modify: 
------------------------------------------------------------------------------------------------------------------------------------------------*/
namespace ZM.AssetFrameWork
{

	public class Singleton<T> where T : new()
	{
		private static T m_Instance;
		public static T Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new T();
				}

				return m_Instance;
			}
		}

	}
}