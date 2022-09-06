using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
public class HostPlatformThird : HostPlatformBase
{
    private string apiHelperClass = "com.weile.api.ApiHelper";
    private string statHelperClass = "com.weile.api.StatHelper";
    private AndroidJavaClass apiHelper = null;
    private AndroidJavaClass statHelper = null;

    public HostPlatformThird()
    {
        if (apiHelper == null)
        {
            apiHelper = new AndroidJavaClass(apiHelperClass);
        }

        if (statHelper == null)
        {
            statHelper = new AndroidJavaClass(statHelperClass);
        }
    }
}
#endif
