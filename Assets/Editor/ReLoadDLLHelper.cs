using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 用于热更工程dll运行时重载，如在运行时，测试工程代码修改编译完成后
/// 将自动热重启无需再点击播放器重新播放
/// 非测试用例由于关联场景数据较多占时不支持热重启
/// </summary>
[InitializeOnLoad]
public static class ReLoadDLLHelper
{
    // 编辑器选项 Auto Refresh 的key
    private const string kKeyOfAutoRefresh = "kAutoRefresh";
    private static FileSystemWatcher _watcher;

    static ReLoadDLLHelper()
    {
        EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
    }

    /// <summary>
    /// 播放模式改变时调用
    /// </summary>
    /// <param name="playingState"></param>
    private static void OnEditorPlayModeStateChanged(PlayModeStateChange playingState)
    {
        switch (playingState)
        {
            // 点击播放按钮
            case PlayModeStateChange.ExitingEditMode:
                break;

            // 进入播放模式
            case PlayModeStateChange.EnteredPlayMode:
                // 主线程里查找游戏物体

                // 注册监听器
                RegisterWatcher();

                // 关闭自动编译
                SetAutoRefresh(false);
                break;

            // 退出播放模式
            case PlayModeStateChange.EnteredEditMode:
                // 恢复自动编译
                SetAutoRefresh(true);

                // 停止监听
                WatchStop();
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 注册监听
    /// </summary>
    private static void RegisterWatcher()
    {
        var path = Application.dataPath + "/../GameLib/";
        path = Path.GetFullPath(path);
        Debug.Log($"RegisterWatcher {path}");
        WatchStop();
        WatcherStart(path, "*.dll");
    }

    private static void SetAutoRefresh(bool b)
    {
        if (EditorPrefs.HasKey(kKeyOfAutoRefresh))
        {
            EditorPrefs.SetBool(kKeyOfAutoRefresh, b);
        }
    }

    /// <summary>
    /// 初始化监听
    /// </summary>
    /// <param name="strWarcherPath">需要监听的目录</param>
    /// <param name="filterType">需要监听的文件类型(筛选器字符串)</param>
    private static void WatcherStart(string strWarcherPath, string filterType)
    {
        _watcher = new FileSystemWatcher();
        //初始化监听
        _watcher.BeginInit();
        //设置监听文件类型
        _watcher.Filter = filterType;
        //设置需要监听的更改类型(如:文件或者文件夹的属性,文件或者文件夹的创建时间;NotifyFilters枚举的内容)
        _watcher.NotifyFilter = NotifyFilters.Attributes |
            NotifyFilters.CreationTime |
            NotifyFilters.DirectoryName |
            NotifyFilters.FileName |
            NotifyFilters.LastAccess |
            NotifyFilters.LastWrite |
            NotifyFilters.Security |
            NotifyFilters.Size;
        //设置监听的路径
        _watcher.Path = strWarcherPath;
        //注册文件改变的监听事件
        _watcher.Changed += WatchChanged;
        _watcher.Created += WatchCreate;    // Mac电脑识别不了Changed
        //设置是否启用监听?
        _watcher.EnableRaisingEvents = true;
        //结束初始化
        _watcher.EndInit();
    }

    private static void WatchChanged(object sender, FileSystemEventArgs e)
    {
        // 需主线程调用
        Reload();
    }

    private static void WatchCreate(object sender, FileSystemEventArgs e)
    {
        Reload();
    }

    private static void Reload()
    {
        ReStart();
    }

    /// <summary>
    /// 重载
    /// </summary>
    private static void ReStart()
    {
        if (GameApp.Instance != null)
        {
            GameApp.Instance.ReloadGame();
            Debug.Log("Restart++++++++++++++");
        }
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    private static void WatchStop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= WatchChanged;
            _watcher.Created -= WatchCreate;
            _watcher.Dispose();
            _watcher = null;
        }
    }
}