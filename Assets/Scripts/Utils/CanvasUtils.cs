using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CanvasUtils
{
    static float width = 1920;
    static float height = 1080;

    // 是否宽屏
    public static bool IsWideScreen()
    {
        float width = Screen.width;
        float height = Screen.height;
        float factor = width > height ? width / height : height / width;
        return factor > 1.85f;
    }

    public static void AdaptCanvas()
    {
        if (GameObject.Find("Canvas") != null)
        {
            var canvasScaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
            float matchWidthOrHeight = IsWideScreen() ? 1f : 0f;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            LogUtils.V("CanvasUtils AdaptCanvas matchWidthOrHeight", matchWidthOrHeight);
        }
        else
        {
            LogUtils.E("AdaptCanvas Cann't Find Canvas");
        }
    }

    public static void Init()
    {
        if (GameObject.Find("Canvas") == null)
        {
            return;
        }
        var tf = GameObject.Find("Canvas").GetComponent<RectTransform>();
        width = tf.sizeDelta.x;
        height = tf.sizeDelta.y;
        LogUtils.V($"CanvasUtils Init {width} {height}");
    }

    public static float GetWidth()
    {
        return width;
    }

    public static float GetHeight()
    {
        return height;
    }

    public static void RemoveAllView()
    {
        if (GameObject.Find("Canvas") != null)
        {
            var root = GameObject.Find("Canvas").transform;
            int count = root.childCount;
            List<GameObject> list = new List<GameObject>();
            for (int i = count - 1; i >= 0; i--)
            {
                list.Add(root.GetChild(i).gameObject);
            }
            for (int i = 0; i < list.Count; i++)
            {
                GameObject.Destroy(list[i]);
            }
        }
        else
        {
            LogUtils.E("RemoveAllView Cann't Find Canvas");
        }
        
    }
}

