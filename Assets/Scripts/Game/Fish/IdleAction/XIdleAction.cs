using System;
using System.Collections.Generic;
using UnityEngine;

// 待机动作
public class XIdleAction : MonoBehaviour
{
    public static bool ShowLog = false;
    public int Rate = 100;
    public int AudioRate = 100;
    public int Low = 0;
    public int High = 0;
    public float Interval;
    public XIdleTimelineNode[] Nodes;
    public GameObject[] GameObjects;

    float m_Time;
    float m_NextTime = -1;
    List<XIdleTimelineNode> m_PlayableList = new List<XIdleTimelineNode>();

    Action<int> m_AudioPlayer;
    Action<string> m_EventPlayer;

    int m_CurrentAudioRate;

    XFish m_XFish;

    public void Reset()
    {
        int r = UnityEngine.Random.Range(1, 101);
        if (r > Rate)
        {
            m_NextTime = 999.0f;
        }
        //LogUtils.V($"XIdleAction Rate {r}");
    }

    public void Clear()
    {
        m_PlayableList.Clear();
        m_AudioPlayer = null;
        m_EventPlayer = null;
    }

    public void SetXFish(XFish fish)
    {
        m_XFish = fish;
    }

    public void SetAudioPlayer(Action<int> cb)
    {
        m_AudioPlayer = cb;
    }

    public void SetEventPlayer(Action<string> cb)
    {
        m_EventPlayer = cb;
    }

    public void UpdateIdleAction(float dt)
    {
        if (m_NextTime < 0)
        {
            m_NextTime = UnityEngine.Random.Range(Low, High) * Interval;
            return;
        }
        m_Time += dt;
        if (m_Time > m_NextTime)
        {
            m_Time -= m_NextTime;
            m_NextTime = -1;
            for (int i = 0; i < Nodes.Length; i++)
            {
                m_PlayableList.Add(Nodes[i]);
            }
        }
        for (int i = m_PlayableList.Count - 1; i >= 0; i--)
        {
            var unit = m_PlayableList[i];
            if (m_Time > unit.delayTime)
            {
                PlayEffect(unit);
                m_PlayableList.RemoveAt(i);
            }
        }
    }

    private void PlayEffect(XIdleTimelineNode unit)
    {
        //LogUtils.V(unit.effectType);
        switch (unit.effectType)
        {
            case XRouteActionType.Event:
                {
                    PlayEvent(unit.param);
                    break;
                }
            case XRouteActionType.Shake:
                {
                    PlayShake(unit);
                    break;
                }
            case XRouteActionType.Audio:
                {
                    PlayAudio(unit);
                    break;
                }
            case XRouteActionType.Animation:
                {
                    PlayAnimation(unit);
                    break;
                }

            case XRouteActionType.Active:
                {
                    SetGameObjectActive(unit.param, true);
                    break;
                }

            case XRouteActionType.InActive:
                {
                    SetGameObjectActive(unit.param, false);
                    break;
                }

            case XRouteActionType.DisableCollision:
                {
                    m_XFish?.SetColliderEnable(false);
                    break;
                }

            case XRouteActionType.RandomModel:
                {
                    RandomModel(unit.param);
                    break;
                }

            default:
                {
                    LogUtils.V("无效的路径动作");
                    break;
                }
        }

    }

    private void PlayTween(XIdleTimelineNode unit)
    {
        int id = 0;
        if (!int.TryParse(unit.param, out id))
        {
            LogUtils.W("PlayTween 无效的id 非整形");
            return;
        }

        var array = GetComponents<TweenerBase>();
        for (int i = 0; i < array.Length; i++)
        {
            var tweener = array[i];
            if (tweener.TweenGroup == id)
            {
                tweener.ResetToBeginning();
            }
        }
    }


    private void PlayEvent(string evt)
    {
        m_EventPlayer?.Invoke(evt);
    }

    private void PlayShake(XIdleTimelineNode unit)
    {
        int id = 0;
        if (int.TryParse(unit.param, out id))
        {
            ShakeUtils.Shake(id);
        }
    }

    private void PlayAudio(XIdleTimelineNode unit)
    {
        int r = UnityEngine.Random.Range(0, 100);
        if (r > m_CurrentAudioRate)
        {
            return;
        }
        int id = 0;
        if (unit.param.IndexOf(",") != -1)
        {
            var array = unit.param.Split(',');
            r = UnityEngine.Random.Range(0, array.Length);
            if (!int.TryParse(array[r], out id))
            {
                LogUtils.W("PlayAudio 无效的id 非整形");
                return;
            }
        }
        else
        {
            if (!int.TryParse(unit.param, out id))
            {
                LogUtils.W("PlayAudio 无效的id 非整形");
                return;
            }
        }
        m_AudioPlayer?.Invoke(id);
    }

    private void PlayAnimation(XIdleTimelineNode unit)
    {
        var animators = gameObject.GetComponentsInChildren<Animator>();
        if (animators.Length == 0)
        {
            LogUtils.W("PlayAnimation 找不到animator");
            return;
        }
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetTrigger(unit.param);
        }
    }

    private void SetGameObjectActive(string param, bool active)
    {
        int idx = 0;
        if (!int.TryParse(param, out idx))
        {
            LogUtils.W("SetGameObjectActive 无效的id 非整形");
            return;
        }
        if (GameObjects != null && idx >= 0 && idx < GameObjects.Length)
        {
            GameObjects[idx].SetActive(active);
        }
    }

    // 这个方法默认是从第0个开始的
    private void RandomModel(string param)
    {
        int idx = 0;
        if (!int.TryParse(param, out idx))
        {
            LogUtils.W("SetGameObjectActive 无效的id 非整形");
            return;
        }
        if (idx >= GameObjects.Length) return;
        int r = UnityEngine.Random.Range(0, idx);
        if (GameObjects[r].activeSelf)
        {
            r += 1;
            if (r >= GameObjects.Length)
                r = 0;
        }
        //LogUtils.I("RandomModel id:" + r);
        for (int i = 0; i <= idx; i++)
        {
            GameObjects[i].SetActive(i == r);
        }
    }
}
