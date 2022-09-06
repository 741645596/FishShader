using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(230 / 255f, 230 / 255f, 0 / 255f)]
[TrackClipType(typeof(ShakeClip))]
[TrackClipType(typeof(ZoomShakeClip))]
[TrackClipType(typeof(PositionShakeClip))]

//这里的继承，为TrackAsset
public class ShakeTrack : TrackAsset
{

}