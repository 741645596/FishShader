using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Tween/Tween Fish Sprite Alpha")]
public class TweenFishSpriteAlpha : TweenerBase
{
    [Range(0f, 1f)]
    public float from = 1;
    [Range(0f, 1f)]
    public float to = 0;
    public bool cascade = false;
    private float current;
    private SpriteRenderer[] mMaskableGraphicArray;
    private List<float> m_OriginAlpha;

    public SpriteRenderer[] cachedMaskableGraphic
    {
        get
        {
            if (mMaskableGraphicArray == null)
            {
                m_OriginAlpha = new List<float>();
                mMaskableGraphicArray = GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < mMaskableGraphicArray.Length; i++)
                {
                    m_OriginAlpha.Add(mMaskableGraphicArray[i].color.a);
                }
            }
            return mMaskableGraphicArray;
        }
    }

    public float value
    {
        get
        {
            return current;
        }
        set
        {
            current = value;
            var array = cachedMaskableGraphic;
            for (int i = 0; i < array.Length; i++)
            {
                var mg = array[i];
                var color = mg.color;
                color.a = m_OriginAlpha[i] * current;
                mg.color = color;
            }
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = from * (1f - factor) + to * factor;
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
