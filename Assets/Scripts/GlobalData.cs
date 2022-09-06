using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEngine;

public static class GlobalData
{
    static float musicVolume;
    static float soundVolume;
    public static float GetMusicVolume()
    {
        return musicVolume;
    }

    public static void SetMusicVolume(float volume)
    {
        musicVolume = volume;
    }

    public static float GetSoundVolume()
    {
        return soundVolume;
    }

    public static void SetSoundVolume(float volume)
    {
        soundVolume = volume;
    }
}
