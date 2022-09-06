using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 辅助工具类
/// </summary>
public static class AuxiliaryTool
{
    /// <summary>
    /// PlayerPrefs和缓存
    /// </summary>
    [MenuItem("Tools/清除本地缓存")]
    public static void ClearPlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
        ClearCache();
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    private static void ClearCache()
    {
        try
        {
            foreach (var file in Directory.GetFileSystemEntries(GetCacheDirectory()))
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(string.Format("删除临时文件失败，异常：{0}", e.Message.ToString()));
        }
    }

    private static string GetCacheDirectory()
    {
        string path = Path.Combine(Application.persistentDataPath, "__cache");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    [MenuItem("Tools/删除空目录")]
    public static void DelEmptyDir()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo("Assets/GameData");
        var dirs = directoryInfo.GetDirectories("*.*", SearchOption.AllDirectories);
        List<string> emptyDirList = new List<string>();
        for (int i = 0; i < dirs.Length; i++)
        {
            var dir = dirs[i].FullName;
            if (dir.Contains("~"))
            {
                continue;
            }
            var directoryInfo1 = new DirectoryInfo(dir);
            if (directoryInfo1.GetFiles().Length == 0)
            {
                emptyDirList.Add(dir);
            }
        }
        for (int i = 0; i < emptyDirList.Count; i++)
        {
            var dir = emptyDirList[i];
            if (Directory.Exists(dir))
            {
                Debug.Log(dir);
                Directory.Delete(dir);
                if (File.Exists(dir + ".meta"))
                {
                    File.Delete(dir + ".meta");
                }
            }
        }
        Debug.Log("删除空目录 Done!");
    }

    private static void GetSubDir(string dirname)
    {
        var dirs = AssetDatabase.GetSubFolders(dirname);
        for (int i = 0; i < dirs.Length; i++)
        {
            var files = Directory.GetFiles(dirs[i]);
            for (int j = 0; j < files.Length; j++)
            {
                if (files[j].EndsWith(".DS_Store"))
                {
                    Debug.Log("delete file:" + files[j]);
                    File.Delete(files[j]);
                }
            }
            files = Directory.GetFiles(dirs[i]);
            if (files.Length == 0)
            {
                Debug.Log("delete:" + dirs[i]);
                try
                {
                    Directory.Delete(dirs[i]);
                    AssetDatabase.Refresh();
                    DelEmptyDir();
                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }
            else
            {
                GetSubDir(dirs[i]);
            }
        }
    }
}
