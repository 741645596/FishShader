using System;
using System.Collections.Generic;
using UnityEngine;

public class FishLogInfo
{
    public DateTime LogTime;
    public LogType LogType;
    public string LogMessage;
    public string StackTrack;

    public FishLogInfo(LogType logType, string logMessage, string stackTrack)
    {
        LogTime = DateTime.Now;
        LogType = logType;
        LogMessage = logMessage;
        StackTrack = stackTrack;
    }
}
