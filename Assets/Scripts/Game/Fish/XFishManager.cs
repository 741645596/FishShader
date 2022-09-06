using System;
using System.Collections.Generic;
using UnityEngine;

public class XFishManager : MonoBehaviour
{
    static XFishManager sInstance;
    public static XFishManager Instance { get { return sInstance; } }
    List<XFish> m_FishList;
    Dictionary<int, XFish> m_FishDic;
    public int FishCount;

    private void Start()
    {
        XFishUtils.Init();
        XRouteUtils.Init();
        sInstance = this;
        m_FishList = new List<XFish>();
        m_FishDic = new Dictionary<int, XFish>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Time.deltaTime == 0) return;
#endif
        UpdateAllFish(Time.deltaTime);
    }

    private void UpdateAllFish(float dt)
    {
        FishCount = m_FishList.Count;
        for (int i = m_FishList.Count - 1; i >= 0; i--)
        {
            m_FishList[i].UpdateFish(dt);
        }
    }

    public void SetFishEnable(XFish fish, bool enabled)
    {
        if (enabled)
        {
            fish.StartUpdate();
            m_FishList.Add(fish);
            m_FishDic[fish.GetUID()] = fish;
        }
        else
        {
            m_FishDic.Remove(fish.GetUID());
            m_FishList.Remove(fish);
        }
    }

    public XFish FindFish(int uid)
    {
        XFish ret = null;
        if (!m_FishDic.TryGetValue(uid, out ret))
        {
            return null;
        }
        return ret;
    }
}
