using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0 / 255f, 230 / 255f, 230 / 255f)]
[TrackClipType(typeof(EffectClip))]

//这里的继承，为TrackAsset
public class EffectTrack : TrackAsset {
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
        foreach (var clip in GetClips()) {
            var effectClip = clip.asset as EffectClip;
            if (effectClip) {
                effectClip.Duration = clip.duration;
            }
        }

        return base.CreateTrackMixer(graph, go, inputCount);
    }
}