using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XConfigRoute
{
    private static XConfigRoute m_Instance;
    public static XConfigRoute Instance { get { if (m_Instance == null) { m_Instance = new XConfigRoute(); } return m_Instance; } }

    private Dictionary<int, XCfgRoute> m_DataDic;

    XConfigRoute()
    {
        m_DataDic = new Dictionary<int, XCfgRoute>();
    }

    public void Clear()
    {
        m_DataDic.Clear();
    }

    public void ReloadBytes(byte[] bytes)
    {
        m_DataDic = new Dictionary<int, XCfgRoute>();
        BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
        int count = reader.ReadInt32();
        //Debug.Log(count);
        for (int k = 0; k < count; k++)
        {
            int id = reader.ReadInt32();
            //Debug.Log(id);
            XCfgRoute info = new XCfgRoute();
            m_DataDic[id] = info;
            info.id = id;
            info.startPos = new Vector2(0, 0);
            info.startPos.x = reader.ReadSingle();
            info.startPos.y = reader.ReadSingle();
            info.routeType = reader.ReadInt32();
            info.totalTime = reader.ReadSingle();
            info.fadeAway = reader.ReadInt32();
            info.swordAction = reader.ReadInt32();
            info.rotate = reader.ReadSingle();
            info.swordTime = reader.ReadSingle();
            info.angle = reader.ReadSingle();
            info.slope = reader.ReadSingle();
            int length = reader.ReadInt32();
            info.pathInfos = new List<XCfgRouteNodeInfo>();
            for (int i = 0; i < length; i++)
            {
                var node = new XCfgRouteNodeInfo();
                node.position = new Vector2(0, 0);
                node.position.x = reader.ReadSingle();
                node.position.y = reader.ReadSingle();

                node.time = reader.ReadSingle();
                node.type = reader.ReadInt32();
                node.rate = reader.ReadSingle();
                node.speed = reader.ReadSingle();
                node.lerp = reader.ReadSingle();
                node.playAni = reader.ReadInt32();
                info.pathInfos.Add(node);
            }
        }
    }

    public XCfgRoute GetRoute(int id)
    {
        if (m_DataDic.ContainsKey(id))
        {
            return m_DataDic[id];
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
