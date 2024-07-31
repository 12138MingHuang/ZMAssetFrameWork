using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZMAssetFrameWork;

namespace ZMAssetFrameWork
{
    public enum BuildType
    {
        /// <summary>
        /// AssetsBundle资源包
        /// </summary>
        AssetsBundle,
    
        /// <summary>
        /// 热更补丁
        /// </summary>
        HotPatch,
    }

    public class BuildBundleCompiler
    {
        /// <summary>
        /// 更新公告
        /// </summary>
        private static string _updateNotice;
        
        /// <summary>
        /// 热更补丁版本
        /// </summary>
        private static string _hotPatchVersion;
        
        /// <summary>
        /// 打包类型
        /// </summary>
        private static BuildType _buildType;
        
        /// <summary>
        /// 资源模块配置数据
        /// </summary>
        private static BundleModuleData _bundleModuleData;
        
        /// <summary>
        /// 打包模块类型
        /// </summary>
        private static BundleModuleEnum _bundleModuleEnum;
        
        /// <summary>
        /// 所有AssetBundle文件路径列表
        /// </summary>
        private static List<string> _allBundlePathList = new List<string>();
        
        /// <summary>
        /// 所有的文件夹的Bundle字典
        /// </summary>
        private static Dictionary<string, List<string>> _allFolderBundleDic = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// 所有的预制体的Bundle字典
        /// </summary>
        private static Dictionary<string, List<string>> _allPrefabsBundleDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// AssetBundle文件输出路径
        /// </summary>
        private static string BundleOutPutPath
        {
            get
            {
                return Application.dataPath + "/../AssetBundle/" + _bundleModuleEnum.ToString() + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            }
        }
        
        /// <summary>
        /// 热更资源文件输出路径
        /// </summary>
        private static string HotAssetsOutPutPath
        {
            get
            {
                return Application.dataPath + "/../HotAssets/" + _bundleModuleEnum.ToString() + "/" + _hotPatchVersion + "/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            }
        }

        /// <summary>
        /// 资源文件路径
        /// </summary>
        private static string ResourcesPath
        {
            get
            {
                return Application.dataPath + "/ZMAssetFrameWork/Resources/";
            }
        }
        
        /// <summary>
        /// 打包AssetBundle资源包
        /// </summary>
        /// <param name="bundleModuleData">资源模块配置数据</param>
        /// <param name="buildType">打包类型</param>
        /// <param name="hotPatchVersion">热更补丁版本</param>
        /// <param name="updateNotice">更新公告</param>
        public static void BuildAssetBundle(BundleModuleData bundleModuleData, BuildType buildType, string hotPatchVersion = "0", string updateNotice = "")
        {
            //初始化打包数据
            Initlization(bundleModuleData, buildType, hotPatchVersion, updateNotice);
            //打包所有的文件夹
            BuildAllFolder();
            //打包父文件夹下的所有子文件夹
            BuildRootSubFolder();
            //打包指定文件夹下的所有预制体
            BuildAllPrefabs();
            //开始调用UnityAPI进行打包AssetBundle资源包
            BuildAllAssetBundle();
        }
        
        /// <summary>
        /// 初始化AssetBundle资源包
        /// </summary>
        /// <param name="bundleModuleData">资源模块配置数据</param>
        /// <param name="buildType">打包类型</param>
        /// <param name="hotPatchVersion">热更补丁版本</param>
        /// <param name="updateNotice">更新公告</param>
        private static void Initlization(BundleModuleData bundleModuleData, BuildType buildType, string hotPatchVersion, string updateNotice)
        {
            //清理数据以防下次打包时有数据残留
            _allBundlePathList.Clear();
            _allFolderBundleDic.Clear();
            _allPrefabsBundleDic.Clear();
            
            _buildType = buildType;
            _hotPatchVersion = hotPatchVersion;
            _updateNotice = updateNotice;
            _bundleModuleData = bundleModuleData;
            _bundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), bundleModuleData.moduleName);
            
            FileHelper.DeleteFolder(BundleOutPutPath);
            Directory.CreateDirectory(BundleOutPutPath);
        }

