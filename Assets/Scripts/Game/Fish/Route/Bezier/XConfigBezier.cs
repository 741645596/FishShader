using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XConfigBezier
{
    private static XConfigBezier m_Instance;
    public static XConfigBezier Instance { get { if (m_Instance == null) { m_Instance = new XConfigBezier(); } return m_Instance; } }

    private Dictionary<int, XCfgBezier> m_DataDic;

    XConfigBezier()
    {
        m_DataDic = new Dictionary<int, XCfgBezier>();
    }

    public void Clear()
    {
        m_DataDic.Clear();
    }

    public void ReloadBytes(byte[] bytes)
    {
        
    }

    public void ReloadFromOther(Dictionary<int, XCfgBezier> dic)
    {
        m_DataDic = dic;
    }

    public void ReloadText(string text)
    {
        List<XCfgBezier> list = new List<XCfgBezier>();
        list = LitJson.JsonMapper.ToObject<List<XCfgBezier>>(text);
        for (int i = 0; i < list.Count; i++)
        {
            m_DataDic[list[i].id] = list[i];
        }
    }

    public XCfgBezier GetRoute(int id)
    {
        if (m_DataDic.ContainsKey(id))
        {
            return m_DataDic[id];
        }
        return null;
    }

    public XCfgBezier GetCloneRoute(int id)
    {
        if (m_DataDic.ContainsKey(id))
        {
            return XCfgBezier.Clone(m_DataDic[id]);
        }
        return null;
    }

#if UNITY_EDITOR
    public List<int> GetAllKeys()
    {
        var keys = new List<int>();
        foreach (var key in m_DataDic)
        {
            keys.Add(key.Key);
        }
        keys.Sort(delegate (int a, int b) { return a > b ? 1 : -1; });
        return keys;
    }
#endif
}
