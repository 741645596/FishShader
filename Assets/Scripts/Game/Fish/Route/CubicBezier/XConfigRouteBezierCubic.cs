using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class XConfigRouteBezierCubic
{
    private static XConfigRouteBezierCubic m_Instance;
    public static XConfigRouteBezierCubic Instance { get { if (m_Instance == null) { m_Instance = new XConfigRouteBezierCubic(); } return m_Instance; } }

    private Dictionary<int, XCfgRouteBezierCubic> m_DataDic;

    XConfigRouteBezierCubic()
    {
        m_DataDic = new Dictionary<int, XCfgRouteBezierCubic>();
    }

    public void Clear()
    {
        m_DataDic.Clear();
    }

    public void ReloadText(string content)
    {
        int count = 0;
        foreach (string text in content.Replace("\r", "").Split(new char[]
            {
                '\n'
            }))
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                string[] array2 = text.Split(new char[]
                {
                        '|'
                });
                List<Vector3> points = XRouteUtils.Str2Vec3Array(array2[2]);
                List<Vector4> ctrls = XRouteUtils.Str2Vec4Array(array2[3]);
                List<float> speeds = new List<float>();
                XRouteUtils.SpeedAndAction(array2[4], speeds);
                var path = new XCfgRouteBezierCubic();
                path.pathId = int.Parse(array2[0]);
                path.desc = array2[1];
                path.Calc(points, ctrls, speeds);
                m_DataDic[path.pathId] = path;
                //LogUtils.I($"{path.pathId} {path.desc}");
                count++;
            }
        }
        LogUtils.I($"初始化{count}三阶贝塞尔路径");
    }

    public void ReloadBytes(byte[] bytes)
    {
        m_DataDic = new Dictionary<int, XCfgRouteBezierCubic>();
    }

    public XCfgRouteBezierCubic GetRoute(int id)
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
