using UnityEngine;
using UnityEngine.Playables;



//继承PlayableAsset
public class AttackEventClip : PlayableAsset
{
    public override double duration
    {
        get
        {
            return 0.5f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<AttackEventBehaviour>.Create(graph, 1);
        //var behaviour = playable.GetBehaviour();
        return playable;
    }
}