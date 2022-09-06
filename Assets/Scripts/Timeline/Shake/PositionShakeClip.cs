using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class PositionShakeClip : PlayableAsset
{
    public Vector3 Amplitude = new Vector3(0.5f, 0.55f, 0);
    public float Frequency = 30.0f;
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.5f, 1f, 1f, 1f), new Keyframe(1.0f, 0f, -0.6f, -0.6f));

    public override double duration
    {
        get
        {
            return 1.0f;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PositionShakeBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.Amplitude = Amplitude;
        behaviour.animationCurve = animationCurve;
        behaviour.Frequency = Frequency;
        return playable;
    }
}