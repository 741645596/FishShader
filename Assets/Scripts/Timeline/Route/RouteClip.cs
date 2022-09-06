using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class RouteClip : PlayableAsset
{
    public bool resetStartPos;
    [Range(-3, 3)]
    public float startX;
    [Range(-3, 3)]
    public float startY;
    [Range(-3, 3)]
    public float endX;
    [Range(-3, 3)]
    public float endY;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RouteBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetParam(resetStartPos, startX, startY, endX, endY);
        return playable;
    }
}