using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions;

class BuildAssetBundleName
{
    //[MenuItem("Tools/Build/测试设置ab包名")]
    static void TestSetAssetBundleName()
    {
        var builder = new BuildAssetBundleName();
        string dataPath = Application.dataPath + "/GameData/AppRes";
        builder.SetAssetBundleNameByDir(dataPath);
        Debug.Log("设置ab包名 成功");
    }

    class CombinUnit
    {
        public string name;
        public string[] list;
    }

    class AssetBundleNameConfig
    {
        public List<CombinUnit> combine;
        public Dictionary<string, int> depth_define;
    }

    AssetBundleNameConfig m_Config;

    Dictionary<string, string> m_CombineAssetBundleMap;



    public BuildAssetBundleName()
    {
        LoadConfig();
    }

    void LoadConfig()
    {
        string packConfigJson = "Assets/GameData/BuildConfig~/AssetBundleNameConfig.json";
        var readAllText = File.ReadAllText(packConfigJson);
        m_Config = JsonMapper.ToObject<AssetBundleNameConfig>(readAllText);
        m_CombineAssetBundleMap = new Dictionary<string, string>();
        for (int i = 0; i < m_Config.combine.Count; i++)
        {
            var unit = m_Config.combine[i];
            var list = unit.list;
            for (int j = 0; j < list.Length; j++)
            {
                m_CombineAssetBundleMap[list[j]] = unit.name;
            }
        }
    }

    public void SetAllAssetBundleName()
    {
        string dataPath = Application.dataPath + "/GameData";
        SetAssetBundleNameByDir(dataPath);
    }

    public void SetAssetBundleNameByDir(string dir)
    {
        var directoryInfo = new DirectoryInfo(dir);
        FileInfo[] allFiles = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            var info = allFiles[i];
            if (info.Name.EndsWith(".meta"))
            {
                continue;
            }
            string RelativePath = info.FullName.Substring(Application.dataPath.Length + 1);
            RelativePath = RelativePath.Replace("\\", "/");
            string abName = CalcAssetBundleName(info.FullName);
            Debug.Log($"{RelativePath} {abName}");
        }
    }

    public string CalcAssetBundleName(string path)
    {
        path = path.Substring(Application.dataPath.Length + 1);
        path = path.Replace("\\", "/");
        string abName = "";
        // ab包层级
        int depth = 2;

        string pathLower = path.ToLower().Replace("/", "_");
        if (Regex.IsMatch(pathLower, @"roomres\w*_fish[0-9]{3}"))
        {
            Match match = Regex.Match(pathLower, "fish[0-9]{3}");
            string fishname = pathLower.Substring(match.Index);
            int id = -1;
            string idStr = fishname.Substring(4, 3);
            int.TryParse(idStr, out id);
            //Debug.Log($"CalcAssetBundleName {path} {id} {idStr}");
            if (id >= 331 && id <= 342)
            {
                depth = 2;
            }
            else if (id > 300)
            {
                depth = 3;
            }


        }
        foreach (var key in m_Config.depth_define)
        {
            if (path.Contains(key.Key))
            {
                depth = key.Value;
                break;
            }
        }

        var array = path.Split('/');
        abName = string.Join("_", array, 0, depth + 1).ToLower();

        if (Regex.IsMatch(abName, @"roomres\w*_fish[0-9]{3}"))
        {
            Match match = Regex.Match(abName, "fish[0-9]{3}");
            abName = abName.Substring(match.Index);
            //Debug.Log(abName);

        }
        // 是否时合并成一个ab包
        if (m_CombineAssetBundleMap.ContainsKey(abName))
        {
            abName = m_CombineAssetBundleMap[abName];
        }
        return abName + ".unity3d";
    }
}
