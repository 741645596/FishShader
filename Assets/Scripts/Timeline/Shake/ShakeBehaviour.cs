using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class ShakeBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;

    ShakeDefine shakeid = ShakeDefine.GoldFish;
    bool enter;

    //在创建的时候调用
    public override void OnPlayableCreate(Playable playable)
    {
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    //每次更新的时候都会调用这个函数
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!enter)
        {
            enter = true;
            ShakeUtils.Shake((int)shakeid);
        }
    }


    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }

    public void SetShakeId(ShakeDefine id)
    {
        shakeid = id;
    }
}