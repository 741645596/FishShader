using System;
using UnityEngine;

[AddComponentMenu("Tween/Tween Rotation")]
public class TweenRotation : TweenerBase
{
    public Vector3 from;
    public Vector3 to;
    public bool quaternionLerp;
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

    public Quaternion value
    {
        get
        {
            return cachedTransform.localRotation;
        }
        set
        {
            cachedTransform.localRotation = value;
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = (quaternionLerp ? Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), factor) : Quaternion.Euler(new Vector3(Mathf.Lerp(from.x, to.x, factor), Mathf.Lerp(from.y, to.y, factor), Mathf.Lerp(from.z, to.z, factor))));
    }

    public static TweenRotation Begin(GameObject go, float duration, Vector3 eulerAngles, float delay = 0f)
    {
        TweenRotation tweenRotation = TweenerBase.Begin<TweenRotation>(go, duration, delay);
        tweenRotation.from = tweenRotation.value.eulerAngles;
        tweenRotation.to = eulerAngles;
        if (duration <= 0f)
        {
            tweenRotation.Sample(1f, isFinished: true);
            tweenRotation.enabled = false;
        }
        return tweenRotation;
    }

    [ContextMenu("Set 'From' to current value")]
    public override void SetStartToCurrentValue()
    {
        from = value.eulerAngles;
    }

    [ContextMenu("Set 'To' to current value")]
    public override void SetEndToCurrentValue()
    {
        to = value.eulerAngles;
    }

    [ContextMenu("Assume value of 'From'")]
    private void SetCurrentValueToStart()
    {
        value = Quaternion.Euler(from);
    }

    [ContextMenu("Assume value of 'To'")]
    private void SetCurrentValueToEnd()
    {
        value = Quaternion.Euler(to);
    }
}
