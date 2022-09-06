using System;
using System.Runtime.InteropServices;
using UnityEngine;
using LitJson;
using Utils;

public class HostPlatformIos : HostPlatformBase
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern string getDeviceCode();

    [DllImport("__Internal")]
    public static extern bool isWeInstalled();

    [DllImport("__Internal")]
    public static extern void initRX(string jsonStr);

    [DllImport("__Internal")]
    public static extern void exitApp();

    [DllImport("__Internal")]
    public static extern void registUser(int type, string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void getCodeWithPhone(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void getAas(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void certification(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void loginRX(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void appzhifu(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void bindPhone(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void unbindPhone(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void resetPwd(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void changePwd(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void getLimit(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void getBuryInPoint(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void share(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void systemShare(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void shareReport(string jsonStr, string callBack);

    [DllImport("__Internal")]
    public static extern void copyToClipboard(string str);

    [DllImport("__Internal")]
    public static extern string getPasteboardString();

    [DllImport("__Internal")]
    public static extern void setMOPushAlias(string alias);

    [DllImport("__Internal")]
    public static extern void removeMOPushAlias();

    [DllImport("__Internal")]
    public static extern void bindingAlias(string alias);

    [DllImport("__Internal")]
    public static extern void reliveBindAlias();

    [DllImport("__Internal")]
    public static extern void iWatchInit();

    [DllImport("__Internal")]
    public static extern bool iWatchSupported();

    [DllImport("__Internal")]
    public static extern void iWatchUpdateContext(string key, string value);

    [DllImport("__Internal")]
    public static extern void iWatchShowContextChange();

    [DllImport("__Internal")]
    public static extern void UserComment(string appid);

    [DllImport("__Internal")]
    public static extern bool IsOpenNotification(); // 是否开启推送通知

    [DllImport("__Internal")]
    public static extern void JumpToNotificationSetting(); // 跳转到通知设置
#else
    //=====================================================================
    public static string getDeviceCode() {
        return "";
    }

    public static bool isWeInstalled() {return false;}
    public static void initRX(string jsonStr){}
    public static void exitApp(){}
    public static void registUser(int type, string jsonStr, string callBack){}
    public static void getCodeWithPhone(string jsonStr, string callBack){}
    public static void getAas(string jsonStr, string callBack){}
    public static void certification(string jsonStr, string callBack){}
    public static void loginRX(string jsonStr, string callBack){}
    public static void appzhifu(string jsonStr, string callBack){}
    public static void bindPhone(string jsonStr, string callBack){}
    public static void unbindPhone(string jsonStr, string callBack){}
    public static void resetPwd(string jsonStr, string callBack){}
    public static void changePwd(string jsonStr, string callBack){}
    public static void getLimit(string jsonStr, string callBack){}
    public static void getBuryInPoint(string jsonStr, string callBack){}
    public static void share(string jsonStr, string callBack){}
    public static void systemShare(string jsonStr, string callBack){}
    public static void shareReport(string jsonStr, string callBack){}
    public static void copyToClipboard(string str){}

    public static string getPasteboardString() {return "";}
    public static void setMOPushAlias(string alias){}
    public static void removeMOPushAlias(){}
    public static void bindingAlias(string alias){}
    public static void reliveBindAlias(){}
    public static void iWatchInit(){}
    public static bool iWatchSupported(){return false;}
    public static void iWatchUpdateContext(string key, string value){}
    public static void iWatchShowContextChange(){}
    public static void UserComment(string appid){}
    public static bool IsOpenNotification() { return false; } // 是否开启推送通知
    public static void JumpToNotificationSetting() { } // 跳转到通知设置
#endif

}
