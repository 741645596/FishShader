using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(0 / 255f, 0 / 255f, 255 / 255f)]
[TrackClipType(typeof(TweenClip))]

//这里的继承，为TrackAsset
public class TweenTrack : TrackAsset
{

}