using System;
using System.Collections.Generic;
using UnityEngine;

// GameObject 对象池
public class ObjectPool : MonoBehaviour
{
    static ObjectPool sInstance;
    public static ObjectPool Instance { get { return sInstance; } }

    Dictionary<int, Pool> m_PoolDic;
    Transform m_InvisibleRoot;

    List<DelayPushToPool> m_UnuseDelayList = new List<DelayPushToPool>();
    List<DelayPushToPool> m_DelayList = new List<DelayPushToPool>();

    const float Interval = 0.1f;

    private void Awake()
    {
        sInstance = this;
        m_PoolDic = new Dictionary<int, Pool>();
        m_InvisibleRoot = transform;
        InvokeRepeating("UpdatePool", 0, Interval);
        gameObject.SetActive(false);
    }

    private void UpdatePool()
    {
        float dt = Interval;
        for (int i = m_DelayList.Count - 1; i >= 0; i--)
        {
            var unit = m_DelayList[i];
            if (unit.duration > 0)
            {
                unit.duration -= dt;
                if (unit.duration <= 0)
                {
                    PushObject(unit.id, unit.go);
                    unit.duration = 0;
                    unit.go = null;
                    unit.id = 0;
                    m_UnuseDelayList.Add(unit);
                    m_DelayList.RemoveAt(i);
                }
            }
        }
    }

    public void Clear()
    {
        m_PoolDic.Clear();
        Destroy(gameObject);
    }

    private Pool GetPool(int id)
    {
        Pool pool = null;
        if (!m_PoolDic.ContainsKey(id))
        {
            pool = new Pool();
            m_PoolDic[id] = pool;
        }
        else
        {
            pool = m_PoolDic[id];
        }
        return pool;
    }

    public GameObject PopObject(int id)
    {
        if (m_InvisibleRoot == null)
        {
            return null;
        }
        Pool pool = GetPool(id);
        GameObject ret = pool.Pop();
        return ret;
    }

    public void PushObject(int id, GameObject go)
    {
        if (m_InvisibleRoot == null || go == null)
        {
            return;
        }
        Pool pool = GetPool(id);
        go.transform.SetParent(m_InvisibleRoot);
        go.transform.localPosition = new Vector3(9999, 0, 0);
        pool.Push(go);
    }

    public void PushObject(int id, GameObject go, float duration)
    {
        if (m_InvisibleRoot == null || go == null)
        {
            return;
        }
        DelayPushToPool unit = null;
        if (m_UnuseDelayList.Count > 0)
        {
            int idx = m_UnuseDelayList.Count - 1;
            unit = m_UnuseDelayList[idx];
            m_UnuseDelayList.RemoveAt(idx);
        }
        else
        {
            unit = new DelayPushToPool();
        }
        unit.id = id;
        unit.go = go;
        unit.duration = duration;
        m_DelayList.Add(unit);
    }
}

