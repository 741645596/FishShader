using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Tween/Tween Model Alpha")]
public class TweenModelAlpha : TweenerBase
{
    [Range(0f, 1f)]
    public float from = 1;
    [Range(0f, 1f)]
    public float to = 0;

    public string Property;
    public int renderQueue = 3000; 

    List<Material> mMaterials;
    float mAlpha;
    int nameID;

    List<Material> cachedMaterials
    {
        get
        {
            if (mMaterials == null)
            {
                nameID = Shader.PropertyToID(Property);
                mMaterials = new List<Material>();
                var meshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    SkinnedMeshRenderer render = meshRenderers[i];
                    int length = render.materials.Length;
                    if (length == 0) continue;
                    for (int j = 0; j < length; j++)
                    {
                        var mat = render.materials[j];
                        if (mat.HasProperty(nameID))
                        {
                            mat.renderQueue = renderQueue;
                            mMaterials.Add(mat);
                        }
                    }
                }
            }
            return mMaterials;
        }
    }

    public float value
    {
        get
        {
            return mAlpha;
        }
        set
        {
            mAlpha = value;
            var list = cachedMaterials;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].SetFloat(nameID, value);
            }
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = from * (1f - factor) + to * factor;
    }

    public static TweenModelAlpha Begin(GameObject go, float duration, float alpha, float delay = 0f)
    {
        TweenModelAlpha tweenAlpha = TweenerBase.Begin<TweenModelAlpha>(go, duration, delay);
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
