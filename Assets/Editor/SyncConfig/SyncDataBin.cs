using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

public class SyncDataBin
{
    [MenuItem("Tools/同步配置/同步策划配表")]
    public static void Sync()
    {
        string root = Path.GetFullPath(Application.dataPath + "/../Config/Config_new");
        Debug.Log(root);
        if (Directory.Exists(root) == false)
        {
            Debug.LogError("请同步Config子模块");
            return;
        }
        string path = Path.GetFullPath(root + "/Json");
        Debug.Log(path);
        // 遍历文件
        string dst = Path.GetFullPath(Application.dataPath + "/GameData/AppRes/Config");
        foreach (string newPath in Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly))
        {
            if (!Directory.Exists(newPath))
            {
                File.Copy(newPath, newPath.Replace(path, dst), true);
                Debug.Log("copy " + newPath);
            }
        }

        path = Path.GetFullPath(root + "/Cs");
        Debug.Log(path);
        dst = Path.GetFullPath(Application.dataPath + "/../Games/FishLogic/Config");
        foreach (string newPath in Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly))
        {
            File.Copy(newPath, newPath.Replace(path, dst), true);
            Debug.Log("copy " + newPath);
        }

    }

    [MenuItem("Tools/同步配置/同步策划配表(Release)")]
    public static void Sync1()
    {
        string root = Path.GetFullPath(Application.dataPath + "/../Config/Config_Release");
        Debug.Log(root);
        if (Directory.Exists(root) == false)
        {
            Debug.LogError("请同步Config子模块");
            return;
        }
        string path = Path.GetFullPath(root + "/Json");
        Debug.Log(path);
        // 遍历文件
        string dst = Path.GetFullPath(Application.dataPath + "/GameData/AppRes/Config");
        foreach (string newPath in Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly))
        {
            if (!Directory.Exists(newPath))
            {
                File.Copy(newPath, newPath.Replace(path, dst), true);
                Debug.Log("copy " + newPath);
            }
        }

        path = Path.GetFullPath(root + "/Cs");
        Debug.Log(path);
        dst = Path.GetFullPath(Application.dataPath + "/../Games/FishLogic/Config");
        foreach (string newPath in Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly))
        {
            File.Copy(newPath, newPath.Replace(path, dst), true);
            Debug.Log("copy " + newPath);
        }
    }
}
