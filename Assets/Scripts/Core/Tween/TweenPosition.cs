using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Tween/Tween Position")]
public class TweenPosition : TweenerBase
{
    public Vector3 from;
    public Vector3 to;
    public bool worldSpace;
    public bool moveBy;
    private bool isRestart;
    Vector3 startPos;

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

    protected override void OnUpdate(float factor, bool isFinished)
    {
        if (moveBy)
        {
            if (!isRestart)
            {
                startPos = value;
                isRestart = true;
            }
            value = startPos + from * (1f - factor) + to * factor;
        }
        else
        {
            value = from * (1f - factor) + to * factor;
        }
    }

    protected override void OnFinish()
    {
        enabled = false;
        isRestart = false;
    }

    public static TweenPosition Begin(GameObject go, float duration, Vector3 pos, bool worldSpace, float delay = 0f)
    {
        TweenPosition tweenPosition = TweenerBase.Begin<TweenPosition>(go, duration, delay);
        tweenPosition.worldSpace = worldSpace;
        tweenPosition.from = tweenPosition.value;
        tweenPosition.to = pos;
        if (duration <= 0f)
        {
            tweenPosition.Sample(1f, isFinished: true);
            tweenPosition.enabled = false;
        }
        return tweenPosition;
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
