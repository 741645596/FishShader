using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Tween/Tween Bezier")]
public class TweenBezier : TweenerBase
{
    public Vector3 from;
    public Vector3 to;
    public float rate = 0;
    public bool worldSpace;
    private bool hasInitMid;
    private Vector3 mid = new Vector3(0f, 0f, 0f);

    private Transform mTrans;
    public Transform cachedTransform
    {
        get
        {
            if (mTrans == null)
            {
                mTrans = base.transform;
            }
            return mTrans;
        }
    }

    public Vector3 value
    {
        get
        {
            if (!worldSpace)
            {
                return cachedTransform.localPosition;
            }
            return cachedTransform.position;
        }
        set
        {
            if (worldSpace)
            {
                cachedTransform.position = value;
            }
            else
            {
                cachedTransform.localPosition = value;
            }
        }
    }

    // 计算二阶贝塞尔曲线控制点
    private void CalcBezierMiddlePosByRate()
    {
        Vector3 p = from - to;
        Vector3 middle = (from + to) / 2;
        Vector3 tmp = new Vector3(p.y, -p.x, p.z);
        Vector3 ret = tmp / 2 + middle;
        if (rate > 0)
        {
            ret.x = ret.x + tmp.x / 2 * (rate - 1);
            ret.y = ret.y + tmp.y / 2 * (rate - 1);
            ret.z = ret.z + tmp.z / 2 * (rate - 1);
        }
        else
        {
            ret.x = ret.x - tmp.x / 2 * (1 - rate);
            ret.y = ret.y - tmp.y / 2 * (1 - rate);
            ret.z = ret.z - tmp.z / 2 * (1 - rate);
        }
        mid = ret;
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        if (!hasInitMid)
        {
            CalcBezierMiddlePosByRate();
            hasInitMid = true;
        }
        float d = factor * factor;
        value = d * (from - 2f * mid + to) + 2f * factor * (mid - from) + from;
    }

    public static TweenBezier Begin(GameObject go, float duration, Vector3 pos, float rate, float delay = 0f)
    {
        TweenBezier tweenBezier = TweenerBase.Begin<TweenBezier>(go, duration, delay);
        tweenBezier.from = tweenBezier.value;
        tweenBezier.to = pos;
        tweenBezier.rate = rate;

        if (duration <= 0f)
        {
            tweenBezier.Sample(1f, isFinished: true);
            tweenBezier.enabled = false;
        }
        return tweenBezier;
    }

    [ContextMenu("Set 'From' to current value")]
    public override void SetStartToCurrentValue()
    {
        from = value;
    }

    [ContextMenu("Set 'To' to current value")]
    public override void SetEndToCurrentValue()
    {
        to = value;
    }

    [ContextMenu("Assume value of 'From'")]
    private void SetCurrentValueToStart()
    {
        value = from;
    }

    [ContextMenu("Assume value of 'To'")]
    private void SetCurrentValueToEnd()
    {
        value = to;
    }
}
