using System;
using UnityEngine;

[AddComponentMenu("Tween/Tween Scale")]
public class TweenScale : TweenerBase
{
    public Vector3 from = Vector3.one;
    public Vector3 to = Vector3.one;
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
            return cachedTransform.localScale;
        }
        set
        {
            cachedTransform.localScale = value;
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = from * (1f - factor) + to * factor;
    }

    public static TweenScale Begin(GameObject go, float duration, Vector3 scale, float delay = 0f)
    {
        TweenScale tweenScale = TweenerBase.Begin<TweenScale>(go, duration, delay);
        tweenScale.from = tweenScale.value;
        tweenScale.to = scale;
        if (duration <= 0f)
        {
            tweenScale.Sample(1f, isFinished: true);
            tweenScale.enabled = false;
        }
        return tweenScale;
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
