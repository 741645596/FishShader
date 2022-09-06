using System.Collections.Generic;
using UnityEngine;
using System;

// 三阶贝塞尔路径
public class XRouteBezierCubic: XRouteBase
{
    XCfgRouteBezierCubic config;
    Vector2 ptHead;
    Vector2 ptTail;
    float m_CurYRotate;

    public void Init(XCfgRouteBezierCubic config)
    {
        animationSpeed = 1.0f;
        this.config = config;
        curMovingTime = 0;
        curMovePathIndex = 1;
        this.ptTail = config.path[0];
        this.ptHead = config.path[0] + (config.path[1] - config.path[0]) * 0.01f;
    }

    public override void GotoFrame(float bornTime)
    {
        OnNodeChange();
        UpdateRoute(bornTime);
    }

    public void SetDepth(float d)
    {
        depth = d;
    }

    public override void UpdateRoute(float dt)
    {
        curMovingTime += dt;
        float num3 = curMovingTime;
        //LogUtils.I($"UpdateRoute {curMovePathIndex} {num3} {config.times[curMovePathIndex]}");
        
        while (curMovePathIndex < config.times.Count && config.times[curMovePathIndex] <= num3)
        {
            curMovePathIndex++;
            if (curMovePathIndex < config.times.Count)
            {
                OnNodeChange();
            }
        }
        if (curMovePathIndex >= config.times.Count)
        {
            alive = false;
            return;
        }
        float num5 = (num3 - config.times[curMovePathIndex - 1]) / (config.times[curMovePathIndex] - config.times[curMovePathIndex - 1]);
        this.ptTail = this.ptHead;
        Vector3 pos = Vector3.Lerp(config.path[curMovePathIndex - 1], config.path[curMovePathIndex], num5);
        if (XRouteUtils.MirrorFlip)
        {
            pos.x = -pos.x;
            pos.y = -pos.y;
        }
        ptHead = pos;
        pos.z = depth;
        localPosition = pos;
        float num6 = config.angles[curMovePathIndex] - config.angles[curMovePathIndex - 1];
        if (num6 > 180f)
        {
            num6 -= 360f;
        }
        if (num6 < -180f)
        {
            num6 += 360f;
        }
        if (defaultAngle == -999)
        {
            float num7 = config.angles[curMovePathIndex - 1] + num6 * num5;
            localEulerAngles = new Vector3(zRotate, m_CurYRotate, num7 + 90);
            if (XRouteUtils.MirrorFlip)
            {
                localEulerAngles.z += 180;
            }
        }
        else
        {
            localEulerAngles = new Vector3(zRotate, m_CurYRotate, defaultAngle);
        }
        alive = true;
    }

    void OnNodeChange()
    {
        m_CurYRotate = 0;
        if (yRotate > 0)
        {
            m_CurYRotate = IsLeftToRight() ? -yRotate : yRotate;
        }
        Vector3 offset = config.path[curMovePathIndex - 1] - config.path[curMovePathIndex];
        velocity = offset.magnitude / (config.times[curMovePathIndex] - config.times[curMovePathIndex - 1]);
        changeNodeCallback?.Invoke(this);
    }

    public override bool IsLeftToRight()
    {
        float offsetX = config.path[curMovePathIndex].x - config.path[curMovePathIndex - 1].x;
        bool ret = offsetX > 0;
        //LogUtils.I($"IsLeftToRight {offsetX}");
        ret = XRouteUtils.MirrorFlip ? !ret : ret;
        return ret;
    }

    public override bool IsAbsoluteLeftToRight()
    {
        int idx = config.path.Count - 1;
        Vector3 offset = config.path[idx] - config.path[0];
        bool ret = offset.x > 0;
        return ret;
    }
}

