using System.Collections.Generic;
using UnityEngine;
using System;

// 三阶贝塞尔路径
public class XCfgBezier
{
    public int id;
    public string desc;
    public int defaultAngle; // 朝向角度，-999 计算正前方角度
    public bool ai; // 使用行为树ai
    public List<XCfgBezierNode> nodes;

    public float GetTotalTime()
    {
        float ret = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            ret += nodes[i].time;
        }
        return ret;
    }

    public bool IsLeftToRight()
    {
        int idx = nodes.Count - 1;
        float offsetX = nodes[0].p1.x - nodes[idx].p1.x;
        bool ret = offsetX > 0;
        return ret;
    }

    public string GetTittle()
    {
        return $"{id}-{desc}";
    }

    public string GetKeyWorld()
    {
        return $"{id}-{desc}";
    }

    public static XCfgBezier Clone(XCfgBezier from)
    {
        string str = LitJson.JsonMapper.ToJson(from);
        return LitJson.JsonMapper.ToObject<XCfgBezier>(str);
    }
}

