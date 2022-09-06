using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(230 / 255f, 230 / 255f, 230 / 255f)]
[TrackClipType(typeof(AnimatorClip))]

//这里的继承，为TrackAsset
public class AnimatorTrack : TrackAsset
{

}