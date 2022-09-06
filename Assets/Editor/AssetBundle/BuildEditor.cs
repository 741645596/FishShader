using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YamlDotNet.Serialization;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class BuildEditor
{

    [MenuItem("Tools/Build/设置ab包名")]
    static void TestSetAssetBundleName()
    {
        BundleBuilder.SetBundleName();
        Debug.Log("设置ab包名 成功");
    }

    [MenuItem("Tools/Build/清除无效ab包名")]
    public static void RemoveUnusedAssetBundleName()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        Debug.Log("清除无效ab包名 成功");
    }

    [MenuItem("Tools/Build/移动dll至ios目录")]
    public static void MoveDllToStrime()
    {
        var sProjectPath = Application.dataPath + "/../PackConfig/NativeProjects/XCodeProject/Data/Raw";
        BundleBuilder.CopyDLL(sProjectPath);
        Debug.Log("移动dll至ios目录:" + sProjectPath);
    }

    //[MenuItem("Tools/Build/矫正贴图压缩配置")]
    //public static void CheckTextureCompressOpt()
    //{
    //    var allAssetPaths = AssetDatabase.GetAllAssetPaths();
    //    TextureImportSet textureImportSet = new TextureImportSet();
    //    textureImportSet.InitConfigList();

    //    List<string> guidsInAtlas = new List<string>();
    //    for (int i = 0; i < allAssetPaths.Length; i++)
    //    {
    //        var allAssetPath = allAssetPaths[i];
    //        var fileName = allAssetPath;
    //        if (fileName.EndsWith(".spriteatlas"))
    //        {
    //            var ids = TextureImportSet.TexturesInAtlas(allAssetPath);
    //            guidsInAtlas.AddRange(ids);
    //        }
    //    }

    //    for (int i = 0; i < allAssetPaths.Length; i++)
    //    {
    //        var relatPath = allAssetPaths[i];

    //        AssetImporter assetImporter = AssetImporter.GetAtPath(relatPath);

    //        TextureImporter textureImporter = assetImporter as TextureImporter;
    //        TextureImportSet.SetTextureCompressOpt(guidsInAtlas, textureImporter, textureImportSet);
    //    }

    //    AssetDatabase.SaveAssets();
    //}

    [MenuItem("Tools/Build/ExportBundle")]
    public static void ExportBundle() {
        CleanPath(BundleBuilder.BundleRootPath);
        // 导出ios平台的ab包
        Debug.Log("Start Export iOS");
        BundleBuilder.Build(BuildTarget.iOS);
        // 导出android平台的ab包
        Debug.Log("Start Export Android");
        BundleBuilder.Build(BuildTarget.Android);
        // 将视频复制到AB包根目录
        BundleBuilder.CopyVideoFiles();
    }

    [MenuItem("Tools/Build/ExportBundleAndroid")]
    public static void ExportBundleAndroid() {
        // 导出android平台的ab包
        Debug.Log("Start Export Android");
        BundleBuilder.Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build/ExportBundleIOS")]
    public static void ExportBundleIOS() {
        // 导出iOS平台的ab包
        Debug.Log("Start Export iOS");
        BundleBuilder.Build(BuildTarget.iOS);
    }

    [MenuItem("Tools/Build/测试分包资源")]
    public static void ExportBundleByTestSplitRes()
    {
        // 导出android平台的ab包
        Debug.Log("Start Export Android");
        BundleBuilder.Build(BuildTarget.Android, true, true);
        //BundleBuilder.CopyToTestAssetsPath();
    }

    //[MenuItem("Tools/Build/测试uwa资源")]
    //public static void ExportBundleByUWATest()
    //{
    //    // 导出android平台的ab包
    //    //Debug.Log("Start Export Android");
    //    //BundleBuilder.Build(BuildTarget.Android, true, true);
    //    //BundleBuilder.CopyToTestAssetsPath();
    //    BundleBuilder.ClearAssetBundlesName();
    //}

    public static void Switch2AndroidPlatform()
    {
        // 如果不是android平台，转为Android平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                LogUtils.E("请先下载Android开发组件");
                return;
            }
            AssetDatabase.Refresh();
        }
    }
    // 打包脚本执行前，需要先执行切换平台，不然执行OnUpdateXcodeProjectNativePath生成XCode工程生成会有bug
    public static void Switch2IOSPlatform()
    {
        // 如果不是ios平台，转为ios平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                LogUtils.E("请先下载iOS开发组件");
                return;
            }
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/Build/BuildAndroid")]
    public static void BuildAndroid() {
        var args = new CommandLineArgsHelper(Environment.GetCommandLineArgs());
        // 是否需要分包
        bool bSplitPackages = args.GetBool("splitPackage");
        Debug.Log($"BuildAndroid bSplitPackages:{bSplitPackages}");
        // 清理AB包路径
        CleanPath(BundleBuilder.BundleRootPath);
        BundleBuilder.ResetCLRPath();
        BundleBuilder.CopyVideoFiles();
        BundleBuilder.Build(BuildTarget.Android, bSplitPackages);
        var sProjectPath = Application.dataPath + "/../PackConfig/NativeProjects/AndroidProject";

        GenAndroidProject(sProjectPath, args);
    }

    static void GenAndroidProject(string sProjPath, CommandLineArgsHelper args) {
        // 如果不是android平台，转为Android平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                LogUtils.E("请先下载Android开发组件");
                return;
            }
        }

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        var disable_dbg = true;
        BuildOptions buildOptions = BuildOptions.AcceptExternalModificationsToPlayer; // | BuildOptions.CompressWithLz4;
        var implementation = ScriptingImplementation.IL2CPP;
        var useMono = args.GetBool("use_mono", false);
        if (useMono) {
            implementation = ScriptingImplementation.Mono2x;
            disable_dbg = false;

            buildOptions |= BuildOptions.Development;
            buildOptions |= BuildOptions.AllowDebugging;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }
        Debug.Log("useMono" + useMono);

        EditorUserBuildSettings.allowDebugging = useMono;
        EditorUserBuildSettings.development = useMono;

        bool ENABLE_POCO = args.GetBool("poco", false);
        var projectpath = args.GetString("projectpath");
        var logfile = args.GetString("logfile");
        SetScriptingDefine(BuildTargetGroup.Android, "ENABLE_POCO", ENABLE_POCO);
        SetScriptingDefine(BuildTargetGroup.Android, "DISABLE_ILRUNTIME_DEBUG", disable_dbg); // IL2CPP不支持
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, implementation);
        
        // 清理缓存路径
        var projAssetsPath = sProjPath + "/unityLibrary/src/main/assets";
        CleanPath(projAssetsPath);

        PlayerSettings.applicationIdentifier = "com.weile.fish"; //设置包名
        PlayerSettings.Android.forceSDCardPermission = false;//允许读写SD卡
        PlayerSettings.SplashScreen.show = false;   //隐藏unity的logo
        PlayerSettings.muteOtherAudioSources = true;//广电总局政策 监听呼入电话信息的时机控制

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions(); //输出选项
        //构建的场景
        buildPlayerOptions.scenes = new[] {
            "Assets/Scenes/Fishing.unity",
        };
        buildPlayerOptions.locationPathName = sProjPath; //输出路径
        buildPlayerOptions.target = BuildTarget.Android; //设置输出平台
        buildPlayerOptions.options = buildOptions;

        // 输出Android工程选项
        BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        SetScriptingDefine(BuildTargetGroup.Android, "DISABLE_ILRUNTIME_DEBUG", false);// 还原宏定义
        
        // var serializer = new Serializer();
        // var s = serializer.Serialize(buildReport);
        // File.WriteAllText("Library/LastBuild.buildreport", s);

        // 删除ndk生成的库
        var libPath32 = sProjPath + "/unityLibrary/src/main/jniLibs/armeabi-v7a/libil2cpp.so";
        if (File.Exists(libPath32))
            File.Delete(libPath32);
        var libPath64 = sProjPath + "/unityLibrary/src/main/jniLibs/arm64-v8a/libil2cpp.so";
        if (File.Exists(libPath64))
            File.Delete(libPath64);

        Debug.Log("导出android工程到成功！导出路径：" + sProjPath);
    }

    [MenuItem("Tools/Build/BuildIOS")]
    public static void BuildIOS() {
        var args = new CommandLineArgsHelper(Environment.GetCommandLineArgs());
        bool bSplitPackages = args.GetBool("splitPackage");
        Debug.Log($"BuildIOS bSplitPackages:{bSplitPackages}");

        CleanPath(BundleBuilder.BundleRootPath);
        BundleBuilder.ResetCLRPath();
        BundleBuilder.CopyVideoFiles();
        BundleBuilder.Build(BuildTarget.iOS, bSplitPackages);
        var sProjectPath = Application.dataPath + "/../../WLXCodeProject40";
        var sTempProjPath = Application.dataPath + "/../../tempIosProject";
        GenXCodeProject(sProjectPath, sTempProjPath);
    }

    static void GenXCodeProject(string sProjPath, string sProjTempPath) {
        // 如果不是ios平台，转为ios平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                LogUtils.E("请先下载iOS开发组件");
                return;
            }
        }
        // 宏定义
        EnableSymbol("DISABLE_ILRUNTIME_DEBUG", true);

        CleanPath(sProjTempPath); //重置输出的缓存路径
        PlayerSettings.applicationIdentifier = "com.weile.fish"; // 设置包名
        PlayerSettings.SplashScreen.show = false;   //隐藏unity的logo

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions(); // 输出选项
        // 构建的场景
        buildPlayerOptions.scenes = new[] {
            "Assets/Scenes/Fishing.unity",
        };
        buildPlayerOptions.locationPathName = sProjTempPath; //输出路径
        buildPlayerOptions.target = BuildTarget.iOS; //设置输出平台
        buildPlayerOptions.options = BuildOptions.None;

        // 输出ios工程选项
        Debug.Log("开始导出ios工程");
        BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        // 还原宏定义
        EnableSymbol("DISABLE_ILRUNTIME_DEBUG", false);

        Debug.Log("导出ios工程成功！正在拷贝资源和代码。");
        //拷贝资源
        CleanPath(sProjPath + "/Data");
        CopyEntireDir(sProjTempPath + "/Data", sProjPath + "/Data");

        //拷贝代码
        OnUpdateXcodeProjectNativePath(sProjPath, sProjTempPath);
        Debug.Log("生成project_ios资源成功，请切换到XCode工程完成最终打包！");
        // 删除temp目录
        if (Directory.Exists(sProjTempPath) == true)
            (new DirectoryInfo(sProjTempPath)).Delete(true);
    }

    /// <summary>
    /// 清除并创建文件夹
    /// </summary>
    public static void CleanPath(string path) {
        if (Directory.Exists(path) == true) {
            (new DirectoryInfo(path)).Delete(true);
        }

        Directory.CreateDirectory(path);
    }

    // 工具函数，文件夹整体拷贝
    static void CopyEntireDir(string sourcePath, string destPath) {
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath));
        }

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
            File.Copy(newPath, newPath.Replace(sourcePath, destPath), true);
        }
    }

    static void OnUpdateXcodeProjectNativePath(string sProjectPath, string sTempProjPath) {
#if UNITY_IOS
        string projPath = PBXProject.GetPBXProjectPath(sProjectPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string tempProjPath = PBXProject.GetPBXProjectPath(sTempProjPath);
        var tempProj = new PBXProject();
        tempProj.ReadFromFile(tempProjPath);

        //删除工程中的多余文件
        var sNativePath = sProjectPath + "/Classes/Native/";
        DirectoryInfo directoryInfo = new DirectoryInfo(sNativePath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            var path = "Classes/Native/" + files[i].Name;
            if (tempProj.FindFileGuidByProjectPath(path) == null)
            {
                Debug.Log("remove file:" + path);
                var file = proj.FindFileGuidByProjectPath(path);
                proj.RemoveFile(file);
                File.Delete(sNativePath + files[i].Name);
            }
        }
        proj.WriteToFile(projPath);

        //从temp目录拷贝文件到工程目录
        var framework = proj.TargetGuidByName("UnityFramework");
        var sTempNativePath = sTempProjPath + "/Classes/Native/";
        DirectoryInfo tempDir = new DirectoryInfo(sTempNativePath);
        FileInfo[] tempFiles = tempDir.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < tempFiles.Length; i++)
        {
            var path = "Classes/Native/" + tempFiles[i].Name;
            var tempFileName = sTempNativePath + tempFiles[i].Name;
            File.Copy(tempFileName, sNativePath + tempFiles[i].Name, true);
            if (proj.FindFileGuidByProjectPath(path) == null)
            {
                Debug.Log("add file:" + path);
                var uid = proj.AddFile(path, path, PBXSourceTree.Source);
                proj.AddFileToBuild(framework, uid);
            }
        }
        proj.WriteToFile(projPath);

        // 拷贝部分库文件到工程目录
        var sTempLibsPath = sTempProjPath + "/Libraries/";
        DirectoryInfo tempLibsDir = new DirectoryInfo(sTempLibsPath);
        FileInfo[] tempLibFiles = tempLibsDir.GetFiles("*.a", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < tempLibFiles.Length; i++)
        {
            var targetPath = sProjectPath + "/Libraries/" + tempLibFiles[i].Name;
            File.Copy(tempLibFiles[i].FullName, targetPath, true);
        }

        // 拷贝 MapFileParser 相关的文件
        string[] copyFiles = { "MapFileParser", "MapFileParser.sh" };
        for (int i = 0; i < copyFiles.Length; i++)
        {
            string srcPath = sTempProjPath + "/" + copyFiles[i];
            if (File.Exists(srcPath))
            {
                string dstPath = sProjectPath + "/" + copyFiles[i];
                File.Copy(srcPath, dstPath, true);
            }
        }

        var readAllText = File.ReadAllText(projPath);
        readAllText = readAllText.Replace("= 微乐捕鱼千炮版.app", "= \"微乐捕鱼千炮版.app\"");
        readAllText = readAllText.Replace("= 微乐捕鱼千炮版", "= \"微乐捕鱼千炮版\"");
        FileInfo f = new FileInfo(projPath);
        StreamWriter sw = f.CreateText();
        sw.WriteLine(readAllText);
        sw.Close();
        sw.Dispose();
#else
        Debug.LogError("工程未切换至ios平台");
#endif
    }

    // 增加或删除一个宏定义
    public static void EnableSymbol(string symbol, bool bEnable)
    {
        BuildTargetGroup targetGroup = BuildTargetGroup.Unknown;
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            targetGroup = BuildTargetGroup.Android;
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            targetGroup = BuildTargetGroup.iOS;
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            targetGroup = BuildTargetGroup.Standalone;

        SetScriptingDefine(targetGroup, symbol, bEnable);
    }

    private static void SetScriptingDefine(BuildTargetGroup targetGroup, string symbol, bool bEnable) {
        bool bFind = false;
        string ori = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        List<string> defineSymbols = new List<string>(ori.Split(';'));
        for (int i = 0; i < defineSymbols.Count; ++i) {
            if (defineSymbols[i] == symbol) {
                bFind = true;
                break;
            }
        }

        if (bEnable && !bFind) {
            defineSymbols.Add(symbol);
        }
        else if (!bEnable && bFind) {
            defineSymbols.Remove(symbol);
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineSymbols.ToArray()));
    }
}