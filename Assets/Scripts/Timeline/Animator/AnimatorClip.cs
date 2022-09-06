using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class AnimatorClip : PlayableAsset
{
    public string triggerName;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        
        var playable = ScriptPlayable<AnimatorBehaviour>.Create(graph, 1);
        AnimatorBehaviour behaviour = playable.GetBehaviour();
        behaviour.SetTriggerName(triggerName);
        return playable;
    }
}