using System.Collections.Generic;
using UnityEngine;

public static class XRouteUtils
{
    public const int DEFAULT_ANGLE = -999;
    static float m_ScaleX = 1;
    static float m_ScaleY = 1;
    static bool m_MirrorFlip = false;

    static float m_RotateLerp = 0.2f;

    private static float design2ViewWidth;

    private static float design2ViewHeight;

    private static float view2DesignWidth;

    private static float view2DesignHeight;

    static Vector2 WorldSize = new Vector2(11.36f, 6.4f);

    public static float ScaleX { get { return m_ScaleX; } }
    public static float ScaleY { get { return m_ScaleY; } }
    public static bool MirrorFlip { get { return m_MirrorFlip; } }

    public static float RotateLerp { get { return m_RotateLerp; } }



    public static void Init()
    {
        if (Camera.main != null)
        {
            Vector3 size = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            WorldSize.x = size.x * 2;
            WorldSize.y = size.y * 2;
            m_ScaleX = size.x / 11.360f * 2.0f;
            m_ScaleY = size.y / 6.400f * 2.0f;
            design2ViewWidth = WorldSize.x / 1136f;
            design2ViewHeight = WorldSize.y / 640f;
            view2DesignWidth = 1136f / WorldSize.x;
            view2DesignHeight = 640f / WorldSize.y;
            LogUtils.V($"XRouteUtils Init {m_ScaleX}  {m_ScaleY} {size.ToString()}");
            LogUtils.V($"XRouteUtils {design2ViewWidth} {design2ViewHeight} {view2DesignWidth} {view2DesignHeight}");
        }
        else
        {
            LogUtils.E("XRouteUtils Init Cann't Find Main Camera");
        }
    }

    public static void SetScreenScale(float sx, float sy)
    {
        m_ScaleX = sx;
        m_ScaleY = sy;
    }

    public static void SetMirrorFlip(bool bMirrorFlip)
    {
        m_MirrorFlip = bMirrorFlip;
    }

    public static void SetRotateLerp(float lerp)
    {
        m_RotateLerp = lerp;
    }
    #region 二阶贝塞尔曲线
    public static Vector2 Design2View(Vector2 pt)
    {
        float x = pt.x * design2ViewWidth;
        float y = pt.y * design2ViewHeight;
        return new Vector2(x, y);
    }

    public static Vector2 View2Design(Vector2 pt)
    {
        float x = pt.x * view2DesignWidth;
        float y = pt.y * view2DesignHeight;
        return new Vector2(x, y);
    }

    public static void SpeedAndAction(string str, List<float> speeds)
    {
        foreach (string text in str.Split(new char[]
        {
                ';'
        }))
        {
            if (text.Length > 0)
            {
                string[] array2 = text.Split(new char[]
                {
                        ','
                });
                if (array2.Length > 1)
                {
                    speeds.Add(float.Parse(array2[0]));
                }
                else
                {
                    speeds.Add(float.Parse(text));
                }
            }
        }
    }

    public static List<Vector3> Str2Vec3Array(string str)
    {
        List<Vector3> list = new List<Vector3>();
        foreach (string text in str.Split(new char[]
        {
                ';'
        }))
        {
            if (text.Length > 0)
            {
                list.Add(Str2Vec3(text));
            }
        }
        return list;
    }

    public static List<Vector4> Str2Vec4Array(string str)
    {
        List<Vector4> list = new List<Vector4>();
        foreach (string text in str.Split(new char[]
        {
                ';'
        }))
        {
            if (text.Length > 0)
            {
                list.Add(Str2Vec4(text));
            }
        }
        return list;
    }

    public static Vector3 Str2Vec3(string str)
    {
        string[] array = str.Split(new char[]
        {
                ','
        });
        float x = float.Parse(array[0]);
        float y = float.Parse(array[1]);
        float z = float.Parse(array[2]);
        Vector2 vector = XRouteUtils.Design2View(new Vector2(x, y));
        return new Vector3(vector.x, vector.y, z);
    }

    public static Vector4 Str2Vec4(string str)
    {
        string[] array = str.Split(new char[]
        {
                ','
        });
        float x = float.Parse(array[0]);
        float y = float.Parse(array[1]);
        float x2;
        float y2;
        if (array.Length == 4)
        {
            x2 = float.Parse(array[2]);
            y2 = float.Parse(array[3]);
        }
        else
        {
            x2 = float.Parse(array[3]);
            y2 = float.Parse(array[4]);
        }
        Vector2 vector = XRouteUtils.Design2View(new Vector2(x, y));
        Vector2 vector2 = XRouteUtils.Design2View(new Vector2(x2, y2));
        return new Vector4(vector.x, vector.y, vector2.x, vector2.y);
    }
    #endregion

    #region 二阶贝塞尔曲线
    // 计算二阶贝塞尔曲线控制点
    public static Vector2 GetBezierMiddlePosByRate(Vector2 startPos, Vector2 endPos, float rate)
    {
        Vector2 ret = new Vector2(0, 0);
        Vector2 p = endPos - startPos;
        Vector2 middle = (startPos + endPos) / 2;
        Vector2 tmp = new Vector2(p.y, -p.x);
        ret = tmp / 2 + middle;
        if (rate > 0)
        {
            ret.x = ret.x + tmp.x / 2 * (rate - 1);
            ret.y = ret.y + tmp.y / 2 * (rate - 1);
        }
        else
        {
            ret.x = ret.x - tmp.x / 2 * (1 - rate);
            ret.y = ret.y - tmp.y / 2 * (1 - rate);
        }
        return ret;
    }

    // 获取直线位置
    public static Vector2 GetLinePos(XRouteNodeInfo info, float t)
    {
        float x = info.startPos.x + (info.endPos.x - info.startPos.x) * t;
        float y = info.startPos.y + (info.endPos.y - info.startPos.y) * t;
        return new Vector2(x, y);
    }

    // 获取贝塞尔计算后的路径位置
    public static Vector2 GetBezierPos(XRouteNodeInfo info, float t)
    {
        float t1 = (1 - t) * (1 - t);
        float t2 = t * (1 - t);
        float t3 = t * t;
        float x = t1 * info.startPos.x + 2 * t2 * info.middlePos.x + t3 * info.endPos.x;
        float y = t1 * info.startPos.y + 2 * t2 * info.middlePos.y + t3 * info.endPos.y;
        return new Vector2(x, y);
    }

    // 计算贝塞尔斜率
    public static float GetBezierAngleByPos(XRouteNodeInfo info, float t)
    {
        float t1 = 2 * (1 - t) * (-1);
        float t2 = 2 * ((1 - t) + (-1) * t);
        float t3 = 2 * t;
        float x = t1 * info.startPos.x + t2 * info.middlePos.x + t3 * info.endPos.x;
        float y = t1 * info.startPos.y + t2 * info.middlePos.y + t3 * info.endPos.y;
        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }
    #endregion

    public static Vector2 ConvertToWorldPosition(float x, float y)
    {
        const float rate = 100.0f;
        return new Vector2((x - 568) / rate * XRouteUtils.ScaleX, (y - 320) / rate * XRouteUtils.ScaleY);
    }
}
