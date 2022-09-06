using System;
using System.Collections.Generic;
using UnityEngine;

public class XRouteAction : MonoBehaviour
{
    public static bool ShowLog = false;
    [SerializeField]
    public XRouteActionNode[] Actions;
    public GameObject[] GameObjects;
    float m_Time;
    List<XRouteTimelineNode> m_PlayableList = new List<XRouteTimelineNode>();

    Action<int> m_AudioPlayer;
    Action<string> m_EventPlayer;

    int m_CurrentAudioRate;

    XFish m_XFish;

    List<bool> GameObjectStates = new List<bool>();

    public void Init()
    {
        for (int i = 0; i < GameObjects.Length; i++)
        {
            if (GameObjects[i] != null)
            {
                GameObjectStates.Add(GameObjects[i].activeSelf);
            }
            else
            {
                GameObjectStates.Add(false);
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < GameObjects.Length; i++)
        {
            if (GameObjects[i] != null)
            {
                GameObjects[i].SetActive(GameObjectStates[i]);
            }
        }
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

    public void OnRouteAni(int ani, float time)
    {
        m_PlayableList.Clear();
        if (ShowLog) LogUtils.V("当前路径动作 ", ani);
        int length = Actions.Length;
        for (int i = 0; i < length; i++)
        {
            var action = Actions[i];
            int r = UnityEngine.Random.Range(1, 101);
            if (r > action.Rate)
            {
                continue;
            }
            if (!CheckCondition(action.Condition, action.CompareValue, ani))
            {
                continue;
            }
            m_CurrentAudioRate = action.AudioRate;
            AddPlayNode(action.Nodes, time);
            break;
        }
        m_Time = time;
    }

    bool CheckCondition(XRouteActionCondition condition, int val, int ani)
    {
        bool ret = false;
        switch (condition)
        {
            case XRouteActionCondition.Equal:
                ret = ani == val;
                break;

            case XRouteActionCondition.Large:
                ret = ani > val;
                break;

            case XRouteActionCondition.LargeEqual:
                ret = ani >= val;
                break;

            case XRouteActionCondition.NotEqual:
                ret = ani != val;
                break;

            case XRouteActionCondition.Less:
                ret = ani < val;
                break;

            case XRouteActionCondition.LessEqual:
                ret = ani <= val;
                break;
        }
        return ret;
    }

    void AddPlayNode(XRouteTimelineNode[] nodes, float time)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            var unit = nodes[i];
            if (unit.delayTime >= time)
            {
                //LogUtils.V(unit.effectType.ToString());
                m_PlayableList.Add(unit);
            }
            else if (unit.effectType == XRouteActionType.Animation
                || unit.effectType == XRouteActionType.DisableCollision
                || unit.effectType == XRouteActionType.Active 
                || unit.effectType == XRouteActionType.InActive)
            {
                PlayEffect(unit);
            }
        }
    }

    public void UpdateRouteAction(float dt)
    {
        m_Time += dt;
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

    private void PlayEffect(XRouteTimelineNode unit)
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

    private void PlayTween(XRouteTimelineNode unit)
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

    private void PlayShake(XRouteTimelineNode unit)
    {
        int id = 0;
        if (int.TryParse(unit.param, out id))
        {
            ShakeUtils.Shake(id);
        }
    }

    private void PlayAudio(XRouteTimelineNode unit)
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

    private void PlayAnimation(XRouteTimelineNode unit)
    {
        string triggerParam = "";
        if (unit.param.IndexOf(",") != -1)
        {
            var array = unit.param.Split(',');
            int r = UnityEngine.Random.Range(0, array.Length);
            triggerParam = array[r];
        }
        else
        {
            triggerParam = unit.param;
        }
        if (m_XFish != null)
        {
            m_XFish.SetTrigger(triggerParam);
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
