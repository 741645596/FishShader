using System.Collections.Generic;
using UnityEngine;
using System;

// 贝塞尔路径基类
public class XRouteBase
{
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public float depth;
    public bool alive;
    public float curMovingTime;
    public float totalTime;
    public int curMovePathIndex;
    public int defaultAngle;
    public float zRotate;
    public float yRotate;

    public int playAni;
    public float animationSpeed;
    public float velocity; // 移动速度

    public Action<XRouteBase> changeNodeCallback;

    public virtual void GotoFrame(float bornTime)
    {
        
    }

    public virtual void UpdateRoute(float dt)
    {
        
    }

    public void UpdateRouteTime(float dt)
    {
        curMovingTime += dt;
        if (curMovingTime > totalTime)
        {
            alive = false;
        }
    }

    public virtual bool IsLeftToRight()
    {
        return true;
    }

    public virtual bool IsAbsoluteLeftToRight()
    {
        return true;
    }
}

