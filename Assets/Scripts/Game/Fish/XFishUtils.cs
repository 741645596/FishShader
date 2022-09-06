using System;
using System.Collections.Generic;
using UnityEngine;

public static class XFishUtils
{
    static Dictionary<int, XFish> m_ColliderMap = new Dictionary<int, XFish>();

    public static void Init()
    {
        m_ColliderMap = new Dictionary<int, XFish>();
    }

    public static void AddColliderFish(int colliderid, XFish fish)
    {
        m_ColliderMap[colliderid] = fish;
    }

    public static void RemoveColliderFish(int colliderid)
    {
        if (m_ColliderMap.ContainsKey(colliderid))
        {
            m_ColliderMap.Remove(colliderid);
        }
    }

    public static int GetColliderFishUID(int colliderid)
    {
        if (m_ColliderMap.ContainsKey(colliderid))
        {
            return m_ColliderMap[colliderid].GetUID();
        }
        return -1;
    }
}
