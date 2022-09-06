using System;

[System.Serializable]
public class XTimelineNode
{
    public XTimelineEffectType effectType;
    public float delayTime;
    public string param;

    public XTimelineNode()
    {
        effectType = XTimelineEffectType.FX;
        delayTime = 0;
        param = "";
    }
}