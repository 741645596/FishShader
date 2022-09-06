using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class TweenClip : PlayableAsset
{
    public string tweenName = "";
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

    public override double duration
    {
        get
        {
            return 1.0f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TweenBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetTween(tweenName, animationCurve);
        return playable;
    }
}