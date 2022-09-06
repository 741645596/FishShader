using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XConfigEffect
{
    public class XCfgEffect
    {
        public int id;
        public string path;
        public float duration; // 毫秒
        public string desc;
    }

    private static XConfigEffect m_Instance;
    public static XConfigEffect Instance { get { if (m_Instance == null) { m_Instance = new XConfigEffect(); } return m_Instance; } }

    private Dictionary<int, XCfgEffect> m_DataDic;

    XConfigEffect()
    {
        m_DataDic = new Dictionary<int, XCfgEffect>();
    }

    public void Clear()
    {
        m_DataDic.Clear();
    }

    public void ReloadBytes(string text)
    {
        m_DataDic = new Dictionary<int, XCfgEffect>();
        List<XCfgEffect> list = LitJson.JsonMapper.ToObject<List<XCfgEffect>>(text);
        for (int i = 0; i < list.Count; i++)
        {
            var unit = list[i];
            unit.duration = unit.duration / 1000.0f;
            m_DataDic[unit.id] = unit;
        }
    }

    public XCfgEffect Get(int id)
    {
        if (m_DataDic.ContainsKey(id))
        {
            return m_DataDic[id];
        }
        return null;
    }

    public List<int> GetAllKeys()
    {
        List<int> list = new List<int>();
        foreach(var key in m_DataDic)
        {
            list.Add(key.Key);
        }
        return list;
    }
}
