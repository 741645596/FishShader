
using UnityEngine;

public class HostPlatformHelper
{
    public static HostPlatformBase platform = null;

    public static void InitPlatform()
    {
#if UNITY_EDITOR
        platform = new HostPlatformEditor();
#elif UNITY_ANDROID
        platform = new HostPlatformThird();
#elif UNITY_IOS
        platform = new HostPlatformIos();
#endif
    }

    public static void InitBuglyAgent()
    {
        LogUtils.I("InitBuglyAgent");
        /*
        BuglyAgent.ConfigDebugMode(false);

#if UNITY_IPHONE || UNITY_IOS
        BuglyAgent.InitWithAppId ("4d59dc8f01");
#elif UNITY_ANDROID
        BuglyAgent.InitWithAppId("cd4a0191b3");
#endif

        BuglyAgent.EnableExceptionHandler();
        */
    }

    // ios 第几代
    public static int GetGeneration()
    {
#if UNITY_IOS || UNITY_IPHONE
        return (int)UnityEngine.iOS.Device.generation;
#else
        return 0;
#endif
    }
}
