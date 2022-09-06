using System;
using System.Collections.Generic;
using UnityEngine;

public class XCamera3DManager: MonoBehaviour
{
    static XCamera3DManager sInstance;

    GameObject m_Root;

    Camera m_Camera;

    public void Start()
    {
        sInstance = this;
        CreateRoot();
        m_Camera = GetComponentInChildren<Camera>();
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
