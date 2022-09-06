using System;
using System.Collections.Generic;
using UnityEngine;

public class XCameraTopManager : MonoBehaviour
{
    static XCameraTopManager sInstance;
    Camera m_Camera;
    GameObject m_Root;

    public void Start()
    {
        sInstance = this;
        m_Camera = GetComponentInChildren<Camera>();
        CreateRoot();
    }

    public void Update()
    {
        if (m_Root.transform.childCount == 0)
        {
            gameObject.SetActive(false);
        }
    }

    void CreateRoot()
    {
        if (m_Root != null)
        {
            GameObject.Destroy(m_Root);
        }
        m_Root = new GameObject("Root");
        m_Root.transform.SetParent(sInstance.transform, false);
    }

    public static void Clear()
    {
        if (sInstance != null)
        {
            sInstance.CreateRoot();
        }
    }

    public static void AddGameObject(GameObject go)
    {
        if (sInstance != null)
        {
            GameObjectUtils.ChangeLayer(go, "Top");
            go.transform.SetParent(sInstance.m_Root.transform);
            sInstance.gameObject.SetActive(true);
        }
    }

    public static Camera GetCamera()
    {
        if (sInstance != null)
        {
            return sInstance.m_Camera;
        }
        return null;
    }
}
