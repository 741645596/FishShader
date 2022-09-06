using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(255 / 255f, 165 / 255f, 0 / 255f)]
[TrackClipType(typeof(AudioCustomClip))]

//这里的继承，为TrackAsset
public class AudioCustomTrack : TrackAsset
{

}