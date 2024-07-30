using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHelper
{
    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="folderPath">文件夹</param>
    public static void DeleteFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "*");
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            Directory.Delete(folderPath);
        }
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">写入数据</param>
    public static void WriteFile(string filePath, byte[] data)
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream fs = File.Create(filePath);
        fs.Write(data, 0, data.Length);
        fs.Dispose();
        fs.Close();
    }
}