        /// <summary>
        /// 打包所有文件夹AssetsBundle资源包
        /// </summary>
        private static void BuildAllFolder()
        {
            if(_bundleModuleData.signFolderPathArr == null || _bundleModuleData.signFolderPathArr.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _bundleModuleData.signFolderPathArr.Length; i++)
            {
                //获取文件夹路径
                string folderPath = _bundleModuleData.signFolderPathArr[i].bundlePath.Replace(@"\", "/");
                if (!IsRepeatBundleFile(folderPath))
                {
                    _allBundlePathList.Add(folderPath);
                    //获取以模块名+_+ABName的格式的AssetBundle包名
                    string bundleName = GenerateBundleName(_bundleModuleData.signFolderPathArr[i].abName);
                    if (!_allFolderBundleDic.ContainsKey(folderPath))
                    {
                        _allFolderBundleDic.Add(bundleName, new List<string>() { folderPath });
                    }
                    else
                    {
                        _allFolderBundleDic[bundleName].Add(folderPath);
                    }
                }
                else
                {
                    Debug.LogError("重复的Bundle文件：" + folderPath);
                }
            }
        }

        /// <summary>
        /// 打包父文件夹下的所有子文件夹
        /// </summary>
        private static void BuildRootSubFolder()
        {
            //检测父文件夹是否有配置，如果没有就直接跳过
            if (_bundleModuleData.rootFolderPathArr == null || _bundleModuleData.rootFolderPathArr.Length == 0)
            {
                return;
            }

            for (int i = 0; i < _bundleModuleData.rootFolderPathArr.Length; i++)
            {
                string path = _bundleModuleData.rootFolderPathArr[i] + "/";
                //获取父文件夹的子文件夹
                string[] subFolderArr = Directory.GetDirectories(path);
                foreach (string item in subFolderArr)
                {
                    path = item.Replace(@"\", "/");
                    int nameIndex = path.LastIndexOf("/") + 1;
                    //获取文件夹同名的AssetBundle名称
                    string bundleName = GenerateBundleName(path.Substring(nameIndex, path.Length - nameIndex));
                    if (!IsRepeatBundleFile(path))
                    {
                        _allBundlePathList.Add(path);
                        if(!_allFolderBundleDic.ContainsKey(bundleName))
                        {
                            _allFolderBundleDic.Add(bundleName, new List<string>() { path });
                        }
                        else
                        {
                            _allFolderBundleDic[bundleName].Add(path);
                        }
                    }
                    else
                    {
                        Debug.LogError("重复的Bundle文件：" + path);
                    }
                    
                    //处理子文件夹资源
                    string[] subFilePahArr = Directory.GetFiles(path, "*");
                    foreach (string subFilePath in subFilePahArr)
                    {
                        //过滤.meta文件
                        if (!subFilePath.EndsWith(".meta"))
                        {
                            string abFilePath = subFilePath.Replace(@"\", "/");
                            if (!IsRepeatBundleFile(abFilePath))
                            {
                                _allBundlePathList.Add(abFilePath);
                                if (!_allFolderBundleDic.ContainsKey(bundleName))
                                {
                                    _allFolderBundleDic.Add(bundleName, new List<string>() { abFilePath });
                                }
                                else
                                {
                                    _allFolderBundleDic[bundleName].Add(abFilePath);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 打包指定文件夹下的所有预制体
        /// </summary>
        private static void BuildAllPrefabs()
        {
            if(_bundleModuleData.prefabPathArr == null || _bundleModuleData.prefabPathArr.Length == 0)
            {
                return;
            }
            
            //获取所有预制体的GUID
            string[] guidArr = AssetDatabase.FindAssets("t:Prefab", _bundleModuleData.prefabPathArr);

            for (int i = 0; i < guidArr.Length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guidArr[i]);
                //计算AssetBundle名称
                string bundleName = GenerateBundleName(Path.GetFileNameWithoutExtension(filePath));
                //如果该AssetBundle不存在，就计算打包数据
                if (!_allBundlePathList.Contains(filePath))
                {
                    //获取预制体所有依赖项
                    string[] dependsArr = AssetDatabase.GetDependencies(filePath);
                    List<string> dependsList = new List<string>();
                    for (int j = 0; j < dependsArr.Length; j++)
                    {
                        string dependPath = dependsArr[j];
                        //如果不是冗余文件，就归纳进行打包
                        if (!IsRepeatBundleFile(dependPath))
                        {
                            _allBundlePathList.Add(dependPath);
                            dependsList.Add(dependPath);
                        }
                    }
                    if (!_allPrefabsBundleDic.ContainsKey(bundleName))
                    {
                        _allPrefabsBundleDic.Add(bundleName, dependsList);
                    }
                    else
                    {
                        Debug.LogError("重复的预制体名字，当前模块下有预制体文件重复：" + bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 打包AssetBundle资源包
        /// </summary>
        private static void BuildAllAssetBundle()
        {
            //修改所有要打包的文件的AssetBundleName
            ModifyAllFileBundleName();
            //生成一份AssetBundle打包配置
            WriteAssetBundleConfig();
            //调用UnityAPI打包AssetBundle
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                BundleOutPutPath, 
                (UnityEditor.BuildAssetBundleOptions)Enum.Parse(typeof(UnityEditor.BuildAssetBundleOptions), BundleSettings.Instance.buildAssetBundleOptions.ToString()), 
                (UnityEditor.BuildTarget)Enum.Parse(typeof(UnityEditor.BuildTarget), BundleSettings.Instance.buildTarget.ToString())
                );
            if(manifest == null)
            {
                EditorUtility.DisplayProgressBar("打包AssetBundle", "打包AssetBundle失败", 1);
                Debug.LogError("打包AssetBundle失败");
            }
            else
            {
                Debug.Log("打包AssetBundle成功");
                DeleteAllBundleMainfestFile();
                EncrypAllBundle();
                if (_buildType == BuildType.HotPatch)
                {
                    GeneratorHotAssets();
                }
            }
            ModifyAllFileBundleName(true);
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 修改或清空AssetBundle
        /// </summary>
        private static void ModifyAllFileBundleName(bool isClear = false)
        {
            int i = 0;
            
            //修改所有文件夹下的AssetBundle name
            foreach (var item in _allFolderBundleDic)
            {
                i++;
                EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / _allFolderBundleDic.Count);
                foreach (string path in item.Value)
                {
                    AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                    if (assetImporter != null)
                    {
                        //这里的文件读取失败是AssetBundle后缀引起的，高版本Unity自动会自动读取,unity文件,导致文件大小异常，解决方案:建议不要AssetBundle后缀,或更换后缀
                        // assetImporter.assetBundleName = isClear ? "" : item.Key + ".unity";
                        assetImporter.assetBundleName = isClear ? "" : item.Key + ".ab";
                    }
                }
            }
            
            //修改所有预制体下的AssetBundle name
            i = 0;
            foreach (var item in _allPrefabsBundleDic)
            {
                i++;
                List<string> dependsList = item.Value;
                foreach (string path in dependsList)
                {
                    EditorUtility.DisplayProgressBar("Modify AssetBundle Name", "Name:" + item.Key, i * 1.0f / _allPrefabsBundleDic.Count);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                    if (assetImporter != null)
                    {
                        //这里的文件读取失败是AssetBundle后缀引起的，高版本Unity自动会自动读取,unity文件,导致文件大小异常，解决方案:建议不要AssetBundle后缀,或更换后缀
                        // assetImporter.assetBundleName = isClear ? "" : item.Key + ".unity";
                        assetImporter.assetBundleName = isClear ? "" : item.Key + ".ab";
                    }
                }
            }
            
            //移除未使用的AssetBundleName
            if (isClear)
            {
                string bundleConfigPath = Application.dataPath + "/" + _bundleModuleEnum.ToString().ToLower() + "AssetBundleConfig.json";
                AssetImporter assetImporter = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = "";
                }
                
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }
        }

        /// <summary>
        /// 生成AssetBundle配置文件
        /// </summary>
        private static void WriteAssetBundleConfig()
        {
            BundleConfig bundleConfig = new BundleConfig();
            bundleConfig.bundleInfoList = new List<BundleInfo>();
            //所有AssetBundle文件字典 key:路径 value:AssetBundleName
            Dictionary<string, string> allBundleFilePathDic = new Dictionary<string, string>();
            //获取到工程内所有的AssetBundleName
            string[] allBundleNameArr = AssetDatabase.GetAllAssetBundleNames();
            
            //遍历所有AssetBundleName
            foreach (string bundleName in allBundleNameArr)
            {
                //获取指定的AssetBundleName下的所有文件路径
                string[] bundleFileArr = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                
                //遍历所有文件路径
                foreach (string filePath in bundleFileArr)
                {
                    if(!filePath.EndsWith(".cs"))
                    {
                        allBundleFilePathDic.Add(filePath, bundleName);
                    }
                }
            }
            
            //计算AssetBundle数据，生成AssetBundle配置文件
            foreach (var item in allBundleFilePathDic)
            {
                //获取文件路径
                string filePath = item.Key;
                if (!filePath.EndsWith(".cs"))
                {
                    BundleInfo bundleInfo = new BundleInfo();
                    bundleInfo.path = filePath;
                    bundleInfo.bundleName = item.Value;
                    bundleInfo.assetName = Path.GetFileName(filePath);
                    bundleInfo.crc = Crc32.GetCrc32(filePath);
                    bundleInfo.bundleDependce = new List<string>();
                    
                    string[] dependceArr = AssetDatabase.GetDependencies(filePath);
                    foreach (string dependcePath in dependceArr)
                    {
                        //如果依赖项不是当前的这个文件，以及依赖项不是cs脚本，就进行处理
                        if (!dependcePath.Equals(filePath) && !dependcePath.EndsWith(".cs"))
                        {
                            string dependceBundleName = "";
                            if (allBundleFilePathDic.TryGetValue(dependcePath, out dependceBundleName))
                            {
                                //如果依赖项已经包含在这个AssetBundle中，则不处理，否则添加到依赖项中
                                if (!bundleInfo.bundleDependce.Contains(dependceBundleName))
                                {
                                    bundleInfo.bundleDependce.Add(dependceBundleName);
                                }
                            }
                        }
                    }
                    bundleConfig.bundleInfoList.Add(bundleInfo);
                }
            }
            
            //生成AssetBundle配置文件
            string json = JsonConvert.SerializeObject(bundleConfig, Formatting.Indented);
            string bundleConfigPath = Application.dataPath + "/" + _bundleModuleEnum.ToString().ToLower() + "AssetBundleConfig.json";
            StreamWriter writer = File.CreateText(bundleConfigPath);
            writer.Write(json);
            writer.Dispose();
            writer.Close();
            //修改AssetBundle配置文件的AssetBundleName
            AssetImporter assetImporter = AssetImporter.GetAtPath(bundleConfigPath.Replace(Application.dataPath, "Assets"));
            if (assetImporter != null)
            {
                //这里的文件读取失败是AssetBundle后缀引起的，高版本Unity自动会自动读取,unity文件,导致文件大小异常，解决方案:建议不要AssetBundle后缀,或更换后缀
                // assetImporter.assetBundleName = _bundleModuleEnum.ToString().ToLower() + "bundleconfig.unity";
                assetImporter.assetBundleName = _bundleModuleEnum.ToString().ToLower() + "bundleconfig.ab";
            }
            
        }
        
        /// <summary>
        /// 是否重复的Bundle文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否重复</returns>
        private static bool IsRepeatBundleFile(string path)
        {
            foreach (string item in _allBundlePathList)
            {
                if (string.Equals(item, path) || item.Contains(path) || path.EndsWith(".cs"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 生成Bundle文件名
        /// </summary>
        /// <param name="abName">ab包名称</param>
        /// <returns>返回完整名称</returns>
        private static string GenerateBundleName(string abName)
        {
            return _bundleModuleEnum.ToString() + "_" + abName;
        }

        /// <summary>
        /// 删除所有AssetBundle自动生成的Mainfest文件
        /// </summary>
        private static void DeleteAllBundleMainfestFile()
        {
            string[] filePathArr = Directory.GetFiles(BundleOutPutPath);
            foreach (string filePath in filePathArr)
            {
                if (filePath.EndsWith(".manifest"))
                {
                    File.Delete(filePath);
                }
            }
        }

        /// <summary>
        /// 加密所有AssetBundle文件
        /// </summary>
        private static void EncrypAllBundle()
        {
            if(!BundleSettings.Instance.bundleEncrypt.isEncrypt)
            {
                return;
            }
            
            DirectoryInfo directoryInfo = new DirectoryInfo(BundleOutPutPath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                EditorUtility.DisplayProgressBar("加密AssetBundle", "正在加密AssetBundle文件：" + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                AES.AESFileEncrypt(fileInfoArr[i].FullName, BundleSettings.Instance.bundleEncrypt.encryptKey);
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("加密AssetBundle完成");
        }

        /// <summary>
        /// 拷贝Bundle文件到StreamingAssets文件夹
        /// </summary>
        /// <param name="bundleModuleData">bundle模块数据</param>
        /// <param name="showTips">是否提示</param>
        public static void CopyBundleToStreamingAssets(BundleModuleData bundleModuleData, bool showTips = true)
        {
            _bundleModuleEnum = (BundleModuleEnum)Enum.Parse(typeof(BundleModuleEnum), bundleModuleData.moduleName);
            //获取目标文件夹下的所有AssetBundle文件
            DirectoryInfo directoryInfo = new DirectoryInfo(BundleOutPutPath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            //Bundle内嵌的目标文件夹
            string streamingAssetsPath = Application.streamingAssetsPath + "/AssetBundle/" + _bundleModuleEnum + "/";
            
            FileHelper.DeleteFolder(streamingAssetsPath);
            Directory.CreateDirectory(streamingAssetsPath);
            
            List<BuiltinBundleInfo> bundleInfoList = new List<BuiltinBundleInfo>();
            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                EditorUtility.DisplayProgressBar("内嵌资源中", "正在内嵌资源：" + fileInfoArr[i].Name, i * 1.0f / fileInfoArr.Length);
                //拷贝文件
                File.Copy(fileInfoArr[i].FullName, streamingAssetsPath + fileInfoArr[i].Name);
                //生成内嵌资源文件信息
                BuiltinBundleInfo bundleInfo = new BuiltinBundleInfo();
                bundleInfo.fileName = fileInfoArr[i].Name;
                bundleInfo.md5 = MD5.GetMd5FromFile(fileInfoArr[i].FullName);
                bundleInfo.size = fileInfoArr[i].Length / 1024;
                bundleInfoList.Add(bundleInfo);
            }
            
            string json = JsonConvert.SerializeObject(bundleInfoList, Formatting.Indented);
            
            if(!Directory.Exists(ResourcesPath))
            {
                Directory.CreateDirectory(ResourcesPath);
            }
            
            //写入配置文件到Ressources文件夹
            FileHelper.WriteFile(ResourcesPath+_bundleModuleEnum+"Info.json", System.Text.Encoding.UTF8.GetBytes(json));
            
            AssetDatabase.Refresh();
            
            EditorUtility.ClearProgressBar();
            if (showTips)
            {
                EditorUtility.DisplayDialog("内嵌操作", "内嵌资源完成 Path" + streamingAssetsPath, "确定");
            }
            Debug.Log("内嵌资源完成 Path" + streamingAssetsPath);
        }

        /// <summary>
        /// 生成热更资源
        /// </summary>
        private static void GeneratorHotAssets()
        {
            FileHelper.DeleteFolder(HotAssetsOutPutPath);
            Directory.CreateDirectory(HotAssetsOutPutPath);
            
            string[] bundlePathArr = Directory.GetFiles(BundleOutPutPath, "*.ab");
            for (int i = 0; i < bundlePathArr.Length; i++)
            {
                string path = bundlePathArr[i];
                EditorUtility.DisplayProgressBar("生成热更资源", "正在生成热更资源：" + Path.GetFileName(path), i * 1.0f / bundlePathArr.Length);
                string disPath = HotAssetsOutPutPath + Path.GetFileName(path);
                File.Copy(path, disPath);
            }
            Debug.Log("生成热更资源完成");
            GeneratorHotAssetsMainfest();
        }

        /// <summary>
        /// 生成热更资源配置清单
        /// </summary>
        private static void GeneratorHotAssetsMainfest()
        {
            //设置资源清单数据
            HotAssetsMainfest hotAssetsMainfest = new HotAssetsMainfest();
            hotAssetsMainfest.updateNotice = _updateNotice;
            hotAssetsMainfest.downLoadURL = BundleSettings.Instance.AssetBundleDownLoadURL + "/HotAssets/" + _bundleModuleEnum + "/" + _hotPatchVersion + "/" + BundleSettings.Instance.buildTarget;
            
            //设置补丁数据
            HotAssetsPatch hotAssetsPatch = new HotAssetsPatch();
            hotAssetsPatch.patchVesion = _hotPatchVersion;
            
            //计算热更补丁文件信息
            DirectoryInfo directoryInfo = new DirectoryInfo(HotAssetsOutPutPath);
            FileInfo[] bundleInfoArr = directoryInfo.GetFiles("*.ab");
            foreach (FileInfo bundleInfo in bundleInfoArr)
            {
                HotFileInfo hotFileInfo = new HotFileInfo();
                hotFileInfo.abName = bundleInfo.Name;
                hotFileInfo.md5 = MD5.GetMd5FromFile(bundleInfo.FullName);
                hotFileInfo.size = bundleInfo.Length / 1024.0f;
                hotAssetsPatch.hotAssetsList.Add(hotFileInfo);
            }
            hotAssetsMainfest.hotAssetsPatchList.Add(hotAssetsPatch);
            
            //把对象转为Json字符串
            string json = JsonConvert.SerializeObject(hotAssetsMainfest, Formatting.Indented);
            FileHelper.WriteFile(Application.dataPath + "/../HotAssets/" + _bundleModuleEnum + "AssetsHotMainfest.json", System.Text.Encoding.UTF8.GetBytes(json));
        }

    }
}
