using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using UnityEngine.UI;

// 解压缩资源
class UnpackAsset
{
    ABMD5List m_ABMD5List = new ABMD5List(); //安装包内存中的资源列表
    ABMD5List m_CurABMD5List = null;        //SD卡中的资源列表
    Dictionary<string, string> m_CurABMD5Dict = null;   //SD卡中资源列表的Md5
    Text m_LaunchTips;
    GameObject m_LaunchProgress;
    RectTransform m_ImgProgress;
    float m_ProgressWidth;

    string ErrorMsg = "";

    string m_PackageDir;
    List<ABMD5Base> m_UnpackList;

    public IEnumerator UnPackFiles()
    {
        ErrorMsg = "";
        m_UnpackList = new List<ABMD5Base>();
        m_PackageDir = AssetsMgr.PACKAGE_PATH;
        ReadCurMD5List();
        yield return ReadMD5List();
        int totalCount = ComputeUnPackFile();
        if (totalCount > 0)
        {
            InitLaunchUI();
            float t1 = Time.time;
            UpdateTips("资源解压中..");
            yield return null;
            UpdateProgress(0);
            int runningCount = 0;
            int count = m_UnpackList.Count;
            for (int i = 0; i < count; i++)
            {
                runningCount++;
                if (runningCount >= 5)
                {
                    runningCount = 0;
                    yield return null;
                    UpdateProgress((float)i / (float)count);
                }
                var info = m_UnpackList[i];
                LogUtils.I($"开始解压文件 {info.Name}");
                UnPackToPersistentDataPath(info.Name);
                LogUtils.I($"解压文件成功 {info.Name}");
                if (GetErrorMessage() != "")
                {
                    break;
                }
            }
            if (GetErrorMessage() == "")
            {
                LogUtils.I($"解压耗时 {Time.time - t1}");
                UpdateTips("解压资源完成");
                UpdateProgress(1);
                yield return new WaitForSeconds(0.1f);
                UpdateProgress(-1);
            }
        }
    }

    void InitLaunchUI()
    {
        var objTips = GameObject.Find("Canvas/LaunchBG/l_tips");
        if (objTips != null)
            m_LaunchTips = objTips.GetComponent<Text>();

        m_LaunchProgress = GameObject.Find("Canvas/LaunchBG/l_progress");
        if (m_LaunchProgress != null)
            m_ProgressWidth = m_LaunchProgress.GetComponent<RectTransform>().rect.width - 10;
        var imgPro = GameObject.Find("Canvas/LaunchBG/l_progress/img_pro");
        if (imgPro != null)
            m_ImgProgress = imgPro.GetComponent<RectTransform>();
    }

    void UpdateTips(string sTips)
    {
        if (m_LaunchTips != null)
            m_LaunchTips.text = sTips;
    }

    void UpdateProgress(float fPercent)
    {
        if (m_LaunchProgress != null)
            m_LaunchProgress.SetActive(fPercent >= 0);
        if (fPercent < 0) return;

        if (m_ImgProgress != null)
        {
            m_ImgProgress.sizeDelta = new Vector2(m_ProgressWidth * fPercent, m_ImgProgress.sizeDelta.y);
        }
    }

    void ReadCurMD5List()
    {
        m_CurABMD5Dict = new Dictionary<string, string>();
        var path = $"{m_PackageDir}/project.unity3d";
        if (File.Exists(path))
        {
            var context = File.ReadAllText(path);
            m_CurABMD5List = JsonMapper.ToObject<ABMD5List>(context);
            for (int i = 0; i < m_CurABMD5List.assets.Length; i++)
            {
                var asset = m_CurABMD5List.assets[i];
                m_CurABMD5Dict[asset.Name] = asset.Md5;
            }
        }
    }

    IEnumerator ReadMD5List()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/project.unity3d");
        webRequest.timeout = 30;
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.responseCode != 200)
        {
            Debug.LogError($"ReadMD5List Error:{webRequest.error}");
        }
        else
        {
            m_ABMD5List = JsonMapper.ToObject<ABMD5List>(webRequest.downloadHandler.text);
        }
        webRequest.Dispose();
        yield return null;
    }

    public int ComputeUnPackFile()
    {
        if (m_CurABMD5List != null && m_CurABMD5List.version < m_ABMD5List.version)
        {
            UpdateTips("正在校验资源请稍后...");
            var info = new DirectoryInfo(Application.persistentDataPath);
            if (info.Exists)
            {
                info.Delete(true);
                LogUtils.I("新安装包首次启动，清理缓存路径");
            }
        }

        LogUtils.I("persistentDataPath:" + m_PackageDir);
        if (!Directory.Exists(m_PackageDir))
        {
            Directory.CreateDirectory(m_PackageDir);
        }
        foreach (var asset in m_ABMD5List.assets)
        {
            string filePath = $"{m_PackageDir}/{asset.Name}";
            // SD卡中已存在文件，且Md5一致就不处理
            if (File.Exists(filePath) && m_CurABMD5Dict.ContainsKey(asset.Name) && m_CurABMD5Dict[asset.Name] == asset.Md5)
            {
                continue;
            }
            m_UnpackList.Add(asset);
        }
        // 版本对比文件需要额外添加
        if (m_UnpackList.Count > 0)
        {
            var unit = new ABMD5Base();
            unit.Name = "project.unity3d";
            m_UnpackList.Add(unit);

            unit = new ABMD5Base();
            unit.Name = "GameConfig.unity3d";
            m_UnpackList.Add(unit);
        }
        return m_UnpackList.Count;
    }

    public void UnPackToPersistentDataPath(string fileName)
    {
        var pathUri = new Uri($"{Application.streamingAssetsPath}/{fileName}");
        UnityWebRequest webRequest = UnityWebRequest.Get(pathUri.AbsoluteUri);
        webRequest.SendWebRequest();
        while (!webRequest.isDone) { }
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.responseCode != 200)
        {
            ErrorMsg = $"UnPack {pathUri.AbsoluteUri} Error {webRequest.error}";
            Debug.LogError(ErrorMsg);
        }
        else
        {
            byte[] bytes = webRequest.downloadHandler.data;
            CreateFile($"{m_PackageDir}/{fileName}", bytes);
        }
        webRequest.Dispose();
    }

    public bool CreateFile(string filePath, byte[] bytes)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string currPath = Path.GetDirectoryName(filePath);

            if (Directory.Exists(currPath) == false)
            {
                Directory.CreateDirectory(currPath);
            }

            FileInfo file = new FileInfo(filePath);

            Stream stream = file.Create();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    public string GetErrorMessage()
    {
        return ErrorMsg;
    }
}