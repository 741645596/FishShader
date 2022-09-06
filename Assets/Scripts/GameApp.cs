using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Utils;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class GameApp : MonoBehaviour
{
    static GameApp sInstance;

    public static GameApp Instance
    {
        get
        {
            return sInstance;
        }
    }

    // 主工程代码版本
    public static int GetGameAppCreateTime()
    {
        return 1;
    }

    static bool m_bRestart = false;
    public static bool IsRestart()
    {
        return m_bRestart;
    }
    public static void SetIsRestart(bool bRestart)
    {
        m_bRestart = bRestart;
    }

    public int TargetFrameRate = 30;
    GameObject m_LaunchUI;
    bool m_bStartUp = false;

    ScreenOrientation orientation;
    Action orientationChangeCallback;


#if UNITY_EDITOR
    bool m_ReloadGame;
    public string LocalServer = ""; //如:192.168.15.12:33070
    [HideInInspector]
    public LanguageType language = LanguageType.CHS;
    public bool UseAssetBundle = false;
#endif

    void Awake()
    {
        sInstance = this;
        Application.targetFrameRate = TargetFrameRate > 0 ? TargetFrameRate : 0;
        CanvasUtils.AdaptCanvas();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //添加sdk回调的Api类
        gameObject.AddComponent<PlatformApiHelper>();
        //关闭多点触控
        Input.multiTouchEnabled = false;
#if ENABLE_POCO
        gameObject.AddComponent<PocoManager>();
#endif

        HostPlatformHelper.InitPlatform();
#if UNITY_EDITOR
        HostPlatformHelper.platform.LocalServer = LocalServer;
        LanguageManager.Instance.SetLanguageType(language);
#endif
    }

    void Start()
    {
        LogUtils.I($"Fish Time {GetGameAppCreateTime()}");
        TestTimeline();
        InitShadwForward();
        orientation = Screen.orientation;
        StartCoroutine(Play());
    }


    void InitShadwForward()
    {
        var go = GameObject.Find("ShadowForward");
        if (go != null)
        {
            CameraUtils.SetShadowForwawrd(go.transform.forward);
#if !UNITY_EDITOR
            GameObject.Destroy(go);
#endif
        }
    }

    public void Restart()
    {
        CanvasUtils.RemoveAllView();
        StartCoroutine(StartGame());
    }

    IEnumerator Play()
    {
        CanvasUtils.Init();
        CameraUtils.AdaptAllCamera();
        yield return new WaitForSeconds(0.1f);
        CameraUtils.InitScale();
        yield return StartGame();
        m_bStartUp = true;
    }

    void UpdateEscape(KeyCode key)
    {
        if (Input.GetKeyDown(key)) {
            AppDomainUtil.InvokeInstanceMethod("Fishing.Startup", "OnEscape");
        }
    }

    IEnumerator StartGame()
    {
        LogUtils.V("内核数：" + SystemInfo.processorCount + " 频率：" + SystemInfo.processorFrequency + " 内存：" + SystemInfo.systemMemorySize);
        bool useAssetBundle = true;
#if UNITY_EDITOR
        useAssetBundle = UseAssetBundle;
#endif
        AssetsMgr.Reset();
        if (AssetsMgr.NeedUnpackAssets())
        {
            var p = new UnpackAsset();
            yield return p.UnPackFiles();
            if (p.GetErrorMessage() != "")
            {
                yield break;
            }
        }
        yield return AssetsMgr.InitABManager(useAssetBundle);
        yield return LoadFishing();
    }

    IEnumerator LoadFishing()
    {
        var dllPath = Path.Combine(Application.dataPath, "../GameLib/Fishing.dll");
        var pdbPath = Path.Combine(Application.dataPath, "../GameLib/Fishing.pdb");
#if !UNITY_EDITOR
        dllPath = AssetsMgr.GetLastestPath("Fishing.unity3d");
        pdbPath = "";
#endif
        yield return AppDomainUtil.load(dllPath, pdbPath);
        AppDomainUtil.InvokeInstanceMethod("Fishing.Startup", "Start");
    }

    public void ExitApp(string tips = "")
    {
        LogUtils.I("退出应用：" + tips);
        m_bStartUp = false;
        StopAllCoroutines();
        Application.Quit();
    }

    private void OnApplicationPause(bool bPause)
    {
        if (!m_bStartUp) return;
        AppDomainUtil.InvokeInstanceMethod("Fishing.Startup", "OnApplicationPause", bPause);
    }

    public void StartListenOrientationChange(Action cb)
    {
        CancelInvoke("UpdateListenOrientationChange");
        InvokeRepeating("UpdateListenOrientationChange", 0.7f, 0.7f);
        orientationChangeCallback = cb;
    }

    void UpdateListenOrientationChange()
    {
        if (orientation != Screen.orientation)
        {
            orientation = Screen.orientation;
            orientationChangeCallback?.Invoke();
        }
    }

#if UNITY_EDITOR
#region 编辑器模式下Reload DLL
    private void Update()
    {
        UpdateEditor();
    }

    void UpdateEditor()
    {
        UpdateEscape(KeyCode.Escape);
        UpdateReloadGame();
    }

    public void ReloadGame()
    {
        m_ReloadGame = true;
    }

    void UpdateReloadGame()
    {
        if (m_ReloadGame)
        {
            LogUtils.I("Reload Game");
            m_ReloadGame = false;

            AssetsMgr.UnloadAll();
            Resources.UnloadUnusedAssets();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Fishing", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    IEnumerator RestartGame()
    {
        AppDomainUtil.InvokeInstanceMethod("Fishing.Startup", "Reload");
        yield return null;
        CanvasUtils.RemoveAllView();
        yield return StartGame();
    }
#endregion
#endif

    void TestTimeline()
    {
        var com = gameObject.GetComponent<PlayableDirector>();
        LogUtils.W($"PlayableDirector {com != null}");
        if (com != null)
        {
            com.Play();
            com.Evaluate();
            com.GetGenericBinding(com);
            UnityEngine.Timeline.TimelineAsset a1 = null;
            a1.fixedDuration = 10;
            UnityEngine.Timeline.ControlPlayableAsset a2 = null;
            a2.active = true;
            UnityEngine.Timeline.ControlPlayableAsset a3 = null;
            a3.active = true;
            UnityEngine.Timeline.AudioPlayableAsset a4 = null;
            a4.loop = true;

            UnityEngine.Timeline.ControlTrack controlTrack = null;
            controlTrack.SetGroup(null);
            //UnityEngine.Timeline.ActivationPlayableAsset activationPlayableAsset = null;
            //Debug.Log(activationPlayableAsset.duration);

            com.Play(a1);
        }
    }

}
