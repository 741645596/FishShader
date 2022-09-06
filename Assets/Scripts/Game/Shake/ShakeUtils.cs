using System.Collections.Generic;
using UnityEngine;

public static class ShakeUtils
{
    static GameObject sBackGround;
    static List<ArtShakeManager> sArtShakeManagers = new List<ArtShakeManager>();
    static Dictionary<int, int> sShakeID2Level = new Dictionary<int, int>
    {
        {102, 1},
        {103, 2},
        {114, 3},
    };

    public static void SetShakeID2Level(Dictionary<int, int> dic)
    {
        sShakeID2Level = dic;
    }

    public static void AddArtShakeManager(ArtShakeManager com)
    {
        sArtShakeManagers.Add(com);
    }

    public static void RemoveArtShakeManager(ArtShakeManager com)
    {
        sArtShakeManagers.Remove(com);
    }

    public static GameObject GetBackGroundGameObject()
    {
        if (sBackGround != null)
        {
            return sBackGround;
        }
        return GameObject.Find("MapRoot/View");
    }

    public static void PlayZoomShake(float duration, float minValue, AnimationCurve animationCurve)
    {
        var go = GetBackGroundGameObject();
        if (go == null) return;

        if (go.GetComponent<ZoomShake>() != null)
        {
            return;
        }
        var com = go.AddComponent<ZoomShake>();
        com.animationCurve = animationCurve;
        com.minValue = minValue;
        com.duration = duration;
        com.Play(true);
    }

    public static void PlayPositionShake(float duration, Vector3 Amplitude, float Frequency, AnimationCurve animationCurve)
    {
        var go = GetBackGroundGameObject();
        if (go == null) return;

        if (go.GetComponent<PositionShake>() != null)
        {
            return;
        }
        var com = go.AddComponent<PositionShake>();
        com.animationCurve = animationCurve;
        com.Amplitude = Amplitude;
        com.duration = duration;
        com.Frequency = Frequency;
        com.Play(true);
    }

    public static void Shake(int id)
    {
        if (sShakeID2Level.ContainsKey(id))
        {
            int level = sShakeID2Level[id];
            ShakeLevel(level);
            return;
        }
        if (ShakeLibrary.ShakeMananger.Instance != null)
        {
            ShakeLibrary.ShakeMananger.Instance.PlayAnimation(id);
        }
    }

    public static void ShakeLevel(int level)
    {
        for (int i = 0; i < sArtShakeManagers.Count; i++)
        {
            if (sArtShakeManagers[i] != null)
            {
                sArtShakeManagers[i].PlayShake(level);
            }
        }
    }
}
