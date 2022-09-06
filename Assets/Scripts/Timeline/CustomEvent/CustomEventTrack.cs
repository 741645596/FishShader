using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(230 / 255f, 0 / 255f, 200f / 255f)]
[TrackClipType(typeof(CustomEventClip))]

//这里的继承，为TrackAsset
public class CustomEventTrack : TrackAsset
{

}