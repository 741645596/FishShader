using System.Collections.Generic;
using UnityEngine;

// 二阶贝塞尔曲线路径测试脚本
public class XRouteQuardaticBezierTest : MonoBehaviour
{
    XRouteBezierQuardatic route;
    public int routeid = 1;
    public bool repeat = true;
    private void Start()
    {
        route = new XRouteBezierQuardatic();
        route.Reset(XConfigRoute.Instance.GetRoute(routeid));
        route.GotoFrame(0);
        transform.localPosition = route.localPosition;
        transform.localEulerAngles = route.localEulerAngles;
    }

    private void Update()
    {
        route.UpdateRoute(Time.deltaTime);
        if (route.alive)
        {
            transform.localPosition = route.localPosition;
            transform.localEulerAngles = route.localEulerAngles;
        }
        else if (repeat)
        {
            route.Reset(XConfigRoute.Instance.GetRoute(routeid));
            route.GotoFrame(0);
            transform.localPosition = route.localPosition;
            transform.localEulerAngles = route.localEulerAngles;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
