using System;

public enum XRouteActionType
{
    Event,              // 触发事件
    Shake,              // 震屏
    Audio,              // 播放音效
    Animation,          // 播放动画
    Tween,              // Tween 动画
    Active,             // 激活对象
    InActive,           // 取消激活对象
    DisableCollision,   // 取消碰撞
    RandomModel,        // 随机显示模型
}
