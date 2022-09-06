using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

public static class AssetsMgr
{
    public readonly static string ABNameSC = "";
    static ABManager assetBundleManger = null;
    static Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();
    static string s_CachedPath;
    static string s_PackagePath;
    public static string PACKAGE_PATH { get { return s_PackagePath; } }

    public static bool NeedUnpackAssets()
    {
        return false;
//#if UNITY_ANDROID && !UNITY_EDITOR
//        return true;
//#else
//        return false;
//#endif
    }

    static string ReadBBH()
    {
        string ret = "bbh";
        // todo 热更资源目录去掉版本号目录，不然每次大版本覆盖安装都需要下载重复的资源
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    string text = ReadStringFromStreamingAssets("GameConfig.unity3d");
        //    var data = JsonMapper.ToObject(text);
        //    ret = data.GetString("version");
        //}
        //else
        //{
        //    var textAsset = Resources.Load<TextAsset>("GameConfig");
        //    if (textAsset != null)
        //    {
        //        var data = JsonMapper.ToObject(textAsset.text);
        //        ret = data.GetString("version");
        //    }
        //}
        return ret;
    }

    // 从streaming Assets目录下读取文本
    public static string ReadStringFromStreamingAssets(string filename)
    {
        var pathUri = new Uri($"{Application.streamingAssetsPath}/{filename}");
        UnityWebRequest webRequest = UnityWebRequest.Get(pathUri.AbsoluteUri);
        webRequest.SendWebRequest();
        while (!webRequest.isDone) { }
        return webRequest.downloadHandler.text;
    }

    // 从streaming Assets目录下读取二进制
    public static byte[] ReadBytesFromStreamingAssets(string filename)
    {
        var pathUri = new Uri($"{Application.streamingAssetsPath}/{filename}");
        UnityWebRequest webRequest = UnityWebRequest.Get(pathUri.AbsoluteUri);
        webRequest.SendWebRequest();
        while (!webRequest.isDone) { }
        return webRequest.downloadHandler.data;
    }

    public static void Reset()
    {
        string bbh = ReadBBH();
        LogUtils.I("ZYBBH: " + bbh);
        cache.Clear();
        assetBundleManger = null;
        s_CachedPath = string.IsNullOrEmpty(bbh) ? Application.persistentDataPath + "/" : Application.persistentDataPath + "/" + bbh;
        if (NeedUnpackAssets())
        {
            s_PackagePath = Application.persistentDataPath + "/pack";
        }
        else
        {
            s_PackagePath = Application.streamingAssetsPath;
        }
        LogUtils.I("包体资源路径:" + s_PackagePath);
        LogUtils.I("缓存资源路径:" + s_CachedPath);
    }

    public static IEnumerator InitABManager(bool useAssetBundle)
    {
        if (useAssetBundle)
        {
            LogUtils.I("Use AssetBundle");
            assetBundleManger = new ABManager();
            yield return assetBundleManger.LoadABConfig();
        }
    }

    public static void AddResCache(string name, UnityEngine.Object obj)
    {
        if (obj == null) return;
        cache[name] = obj;
    }

    public static string FullPath(string path)
    {
        path = "Assets/GameData/" + path;
        return path;
    }

    public static IEnumerator CleanupAssets()
    {
        cache.Clear();
        yield return Resources.UnloadUnusedAssets();
    }

    public static void UnloadAll()
    {
        cache.Clear();
        if (assetBundleManger != null)
        {
            assetBundleManger.UnloadAll();
        }
    }

    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        T ret = null;
        if (cache.ContainsKey(path))
        {
            ret = cache[path] as T;
            return ret;
        }
        if (assetBundleManger != null)
        {
            return assetBundleManger.Load<T>(path);
        }
        else
        {
#if UNITY_EDITOR
            ret = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (ret == null)
            {
                LogUtils.E("AssetsMgr", $"Load Res Error {path}");
            }
            return ret;
#else
            return null;
#endif
        }
    }

    public static void LoadAsync<T>(string path, Action<T> cb) where T : UnityEngine.Object
    {
        if (cache.ContainsKey(path))
        {
            T ret = cache[path] as T;
            cb?.Invoke(ret);
            return;
        }
        if (assetBundleManger != null)
        {
            assetBundleManger.LoadAsync<T>(path, cb);
        }
        else
        {
#if UNITY_EDITOR
            T ret = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            cb?.Invoke(ret);
#else
            return;
#endif
        }
    }

    public static IEnumerator LoadDll(string path, Action<MemoryStream>callBack)
    {
        var pathUri = new Uri(path);
        UnityWebRequest webRequest = UnityWebRequest.Get(pathUri.AbsoluteUri);
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.responseCode != 200)
        {
            LogUtils.E("LoadFile Error:" + path);
            callBack(null);
            yield break;
        }
        byte[] bytes = webRequest.downloadHandler.data;
        if (bytes != null)
        {
            var stream = new MemoryStream(bytes);
            callBack(stream);
        }
        webRequest.Dispose();
        yield return null;
    }

    public static string GetLastestPath(string res, bool needEncpty = true)
    {
        // 加密
        if (needEncpty)
        {
            res = EncptyABName(res);
        }
        //LogUtils.I("GetLastestPath:" + res);
        var p = Path.Combine(s_CachedPath, res);
        if (!File.Exists(p))
        {
            p = Path.Combine(s_PackagePath, res);
        }
        return p;
    }

    public static string GetABCachedPath()
    {
        return s_CachedPath + "/";
    }

    public static string EncptyABName(string abName)
    {
        if (!string.IsNullOrEmpty(ABNameSC))
        {
            abName = $"{MD5Utils.GetMD5(abName + ABNameSC)}.unity3d";
        }
        return abName;
    }

    public static List<string> GetAllAssetBundleName()
    {
        if (assetBundleManger != null)
        {
            return assetBundleManger.GetAllAssetBundleName();
        }
        return null;
    }
}
