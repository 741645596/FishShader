using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class PositionShakeBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;
    public Vector3 Amplitude;
    public float Frequency;
    public AnimationCurve animationCurve;

    bool enter;

    public override void OnPlayableCreate(Playable playable)
    {
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!enter)
        {
            enter = true;
            ShakeUtils.PlayPositionShake((float)playable.GetDuration(), Amplitude, Frequency, animationCurve);
        }
    }


    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }
}