using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XConfigAudio
{
    public class XCfgAudio
    {
        public int id;
        public string path;
    }

    private static XConfigAudio m_Instance;
    public static XConfigAudio Instance { get { if (m_Instance == null) { m_Instance = new XConfigAudio(); } return m_Instance; } }

    private Dictionary<int, XCfgAudio> m_DataDic;

    XConfigAudio()
    {
        m_DataDic = new Dictionary<int, XCfgAudio>();
    }

    public void Clear()
    {
        m_DataDic.Clear();
    }

    public void ReloadBytes(string text)
    {
        m_DataDic = new Dictionary<int, XCfgAudio>();
        List<XCfgAudio> list = LitJson.JsonMapper.ToObject<List<XCfgAudio>>(text);
        for (int i = 0; i < list.Count; i++)
        {
            m_DataDic[list[i].id] = list[i];
        }
    }

    public XCfgAudio Get(int id)
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
        foreach (var key in m_DataDic)
        {
            list.Add(key.Key);
        }
        return list;
    }
}
