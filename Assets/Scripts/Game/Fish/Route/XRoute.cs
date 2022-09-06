using System.Collections.Generic;
using UnityEngine;
using System;

public class XRoute
{
    Transform transform;
    XRouteBase route;
    int defaultAngle;
    float yRotate;
    float zRotate;

    public void Reset(int id)
    {
        InitRoute(id, Vector2.zero, false);
    }

    public void Reset(int id, Vector2 startPos)
    {
        InitRoute(id, startPos, true);
    }

    void InitRoute(int id, Vector2 startPos, bool resetStartPos)
    {
        XCfgBezier config = XConfigBezier.Instance.GetRoute(id);
        InitRouteBezier(config, startPos, resetStartPos);
        //XCfgRoute config = XConfigRoute.Instance.GetRoute(id);
        //if (config != null)
        //{
        //    InitXRouteBezierQuardatic(config, startPos, resetStartPos);
        //    return;
        //}
        //XCfgRouteBezierCubic xCfgRouteBezierCubic = XConfigRouteBezierCubic.Instance.GetRoute(id);
        //if (xCfgRouteBezierCubic != null)
        //{
        //    InitXConfigRouteBezierCubic(xCfgRouteBezierCubic, startPos, resetStartPos);
        //    return;
        //}
    }

    //void InitXRouteBezierQuardatic(XCfgRoute config, Vector2 startPos, bool resetStartPos)
    //{
    //    var r = new XRouteBezierQuardatic();
    //    r.defaultAngle = defaultAngle;
    //    r.yRotate = yRotate;
    //    r.zRotate = zRotate;
    //    r.totalTime = config.totalTime;
    //    if (resetStartPos)
    //    {
    //        r.Reset(config, startPos);
    //    }
    //    else
    //    {
    //        r.Reset(config);
    //    }
    //    route = r;
    //}

    //void InitXConfigRouteBezierCubic(XCfgRouteBezierCubic config, Vector2 startPos, bool resetStartPos)
    //{
    //    XRouteBezierCubic r = new XRouteBezierCubic();
    //    r.totalTime = config.times[config.times.Count - 1];
    //    r.defaultAngle = defaultAngle;
    //    r.yRotate = yRotate;
    //    r.zRotate = zRotate;
    //    r.Init(config);
    //    route = r;
    //}

    void InitRouteBezier(XCfgBezier config, Vector2 startPos, bool resetStartPos)
    {
        XRouteBezier r = new XRouteBezier();
        r.totalTime = config.GetTotalTime();
        r.defaultAngle = defaultAngle != XRouteUtils.DEFAULT_ANGLE ? defaultAngle : config.defaultAngle;
        r.yRotate = yRotate;
        r.zRotate = zRotate;
        r.alive = true;
        r.Init(config, XRouteUtils.ConvertToWorldPosition(startPos.x, startPos.y), resetStartPos);
        route = r;
    }

    public void SetLocalEulerAngles(int defaultAngle, float yRotate, float zRotate)
    {
        this.defaultAngle = defaultAngle;
        this.yRotate = yRotate;
        this.zRotate = zRotate;
        //localEulerAngles = vector3;
    }

    public void GotoFrame(float bornTime)
    {
        route.GotoFrame(bornTime);
        if (route.alive)
        {
            transform.localPosition = route.localPosition;
            transform.localEulerAngles = route.localEulerAngles;
        }
        else
        {
            //LogUtils.V($"GotoFrame {bornTime}");
        }
    }

    public void UpdateRoute(float dt)
    {
        if (route == null)
        {
            return;
        }
        route.UpdateRoute(dt);
        transform.localPosition = route.localPosition;
        transform.localEulerAngles = route.localEulerAngles;
    }

    public void UpdateRouteTime(float dt)
    {
        if (route == null)
        {
            return;
        }
        route.UpdateRouteTime(dt);
    }

    public bool IsRouteAlive()
    {
        if (route != null)
        {
            return route.alive;
        }
        return false;
    }

    public bool IsAbsoluteLeftToRight()
    {
        if (route != null)
        {
            return route.IsAbsoluteLeftToRight();
        }
        return true;
    }

    public bool IsLeftToRight()
    {
        if (route != null)
        {
            return route.IsLeftToRight();
        }
        return true;
    }

    public float GetCurMovingTime()
    {
        if (route != null)
        {
            return route.curMovingTime;
        }
        return 0f;
    }

    public void SetChangeNodeCallback(Action<XRouteBase> cb)
    {
        if (route != null)
        {
            route.changeNodeCallback = cb;
        }
    }

    public void SetTransfrom(Transform tf)
    {
        transform = tf;
    }

    public void SetDepth(float depth)
    {
        if (route != null)
        {
            route.depth = depth;
        }
    }

}
