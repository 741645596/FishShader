using System;
using UnityEngine;

// 三阶贝塞尔曲线路径测试脚本
class XRouteBezierCubicTest: MonoBehaviour
{
    public int routeid = 100109;
    public bool repeat = true;
    public float depth = 0;
    XRouteBezierCubic route = new XRouteBezierCubic();

    public void Start()
    {
        route.SetDepth(depth);
        route.Init(XConfigRouteBezierCubic.Instance.GetRoute(routeid));
        route.GotoFrame(0);
        transform.localPosition = route.localPosition;
        transform.localEulerAngles = route.localEulerAngles;
    }

    public void Update()
    {
        route.UpdateRoute(Time.deltaTime);
        if (route.alive)
        {
            transform.localPosition = route.localPosition;
            transform.localEulerAngles = route.localEulerAngles;
        }
        else if (repeat)
        {
            route.Init(XConfigRouteBezierCubic.Instance.GetRoute(routeid));
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

    public XRouteBezierCubic GetRoute()
    {
        return route;
    }
}
