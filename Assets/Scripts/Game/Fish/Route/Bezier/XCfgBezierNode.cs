using System.Collections.Generic;
using UnityEngine;
using System;


public class BezierVector3
{
    public float x;
    public float y;
    public float z;

    public void SetValue(Vector3 p)
    {
        x = p.x;
        y = p.y;
        z = p.z;
    }

    public Vector3 GetValue()
    {
        return new Vector3(x, y, z);
    }
}

// 三阶贝塞尔路径结点
public class XCfgBezierNode
{
    public float time;
    public int ani;
    public float speed;
    public int type; // 0 贝塞尔 1 直线 2 原地不动
    public BezierVector3 p1;
    public BezierVector3 c1;
    public BezierVector3 c2;


    public XCfgBezierNode()
    {
        p1 = new BezierVector3();
        c1 = new BezierVector3();
        c2 = new BezierVector3();
    }
}

