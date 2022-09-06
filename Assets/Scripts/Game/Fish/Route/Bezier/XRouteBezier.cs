using System.Collections.Generic;
using UnityEngine;
using System;

// 三阶贝塞尔路径
public class XRouteBezier : XRouteBase
{
    XCfgBezier config;

    Vector3 lastPostion;

    Vector3 p1;
    Vector3 p2;
    Vector3 c1;
    Vector3 c2;
    float time;
    int moveType;

    float m_CurYRotate;

    bool resetStartPos;
    Vector2 startPos;

    public bool syncPoint; // 一直同步配表数据

    public void Init(XCfgBezier config)
    {
        animationSpeed = 1.0f;
        this.config = config;
        curMovingTime = 0;
        curMovePathIndex = 1;
    }

    public void Init(XCfgBezier config, Vector2 startPos, bool resetStartPos)
    {
        animationSpeed = 1.0f;
        this.config = config;
        curMovingTime = 0;
        curMovePathIndex = 1;
        this.startPos = startPos;
        this.resetStartPos = resetStartPos;
    }

    public override void GotoFrame(float bornTime)
    {
        alive = true;
        for (int i = 1; i < config.nodes.Count; i++)
        {
            curMovePathIndex = i;
            var node = config.nodes[i];
            if (bornTime >= node.time)
            {
                bornTime -= node.time;
            }
            else
            {
                curMovingTime = bornTime;
                ChangeNode(i);
                break;
            }
        }
        UpdateRoute(0);
    }

    void ChangeNode(int idx)
    {
        OnNodeChange();
        if (resetStartPos && idx == 1)
        {
            p1 = startPos;
        }
        else
        {
            p1 = config.nodes[idx - 1].p1.GetValue();
        }
        p2 = config.nodes[idx].p1.GetValue();
        c1 = config.nodes[idx - 1].c2.GetValue();
        c2 = config.nodes[idx].c1.GetValue();
        time = config.nodes[idx].time;
        moveType = config.nodes[idx].type;

        switch (moveType)
        {
            case XRouteConsts.ROUTE_TYPE_LINE:
            case XRouteConsts.ROUTE_TYPE_STANDING:
                {
                    if (defaultAngle == XRouteUtils.DEFAULT_ANGLE)
                    {
                        CalcAngle(p1, p2);
                    }
                    else
                    {
                        localEulerAngles = new Vector3(zRotate, m_CurYRotate, defaultAngle);
                    }
                    break;
                }
        }
    }

    public override void UpdateRoute(float dt)
    {
        curMovingTime += dt;
        while (curMovingTime >= time)
        {
            curMovingTime -= time;
            curMovePathIndex++;
            if (curMovePathIndex >= config.nodes.Count)
            {
                alive = false;
                return;
            }
            ChangeNode(curMovePathIndex);
        }
#if UNITY_EDITOR
        if (syncPoint)
        {
            ChangeNode(curMovePathIndex);
        }
#endif
        UpdateRate(curMovingTime / time);
        alive = true;
    }

    public void UpdateRouteByTime(float passTime)
    {
        curMovePathIndex = 1;
        for (int i = 1; i < config.nodes.Count; i++)
        {
            float t1 = config.nodes[i].time;
            if (passTime >= t1)
            {
                curMovePathIndex++;
                passTime -= t1;
            }
            else
            {
                break;
            }
        }
        curMovingTime = passTime;
        if (curMovePathIndex >= config.nodes.Count)
        {
            alive = false;
            return;
        }

        ChangeNode(curMovePathIndex);
        UpdateRate(curMovingTime / time);
        alive = true;
    }

    void UpdateRate(float t)
    {
        switch (moveType)
        {
            case XRouteConsts.ROUTE_TYPE_BEZIRER:
                {
                    float nt = t;
                    t = 1 - t;
                    float t3 = t * t * t;
                    float t2 = 3.0f * t * t * nt;
                    float t1 = 3.0f * t * nt * nt;
                    float t0 = nt * nt * nt;

                    localPosition.x = p1.x * t3 + c1.x * t2 + c2.x * t1 + p2.x * t0;
                    localPosition.y = p1.y * t3 + c1.y * t2 + c2.y * t1 + p2.y * t0;
                    localPosition.z = 0;// p1.z * t3 + c1.z * t2 + c2.z * t1 + p2.z * t0;
                    localPosition.z += depth;

                    if (defaultAngle == XRouteUtils.DEFAULT_ANGLE)
                    {
                        CalcAngle(lastPostion, localPosition);
                    }
                    else
                    {
                        localEulerAngles = new Vector3(zRotate, m_CurYRotate, defaultAngle);
                    }
                    break;
                }

            case XRouteConsts.ROUTE_TYPE_LINE:
                {
                    localPosition = Vector3.Lerp(p1, p2, t);
                    localPosition.z += depth;
                    break;
                }

            case XRouteConsts.ROUTE_TYPE_STANDING:
                {
                    localPosition = p2;
                    localPosition.z += depth;
                    break;
                }
        }
        lastPostion = localPosition;
        if (XRouteUtils.MirrorFlip)
        {
            localPosition.x = -localPosition.x;
            localPosition.y = -localPosition.y;
        }
        localPosition.x *= XRouteUtils.ScaleX;
        localPosition.y *= XRouteUtils.ScaleY;

    }

    void CalcAngle(Vector3 from, Vector3 to)
    {
        Vector3 offset = XRouteUtils.MirrorFlip ? from - to : to - from;
        if (offset.magnitude > 0.001f)
        {
            Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.up, offset).eulerAngles;
            localEulerAngles = new Vector3(zRotate, m_CurYRotate, eulerAngles.z);
        }
    }

    void OnNodeChange()
    {
        m_CurYRotate = 0;
        if (yRotate > 0)
        {
            m_CurYRotate = IsLeftToRight() ? -yRotate : yRotate;
        }
        Vector3 offset = config.nodes[curMovePathIndex - 1].p1.GetValue() - config.nodes[curMovePathIndex].p1.GetValue();
        velocity = offset.magnitude / (config.nodes[curMovePathIndex].time);
        playAni = config.nodes[curMovePathIndex].ani;
        changeNodeCallback?.Invoke(this);
    }

    public override bool IsLeftToRight()
    {
        float offsetX = config.nodes[curMovePathIndex].p1.x - config.nodes[curMovePathIndex - 1].p1.x;
        bool ret = offsetX > 0;
        //LogUtils.I($"IsLeftToRight {offsetX}");
        ret = XRouteUtils.MirrorFlip ? !ret : ret;
        return ret;
    }

    public override bool IsAbsoluteLeftToRight()
    {
        int idx = config.nodes.Count - 1;
        float x = config.nodes[idx].p1.x - config.nodes[0].p1.x;
        bool ret = x > 0;
        return ret;
    }
}

