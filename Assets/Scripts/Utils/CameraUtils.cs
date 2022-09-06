using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraUtils
{
    static Camera _MainCamera;
    static Camera _UICamera;
    static Camera _EffectCamera;
    static Camera _3DCamera;
    static Camera _TopCamera;
    static GameObject GlobalVolume;

    static RectTransform _CanvasRectTransform;
    static float _Scale = -1;

    static Rect _WorldScreenRect = new Rect(-6.4f, -3.6f, 12.8f, 7.2f);
    static Vector3 _3DScale = Vector3.one;

    static int _MainCameraCullingMask = -1;
    static int _UICameraCullingMask = -1;

    static Vector3 _ShadowFoward = Vector3.zero;

    // 适配摄像机(只针对正交摄像机)
    public static void AdaptAllCamera()
    {
        GlobalVolume = GameObject.Find("GlobalVolume");
        AdaptCamera(GetMainCamera());
        AdaptCamera(GetUICamera());
        AdaptCamera(GetEffectCamera());
        AdaptCamera(XCameraTopManager.GetCamera());
        _MainCameraCullingMask = GetMainCamera().cullingMask;
        _UICameraCullingMask = GetUICamera().cullingMask;
        LogUtils.V($"Main Camera Culling Mask {_MainCameraCullingMask}");
    }

    public static void InitScale()
    {
        if (GameObject.Find("Canvas"))
        {
            _Scale = GameObject.Find("Canvas").transform.localScale.x;
        }
        float s = 100 * _Scale;
        _3DScale = new Vector3(s, s, s);
        Vector3 size = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        _WorldScreenRect = new Rect(-size.x, -size.y, 2 * size.x, 2 * size.y);
        LogUtils.V("CameraUtils InitScale");
        LogUtils.V(_Scale);
        LogUtils.V(_3DScale);
        LogUtils.V(_WorldScreenRect);
    }

    // 适配摄像机(只针对正交摄像机)
    public static void AdaptCamera(Camera camera)
    {
        if (camera == null) return;
        float designRatio = 0.5633803f;
        float screenRatio = (float)Screen.height / (float)Screen.width;

        if (designRatio > screenRatio)
        {
            camera.orthographicSize = 3.2f;
        }
        else
        {
            camera.orthographicSize = screenRatio * 1136f / 200f;
        }
    }

    // 适配摄像机(只针对正交摄像机)
    static void AdaptCamera(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var camera = go.GetComponent<Camera>();
        AdaptCamera(camera);
    }

    public static void SetEnablePostProcessing(bool enable)
    {
        var camera = GetMainCamera();
        if (camera == null) return;
        var data = camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = enable;
        if (GlobalVolume != null)
        {
            GlobalVolume.SetActive(enable);
        }
    }

    public static Camera GetMainCamera()
    {
        if (_MainCamera == null)
        {
            if (GameObject.Find("MainCamera"))
            {
                _MainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
            }
        }
        return _MainCamera;
    }

    public static Camera GetEffectCamera()
    {
        if (_EffectCamera == null)
        {
            if (GameObject.Find("EffectCamera"))
            {
                _EffectCamera = GameObject.Find("EffectCamera").GetComponent<Camera>();
            }
        }
        return _EffectCamera;
    }

    public static Camera GetUICamera()
    {
        if (_UICamera == null)
        {
            if (GameObject.Find("UICamera"))
            {
                _UICamera = GameObject.Find("UICamera").GetComponent<Camera>();
            }
        }
        return _UICamera;
    }

    public static Camera Get3DCamera()
    {
        if (_3DCamera == null)
        {
            if (GameObject.Find("3DRoot/Fish3DCamera"))
            {
                _3DCamera = GameObject.Find("3DRoot/Fish3DCamera").GetComponent<Camera>();
            }
        }
        return _3DCamera;
    }

    public static Camera GetTopCamera()
    {
        if (_TopCamera == null)
        {
            if (GameObject.Find("TopRoot/TopCamera"))
            {
                _TopCamera = GameObject.Find("TopRoot/TopCamera").GetComponent<Camera>();
            }
        }
        return _TopCamera;
    }

    public static RectTransform GetRectTransform()
    {
        if (_CanvasRectTransform == null)
        {
            if (GameObject.Find("Canvas"))
            {
                _CanvasRectTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();
            }
        }
        return _CanvasRectTransform;
    }


    public static float GetScale()
    {
        return _Scale;
    }

    public static Vector3 Get3DScale()
    {
        return _3DScale;
    }

    // 屏幕坐标到ui坐标
    public static Vector2 ScreenPointToWorldPoint(Vector2 pos)
    {
        return _MainCamera.ScreenToWorldPoint(pos);
    }

    // 屏幕坐标到ui坐标
    public static Vector2 ScreenPointToUIPoint(Vector2 pos)
    {
        Vector2 uiPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetRectTransform(), pos, GetUICamera(), out uiPos);
        return uiPos;
    }

    // 世界坐标到ui坐标
    public static Vector2 WorldPointToScreenPoint(Vector3 worldPos)
    {
        return _MainCamera.WorldToScreenPoint(worldPos);
    }

    // 世界坐标到ui坐标
    public static Vector2 WorldPointToUIPoint(Vector3 worldPos)
    {
        worldPos = worldPos / GetScale();
        return worldPos;
    }

    // ui坐标到世界坐标
    public static Vector3 UIPointToWorldPoint(Vector2 uiPos)
    {
        uiPos = uiPos * GetScale();
        return uiPos;
    }

    public static bool IsWorldPointInScreen(Vector3 worldPos)
    {
        Vector2 p = new Vector2(worldPos.x, worldPos.y);
        return _WorldScreenRect.Contains(p);
    }

    public static Vector2 GetWorldSize()
    {
        return new Vector2(_WorldScreenRect.width, _WorldScreenRect.height);
    }

    public static void SetMainCameraVisible(bool bo)
    {
        if (bo)
        {
            GetMainCamera().cullingMask = _MainCameraCullingMask;
        }
        else
        {
            GetMainCamera().cullingMask = 0;
        }
    }

    public static void SetUICameraVisible(bool bo)
    {
        if (bo)
        {
            GetUICamera().cullingMask = _UICameraCullingMask;
        }
        else
        {
            GetUICamera().cullingMask = 0;
        }
    }

    public static Vector3 GetShadowForward()
    {
        return _ShadowFoward;
    }

    public static void SetShadowForwawrd(Vector3 vector3)
    {
        LogUtils.I($"SetShadowForwawrd {vector3}");
        _ShadowFoward = vector3;
    }

    public static float GetBloomThreshold()
    {
        var go = GameObject.Find("GlobalVolume");
        if (go != null)
        {
            var com = go.GetComponent<UnityEngine.Rendering.Volume>();
            UnityEngine.Rendering.Universal.Bloom bloom;
            com.sharedProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
            return bloom.threshold.value;
        }
        return 0f;
    }

    

    public static void SetBloomThreshold(float val)
    {
        var go = GameObject.Find("GlobalVolume");
        if (go != null)
        {
            var com = go.GetComponent<UnityEngine.Rendering.Volume>();
            UnityEngine.Rendering.Universal.Bloom bloom;
            com.sharedProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
            bloom.threshold.value = val;
        }
    }

    public static float GetBloomIntensity()
    {
        var go = GameObject.Find("GlobalVolume");
        if (go != null)
        {
            var com = go.GetComponent<UnityEngine.Rendering.Volume>();
            UnityEngine.Rendering.Universal.Bloom bloom;
            com.sharedProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
            return bloom.intensity.value;
        }
        return 0f;
    }

    public static void SetBloomIntensity(float val)
    {
        var go = GameObject.Find("GlobalVolume");
        if (go != null)
        {
            var com = go.GetComponent<UnityEngine.Rendering.Volume>();
            UnityEngine.Rendering.Universal.Bloom bloom;
            com.sharedProfile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out bloom);
            bloom.intensity.value = val;
        }
    }
}

