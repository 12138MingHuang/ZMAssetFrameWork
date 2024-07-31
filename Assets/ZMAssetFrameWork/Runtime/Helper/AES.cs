using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ZMAssetFrameWork
{

    /// <summary>
    /// AES加密解密类
    /// </summary>
    public class AES
    {
        private static string _AESHead = "AESEncrypt";

        /// <summary>
        /// 对指定文件进行AES加密
        /// </summary>
        /// <param name="path">待加密文件的路径</param>
        /// <param name="EncrptyKey">加密密钥</param>
        /// <remarks>
        /// 此方法使用AES算法对指定文件进行加密，加密后的文件将覆盖原文件。
        /// </remarks>
        public static void AESFileEncrypt(string path, string EncrptyKey)
        {
            if (!File.Exists(path))
                return;

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (fs != null)
                    {
                        //读取字节头，判断是否已经加密过了
                        byte[] headBuff = new byte[10];
                        fs.Read(headBuff, 0, headBuff.Length);
                        string headTag = Encoding.UTF8.GetString(headBuff);
                        if (headTag == _AESHead)
                        {
                            #if UNITY_EDITOR
                            Debug.Log(path + "已经加密过了！");
                            #endif
                            return;
                        }
                        //Debug.Log("加密文件:"+path);
                        //加密并且写入字节头
                        fs.Seek(0, SeekOrigin.Begin);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        byte[] headBuffer = Encoding.UTF8.GetBytes(_AESHead);
                        fs.Write(headBuffer, 0, headBuffer.Length);
                        byte[] EncBuffer = AESEncrypt(buffer, EncrptyKey);
                        fs.Write(EncBuffer, 0, EncBuffer.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 使用AES算法解密文件。
        /// </summary>
        /// <param name="path">要解密的文件路径。</param>
        /// <param name="EncrptyKey">用于解密的密钥。</param>
        /// <remarks>
        /// 该方法使用指定的密钥对指定路径的文件进行AES解密操作。
        /// </remarks>
        public static void AESFileDecrypt(string path, string EncrptyKey)
        {
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (fs != null)
                    {
                        byte[] headBuff = new byte[10];
                        fs.Read(headBuff, 0, headBuff.Length);
                        string headTag = Encoding.UTF8.GetString(headBuff);
                        if (headTag == _AESHead)
                        {
                            byte[] buffer = new byte[fs.Length - headBuff.Length];
                            fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                            fs.Seek(0, SeekOrigin.Begin);
                            fs.SetLength(0);
                            byte[] DecBuffer = AESDecrypt(buffer, EncrptyKey);
                            fs.Write(DecBuffer, 0, DecBuffer.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 使用AES算法对文件进行解密，返回解密后的字节数组。
        /// </summary>
        /// <param name="path">要解密文件的路径。</param>
        /// <param name="EncrptyKey">解密密钥。</param>
        /// <returns>解密后的字节数组。</returns>
        public static byte[] AESFileByteDecrypt(string path, string EncrptyKey)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            byte[] DecBuffer = null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs != null)
                    {
                        byte[] headBuff = new byte[10];
                        fs.Read(headBuff, 0, headBuff.Length);
                        string headTag = Encoding.UTF8.GetString(headBuff);
                        if (headTag == _AESHead)
                        {
                            byte[] buffer = new byte[fs.Length - headBuff.Length];
                            fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                            DecBuffer = AESDecrypt(buffer, EncrptyKey);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return DecBuffer;
        }

        /// <summary>
        /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="EncryptString">待加密密文</param>
        /// <param name="EncryptKey">加密密钥</param>
        public static string AESEncrypt(string EncryptString, string EncryptKey)
        {
            return Convert.ToBase64String(AESEncrypt(Encoding.Default.GetBytes(EncryptString), EncryptKey));
        }

        /// <summary>
        /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="EncryptString">待加密密文</param>
        /// <param name="EncryptKey">加密密钥</param>
        public static byte[] AESEncrypt(byte[] EncryptByte, string EncryptKey)
        {
            if (EncryptByte.Length == 0)
            {
                throw (new Exception("明文不得为空"));
            }
            if (string.IsNullOrEmpty(EncryptKey))
            {
                throw (new Exception("密钥不得为空"));
            }
            byte[] m_strEncrypt;
            byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            Rijndael m_AESProvider = Rijndael.Create();
            try
            {
                MemoryStream m_stream = new MemoryStream();
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptKey, m_salt);
                ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
                CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strEncrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                m_AESProvider.Clear();
            }
            return m_strEncrypt;
        }


        /// <summary>
        /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="DecryptString">待解密密文</param>
        /// <param name="DecryptKey">解密密钥</param>
        public static string AESDecrypt(string DecryptString, string DecryptKey)
        {
            return Convert.ToBase64String(AESDecrypt(Encoding.UTF8.GetBytes(DecryptString), DecryptKey));
        }

        /// <summary>
        /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="DecryptString">待解密密文</param>
        /// <param name="DecryptKey">解密密钥</param>
        public static byte[] AESDecrypt(byte[] DecryptByte, string DecryptKey)
        {
            if (DecryptByte.Length == 0)
            {
                throw (new Exception("密文不得为空"));
            }
            if (string.IsNullOrEmpty(DecryptKey))
            {
                throw (new Exception("密钥不得为空"));
            }
            byte[] m_strDecrypt;
            byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            Rijndael m_AESProvider = Rijndael.Create();
            try
            {
                MemoryStream m_stream = new MemoryStream();
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(DecryptKey, m_salt);
                ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
                CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strDecrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                m_AESProvider.Clear();
            }
            return m_strDecrypt;
        }


        /// <summary>
        /// 默认向量
        /// </summary>
        private const string Iv = "abcdefghijklmnop";
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="str">需要加密的字符串</param>
        /// <param name="key">32位密钥</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string str, string key)
        {
            try
            {
                Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
                Byte[] toEncryptArray = System.Text.Encoding.UTF8.GetBytes(str);
                var rijndael = new System.Security.Cryptography.RijndaelManaged();
                rijndael.Key = keyArray;
                rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
                rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
                System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateEncryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return "";
            }
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="str">需要解密的字符串</param>
        /// <param name="key">32位密钥</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string str, string key)
        {
            try
            {
                Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
                Byte[] toEncryptArray = Convert.FromBase64String(str);
                var rijndael = new System.Security.Cryptography.RijndaelManaged();
                rijndael.Key = keyArray;
                rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
                rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
                System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateDecryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return System.Text.Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return "";
            }

        }
    }
}
