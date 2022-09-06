using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;
using System.Text.RegularExpressions;

class LogAssetBundleDepency
{
    
    [MenuItem("Tools/分包/ab包的所有依赖关系")]
    public static void AllDepency()
    {
        DoIt(false);
    }

    [MenuItem("Tools/分包/ab包的直接依赖关系")]
    public static void DirectDepency()
    {
        DoIt(true);
    }

    static void DoIt(bool direct)
    {
        string outputFileName = direct ? "ABDirectDepency.txt" : "ABDepency.txt";
        var manifestPath = "Assets/StreamingAssets/ABFishing.unity3d";
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("请先打安卓ab包： " + manifestPath);
            return;
        }
        AssetBundle single = AssetBundle.LoadFromFile(manifestPath, 0, 48);
        if (single == null)
        {
            Debug.LogError("加载ab清单失败，请检查资源，目录为： " + manifestPath);
            return;
        }
        AssetBundleManifest singleManifest = single.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        single.Unload(false);
        if (singleManifest == null)
        {
            Debug.LogError("加载AssetBundleManifest失败，请检查是否存在资源清单，资源目录为： " + manifestPath);
            return;
        }
        List<string> list = new List<string>();
        var allAssetBundleArray = singleManifest.GetAllAssetBundles();
        for (int i = 0; i < allAssetBundleArray.Length; i++)
        {
            string shortName = allAssetBundleArray[i];
            string[] dependencies = singleManifest.GetAllDependencies(shortName);
            if (direct)
            {
                dependencies = singleManifest.GetDirectDependencies(shortName);
            }
            //gamedata_appres_avatar
            string refrenceStr = "";
            for (int j = 0; j < dependencies.Length; j++)
            {
                refrenceStr += "\n   " + dependencies[j];

            }
            string str = $"start {shortName} {dependencies.Length}{refrenceStr}\n";
            list.Add(str);
            Debug.Log(str);
        }

        string output = String.Join("\n", list.ToArray());
        File.WriteAllText(outputFileName, output);
        Debug.Log($"AB包的依赖关系{list.Count}个已输出到：{outputFileName}");
    }
}
