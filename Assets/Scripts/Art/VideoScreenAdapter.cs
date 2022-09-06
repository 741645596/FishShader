using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
class VideoScreenAdapter : MonoBehaviour
{
    public float soundVolume;
    VideoPlayer vp;
    public void Start()
    {
        vp = GetComponent<VideoPlayer>();
        vp.aspectRatio = CanvasUtils.IsWideScreen() ? VideoAspectRatio.FitHorizontally : VideoAspectRatio.FitVertically;
    }

    private void Update()
    {
        soundVolume = GlobalData.GetSoundVolume();
        vp.SetDirectAudioVolume(0, soundVolume);
    }
}
