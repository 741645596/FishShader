using System;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Tween/Tween Alpha")]
public class TweenAlpha : TweenerBase
{
    [Range(0f, 1f)]
    public float from = 1;
    [Range(0f, 1f)]
    public float to = 0;
    public bool cascade = false;
    private MaskableGraphic mMaskableGraphic;
    private MaskableGraphic[] mMaskableGraphicArray;

    public MaskableGraphic cachedMaskableGraphic
    {
        get
        {
            if (mMaskableGraphic == null)
            {
                mMaskableGraphic = GetComponent<MaskableGraphic>();
            }
            return mMaskableGraphic;
        }
    }

    public float value
    {
        get
        {
            return cachedMaskableGraphic.color.a;
        }
        set
        {
            if (cascade)
            {
                if (mMaskableGraphicArray == null)
                {
                    mMaskableGraphicArray = GetComponentsInChildren<MaskableGraphic>();
                }
                if (mMaskableGraphicArray != null)
                {
                    for (int i = 0; i < mMaskableGraphicArray.Length; i++)
                    {
                        var mg = mMaskableGraphicArray[i];
                        var color = mg.color;
                        color.a = value;
                        mg.color = color;
                    }
                }
            }
            else
            {
                var color = cachedMaskableGraphic.color;
                color.a = value;
                cachedMaskableGraphic.color = color;
            }
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = from * (1f - factor) + to * factor;
    }

    public static TweenAlpha Begin(GameObject go, float duration, float alpha, float delay = 0f)
    {
        TweenAlpha tweenAlpha = TweenerBase.Begin<TweenAlpha>(go, duration, delay);
        tweenAlpha.from = tweenAlpha.value;
        tweenAlpha.to = alpha;
        if (duration <= 0f)
        {
            tweenAlpha.Sample(1f, isFinished: true);
            tweenAlpha.enabled = false;
        }
        return tweenAlpha;
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
