using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]

public class ZoomShakeBehaviour : PlayableBehaviour
{
    private PlayableDirector playableDirector;
    float minValue;
    AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.5f, 1f, 1f, 1f), new Keyframe(1.0f, 1f, -0.6f, -0.6f));

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
            ShakeUtils.PlayZoomShake((float)playable.GetDuration(), minValue, animationCurve);
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        enter = false;
    }

    public void SetShakeParam(float minValue, AnimationCurve ac)
    {
        this.minValue = minValue;
        animationCurve = ac;
    }
}