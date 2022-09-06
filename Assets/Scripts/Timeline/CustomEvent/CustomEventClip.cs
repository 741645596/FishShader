using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class CustomEventClip : PlayableAsset
{
    public string evt;
    public override double duration
    {
        get
        {
            return 0.5f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CustomEventBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetEvent(evt);
        return playable;
    }
}