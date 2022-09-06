using UnityEngine;

// 路径结点配置信息
public class XCfgRouteNodeInfo
{
    public Vector2 position; // 当前结点位置
    public float time; // 移动时间
    public int type; // 路径类型
    public float rate; // 贝塞尔比率
    public float speed;
    public float lerp; // 旋转插值
    public int playAni; // 是否播放动画
}
