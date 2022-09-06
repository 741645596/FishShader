using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;


// 受击变红
public class XFishTurnRed
{
    enum TurnRedType
    {
        None,
        Sprite,
        MatCap,
        PBR,
    }


    class MaterialInfo
    {
        public Material material;
        public Color srcColor;
        public Color hitColor;
        public float srcMultipleValue;
        public float hitMultiple;
        public SpriteRenderer spriteRenderer;

    }

    public const float HIT_COLOR_ANI_TIME1 = 0.2f; //击中动画播放时长（第一次）
    public const float HIT_COLOR_ANI_TIME2 = 0.2f; //击中动画播放时长（第二次）

    public Color RED_COLOR = new Color(1, 0.45f, 0.45f, 1);

    TurnRedType turnRedType = TurnRedType.None;

    static int _ShdPropIsHurt = Shader.PropertyToID("_IsHurt");
    static int _ShdPropHurtMultiple = Shader.PropertyToID("_HurtMultiple");
    static int _ShdPropHurtColor = Shader.PropertyToID("_HurtColor");

    static int _HitColor = Shader.PropertyToID("_HitColor");
    static int _HitMultiple = Shader.PropertyToID("_HitMultiple");
    static int _HitColorChannel = Shader.PropertyToID("_HitColorChannel");
    static int _OverlayColor = Shader.PropertyToID("_OverlayColor");
    static int _OverlayMultiple = Shader.PropertyToID("_OverlayMultiple");

    List<MaterialInfo> list;


    bool runAction;
    float elapse;

    Color redColor = new Color(1, 1, 1, 1);
    Color defColor = new Color(1, 0, 0, 1);

    public void Update(float dt)
    {
        if (!runAction)
        {
            return;
        }
        elapse += dt;
        if (elapse < HIT_COLOR_ANI_TIME1)
        {
            float ratio = Mathf.Clamp01(elapse / HIT_COLOR_ANI_TIME1);
            SetRedIn(ratio);
        }
        else if (elapse < HIT_COLOR_ANI_TIME1 + HIT_COLOR_ANI_TIME2)
        {
            float ratio = (elapse - HIT_COLOR_ANI_TIME1) / HIT_COLOR_ANI_TIME2;
            SetRedOut(ratio);
        }
        else
        {
            StopAction();
        }
    }

    void SetRedIn(float ratio)
    {
        switch (turnRedType)
        {
            case TurnRedType.Sprite:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        info.srcColor = Color.Lerp(info.srcColor, RED_COLOR, ratio);
                        list[i].spriteRenderer.color = info.srcColor;
                    }
                    break;
                }

            case TurnRedType.MatCap:
                {
                    break;
                }

            case TurnRedType.PBR:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        Color value2 = Color.Lerp(info.srcColor, info.hitColor, ratio);
                        float value3 = Mathf.Lerp(info.srcMultipleValue, info.hitMultiple, ratio);
                        var mat = list[i].material;
                        mat.SetColor(_OverlayColor, value2);
                        mat.SetFloat(_OverlayMultiple, value3);
                    }
                    break;
                }
        }
        
    }

    void SetRedOut(float ratio)
    {
        switch (turnRedType)
        {
            case TurnRedType.Sprite:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        info.srcColor = Color.Lerp(info.srcColor, Color.white, ratio);
                        list[i].spriteRenderer.color = info.srcColor;
                    }
                    break;
                }

            case TurnRedType.MatCap:
                {
                    break;
                }

            case TurnRedType.PBR:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        Color value2 = Color.Lerp(info.hitColor, Color.white, ratio);
                        float value3 = Mathf.Lerp(info.hitMultiple, info.srcMultipleValue, ratio);
                        var mat = list[i].material;
                        mat.SetColor("_OverlayColor", value2);
                        mat.SetFloat("_OverlayMultiple", value3);
                    }
                    break;
                }
        }
    }


    public void Play()
    {
        switch (turnRedType)
        {
            case TurnRedType.Sprite:
                {
                    elapse = 0;
                    break;
                }

            case TurnRedType.MatCap:
                {
                    elapse = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].material.SetFloat(_ShdPropIsHurt, 1.0f);
                    }
                    break;
                }

            case TurnRedType.PBR:
                {
                    elapse = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        var mat = info.material;
                        info.material = mat;
                        info.hitColor = mat.GetColor("_HitColor");
                        info.srcColor = mat.GetColor("_OverlayColor");
                        info.hitMultiple = mat.GetFloat("_HitMultiple");
                        if (mat.GetFloat("_HitColorChannel") == 0f)
                        {
                            info.srcMultipleValue = 0f;
                        }
                        else
                        {
                            info.srcMultipleValue = 1f;
                        }
                    }
                    break;
                }
        }
        runAction = true;
    }

    public void StopAction()
    {
        elapse = 0;
        runAction = false;
        redColor.r = 1;
        redColor.g = 1;
        redColor.b = 1;
        switch (turnRedType)
        {
            case TurnRedType.Sprite:
                {
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].spriteRenderer.color = Color.white;
                        }
                    }
                    break;
                }

            case TurnRedType.MatCap:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var mat = list[i].material;
                        mat.SetFloat(_ShdPropIsHurt, 0);
                        if (mat.HasProperty(_ShdPropHurtMultiple))
                        {
                            mat.SetFloat(_ShdPropHurtMultiple, 0);
                        }
                        else
                        {
                            var hurtColor = list[i].srcColor;
                            mat.SetColor(_ShdPropHurtColor, hurtColor);
                        }
                    }
                    break;
                }

            case TurnRedType.PBR:
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var info = list[i];
                        var mat = info.material;
                        mat.SetColor(_OverlayColor, Color.white);
                        mat.SetFloat(_OverlayMultiple, info.srcMultipleValue);
                    }
                    break;
                }
        }
    }

    public void AddMaterial(Material mat)
    {
        if (mat.HasProperty(_ShdPropHurtColor))
        {
            if (list == null)
            {
                list = new List<MaterialInfo>();
            }
            turnRedType = TurnRedType.MatCap;
            var info = new MaterialInfo();
            info.material = mat;
            list.Add(info);
            var color = mat.GetColor(_ShdPropHurtColor);
            if (color.r == 0
                && color.g == 0
                && color.b == 0
                && color.a == 0)
                color = defColor;

            info.srcColor = color;
        }
        else if (mat.HasProperty(_OverlayColor))
        {
            if (mat.GetFloat(_HitColorChannel) == 2f)
            {
                return;
            }
            if (list == null)
            {
                list = new List<MaterialInfo>();
            }
            turnRedType = TurnRedType.PBR;
            var info = new MaterialInfo();
            info.material = mat;
            info.hitColor = mat.GetColor(_HitColor);
            info.srcColor = mat.GetColor(_OverlayColor);
            info.hitMultiple = mat.GetFloat(_HitMultiple);
            if (mat.GetFloat(_HitColorChannel) == 0f)
            {
                info.srcMultipleValue = 0f;
            }
            else
            {
                info.srcMultipleValue = 1f;
            }
            list.Add(info);
        }
    }

    public void AddSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        turnRedType = TurnRedType.Sprite;
        if (list == null)
        {
            list = new List<MaterialInfo>();
        }
        var info = new MaterialInfo();
        info.spriteRenderer = spriteRenderer;
        info.srcColor = Color.white;
        list.Add(info);
    }

    /*
    public void Test()
    {
        switch (turnRedType)
        {
            case TurnRedType.Sprite:
                {
                    break;
                }

            case TurnRedType.MatCap:
                {
                    break;
                }

            case TurnRedType.PBR:
                {
                    break;
                }
        }
    }
    */
}