using System;
using System.Collections.Generic;
using UnityEngine;

public static class XBulletUtils
{
    static Vector3 m_CameraDir = new Vector3(0, -1, 0);
    static Rect m_WorldRect = new Rect(-7.2f, -3.60f, 12.80f, 7.20f);
    static Rect m_UIRect = new Rect(-720, -360, 1280, 720);
    static RaycastHit[] m_HitInfos = new RaycastHit[48];

    static Dictionary<int, int> m_TmpFishUidMap = new Dictionary<int, int>();

    static List<XCameraRaycast> m_OtherCameraRaycast = new List<XCameraRaycast>();

    public static void Init()
    {
        LogUtils.V("XBulletUtils.Init");
        m_CameraDir = new Vector3(0, 0, 1);
        if (GameObject.Find("Canvas") != null)
        {
            RectTransform tf = GameObject.Find("Canvas").GetComponent<RectTransform>();
            float height = tf.sizeDelta.y + 10;
            float width = tf.sizeDelta.x + 10;
            m_UIRect = new Rect(-width / 2, -height / 2, width, height);
        }
        else
        {
            LogUtils.E("XBulletUtils Init Cann't Find Canvas");
        }
        LogUtils.V("Init UI Rect:");
        LogUtils.V(m_UIRect);
        if (Camera.main != null)
        {
            Vector3 size = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            size.x += 0.2f;
            size.y += 0.2f;
            m_WorldRect = new Rect(-size.x, -size.y, 2 * size.x, 2 * size.y);
        }
        else
        {
            LogUtils.E("XBulletUtils Init Cann't Find Main Camera");
        }
        LogUtils.V("Init World Rect:");
        LogUtils.V(m_WorldRect);
        m_OtherCameraRaycast = new List<XCameraRaycast>();
    }

    public static Rect GetUIRect()
    {
        return m_UIRect;
    }

    public static Rect GetWorldRect()
    {
        return m_WorldRect;
    }

    public static void AddCameraRaycast(XCameraRaycast cameraRaycast)
    {
        m_OtherCameraRaycast.Add(cameraRaycast);
    }

    public static void RemoveCameraRaycast(XCameraRaycast cameraRaycast)
    {
        m_OtherCameraRaycast.Remove(cameraRaycast);
    }

    public static bool BulletAndFish(Vector3 worldPos, float radius)
    {
        int hitCount = Physics.SphereCastNonAlloc(worldPos, radius, m_CameraDir, m_HitInfos);
        if (hitCount == 0)
        {
            int count = m_OtherCameraRaycast.Count;
            if (count > 0)
            {
                Vector2 screenPos = CameraUtils.WorldPointToScreenPoint(worldPos);
                for (int i = count - 1; i >= 0; i--)
                {
                    if (m_OtherCameraRaycast[i].GetColliderResult(screenPos, radius, m_HitInfos) > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    public static void GetCollisionFish(List<int> list, Vector3 worldPos, float radius)
    {
        worldPos.z = 0;
        int hitCount = Physics.SphereCastNonAlloc(worldPos, radius, m_CameraDir, m_HitInfos);
        if (hitCount > 0)
        {
            AddHitResultToList(list, hitCount);
        }
        int count = m_OtherCameraRaycast.Count;
        if (count > 0)
        {
            Vector2 screenPos = CameraUtils.WorldPointToScreenPoint(worldPos);
            for (int i = count - 1; i >= 0; i--)
            {
                hitCount = m_OtherCameraRaycast[i].GetColliderResult(screenPos, radius, m_HitInfos);
                AddHitResultToList(list, hitCount);
            }
        }
        if (list.Count > 0)
        {
            m_TmpFishUidMap.Clear();
        }
    }

    static void AddHitResultToList(List<int> list, int hitCount)
    {
        //LogUtils.V("XBulletUtils", count);
        for (int j = 0; j < hitCount; j++)
        {
            var info = m_HitInfos[j];
            int fishUid = XFishUtils.GetColliderFishUID(info.collider.GetInstanceID());
            if (fishUid > 0)
            {
                if (!m_TmpFishUidMap.ContainsKey(fishUid))
                {
                    list.Add(fishUid);
                    m_TmpFishUidMap[fishUid] = 1;
                }
            }
        }
    }
}
