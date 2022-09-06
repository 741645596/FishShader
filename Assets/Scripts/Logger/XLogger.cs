using System;
using System.Collections.Generic;
using UnityEngine;

public class XLogger : MonoBehaviour
{
    List<FishLogInfo> logInfoQueue = new List<FishLogInfo>();
    static XLogger  Instance = null;
    private void Start()
    {
        Instance = this;
        Application.logMessageReceived += OnLogMessageReceived;
    }

    public void OnDestory()
    {
        Instance = null;
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    public void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
    {
        if (logType == LogType.Assert)
        {
            logType = LogType.Error;
        }

        var node = new FishLogInfo(logType, logMessage, stackTrace);
        logInfoQueue.Insert(0, node);

        while (logInfoQueue.Count > 1000)
        {
            logInfoQueue.RemoveAt(logInfoQueue.Count - 1);
        }
    }

    public static List<FishLogInfo> GetQueue()
    {
        if (Instance != null)
        {
            return Instance.logInfoQueue;
        }
        return null;
    }
}
