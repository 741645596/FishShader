using System;
using System.Collections.Generic;
using UnityEngine;


public class Pool
{
    public List<GameObject> m_GameObjectList = new List<GameObject>();

    public GameObject Pop()
    {
        GameObject ret = null;
        int count = m_GameObjectList.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            ret = m_GameObjectList[i];
            if (ret != null)
            {
                m_GameObjectList.RemoveAt(i);
                return ret;
            }
            else
            {
                m_GameObjectList.RemoveAt(i);
            }
        }
        return null;
    }

    public void Push(GameObject go)
    {
        m_GameObjectList.Add(go);
    }
}

