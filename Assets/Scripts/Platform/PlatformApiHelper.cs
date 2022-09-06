using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Utils;

public class PlatformApiHelper : MonoBehaviour
{
    private static PlatformApiHelper _instance;

    public static PlatformApiHelper Instance {
        get {
            if (_instance == null) {
                LogUtils.E("PlatformApiHelper实例尚未初始化");
            }

            return _instance;
        }
    }

    public PlatformApiHelper() {
        if (_instance != null) {
            LogUtils.E("PlatformApiHelper实例已经存在");
        }
        _instance = this;
        this.InitCallBackDic();
    }

    //账号注册的回调
    public void onRegisterUserResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnRegisterUserResult", result);
    }
    //获取验证码的回调
    public void onGetCodeWithPhoneResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGetCodeWithPhoneResult", result);
    }
    //登录回调
    public void onLoginResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnLoginResult", result);
    }

    //游客登陆的回调
    public void onLoginByVisitorResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnVisitorLoginResult", result);
    }
    //账号登陆的回调
    public void onLoginByAccountResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnAccountLoginResult", result);
    }
    //第三方登陆的回调
    public void onLoginByThirdPartResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnThirdPartyLoginResult", result);
    }
    //微信登陆的回调
    public void onLoginByWeiResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnWeiLoginResult", result);
    }
    //游客激活的回调
    public void onGuessActiveResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGuessActiveResult", result);
    }
    //登录状态变化
    //status: 0:未登陆、1：登陆中、2：已登陆 4：登出中;
    public void onLoginStatusChanged(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnLoginStatusChanged", result);
    }
    //防沉迷的回调
    public void onGetAasResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGetAasResult", result);
    }
    //实名认证的回调
    public void onCertificationResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnCertificationResult", result);
    }
    //绑定手机的回调
    public void onBindPhoneResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnBindPhoneResult", result);
    }
    // 解绑手机的回调
    public void onUnbindPhoneResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnUnbindPhoneResult", result);
    }
    //修改密码的回调
    public void onChangePwdResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnChangePwdResult", result);
    }
    //密码重置的回调
    public void onResetPwdResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnResetPwdResult", result);
    }
    //支付的回调
    public void onZhifuResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnZhifuResult", result);
    }
    //获取分享次数限制的回调
    public void onGetLimitResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGetLimitResult", result);
    }
    //获取埋点限制的回调
    public void onGetBuryInPointResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGetBuryInPointResult", result);
    }
    //分享的回调
    public void onShareResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnShareResult", result);
    }
    //系统分享的回调
    public void onSystemShareResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnSystemShareResult", result);
    }
    //上报分享结果的回调
    public void onShareReportResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnShareReportResult", result);
    }
    //剪切板
    public void onGetPasteboardString(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnGetPasteboardString", result);
    }
    //获取权限的回调
    public void onPermissionResult(string result)
    {
        AppDomainUtil.HandleCallBack("Fishing.Bridge", "OnPermissionResult", result);
    }

    private Dictionary<string, Action<string>> callbackDictionary;

    private void InitCallBackDic() {
        callbackDictionary = new Dictionary<string, Action<string>>();
    }

    public void SetCallback(string key, Action<string> action) {
        if (callbackDictionary.ContainsKey(key)) {
            if (callbackDictionary[key] != action) {
                LogUtils.W("SetCallback--存在相同的key--:" + key);
            }

            callbackDictionary[key] = action;
        }
        else {
            callbackDictionary.Add(key, action);
        }
    }

    public void CallBack(string resultStr) {//JNI 回调
        var jsonData = JsonMapper.ToObject(resultStr);
        string key = jsonData.GetString("key");
        string result = jsonData.GetString("rslt");

        LogUtils.I("PlatformApiHelper CallBack :" + resultStr);
        LogUtils.I("resultStr : " + "functionName: " + key + "--args:" + result);
        if (callbackDictionary.ContainsKey(key) && callbackDictionary[key] != null) {
            callbackDictionary[key](result);
            LogUtils.I("PlatformApiHelper:CallBack " + key);
        }
    }

    public void ReleaseCallback(string key) {
        if (!callbackDictionary.ContainsKey(key)) {
            return;
        }

        var callback = this.callbackDictionary[key];
        if (callback != null) {
            this.callbackDictionary[key] = null;
        }
    }
}
