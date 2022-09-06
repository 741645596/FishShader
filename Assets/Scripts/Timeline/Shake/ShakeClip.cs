using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class ShakeClip : PlayableAsset
{
    public ShakeDefine shakeid = ShakeDefine.GoldFish;
    [SerializeField, HideInInspector]
    public int customShakeid;

    public override double duration
    {
        get
        {
            return 0.5f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ShakeBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetShakeId(shakeid);
        return playable;
    }
}