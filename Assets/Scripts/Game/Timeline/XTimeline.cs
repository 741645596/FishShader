using System;
using System.Collections.Generic;
using UnityEngine;

public class XTimeline : MonoBehaviour
{
    public int TimelineGroup = 0;
    public int AudioRate = 100;
    [SerializeField]
    public XTimelineNode[] Nodes;
    public GameObject[] GameObjects;
    float m_TotalTime;
    float m_Time;
    List<XTimelineNode> m_PlayableList;

    Action<int> m_AudioPlayer;
    Action<int> m_EffectPlayer;
    Action<string> m_EventPlayer;
    bool m_EnableShake;

    private void Awake()
    {
        m_TotalTime = -1;
        m_PlayableList = new List<XTimelineNode>();
    }

    public void Update()
    {
        m_Time += Time.deltaTime;
        for (int i = m_PlayableList.Count - 1; i >= 0; i--)
        {
            var unit = m_PlayableList[i];
            if (m_Time > unit.delayTime)
            {
                PlayEffect(unit);
                m_PlayableList.RemoveAt(i);
            }
        }
        if (m_PlayableList.Count == 0)
        {
            enabled = false;
        }
    }

    public int GetTimelineGroup()
    {
        return TimelineGroup;
    }

    public void SetEnableShake(bool bo)
    {
        m_EnableShake = bo;
    }

    public void SetAudioPlayer(Action<int> cb)
    {
        m_AudioPlayer = cb;
    }

    public void SetEffectPlayer(Action<int> cb)
    {
        m_EffectPlayer = cb;
    }

    public void SetEventPlayer(Action<string> cb)
    {
        m_EventPlayer = cb;
    }

    private void PlayEffect(XTimelineNode unit)
    {
        //LogUtils.V(unit.effectType);
        switch (unit.effectType)
        {
            case XTimelineEffectType.FX:
                {
                    PlayFX(unit);
                    break;
                }
            case XTimelineEffectType.Event:
                {
                    PlayEvent(unit.param);
                    break;
                }
            case XTimelineEffectType.Shake:
                {
                    PlayShake(unit);
                    break;
                }
            case XTimelineEffectType.Audio:
                {
                    PlayAudio(unit);
                    break;
                }
            case XTimelineEffectType.Animation:
                {
                    PlayAnimation(unit);
                    break;
                }
            case XTimelineEffectType.DropLabel:
            case XTimelineEffectType.DropCoin:
            case XTimelineEffectType.SmallBonusWheel:
            case XTimelineEffectType.BossBonusWheel:
            case XTimelineEffectType.GoldBonusWheel:
            case XTimelineEffectType.PropDropWheel:
                {
                    string evt = unit.effectType.ToString();
                    PlayEvent(evt);
                    break;
                }

            case XTimelineEffectType.Tween:
                {
                    PlayTween(unit);
                    break;
                }

            case XTimelineEffectType.Active:
                {
                    SetGameObjectActive(unit.param, true);
                    break;
                }

            case XTimelineEffectType.InActive:
                {
                    SetGameObjectActive(unit.param, false);
                    break;
                }

            case XTimelineEffectType.FishFadeOut:
                {
                    FishFadeOut(unit.param);
                    m_EventPlayer?.Invoke("FishFadeOut");
                    break;
                }

            case XTimelineEffectType.ChangeTo3DCamera:
                {
                    float z = 0;
                    if (!float.TryParse(unit.param, out z))
                    {
                        LogUtils.W("ChangeTo3DCamera 无效的z");
                        return;
                    }
                    GameObjectUtils.ChangeLayer(gameObject, "Fish3D");
                    XCamera3DManager.AddGameObject(gameObject);
                    var pos = transform.localPosition;
                    pos.z = z;
                    gameObject.transform.localPosition = pos;
                    break;
                }
        }
    }

    private void PlayTween(XTimelineNode unit)
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
                tweener.ResetToBeginningAndPlayForward();
            }
        }
    }

    private void PlayFX(XTimelineNode unit)
    {
        int id = 0;
        if (!int.TryParse(unit.param, out id))
        {
            LogUtils.W("PlayFX 无效的id 非整形");
            return;
        }
        m_EffectPlayer?.Invoke(id);
    }

    private void PlayEvent(string evt)
    {
        m_EventPlayer?.Invoke(evt);
    }

    private void PlayShake(XTimelineNode unit)
    {
        if (!m_EnableShake) return;
        int id = 0;
        if (int.TryParse(unit.param, out id))
        {
            ShakeUtils.Shake(id);
        }
    }

    private void PlayAudio(XTimelineNode unit)
    {
        int r = UnityEngine.Random.Range(0, 100);
        if (r > AudioRate)
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

    private void PlayAnimation(XTimelineNode unit)
    {
        XFish fish = GetComponent<XFish>();
        if (fish != null)
        {
            fish.SetTrigger(unit.param);
        }
        else
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

    private void FishFadeOut(string param)
    {
        float duration = 0;
        if (!float.TryParse(param, out duration))
        {
            LogUtils.W("FishFadeOut 无效的 持续时间");
            return;
        }
        var com = GetComponent<XFish>();
        if (com != null)
        {
            com.PlayFadeOut(duration);
        }
    }

    public void Play()
    {
        Play(0);
    }

    public void Play(float startTime)
    {
        m_EnableShake = true;
        if (Nodes == null || Nodes.Length == 0)
        {
            return;
        }
        m_Time = 0;
        enabled = true;
        m_PlayableList.Clear();
        for (int i = 0; i < Nodes.Length; i++)
        {
            m_PlayableList.Add(Nodes[i]);
        }
    }

    public void Stop()
    {
        m_Time = 0;
        enabled = false;
        m_PlayableList.Clear();
    }

    public float GetTotalTime()
    {
        if (m_TotalTime > 0)
        {
            return m_TotalTime;
        }
        for (int i = 0; i < Nodes.Length; i++)
        {
            if (Nodes[i].delayTime > m_TotalTime)
            {
                m_TotalTime = Nodes[i].delayTime;
            }
        }
        return m_TotalTime;
    }
}
