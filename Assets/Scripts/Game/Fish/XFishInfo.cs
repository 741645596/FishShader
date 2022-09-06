using System.Collections.Generic;
using UnityEngine;


public enum FishFlipType
{
    None = 0,
    FlipX = 1,
    CenterFlip = 2, //中心位置
    Forward = 3,
}

public class XFishInfo : MonoBehaviour
{
    // 阴影
    public float Distance = 1;
    public float Scale = 1f;
    public Vector3 LightDir = new Vector3(-3.53f, 6.89f, 0.388f);
    GameObject LightDirObj;
    Vector4 _shadowPlane;
    Vector4 _shadowFadeParams;

    // 倾斜角
    public float YRotate = 0;
    public float ZRotate = 0;
    public int RouteAngle = XRouteUtils.DEFAULT_ANGLE;

    // 锁定点信息
    public Transform[] LockBones;

    List<Transform> m_LockBones;
    int m_Index;

    public FishFlipType FlipType; // 是否左右翻转
    public bool UseXCameraRayCast;
    public bool UseIdleR; // 使用idle的镜像动作

    private List<Material> mMatList;

    static int _ShadowProjDir = Shader.PropertyToID("_ShadowProjDir");
    static int _Scale = Shader.PropertyToID("_Scale");

    XCameraRaycast m_XCameraRaycast;

    [Range(0.5f, 1.5f)]
    public float SpeedMin = 1.0f;
    [Range(0.5f, 1.5f)]
    public float SpeedMax = 1.0f;
    public float Velocity = 0;


    public void InitShadow(List<Material> shadowMat)
    {
        mMatList = shadowMat;
        Vector4 forward = CameraUtils.GetShadowForward();
        for (int i = 0; i < mMatList.Count; i++)
        {
            var mat = mMatList[i];
            forward.w = Distance;// + base.transform.position.z;
            mat.SetVector(_ShadowProjDir, forward);
            mat.SetFloat(_Scale, Scale);
        }
    }


    public void UpdateShadow()
    {
        Vector4 forward = CameraUtils.GetShadowForward();
        for (int i = 0; i < mMatList.Count; i++)
        {
            var mat = mMatList[i];
            forward.w = Distance;// + base.transform.position.z;
            mat.SetVector(_ShadowProjDir, forward);
            mat.SetFloat(_Scale, Scale);
        }
    }

    #region 锁定点

    public void InitLockPoint()
    {
        m_LockBones = new List<Transform>();
        for (int i = 0; i < LockBones.Length; i++)
        {
            if (LockBones[i] != null)
            {
                m_LockBones.Add(LockBones[i]);
            }
        }
        if (UseXCameraRayCast)
        {
            m_XCameraRaycast = GetComponentInChildren<XCameraRaycast>();
        }
    }

    Vector3 GetLockPoint(int idx)
    {
        if (m_XCameraRaycast != null)
        {
            return m_XCameraRaycast.GetWorldPoint(m_LockBones[idx].position);
        }
        else
        {
            return m_LockBones[idx].position;
        }
    }

    public Vector3 GetLockPoint()
    {
        if (m_LockBones.Count > 0)
        {
            Vector3 pos = Vector3.zero;
            pos = GetLockPoint(0);
            if (IsPointActive(0) && CameraUtils.IsWorldPointInScreen(pos))
            {
                m_Index = 0;
                return pos;
            }
            pos = GetLockPoint(m_Index);
            if (IsPointActive(m_Index) && CameraUtils.IsWorldPointInScreen(pos))
            {
                return pos;
            }

            for (int i = 0; i < m_LockBones.Count; i++)
            {
                pos = GetLockPoint(i); 
                if (IsPointActive(i) && CameraUtils.IsWorldPointInScreen(pos))
                {
                    m_Index = i;
                    break;
                }
            }
            return pos;
        }
        else
        {
            return transform.position;
        }
    }

    public bool IsPointActive(int idx)
    {
        if(m_LockBones[idx] && m_LockBones[idx].gameObject.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    public bool IsInScreen()
    {
        if (m_LockBones.Count > 0)
        {
            Vector3 pos = Vector3.zero;
            pos = GetLockPoint(0);
            if (CameraUtils.IsWorldPointInScreen(pos))
            {
                m_Index = 0;
                return true;
            }
            pos = GetLockPoint(m_Index);
            if (CameraUtils.IsWorldPointInScreen(pos))
            {
                return true;
            }
            bool ret = false;
            for (int i = 0; i < m_LockBones.Count; i++)
            {
                if (CameraUtils.IsWorldPointInScreen(GetLockPoint(i)))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        else
        {
            return CameraUtils.IsWorldPointInScreen(transform.position);
        }
    }

    public List<Vector3> GetAllLockPoint()
    {
        List<Vector3> list = new List<Vector3>();
        if (m_LockBones.Count > 0)
        {
            for (int i = 0; i < m_LockBones.Count; i++)
            {
                list.Add(GetLockPoint(i));
            }
        }
        else
        {
            list.Add(transform.position);
        }
        return list;
    }
#endregion
}
