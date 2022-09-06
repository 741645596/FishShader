using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//继承PlayableAsset
public class AudioCustomClip : PlayableAsset
{
    double m_Duration = 0.5f;
    public AudioClip clip;
    //public bool loop;

    public override double duration
    {
        get
        {
            if (clip == null)
                return base.duration;

            // use this instead of length to avoid rounding precision errors,
            return (double)clip.samples / clip.frequency;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        if (clip == null)
        {
            return Playable.Null;
        }
        var playable = ScriptPlayable<AudioCustomBehaviour>.Create(graph, 1);
        var behaviour = playable.GetBehaviour();
        behaviour.SetAudioClip(clip);
        return playable;
    }


}