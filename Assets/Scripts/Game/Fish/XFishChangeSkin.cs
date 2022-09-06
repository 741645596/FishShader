using System;
using UnityEngine;
using System.Collections.Generic;

class XFishChangeSkin : MonoBehaviour
{
    public GameObject[] Nodes;
    public float Interval = 5.0f;
    float time = 0;
    int index = -1;

    public void Reset()
    {
        time = Interval + 1;
    }

    public void UpdateSkin()
    {
        time += Time.deltaTime;
        if (time > Interval)
        {
            time = 0;
            UpdateNext();
        }
    }

    void UpdateNext()
    {
        int count = Nodes.Length;
        if (index >= 0 && index < count)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (i != index)
                {
                    list.Add(i);
                }
            }
            int r = UnityEngine.Random.Range(0, count - 1);
            index = list[r];
        }
        else
        {
            index = UnityEngine.Random.Range(0, count);
        }
        for (int i = 0; i < count; i++)
        {
            Nodes[i].SetActive(i == index);
        }
    }
}
