using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class AnimatorBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;
    bool enter;
    string triggerName;

    XTimelineBridge GetListener()
    {
        var com = playableDirector.gameObject.GetComponent<XTimelineBridge>();
        if (com == null)
        {
            LogUtils.W($"AnimatorBehaviour 无法找到监听组件 {playableDirector.gameObject.name}");
            return null;
        }
        return com;
    }

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
            var listener = GetListener();
            if (listener != null)
            {
                listener.OnTriggerAnimatorTrigger(triggerName);
            }
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }

    public void SetTriggerName(string name)
    {
        triggerName = name;
    }
}