using System;

// 不能在中间穿插值，只能添加到最后面
public enum XTimelineEffectType
{
    FX,                 // 播放特效
    Event,              // 触发事件
    Shake,              // 震屏
    Audio,              // 播放音效
    Animation,          // 播放动画
    DropCoin,           // 掉落金币
    DropLabel,          // 掉落数字
    End,                // 结束标记
    SmallBonusWheel,    // 普通大鱼彩盘
    GoldBonusWheel,     // 奖金鱼彩盘
    BossBonusWheel,     // boss彩盘
    Tween,              // Tween 动画
    Active,             // 激活对象
    InActive,           // 取消激活对象
    FishFadeOut,        // 鱼渐隐消失
    ChangeTo3DCamera,   // 切换到3d摄像机
    PropDropWheel,      // 物品掉落彩盘
}
