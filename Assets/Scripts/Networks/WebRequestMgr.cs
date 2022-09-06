using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestMgr : MonoBehaviour
{
    static WebRequestMgr instance;
    public static WebRequestMgr Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("WebRequestMgr");
                //DontDestroyOnLoad(gameObject);
                instance = gameObject.AddComponent<WebRequestMgr>();
            }
            return instance;
        }
    }

    /// <summary>
    /// GET请求
    /// </summary>
    public void Get(string url, Dictionary<string, string> headers, Action<UnityWebRequest> action, int timeout = 5)
    {
        LogUtils.I($"=====get:{url}");
        StartCoroutine(doGet(url, headers, action, timeout));
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    public void Post(string serverURL, Dictionary<string, string> headers,
        string postData, Action<UnityWebRequest> action, int timeout)
    {
        LogUtils.I($"=====post:{serverURL},data = {postData}");
        StartCoroutine(doPost(serverURL, headers, postData, action, timeout));
    }
    /// <summary>
    /// 下载文件（支持断点续传）
    /// 一般下载大文件才用这个接口。比如说整包更新
    /// </summary>
    /// <param name="filePath">储存文件的路径+文件名
    public void GetFile(string url, string filePath, Action<bool> callBack, Action<float> progress = null)
    {
        StartCoroutine(doGetStackFile(url, filePath, callBack, progress));
    }
    public void GetFile(string url, string filePath, Action<UnityWebRequest> action)
    {
        StartCoroutine(doGetFile(url, filePath, action));
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    public void GetTexture(string url, Action<Texture2D> action, int playerId = 0)
    {
        StartCoroutine(doGetTexture(url, action, playerId));
    }
    /// <summary>
    /// 上传数据
    /// </summary>
    public void Upload(string url, byte[] data, Action<bool> action)
    {
        StartCoroutine(doUpload(url, data, action));
    }

    //=========================================================================
    /// <summary>
    /// GET请求
    /// </summary>
    IEnumerator doGet(string url, Dictionary<string, string> headers, Action<UnityWebRequest> actionResult, int timeout)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = timeout;
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
        yield return request.SendWebRequest();
        try
        {
            actionResult?.Invoke(request);
        }
        catch (Exception e)
        {
            Debug.LogError("WebRequestMgr Get Error: ");
            throw e;
        }
        request.Dispose();
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    IEnumerator doPost(string serverURL, Dictionary<string, string> headers, string postData, Action<UnityWebRequest> action, int timeout = 5)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        UnityWebRequest request = new UnityWebRequest(serverURL, UnityWebRequest.kHttpVerbPOST)
        {
            timeout = timeout,
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        yield return request.SendWebRequest();
        try
        {
            action?.Invoke(request);
        }
        catch (Exception e)
        {
            Debug.LogError("WebRequestMgr Post Error: ");
            throw e;
        }
        request.Dispose();
    }
    /// <summary>
    /// 下载文件（支持断点续传）
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="filePath">储存文件的路径和文件名 like 'Application.persistentDataPath+"/unity3d.html"'</param>
    /// <param name="succCallBack">下载完成的回调</param>
    /// <param name="proCallBack">下载进度的回调</param>
    /// <returns></returns>
    IEnumerator doGetStackFile(string url, string filePath, Action<bool> callBack, Action<float> progress = null)
    {
        float percent = 0;
        UnityWebRequest headRequest = UnityWebRequest.Head(url);
        yield return headRequest.SendWebRequest();
        if (headRequest.result == UnityWebRequest.Result.ConnectionError 
            || headRequest.result == UnityWebRequest.Result.ProtocolError
            || headRequest.responseCode != 200)
        {
            LogUtils.I("Start Down error:" + headRequest.result + " code:" + headRequest.responseCode);
            callBack?.Invoke(false);
        }
        else
        {
            string dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            // 计算已下载的长度
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            long fileLength = fs.Length;
            fs.Close();
            fs.Dispose();

            long totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));
            if (fileLength < totalLength)
            {
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.downloadHandler = new DownloadHandlerFile(filePath, true);
                request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                request.SendWebRequest();
                while (true)
                {
                    percent = ((long)request.downloadedBytes + fileLength) / (float)totalLength;
                    progress?.Invoke(percent);

                    if (request.downloadProgress >= 1.0f)
                    {
                        percent = 1.0f;
                        break;
                    }
                    yield return null;
                }
                request.Dispose();
            }
            else
            {
                percent = 1f;
            }
            if (percent >= 1f)
            {
                callBack?.Invoke(true);
            }
        }
        headRequest.Dispose();
    }
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="filePath">储存文件的路径+文件名
    /// <param name="action">请求发起后处理回调结果的委托,处理请求对象</param>
    /// <returns></returns>
    IEnumerator doGetFile(string url, string filePath, Action<UnityWebRequest> action)
    {
        var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        request.downloadHandler = new DownloadHandlerFile(filePath);
        yield return request.SendWebRequest();
        action?.Invoke(request);
        request.Dispose();
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    IEnumerator doGetTexture(string url, Action<Texture2D> action, int playerId)
    {
        var index = url.LastIndexOf(".");
        if (index < 0)
        {
            LogUtils.W("head url extension error");
            action(null);
            yield break;
        }
        var sExt = url.Remove(0, index);
        var fileName = $"r_{playerId}_{MD5Utils.GetMD5(url)}{sExt}";
        var path = $"{Application.persistentDataPath}/remote/{fileName}";
        if (!File.Exists(path))
        {
            var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            req.downloadHandler = new DownloadHandlerFile(path);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                action?.Invoke(null);
                req.Dispose();
                yield break;
            }
            req.Dispose();
            LogUtils.I("get remote texture success url:" + url);
        }
        var pathUri = new Uri(path);
        url = pathUri.AbsoluteUri;
        LogUtils.I("get texture from cache:" + url);
        UnityWebRequest request = new UnityWebRequest(url);
        DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(true);
        request.downloadHandler = downloadTexture;
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            action?.Invoke(null);
            request.Dispose();
            yield break;
        }
        action?.Invoke(downloadTexture.texture);
        request.Dispose();
    }

    /// <summary>
    /// 上传数据
    /// </summary>
    IEnumerator doUpload(string url, byte[] data, Action<bool> action, string contentType="application/octet-stream")
    {
        UnityWebRequest request = new UnityWebRequest(url);
        UploadHandler handler = new UploadHandlerRaw(data);
        handler.contentType = contentType;
        request.uploadHandler = handler;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            action?.Invoke(false);
            yield return null;
        }
        action?.Invoke(true);
        request.Dispose();
    }

    public void XZRoomRes(string url, string path, Action<bool> finish, Action<long> progress)
    {
        StartCoroutine(XZRoomResImpl(url, path, finish, progress));
    }

    IEnumerator XZRoomResImpl(string url, string path, Action<bool> finish, Action<long> progress)
    {
        var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        DownloadHandlerBuffer downloadHandlerFile = new DownloadHandlerBuffer();
        uwr.timeout = 300;
        uwr.method = "GET";
        uwr.downloadHandler = downloadHandlerFile;
        UnityWebRequestAsyncOperation request = uwr.SendWebRequest();
        ulong offset = 0;
        int lowSpeedCount = 0;
        ulong last = 0;
        bool bAbort = false;
        while (!request.isDone)
        {
            ulong cur = uwr.downloadedBytes;
            offset = cur - last;
            last = cur;
            // 卡在0字节
            if (offset <= 0)
            {
                lowSpeedCount++;
                if (lowSpeedCount > 180)
                {
                    bAbort = true;
                    uwr.Abort();
                    yield return new WaitForSeconds(0.5f);
                    break;
                }
            }
            else
            {
                lowSpeedCount = 0;
            }
            progress?.Invoke((long)uwr.downloadedBytes);
            yield return null;
        }
        yield return null;
        if (bAbort)
        {
            finish?.Invoke(false);
            yield break;
        }
        else if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            finish?.Invoke(false);
            yield break;
        }
        byte[] bytes = uwr.downloadHandler.data;
        if (!SaveFile(path, bytes))
        {
            finish?.Invoke(false);
            yield break;
        }
        finish?.Invoke(true);
    }

    bool SaveFile(string filePath, byte[] bytes)
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

            File.WriteAllBytes(filePath, bytes);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }

        return true;
    }
}
