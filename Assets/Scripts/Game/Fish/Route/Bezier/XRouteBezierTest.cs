using System;
using UnityEngine;
using Dreamteck.Splines;

// 贝塞尔曲线路径测试脚本
class XRouteBezierTest : MonoBehaviour
{
    public static XRouteBezierTest Create(int id)
    {
        GameObject go = new GameObject("XRouteBezierTest");
        var com = go.AddComponent<XRouteBezierTest>();
        com.routeid = id;
        return com;
    }

    public int routeid = 1;
    public bool repeat = true;
    public float depth = 0;

    Vector3 lastPostion;

    XRouteBezier route = new XRouteBezier();


    public void Start()
    {
        var config = XConfigBezier.Instance.GetRoute(routeid);
        route.syncPoint = true;
        route.depth = depth;
        route.Init(XConfigBezier.Instance.GetRoute(routeid));
        route.GotoFrame(0);
       
        transform.localEulerAngles = route.localEulerAngles;
        transform.localPosition = route.localPosition;

    }

    public void Update()
    {
        route.UpdateRoute(Time.deltaTime);
        if (route.alive)
        {
            transform.up = route.localPosition - lastPostion;
            //Debug.Log(route.localPosition - lastPostion);
            //transform.localEulerAngles = route.localEulerAngles;
            transform.localPosition = route.localPosition;
            lastPostion = route.localPosition;
        }
        else if (repeat)
        {
            route.Init(XConfigBezier.Instance.GetRoute(routeid));
            route.GotoFrame(0);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
