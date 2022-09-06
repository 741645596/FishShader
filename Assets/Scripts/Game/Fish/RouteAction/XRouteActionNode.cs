using System;

[System.Serializable]
public class XRouteTimelineNode
{
    public XRouteActionType effectType;
    public float delayTime;
    public string param;

    public XRouteTimelineNode()
    {
        effectType = XRouteActionType.Shake;
        delayTime = 0;
        param = "";
    }
}

[System.Serializable]
public class XRouteActionNode
{
    public int Rate = 100;
    public XRouteActionCondition Condition;
    public int CompareValue = 0;
    public int AudioRate = 100;
    public XRouteTimelineNode[] Nodes;
}