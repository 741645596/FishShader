using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class AttackEventBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;
    bool enter;

    XTimelineBridge GetListener()
    {
        var com = playableDirector.gameObject.GetComponent<XTimelineBridge>();
        if (com == null)
        {
            LogUtils.W($"AttackBehaviour 无法找到监听组件 {playableDirector.gameObject.name}");
            return null;
        }
        return com;
    }

    //在创建的时候调用
    public override void OnPlayableCreate(Playable playable)
    {
        //此时，给“导演”赋值，可以类比于实例化吗？别人通过调用这个来控制自己的动画
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    //每次更新的时候都会调用这个函数
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!enter)
        {
            enter = true;
            var listener = GetListener();
            if (listener != null)
            {
                listener.OnTriggerAttackEvent();
            }
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }
}