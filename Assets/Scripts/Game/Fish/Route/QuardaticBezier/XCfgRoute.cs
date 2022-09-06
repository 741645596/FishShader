using UnityEngine;
using System.Collections.Generic;

// 单条路径配置信息
public class XCfgRoute
{
    public int id;
    public Vector2 startPos; // 起始位置
    public int routeType; //
    public float totalTime;
    public List<XCfgRouteNodeInfo> pathInfos;
    public int fadeAway;
    public int swordAction;
    public float rotate;
    public float swordTime;
    public float angle;
    public float slope;
}