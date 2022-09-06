using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class ZoomShakeClip : PlayableAsset
{
    public float minValue = 1.0f;
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1.0f, 1f, -0.6f, -0.6f));

    public override double duration
    {
        get
        {
            return 1.0f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ZoomShakeBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetShakeParam(minValue, animationCurve);
        return playable;
    }
}