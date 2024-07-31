using System.IO;
using System.Text;

namespace ZMAssetFrameWork
{
    public class MD5
    {
        /// <summary>
        /// 从指定文件路径获取文件的MD5值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件的MD5值</returns>
        public static string GetMd5FromFile(string path)
        {
            using (System.Security.Cryptography.MD5 md5File = System.Security.Cryptography.MD5.Create())
            {
                using (FileStream fileRead = File.OpenRead(path))
                {
                    byte[] md5Buffer = md5File.ComputeHash(fileRead);
                    md5File.Clear();
                    StringBuilder sbMd5 = new StringBuilder();
                    for (int i = 0; i < md5Buffer.Length; i++)
                    {
                        sbMd5.Append(md5Buffer[i].ToString("X2"));
                    }
                    return sbMd5.ToString();
                }
            }

        }

        /// <summary>
        /// 从字符串中获取MD5哈希值
        /// </summary>
        /// <param name="msg">需要计算哈希值的字符串</param>
        /// <returns>返回该字符串的MD5哈希值</returns>
        public static string GetMd5FromString(string msg)
        {
            //1.创建一个用来计算MD5值的类的对象
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                //把字符串转换成byte[]
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
                //2.计算给定字符串的MD5值，如何把一个长度为16的byte[]数组转换为一个长度为32的字符串：就是把每个byte转成16进制同时保留2位即可
                byte[] md5Buffer = md5.ComputeHash(msgBuffer);
                md5.Clear();
                StringBuilder sbMd5 = new StringBuilder();
                for (int i = 0; i < md5Buffer.Length; i++)
                {
                    //字母小写：x ;字母大写：X
                    sbMd5.Append(md5Buffer[i].ToString("X2"));
                }

                return sbMd5.ToString();
            }
        }
    }
}
