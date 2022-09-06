using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//继承PlayableAsset
public class EffectClip : PlayableAsset {
    double m_Duration = 0.5f;

    public GameObject prefab;
    public bool worldPosition;
    public Vector3 offset;

    [NonSerialized]
    public double Duration;
    public override double duration {
        get {
            if (prefab == null)
                return 0;

            var calcParticleSystemDuration = GameObjectUtils.CalcParticleSystemDuration(prefab);
            if (calcParticleSystemDuration == 0)
                return Duration;

            return calcParticleSystemDuration;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var playable = ScriptPlayable<EffectBehaviour>.Create(graph, 1);

        var du = (float)Duration;
        var behaviour = playable.GetBehaviour();
        behaviour.SetParam(prefab, worldPosition, offset, du);
        return playable;
    }
}