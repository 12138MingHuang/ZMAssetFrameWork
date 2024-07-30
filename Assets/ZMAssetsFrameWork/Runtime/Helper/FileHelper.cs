using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHelper
{
    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="folderPath"></param>
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
}
