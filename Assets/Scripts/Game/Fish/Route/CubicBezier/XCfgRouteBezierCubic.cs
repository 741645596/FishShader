using System.Collections.Generic;
using UnityEngine;
using System;

// 三阶贝塞尔路径
public class XCfgRouteBezierCubic
{
    public int pathId;
    public string desc;
    public List<Vector3> path = new List<Vector3>();
    public List<float> times = new List<float>();
    public List<float> angles = new List<float>();

    


    public void Calc(List<Vector3> points, List<Vector4> ctrls, List<float> speeds)
    {
        List<Vector3> list = new List<Vector3>();
        list.Clear();
        this.times.Clear();
        list.Add(points[0]);
        this.times.Add(0f);
        float num = 0f;
        List<Vector3> list2 = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 control = new Vector2(ctrls[i].z, ctrls[i].w);
            Vector2 control2 = new Vector2(ctrls[i + 1].x, ctrls[i + 1].y);
            list2.Clear();
            GetCubicBezierVertices(points[i], control, control2, points[i + 1], 10, list2);
            list.AddRange(list2.GetRange(1, list2.Count - 1));

            float num2 = speeds[i];
            for (int j = 0; j < list2.Count - 1; j++)
            {
                Vector2 vector = XRouteUtils.View2Design(list2[j]);
                Vector2 vector2 = XRouteUtils.View2Design(list2[j + 1]);
                float num3 = Vector2.Distance(vector, vector2) / num2;
                num += num3;
                this.times.Add(num);
                float num4 = Vector2.SignedAngle(vector2 - vector, Vector2.left);
                this.angles.Add(-num4);
            }
        }
        this.angles.Add(this.angles[this.angles.Count - 1]);
        int k = 0;
        int count = list.Count;
        //LogUtils.I($"{pathId} {count} {times.Count} {angles.Count}");
        Vector2 size = CameraUtils.GetWorldSize();
        float halfViewWidth = size.x * 0.5f;
        float halfViewHeight = size.y * 0.5f;
        while (k < count)
        {
            Vector3 vector3 = list[k];
            float x = vector3.x - halfViewWidth;
            float y = vector3.y - halfViewHeight;
            this.path.Add(new Vector3(x, y, vector3.z));
            k++;
        }
    }

    void GetCubicBezierVertices(Vector3 origin, Vector2 control1, Vector2 control2, Vector3 destination, int segments, List<Vector3> path)
    {
        float num = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Pow(1f - num, 3f) * origin.x + 3f * Mathf.Pow(1f - num, 2f) * num * control1.x + 3f * (1f - num) * num * num * control2.x + num * num * num * destination.x;
            float y = Mathf.Pow(1f - num, 3f) * origin.y + 3f * Mathf.Pow(1f - num, 2f) * num * control1.y + 3f * (1f - num) * num * num * control2.y + num * num * num * destination.y;
            float z = Mathf.Lerp(origin.z, destination.z, (float)i / (float)segments);
            path.Add(new Vector3(x, y, z));
            num += 1f / (float)segments;
        }
        path.Add(destination);
    }
}

