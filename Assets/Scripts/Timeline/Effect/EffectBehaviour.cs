using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class EffectBehaviour : PlayableBehaviour
{
    PlayableDirector playableDirector;
    bool enter;
    GameObject prefab;
    bool worldPosition;
    Vector3 offset = Vector3.zero;

    XTimelineBridge GetListener()
    {
        var com = playableDirector.gameObject.GetComponent<XTimelineBridge>();
        if (com == null)
        {
            LogUtils.W($"EffectBehaviour 无法找到监听组件 {playableDirector.gameObject.name}");
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
            if (listener != null && prefab != null)
            {
                listener.OnTriggerPlayEffect(prefab, worldPosition, offset, duration);
            }
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }

    private float duration;
    public void SetParam(GameObject go, bool w, Vector3 pos, float duration)
    {
        prefab = go;
        worldPosition = w;
        offset = pos;
        this.duration = duration;
    }
}