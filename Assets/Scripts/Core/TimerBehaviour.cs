using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerBehaviour : MonoBehaviour
{
    class tagTimer
    {
        public int needRemove;
        public int id;
        public float interval;
        public float elapsed;
        public int count;
        public Action callback;
    }
    int m_Index;
    List<tagTimer> m_TimerList;

    private void Start()
    {

    }

    private void Update()
    {
        if (m_TimerList == null) return;
        for (int i = m_TimerList.Count - 1; i >= 0; i--)
        {
            var timer = m_TimerList[i];
            if (timer.needRemove == 1)
            {
                m_TimerList.RemoveAt(i);
                continue;
            }
            timer.elapsed += Time.deltaTime;
            if (timer.count < 0)
            {
                if (timer.elapsed > timer.interval)
                {
                    timer.callback?.Invoke();
                    timer.elapsed -= timer.interval;
                }
            }
            else
            {
                if (timer.elapsed > timer.interval)
                {
                    timer.callback?.Invoke();
                    timer.elapsed -= timer.interval;
                    timer.count--;
                    if (timer.count == 0)
                    {
                        m_TimerList.RemoveAt(i);
                    }
                }
            }
        }
        if (m_TimerList.Count == 0)
        {
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        m_TimerList = null;
    }

    public int StartTimer(float interval, int count, Action callback)
    {
        m_Index++;
        if (m_TimerList == null)
        {
            m_TimerList = new List<tagTimer>();
        }
        var timer = new tagTimer();
        timer.id = m_Index;
        timer.interval = interval;
        timer.count = count;
        timer.callback = callback;
        m_TimerList.Add(timer);
        enabled = true;
        return m_Index;
    }

    public void StopTimer(int id)
    {
        if (m_TimerList != null)
        {
            for (int i = m_TimerList.Count - 1; i >= 0; i--)
            {
                if (m_TimerList[i].id == id)
                {
                    m_TimerList[i].needRemove = 1;
                    break;
                }
            }
        }
    }
}
