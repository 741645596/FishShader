using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class XTimelineBridge : MonoBehaviour 
{
    static bool EnableLog = false;
    public static void SetEnableLog(bool bo)
    {
        EnableLog = bo;
    }

    public static void ReloadTimeline(GameObject go, string timelinePath)
    {
        var playableDirector = go.GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            return;
        }
        PlayableAsset asset = AssetsMgr.Load<PlayableAsset>(timelinePath);
        if (asset == null)
        {
            return;
        }
        playableDirector.playableAsset = asset;
    }

    public GameObject root;
    public bool flipRoute;
    public FishFlipType flipType; // 是否左右翻转

    Action<int> attackEventCallback;
    Action<string> customEventCallback;
    Action<AudioClip> audioClipCallback;

    Transform effectRoot;
    int m_AttackIndex;
    GameObject audioRoot;


    float mEvaluateTime;
    bool mSyncing; // 同步状态
    string mSyncAnimatorTriggerName;
    int mSortingOrder;
    string mLayer;

    PlayableDirector mPlayableDirector;
    PlayableDirector playableDirector
    {
        get
        {
            if (mPlayableDirector == null)
            {
                mPlayableDirector = GetComponent<PlayableDirector>();
            }
            return mPlayableDirector;
        }
    }

    private void OnDestroy()
    {
        if (audioRoot != null)
        {
            GameObject.Destroy(audioRoot);
            audioRoot = null;
        }
    }

    GameObject GetRoot()
    {
        if (root != null)
        {
            return root;
        }
        return gameObject;
    }

    [ContextMenu("TestSyncTime")]
    void TestSyncTime()
    {
        SyncTimeline(10.0f);
    }

    IEnumerator DoTestSyncTimeImpl(float time)
    {
        yield return StartCoroutine(PlaySync(time));
        float resetTime = GetTotalTime() - time;
        yield return new WaitForSeconds(resetTime);
        gameObject.SetActive(false);
    }



    [ContextMenu("Replay")]
    public void Replay()
    {
        gameObject.SetActive(true);
        m_AttackIndex = 0;
        playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Play();
        CancelInvoke("AutoHide");
        Invoke("AutoHide", GetTotalTime());
    }

    void AutoHide()
    {
        gameObject.SetActive(false);
    }

    #region 自己同步
    public void SyncTimeline(float time)
    {
        gameObject.SetActive(true);
        Replay();
        StartCoroutine(PlaySync(time));
    }

    public bool Evaluate(float dt)
    {
        mEvaluateTime = dt;
        playableDirector.time += dt;
        playableDirector.Evaluate();
        mEvaluateTime = 0;
        return playableDirector.time < playableDirector.duration;
    }

    public float GetEvaluateTime()
    {
        return mEvaluateTime;
    }

    // 按时间同步
    IEnumerator PlaySync(float syncTime)
    {
        playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        mSyncing = true;
        mSyncAnimatorTriggerName = "";
        float syncAnimatorTime = 0;
        playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Play();
        float dt = 0.1f;
        float time = 0;
        mEvaluateTime = dt;
        string lastTriggerName = "";
        while (time < syncTime)
        {
            playableDirector.Evaluate();
            playableDirector.time += dt;
            time += dt;
            if (mSyncAnimatorTriggerName != "")
            {
                if (mSyncAnimatorTriggerName != lastTriggerName)
                {
                    lastTriggerName = mSyncAnimatorTriggerName;
                    syncAnimatorTime = 0;
                }
                syncAnimatorTime += dt;
            }
        }
        mEvaluateTime = 0;
        mSyncing = false;
        yield return null;
        if (mSyncAnimatorTriggerName != "")
        {
            SetTrigger(mSyncAnimatorTriggerName, syncAnimatorTime);
        }
    }
    #endregion


    [ContextMenu("GetTotalTime")]
    public float GetTotalTime()
    {
        float ret = 0.01f;
        if (playableDirector != null)
        {
            ret = (float)playableDirector.duration;
        }
        if (EnableLog)
        {
            LogUtils.I($"XTimelineBridge GetTotalTime {ret}");
        }
        return ret;
    }

    #region 热更桥接接口

    public void SetEffectRoot(Transform r)
    {
        effectRoot = r;
    }

    public void SetEffectLayerAndSortingOrder(string layer, int sortingOrder)
    {
        mSortingOrder = sortingOrder;
        mLayer = layer;
    }

    public void SetAttackEventListener(Action<int> cb)
    {
        attackEventCallback = cb;
    }

    public void SetCustomEventListener(Action<string> cb)
    {
        customEventCallback = cb;
    }

    public void SetAudioListener(Action<AudioClip> cb)
    {
        audioClipCallback = cb;
    }
    #endregion


    #region timeline触发事件处理
    public void OnTriggerAttackEvent()
    {
        var player = GetRoot();
        if (EnableLog)
        {
            LogUtils.I($"{player.name} OnTriggerAttackEvent");
        }
        m_AttackIndex++;
        attackEventCallback?.Invoke(m_AttackIndex);
    }

    public void OnTriggerCustomEvent(string str)
    {
        var player = GetRoot();
        if (EnableLog)
        {
            LogUtils.I($"{player.name} OnTriggerCustomEvent {str}");
        }
        customEventCallback?.Invoke(str);
    }

    // timeline触发action
    public void OnTriggerAnimatorTrigger(string name)
    {
        if (EnableLog)
        {
            LogUtils.I($"OnTriggerAnimatorTrigger {name}");
        }
        if (mSyncing)
        {
            mSyncAnimatorTriggerName = name;
            return;
        }
        SetTrigger(name, 0);
    }

    void SetTrigger(string name, float time)
    {
        var player = GetRoot();
        var animators = player.GetComponentsInChildren<Animator>();
        if (animators.Length == 0)
        {
            LogUtils.W($"AnimatorBehaviour 找不到animator {player.name}");
            return;
        }
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetTrigger(name);
        }
        //if (time > 0)
        //{
        //    for (int i = 0; i < animators.Length; i++)
        //    {
        //        var animator = animators[i];
        //        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //        if (info.length > 0)
        //        {
        //            float percent = (time % info.length) / info.length;
        //            LogUtils.V($"{percent}  {info.length}");
        //            animator.Play(info.fullPathHash, -1, percent);
        //        }
        //    }
        //}
    }

    // timeline触发播放特效
    public void OnTriggerPlayEffect(GameObject prefab, bool worldPosition, Vector3 offset, float duration)
    {
        var player = GetRoot();
        if (EnableLog)
        {
            string name = prefab != null ? prefab.name : "";
            LogUtils.I($"{player.name} OnTriggerPlayEffect {name}");
        }
        var go = GameObject.Instantiate(prefab, effectRoot);
        if (mSortingOrder != 0 && !string.IsNullOrEmpty(mLayer))
        {
            GameObjectUtils.SetSortingOrderAndLayer(go, mLayer, mSortingOrder);
        }
        if (worldPosition)
        {
            float z = player.transform.position.z;
            offset.z += z;
            go.transform.position = offset;
        }
        else
        {
            var pos = player.transform.position;
            pos += offset;
            go.transform.position = pos;
        }
        //float duration = GameObjectUtils.CalcParticleSystemDuration(go);
        GameObject.Destroy(go, duration);
    }

    // timeline触发播放音效
    public void OnTriggerPlayAudio(AudioClip clip)
    {
        if (mSyncing)
        {
            return;
        }
        var player = GetRoot();
        if (EnableLog)
        {
            string name = clip.name;
            LogUtils.I($"{player.name} OnTriggerPlayAudio {name}");
        }
        if (audioClipCallback == null)
        {
            PlayAudio(clip);
        }
        else
        {
            audioClipCallback.Invoke(clip);
        }
    }

    // 播放音效接口
    void PlayAudio(AudioClip clip)
    {
        if (audioRoot == null)
        {
            audioRoot = new GameObject("XTimelineBridgeAudioRoot");
        }
        var array = audioRoot.GetComponents<AudioSource>();
        for (int i = 0; i < array.Length; i++)
        {
            if (!array[i].isPlaying)
            {
                array[i].clip = clip;
                array[i].Play();
                return;
            }
        }
        AudioSource audioSource = audioRoot.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }

    // timeline触发播放Tween动作
    public void OnTriggerTween(string tweenName, float duration, AnimationCurve curve)
    {
        var player = GetRoot();
        if (EnableLog)
        {
            LogUtils.I($"{player.name} OnTriggerTween {tweenName}");
        }
        var array = player.GetComponents<TweenerBase>();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].tweenName == tweenName)
            {
                array[i].duration = duration;
                array[i].animationCurve = curve;
                array[i].ResetToBeginningAndPlayForward();
                return;
            }
        }
    }

    // timeline更新路径动作
    public void OnRoutePosition(Vector2 routePos)
    {
        //LogUtils.I(routePos);
        var player = GetRoot();
        var tf = player.transform;
        var pos = tf.localPosition;
        pos.x = routePos.x;
        pos.y = routePos.y;
        if (flipRoute)
        {
            pos.x = -pos.x;
            pos.y = -pos.y;
        }
        switch (flipType)
        {
            case FishFlipType.Forward:
                {
                    tf.transform.up = pos - tf.localPosition;
                    break;
                }
            case FishFlipType.FlipX:
                {
                    var offsetX = pos.x - tf.localPosition.x;
                    var localScale = tf.localScale;
                    float x = Mathf.Abs(localScale.x);
                    if (offsetX > 0)
                    {
                        localScale.x = x;
                        tf.localScale = localScale;
                    }
                    else if (offsetX < 0)
                    {
                        localScale.x = -x;
                        tf.localScale = localScale;
                    }
                    break;
                }
        }
        tf.localPosition = pos;
    }


    // timeline触发播放shaderFade动作
    public void OnTriggerShaderFade(string shaderProperty, float duration, bool endRest)
    {
        var player = GetRoot();
        if (EnableLog)
        {
            LogUtils.I($"{player.name} OnTriggerShaderFade {shaderProperty}");
        }

        var com = player.GetComponent<TweenModelAlpha>();
        if (com == null)
        {
            com = player.AddComponent<TweenModelAlpha>();
        }
        com.duration = duration;
        com.from = 1;
        com.to = 0;
        com.Property = shaderProperty;
        com.ResetToBeginningAndPlayForward();
        if (endRest) com.SetOnFinished(() => {
            com.value = 1;
        });
    }
    #endregion
}
