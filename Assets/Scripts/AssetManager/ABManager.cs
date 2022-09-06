using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

class ABManager
{
    class ABAssetConfig
    {
        public string ABName = "";
        public string AssetName = "";
    }

    private readonly Dictionary<string, AssetBundle> m_LoadedAssetBundles = new Dictionary<string, AssetBundle>();
    private AssetBundleManifest m_AssetBundleManifest = null;
    private Dictionary<string, string> m_PathToAssetBundleName = new Dictionary<string, string>();
    private bool cancel = false;
    private const int Head_Offset = 48;  // ab包头部偏移

    public ABManager()
    {
        LoadABManifest("ABFishing.unity3d");
    }

    /// <summary>
    /// 加载模块总依赖文件
    /// </summary>
    /// <param name="name">模块名称</param>
    public void LoadABManifest(string moduleName, bool isReload = false)
    {
        if (m_AssetBundleManifest != null && !isReload)
        {
            return;
        }

        var abPath = AssetsMgr.GetLastestPath(moduleName);
        AssetBundle single = AssetBundle.LoadFromFile(abPath, 0, Head_Offset);
        if (single == null)
        {
            LogUtils.W("加载ab清单失败，请检查资源，目录为： " + abPath);
            return;
        }

        m_AssetBundleManifest = single.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if (m_AssetBundleManifest == null)
        {
            LogUtils.W("加载AB失败，请检查是否存在资源清单，资源目录为： " + abPath);
            return;
        }
        single.Unload(false);
    }
    /// <summary>
    /// 加载ab包与资源的对应关系
    /// </summary>
    /// <param filePath="path">文件的路径</param>
    /// <returns></returns>
    public IEnumerator LoadABConfig()
    {
        m_PathToAssetBundleName = new Dictionary<string, string>();
        var path = AssetsMgr.GetLastestPath("ABConfig.unity3d");
        var text = "";
        var pathUri = new Uri(path);
        UnityWebRequest webRequest = UnityWebRequest.Get(pathUri.AbsoluteUri);
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.responseCode != 200)
        {
            LogUtils.E("LoadFile Error:" + path);
            yield return null;
        }
        text = webRequest.downloadHandler.text;
        webRequest.Dispose();
        if (!String.IsNullOrEmpty(text))
        {
            var cfgList = JsonMapper.ToObject<Dictionary<string, string>>(text);
            foreach (var asset in cfgList)
            {
                var filePath = $"assets/gamedata/{asset.Key}";
                m_PathToAssetBundleName[filePath.ToLower()] = $"{asset.Value.ToLower()}.unity3d";
            }
        }
        yield return null;
    }

    private string PathToAssetBundleName(string path)
    {
        path = path.ToLower();
        string dir = Path.GetDirectoryName(path);
        dir = dir.Replace("\\", "/");
        if (m_PathToAssetBundleName.ContainsKey(dir))
        {
            return m_PathToAssetBundleName[dir];
        }
        return "";
    }

    public List<string> GetAllAssetBundleName()
    {
        List<string> list = new List<string>();
        foreach (var key in m_PathToAssetBundleName)
        {
            list.Add(key.Value);
        }
        return list;
    }

    public T Load<T>(string filePath) where T : UnityEngine.Object
    {
        var assetBundleName = PathToAssetBundleName(filePath);
        if (string.IsNullOrEmpty(assetBundleName))
        {
            LogUtils.E("ABManager", "AssetBundleManager Load error can't find res:" + filePath);
            return null;
        }
        var assetPath = AssetsMgr.GetLastestPath(assetBundleName);
        AssetBundle asset = GetAssetBundle(assetBundleName, assetPath);
        if (asset == null)
        {
            LogUtils.E("ABManager", $"GetAssetBundle Error {assetBundleName}");
            return null;
        }
        T ret = asset.LoadAsset<T>(filePath);
        AssetsMgr.AddResCache(filePath, ret);
        if (ret == null)
        {
            LogUtils.E("ABManager", $"LoadAsset Error {assetBundleName}");
        }
        return ret;
    }

    /// 卸载ab
    public void Unload(string assetBundleName, bool unload = false)
    {
        if (m_LoadedAssetBundles.ContainsKey(assetBundleName))
        {
            m_LoadedAssetBundles[assetBundleName].Unload(unload);
            m_LoadedAssetBundles.Remove(assetBundleName);
        }
    }

    /// <summary>
    /// 卸载所以的ab包
    /// </summary>
    /// <param name="unload">是否卸载实例化的资源</param>
    public void UnloadAll(bool unload = false)
    {
        AssetBundle.UnloadAllAssetBundles(unload);
        m_LoadedAssetBundles.Clear();
    }

    /// <summary>
    /// 异步加载ab包
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">ab路径</param>
    /// <param name="key">ab内部路径</param>
    /// <param name="action">加载成功回调</param>
    public void LoadAsync<T>(string filePath, Action<T> action) where T : UnityEngine.Object
    {
        var assetBundleName = PathToAssetBundleName(filePath);
        if (string.IsNullOrEmpty(assetBundleName))
        {
            LogUtils.E("ABManager", "AssetBundleManager Load error can't find res:" + filePath);
            return;
        }
        var assetPath = AssetsMgr.GetLastestPath(assetBundleName);

        AssetBundle asset = GetAssetBundle(assetBundleName, assetPath);
        if (asset == null)
        {
            action?.Invoke(null);
            return;
        }

        cancel = false;
        var req = asset.LoadAssetAsync(filePath);
        req.completed += (o) =>
        {
            if (cancel)
            {
                return;
            }
            T obj = ((AssetBundleRequest)o).asset as T;
            AssetsMgr.AddResCache(filePath, obj);
            action?.Invoke(obj);
        };
    }

    /// <summary>
    /// 取消异步加载ab包
    /// </summary>
    public void CancelLoadAsync()
    {
        cancel = true;
    }

    private AssetBundle GetAssetBundle(string assetname, string path)
    {
        if (m_LoadedAssetBundles.ContainsKey(assetname))
        {
            return m_LoadedAssetBundles[assetname];
        }
        return LoadAssetbundle(assetname, path);
    }

    private AssetBundle LoadAssetbundle(string assetBundleName, string path)
    {
        LoadDependencies(assetBundleName);
        Unload(assetBundleName); // 确保asset首次加载
        //LogUtils.V("ABManager LoadAssetbundle", assetBundleName + " " + path);
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path, 0, Head_Offset);
        if (assetBundle != null)
        {
            m_LoadedAssetBundles.Add(assetBundleName, assetBundle);
        }
        return assetBundle;
    }

    /// <summary>
    /// 加载依赖资源
    /// </summary>
    /// <param name="assetname"></param>
    private void LoadDependencies(string assetBundleName)
    {
        List<string> list = new List<string>();
        var deps = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
        //循环加载依赖项
        for (int i = 0; i < deps.Length; i++)
        {
            string dependAssetBundleName = deps[i];
            if (!m_LoadedAssetBundles.ContainsKey(dependAssetBundleName))
            {
                string path = AssetsMgr.GetLastestPath(deps[i]);
                //LogUtils.V("ABManager LoadDependencies", dependAssetBundleName + " " + path);
                AssetBundle assetBundle = AssetBundle.LoadFromFile(path, 0, Head_Offset);
                if (assetBundle != null)
                {
                    m_LoadedAssetBundles.Add(dependAssetBundleName, assetBundle);
                }
            }
        }
    }
}

