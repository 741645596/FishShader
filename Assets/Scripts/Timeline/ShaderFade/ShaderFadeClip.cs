using System;
using UnityEngine;
using UnityEngine.Playables;

//继承PlayableAsset
public class ShaderFadeClip : PlayableAsset {
    public string ShaderProperty = "_Alpha";
    public float Duration;
    public bool EndReset;

    public override double duration
    {
        get
        {
            return 0.5f;
        }
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var playable = ScriptPlayable<ShaderFadeBehaviour>.Create(graph, 1);

        var du = (float)Duration;
        var behaviour = playable.GetBehaviour();
        behaviour.SetParam(ShaderProperty, du, EndReset);
        return playable;
    }
}