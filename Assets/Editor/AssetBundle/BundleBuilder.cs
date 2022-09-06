using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class BundleBuilder
{
    public class FileInformation
    {
        public string FileName { get; set; }// 文件名字
        public string FullPath { get; set; }//绝对路径
        public string RelativePath { get; set; }//相对路径
        public string ABPath { get; set; }// AB标签
    }
    public class VersionConfig
    {
        public int vhall;
        public bool bencryname; //是否加密文件名
        public JsonData group;
        public bool bsplitpackages;//是否分包
        public JsonData packages;
    }
    // AB包输出路径(跟主工程目录同级)
    public static string BundleRootPath = Application.dataPath + "/../../ABFishing/";
    static string curBuildTarget = "";
    static public string BundlePath
    {
        get
        {
            if (curBuildTarget.Length > 0)
                return BundleRootPath + curBuildTarget;
            else
                return BundleRootPath + EditorUserBuildSettings.activeBuildTarget.ToString();
        }
    }
    static public string AbBytePath
    {
        get { return BundlePath + "/ABConfig.unity3d"; }
    }

    static string m_GameDataPath = Application.dataPath + "/GameData";
    static string m_ILRGenPath = Application.dataPath + "/Scripts/ILRuntime/Generated";
    static List<FileInformation> FileList = new List<FileInformation>();

    static VersionConfig versionCfg = new VersionConfig();
    static Dictionary<int, bool> mainPackage = new Dictionary<int, bool>();
    static BuildAssetBundleName m_BuildAssetBundleName;

    public static void Build(BuildTarget buildTarget, bool bSplitPackages = false, bool bTestSlipRes = false)
    {
        Debug.Log("BundleBuilder.Build:" + bSplitPackages);
        curBuildTarget = buildTarget.ToString();
        //ExcludeDir.Add("GameData/TestRes", 1);
        ReadVersion();
        //设置AB包名字
        SetBundleName();
        //打ab包到特定外部目录
        AssetBundleManifest manifest = BunildAssetBundle(buildTarget);
        //ab包加密
        if (!bTestSlipRes)
        {
            EncryptAssetBundles(BundlePath);
        }
        //拷贝DLL
        CopyDLL(BundlePath + "/");
        //文件名加密
        EncryptABName(BundlePath);
        //生成所有ab包资源的MD5文件
        CreateABMD5(BundlePath, manifest);
        //拷贝到资源目录
        CopyToAssetsPath(Application.streamingAssetsPath, bSplitPackages);
        //测试分包
        if (bTestSlipRes)
        {
            CopyToTestAssetsPath();
        }
        //刷新编辑器
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("AB文件打包到外部路径成功！");
    }

    public static void ResetCLRPath()
    {
        LogUtils.I("ResetCLRPath:" + m_ILRGenPath);
        BuildEditor.CleanPath(m_ILRGenPath);
        ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis();
        AssetDatabase.Refresh();
    }
    public static void ClearCLRPath()
    {
        LogUtils.I("ClearCLRPath:" + m_ILRGenPath);
        BuildEditor.CleanPath(m_ILRGenPath);
        AssetDatabase.Refresh();
    }

    public static Dictionary<string, string> ABPathMap = new Dictionary<string, string>();
    static Dictionary<string, string> Md5Name2ABName = new Dictionary<string, string>();

    static List<FileInformation> GetAllFiles(DirectoryInfo dir, int depth)
    {
        FileInfo[] allFile = dir.GetFiles();
        Md5Name2ABName = new Dictionary<string, string>();
        foreach (FileInfo fi in allFile)
        {
            if (fi.Name.IndexOf(".git") >= 0 || fi.Name.IndexOf(".DS_Store") >= 0)
                continue;
            if (fi.Name.EndsWith(".meta") == false)
            {
                //Debug.Log("fileName:"+ fi.Name + " ===== AbName:" + name);
                var abName = m_BuildAssetBundleName.CalcAssetBundleName(fi.FullName);
                var key = AssetsMgr.EncptyABName(abName);
                if (!ABPathMap.ContainsKey(key))
                {
                    string path = fi.FullName.Substring(Application.dataPath.Length + 1);
                    path = path.Replace("\\", "/");
                    var array = path.Split('/');
                    ABPathMap[key] = array[1].ToLower();
                    //Debug.Log($"key:{key} ===== Module:{ABPathMap[key]}");
                    if (Md5Name2ABName.ContainsKey(key))
                    {
                        if (Md5Name2ABName[key] != abName)
                        {
                            Debug.LogError($"转换后的md5名字重复 {abName} {Md5Name2ABName[key]}");
                        }
                    }
                    else
                    {
                        //Debug.Log($"{abName} => {key}");
                        Md5Name2ABName[key] = abName;
                    }
                }
                //Debug.Log($"fileName:{fi.Name} ===== abMd5Name:{abName}");
                FileList.Add(new FileInformation { FileName = fi.Name, FullPath = fi.FullName, ABPath = abName });
            }
        }

        DirectoryInfo[] allDir = dir.GetDirectories();

        foreach (DirectoryInfo d in allDir)
        {
            var name = d.Name.ToLower();
            if (depth == 1)
            {
                if (name.IndexOf(".git") >= 0)
                    continue;
            }
            if (name.Contains("~"))
            {
                continue;
            }
            GetAllFiles(d, depth + 1);
        }

        return FileList;
    }

    private static Dictionary<string, string> AbConfigDictionary() {
        string packConfigJson = "Assets/GameData/BuildConfig~/AssetBundleNameConfig.json";
        var readAllText = File.ReadAllText(packConfigJson);
        JsonData config = JsonMapper.ToObject(readAllText);
        var configKeys = config.Keys;
        Dictionary<string, string> di = new Dictionary<string, string>();

        foreach (var key in configKeys) {
            JsonData jsonData = config[key];
            var unity3d = key.ToLower();

            for (int i = 0; i < jsonData.Count; i++) {
                var bundle_name = jsonData[i].ToString();
                bundle_name = bundle_name.ToLower();

                di[bundle_name] = unity3d;
            }
        }

        return di;
    }
    static string CalcAssetBundleName(string path)
    {
        path = (path.Replace("\\", "/")).ToLower();
        return path;
    }

    public static void SetBundleName()
    {
        FileList.Clear();
        m_BuildAssetBundleName = new BuildAssetBundleName();

        ABPathMap = new Dictionary<string, string>();
        GetAllFiles(new DirectoryInfo(m_GameDataPath + "/"), 1);
        int index = 0;

        TextureImportSet textureImportSet = new TextureImportSet();
        textureImportSet.InitConfigList();

        List<String> guidsInAtlas = new List<string>();
        foreach (FileInformation info in FileList)
        {
            var fileName = info.FileName;
            if (fileName.EndsWith(".spriteatlas"))
            {
                info.RelativePath = info.FullPath.Substring(Application.dataPath.Length + 1);
                string relatPath = "Assets/" + info.RelativePath.Replace("\\", "/");

                var ids = TextureImportSet.TexturesInAtlas(relatPath);
                guidsInAtlas.AddRange(ids);
            }
        }

        foreach (FileInformation info in FileList)
        {
            index++;
            var fileName = info.FileName;
            //EditorUtility.DisplayProgressBar("给目录" + dir + "计算AB标签", fileName, index * 1.0f / FileList.Count);
            info.RelativePath = info.FullPath.Substring(Application.dataPath.Length + 1);
            string relatPath = "Assets/" + info.RelativePath.Replace("\\", "/");
            AssetImporter assetImporter = AssetImporter.GetAtPath(relatPath);
            string abName = info.ABPath;
            if (assetImporter != null) // && abName != assetImporter.assetBundleName)
            {
                assetImporter.assetBundleName = abName;
            }
            TextureImporter textureImporter = assetImporter as TextureImporter;
            textureImportSet.SetDefaultTextureCompressOpt(guidsInAtlas, textureImporter);
        }
    }

    private static void nop() {
        throw new NotImplementedException();
    }

    static void ClearBundleName(string dir)
    {
        GetAllFiles(new DirectoryInfo(m_GameDataPath + "/" + dir), 1);
        int index = 0;
        foreach (FileInformation info in FileList)
        {
            index++;
            info.RelativePath = info.FullPath.Substring(Application.dataPath.Length + 1);
            EditorUtility.DisplayProgressBar("给目录" + dir + "清除AB标签", info.RelativePath, index * 1.0f / FileList.Count);
            string relatPath = "Assets/" + info.RelativePath.Replace("\\", "/");
            AssetImporter assetImporter = AssetImporter.GetAtPath(relatPath);
            if (assetImporter != null && assetImporter.assetBundleName != "")
            {
                assetImporter.assetBundleName = "";
            }
        }
    }

    public static void ClearAssetBundlesName()
    {
        //ClearBundleName("");
        foreach (var item in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetDatabase.RemoveAssetBundleName(item, true);
        }

        Debug.Log("清除AB包名完成");
    }

    public static void CopyDLL(string dstDir)
    {
        var dllPath = Path.Combine(Application.dataPath, "../GameLib/Fishing.dll");
        File.Copy(dllPath, dstDir + "/Fishing.unity3d", true);
    }

    // 将视频拷贝到打包目录
    public static void CopyVideoFiles()
    {
        var videoPath = Application.dataPath + "/GameData/Video~";
        var resVideoPath = BundleRootPath + "Video";

        BuildEditor.CleanPath(resVideoPath);
        if (!Directory.Exists(videoPath))
            return;

        foreach (string dirPath in Directory.GetDirectories(videoPath, "*", SearchOption.AllDirectories))
        {
            var path = resVideoPath + dirPath.Replace(videoPath, "");
            Directory.CreateDirectory(path);
        }

        JsonData data = new JsonData();
        foreach (string newPath in Directory.GetFiles(videoPath, "*.mp4", SearchOption.AllDirectories))
        {
            var fileName = newPath.Replace(videoPath, resVideoPath);
            fileName = fileName.Replace(".mp4", ".unity3d");
            File.Copy(newPath, fileName, true);
            fileName = fileName.Replace('\\', '/');
            string shortName = fileName.Replace(resVideoPath + "/", "");
            data[shortName] = MD5Utils.BuildFileMd5(fileName);
        }
        //保存Md5信息
        string videoMd5Path = resVideoPath + "/VideoMd5.json";
        FileInfo file = new FileInfo(videoMd5Path);
        StreamWriter sw = file.CreateText();
        sw.WriteLine(data.ToJson());
        sw.Close();
        sw.Dispose();

        CopyInfoFiles();
    }

    // 将额外信息到包体 拷贝到打包目录
    public static void CopyInfoFiles()
    {
        var videoPath = Application.dataPath + "/GameData/UpdateInfo~";
        var resVideoPath = BundleRootPath + "Info";

        BuildEditor.CleanPath(resVideoPath);
        if (!Directory.Exists(videoPath))
            return;

        foreach (string dirPath in Directory.GetDirectories(videoPath, "*", SearchOption.AllDirectories))
        {
            var path = resVideoPath + dirPath.Replace(videoPath, "");
            Directory.CreateDirectory(path);
        }

        JsonData data = new JsonData();
        foreach (string newPath in Directory.GetFiles(videoPath, "*.txt", SearchOption.AllDirectories))
        {
            var fileName = newPath.Replace(videoPath, resVideoPath);
            fileName = fileName.Replace(".txt", ".unity3d");
            File.Copy(newPath, fileName, true);
        }
    }

    public static int GetBuildNo()
    {
        System.TimeSpan ts = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        int t = System.Convert.ToInt32(ts.TotalMilliseconds / 1000.0);
        return t;
    }

    // 生成md5
    static void CreateABMD5(string path, AssetBundleManifest manifest)
    {
        string fileName = "project.unity3d";
        string ABMD5Path = path + "/" + fileName;
        FileInfo file = new FileInfo(ABMD5Path);
        StreamWriter sw = file.CreateText();

        JsonData data = new JsonData();
        data["version"] = versionCfg.vhall;    //todo 读取配置
        System.TimeSpan ts = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        int t = System.Convert.ToInt32(ts.TotalMilliseconds / 1000.0);
        data["time"] = t;
        data["small"] = GetBuildNo();
        var assetList = new JsonData();
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.Equals(fileName))
                continue;
            if (files[i].Name.EndsWith(".meta") == false && files[i].Name.EndsWith(".manifest") == false)
            {
                var name = ABsoluteToRelative(files[i].FullName, path);
                var groupId = GetGroupId(name);
                JsonData abmd5Base = new JsonData();
                abmd5Base["Name"] = name;
                abmd5Base["Group"] = groupId;
                abmd5Base["Size"] = files[i].Length;
                // 计算MD5
                //var hash = manifest.GetAssetBundleHash(name);
                //if (!hash.isValid)
                    abmd5Base["Md5"] = MD5Utils.BuildFileMd5(files[i].FullName);
                //else
                //    abmd5Base["Md5"] = hash.ToString();
                assetList.Add(abmd5Base);
            }
        }
        data["assets"] = assetList;

        sw.WriteLine(data.ToJson());
        sw.Close();
        sw.Dispose();
    }

    static int GetGroupId(string name)
    {
        var path = ".";
        if (ABPathMap != null && ABPathMap.ContainsKey(name))
            path = ABPathMap[name];
        int ret = 0;
        if (versionCfg != null && versionCfg.group != null && !versionCfg.group.IsNull(path))
            ret = versionCfg.group.GetInt(path, 0);
        //Debug.Log($"GetGroupId {path} {name} {ret}");
        return ret;
    }

    static string GetAbPath(string name)
    {
        if (name.IndexOf("gamedata/") >= 0)
        {
            var idx = name.LastIndexOf("/");
            name = name.Substring(idx + 1);
            return name;
        }
        return ".";
    }

    public static void ReadVersion()
    {
        versionCfg = new VersionConfig();
        string str = File.ReadAllText(Application.dataPath + "/GameData/BuildConfig~/VHall.txt");
        versionCfg = JsonMapper.ToObject<VersionConfig>(str.ToLower());
        mainPackage = new Dictionary<int, bool>();
        string strMain = "";
        if (!versionCfg.packages.IsNull("main"))
        {
            var main = versionCfg.packages["main"];
            if (main.IsArray)
            {
                for (int i = 0; i < main.Count; i++)
                {
                    int k = (int)main[i];
                    mainPackage[k] = true;
                    strMain += $"{k},";
                }
            }
        }
        Debug.Log($"ReadVersion version:{versionCfg.vhall}, bSlipPackage:{versionCfg.bsplitpackages}, mainPackage:{strMain}");
    }

    static string ABsoluteToRelative(string fullName, string buildPath = "")
    {
        if (buildPath == "")
            buildPath = BundlePath;
        string fullPath = Path.GetFullPath(buildPath) + "\\";
        fullPath = fullPath.Replace('\\', '/');
        fullName = fullName.Replace('\\', '/');
        string relativeFullName = fullName.Replace(fullPath, "");
        return relativeFullName;
    }

    static AssetBundleManifest BunildAssetBundle(BuildTarget buildTarget)
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        Debug.Log("allBundles length " + allBundles.Length);
        // 资源路径 ： 资源所属的AB包
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            string[] allResPath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allResPath.Length; j++)
            {
                if (allResPath[j].EndsWith(".cs"))
                    continue;
                //Debug.Log(allBundles[i] + " " + allResPath[j]);
                string dir = Path.GetDirectoryName(allResPath[j]);
                dir = dir.Replace("\\", "/");
                resPathDic[dir] = allBundles[i];
                //resPathDic.Add(allResPath[j], allBundles[i]);
            }
        }

        // 清除之前生成的AssetBundle文件
        ResetDir(BundlePath);
        ResetDir(Application.streamingAssetsPath);
        Debug.Log("---------------------------------------");

        //生成AB包对应的配置表
        WriteABConfig(resPathDic);
        //打ab包
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(BundlePath,
            BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
        if (manifest == null)
            Debug.LogError("AssetBundle 打包失败！");
        DeleteMainfest();
        Debug.Log("AssetBundle 打包完毕");
        return manifest;
    }

    static void WriteABConfig(Dictionary<string, string> resPathDic)
    {
        FileInfo file = new FileInfo(AbBytePath);
        StreamWriter sw = file.CreateText();
        Dictionary<string, string> ABConfig = new Dictionary<string, string>();
        //JsonData ABConfig = new JsonData();
        foreach (string path in resPathDic.Keys)
        {
            var filePath = path.ToLower().Replace("assets/gamedata/", "");
            ABConfig[filePath] = resPathDic[path].Replace(".unity3d", "");
        }
        sw.WriteLine(JsonMapper.ToJson(ABConfig));
        sw.Close();
        sw.Dispose();
    }

    static void ResetDir(string path)
    {
        if (Directory.Exists(path) == true)
            (new DirectoryInfo(path)).Delete(true);
        Directory.CreateDirectory(path);
    }

    static void DeleteMainfest()
    {
        Debug.Log("清除无用文件");
        // 将Android或者iOS文件删除
        //string sTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        DirectoryInfo directoryInfo = new DirectoryInfo(BundleRootPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".manifest") || files[i].Name.Equals("assets"))
            {
                File.Delete(files[i].FullName);
            }
            if (files[i].Name.EndsWith(curBuildTarget))
            {
                File.Move(files[i].FullName, BundlePath + "/ABFishing.unity3d");
            }
        }
    }
    static public void CopyToAssetsPath(string assetPath, bool bSplitPackages)
    {
        foreach (string dirPath in Directory.GetDirectories(BundlePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(BundlePath, assetPath));
        }

        foreach (string newPath in Directory.GetFiles(BundlePath, "*.*", SearchOption.AllDirectories))
        {
            var name = ABsoluteToRelative(Path.GetFullPath(newPath), BundlePath);
            int groupId = GetGroupId(name);
            // 过滤掉分包文件
            if (bSplitPackages && !mainPackage.ContainsKey(groupId))
            {
                LogUtils.I($"split file name:{name}, groupId:{groupId}");
                continue;
            }
            File.Copy(newPath, newPath.Replace(BundlePath, assetPath), true);
        }
        // 重写分包的版本文件
        if (bSplitPackages)
        {
            var projectFile = $"{assetPath}/project.unity3d";
            if (!File.Exists(projectFile))
            {
                LogUtils.E("CopyToAssetsPath error not exists file " + projectFile);
                return;
            }
            //读取版本文件
            JsonData data = new JsonData();
            var readAllText = File.ReadAllText(projectFile);
            JsonData config = JsonMapper.ToObject(readAllText);
            data["version"] = config.GetInt("version", 0);
            data["time"] = config.GetInt("time", 0);
            data["small"] = config.GetInt("small", 0);
            JsonData assetList = new JsonData();
            var abList = config["assets"];
            foreach (JsonData abmd5Base in abList)
            {
                var groupId = abmd5Base.GetInt("Group", 0);
                if (!mainPackage.ContainsKey(groupId))
                {
                    LogUtils.I($"分包资源:{abmd5Base.GetString("Name")},Group:{groupId}");
                    continue;
                }
                assetList.Add(abmd5Base);
            }
            data["assets"] = assetList;
            FileInfo file = new FileInfo(projectFile);
            StreamWriter sw = file.CreateText();
            sw.WriteLine(data.ToJson());
            sw.Close();
            sw.Dispose();
        }
    }

    static public void CopyToTestAssetsPath()
    {
        var dir = Application.dataPath + "/../PackConfig/ABFishing/SplitPath/";
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir);
        }
        Directory.CreateDirectory(dir);
        

        foreach (string newPath in Directory.GetFiles(BundlePath, "*.*", SearchOption.AllDirectories))
        {
            var name = ABsoluteToRelative(Path.GetFullPath(newPath), BundlePath);
            string subDir = "default";
            if (ABPathMap != null && ABPathMap.ContainsKey(name))
            {
                subDir = ABPathMap[name];
            }
            int ret = 0;
            if (versionCfg != null && versionCfg.group != null && !versionCfg.group.IsNull(subDir))
            {
                ret = versionCfg.group.GetInt(subDir, 0);
            }
            subDir = dir + $"data{ret}/";
            if (!Directory.Exists(subDir))
            {
                Directory.CreateDirectory(subDir);
            }
            File.Copy(newPath, subDir + name, true);

        }
    }


    //=============AB包的加密解密
    static void EncryptAssetBundles(string path = "")
    {
        var dirPath = string.IsNullOrEmpty(path) ? Application.streamingAssetsPath : path;
        Debug.Log("EncryptAssetBundles:" + dirPath);
        foreach (string newPath in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
        {
            if (newPath.EndsWith(".unity3d") && !newPath.EndsWith("ABConfig.unity3d"))
            {
                EncryptABFile(newPath);
                //Debug.Log("EncryptAssetBundles:" + newPath);
            }
        }
    }

    static void EncryptABName(string path = "")
    {
        if (AssetsMgr.ABNameSC.Length <= 0)
            return;

        var dirPath = string.IsNullOrEmpty(path) ? Application.streamingAssetsPath : path;
        Debug.Log("EncryptABName:" + dirPath);
        DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest"))
                continue;
            var fileName = ABsoluteToRelative(files[i].FullName, dirPath);
            if (fileName == "project.unity3d")
                continue;

            var newName = AssetsMgr.EncptyABName(fileName);
            Debug.Log($"{fileName}文件名加密为:{newName}");
            File.Move(files[i].FullName, $"{dirPath}/{newName}");
        }
    }

    const int AssetBundle_Offset = 48;
    static string AB_Head = "ABEncrypt";
    static int AB_Head_Length = 9;
    
    /// <summary>
    /// ab包加密
    /// </summary>
    /// <param name="abFilePath"></param>
    static void EncryptABFile(string abFilePath)
    {
        try
        {
            if (File.Exists(abFilePath) == false)
            {
                Debug.LogError("ab包路径不存在：" + abFilePath);
                return;
            }

            byte[] filedata = File.ReadAllBytes(abFilePath);
            var decryptHeadByte = filedata.Skip(0).Take(AB_Head_Length).ToArray();
            if ((Encoding.UTF8.GetString(decryptHeadByte) == AB_Head))
            {
                return;
            }

            int filelen = (AssetBundle_Offset + filedata.Length);
            byte[] buffer = new byte[filelen];
            // 固定头加随机定长头部偏移字节数组
            byte[] headBytes = Encoding.UTF8.GetBytes(AB_Head);
            for (int i = 0; i < headBytes.Length; i++)
            {
                buffer[i] = headBytes[i];
            }
            string fileName = Path.GetFileName(abFilePath);
            var randomBytes = GetRandomBytes(fileName);
            for (int i = 0; i < randomBytes.Length; i++)
            {
                buffer[headBytes.Length + i] = randomBytes[i];
            }

            for (int i = 0; i < filedata.Length; i++)
            {
                buffer[AssetBundle_Offset + i] = filedata[i];
            }

            FileStream fs = File.OpenWrite(abFilePath);
            fs.Write(buffer, 0, filelen);
            fs.Close();
        }
        catch (Exception message)
        {
            Debug.LogError(message);
        }
    }

    /// <summary>
    /// ab包解密
    /// </summary>
    /// <param name="abFilePath"></param>
    static void DecryptABFile(string abFilePath)
    {
        try
        {
            if (File.Exists(abFilePath) == false)
            {
                Debug.LogError("ab包路径不存在：" + abFilePath);
                return;
            }
            // 判断是否有加密
            byte[] filedata = File.ReadAllBytes(abFilePath);
            var decryptHeadByte = filedata.Skip(0).Take(AB_Head_Length).ToArray();
            if (Encoding.UTF8.GetString(decryptHeadByte) != AB_Head)
            {
                return;
            }

            int filelen = filedata.Length - AssetBundle_Offset;
            byte[] buffer = new byte[filelen];
            for (int i = 0; i < filelen; i++)
            {
                buffer[i] = filedata[AssetBundle_Offset + i];
            }
            Debug.Log("解密ab: " + abFilePath);

            using (Stream fs = File.Open(abFilePath, FileMode.Create))
            {
                fs.Write(buffer, 0, filelen);
                fs.Close();
            }
        }
        catch (Exception message)
        {
            Debug.LogError(message);
        }
    }

    static byte[] GetRandomBytes(string filename)
    {
        var md5 = System.Security.Cryptography.MD5.Create();
        var fileMD5Bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(filename));
        byte[] bytes = new byte[AssetBundle_Offset - AB_Head_Length];
        int idx = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = fileMD5Bytes[idx];
            idx++;
            if (idx >= fileMD5Bytes.Length)
            {
                idx = 0;
            }
        }
        return bytes;
    }
}
