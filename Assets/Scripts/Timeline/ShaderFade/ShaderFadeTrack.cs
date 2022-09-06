using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(100 / 255f, 230 / 255f, 230 / 255f)]
[TrackClipType(typeof(ShaderFadeClip))]

//这里的继承，为TrackAsset
public class ShaderFadeTrack : TrackAsset {

}