using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class RouteBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;
    XTimelineBridge mBridge;
    bool resetStartPos;
    float startX;
    float startY;
    float endX;
    float endY;
    float m_Time;


    XTimelineBridge GetListener()
    {
        if (mBridge != null)
        {
            return mBridge;
        }
        var com = playableDirector.gameObject.GetComponent<XTimelineBridge>();
        if (com == null)
        {
            LogUtils.W($"AnimatorBehaviour 无法找到监听组件 {playableDirector.gameObject.name}");
            return null;
        }
        mBridge = com;
        return com;
    }

    //在创建的时候调用
    public override void OnPlayableCreate(Playable playable)
    {
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    Vector2 CalcPosByPercent(float px, float py)
    {
        var size = CameraUtils.GetWorldSize();
        return new Vector2(px * size.x / 2, py * size.y / 2);
    }

    //每次更新的时候都会调用这个函数
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var bridge = GetListener();
        if (bridge == null)
        {
            return;
        }
        float dt = bridge.GetEvaluateTime();
        if (!(dt > 0))
        {
            dt = info.deltaTime;
        }
        m_Time += dt;
        float percent = m_Time / (float)playable.GetDuration();
        Vector2 startPos = CalcPosByPercent(startX, startY);
        Vector2 endPos = CalcPosByPercent(endX, endY);
        Vector2 pos = Vector2.Lerp(startPos, endPos, percent);
        bridge.OnRoutePosition(pos);
    }

    public override void OnGraphStart(Playable playable)
    {
        m_Time = 0;
    }

    public void SetParam(bool resetStartPos, float sx, float sy, float dx, float dy)
    {
        this.resetStartPos = resetStartPos;
        startX = sx;
        startY = sy;
        endX = dx;
        endY = dy;
    }
}