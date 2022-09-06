using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TweenerBase : MonoBehaviour
{
    //[Attribute]
    public enum Method
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        BounceIn,
        BounceOut
    }

    public enum Style
    {
        Once,
        Loop,
        PingPong
    }
    public string tweenName = "";
    public int TweenGroup = 0;
    public Method method;
    public Style style;
    public float delay;
    public float duration = 1f;
    public bool steeperCurves;
    public float timeScale = 1f;
    public bool ignoreTimeScale = true;
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));


    private bool mStarted;
    private float mStartTime;
    private float mDuration;
    private float mAmountPerDelta = 1000f;
    private float mFactor;
    private Action mOnFinished;

    public float amountPerDelta
    {
        get
        {
            if (duration == 0f)
            {
                return 1000f;
            }
            if (mDuration != duration)
            {
                mDuration = duration;
                mAmountPerDelta = Mathf.Abs(1f / duration) * Mathf.Sign(mAmountPerDelta);
            }
            return mAmountPerDelta;
        }
    }

    public float tweenFactor
    {
        get
        {
            return mFactor;
        }
        set
        {
            mFactor = Mathf.Clamp01(value);
        }
    }

    private void Start()
    {
        DoUpdate(0.0f);
    }

    protected void Update()
    {
        float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        DoUpdate(Time.deltaTime);
    }


    protected virtual void DoUpdate(float dt)
    {
        float num = dt;
        float num2 = Time.time;
        if (!mStarted)
        {
            num = 0f;
            mStarted = true;
            mStartTime = num2 + delay;
        }
        if (num2 < mStartTime)
        {
            return;
        }
        mFactor += ((duration == 0f) ? 1f : (amountPerDelta * num * timeScale));
        if (style == Style.Loop)
        {
            if (mFactor > 1f)
            {
                mFactor -= Mathf.Floor(mFactor);
            }
        }
        else if (style == Style.PingPong)
        {
            if (mFactor > 1f)
            {
                mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
                mAmountPerDelta = 0f - mAmountPerDelta;
            }
            else if (mFactor < 0f)
            {
                mFactor = 0f - mFactor;
                mFactor -= Mathf.Floor(mFactor);
                mAmountPerDelta = 0f - mAmountPerDelta;
            }
        }
        if (style == Style.Once && (duration == 0f || mFactor > 1f || mFactor < 0f))
        {
            mFactor = Mathf.Clamp01(mFactor);
            Sample(mFactor, isFinished: true);
            mOnFinished?.Invoke();
            OnFinish();
        }
        else
        {
            Sample(mFactor, isFinished: false);
        }
    }

    public void SetOnFinished(Action action)
    {
        mOnFinished = action;
    }

    protected abstract void OnUpdate(float factor, bool isFinished);

    public void Sample(float factor, bool isFinished)
    {
        float num = Mathf.Clamp01(factor);
        if (method == Method.EaseIn)
        {
            num = 1f - Mathf.Sin((float)Math.PI / 2f * (1f - num));
            if (steeperCurves)
            {
                num *= num;
            }
        }
        else if (method == Method.EaseOut)
        {
            num = Mathf.Sin((float)Math.PI / 2f * num);
            if (steeperCurves)
            {
                num = 1f - num;
                num = 1f - num * num;
            }
        }
        else if (method == Method.EaseInOut)
        {
            num -= Mathf.Sin(num * ((float)Math.PI * 2f)) / ((float)Math.PI * 2f);
            if (steeperCurves)
            {
                num = num * 2f - 1f;
                float num2 = Mathf.Sign(num);
                num = 1f - Mathf.Abs(num);
                num = 1f - num * num;
                num = num2 * num * 0.5f + 0.5f;
            }
        }
        else if (method == Method.BounceIn)
        {
            num = BounceLogic(num);
        }
        else if (method == Method.BounceOut)
        {
            num = 1f - BounceLogic(1f - num);
        }
        OnUpdate((animationCurve != null) ? animationCurve.Evaluate(num) : num, isFinished);
    }

    private float BounceLogic(float val)
    {
        val = ((val < 0.363636f) ? (7.5685f * val * val) : ((val < 0.727272f) ? (7.5625f * (val -= 0.545454f) * val + 0.75f) : ((!(val < 0.90909f)) ? (7.5625f * (val -= 0.9545454f) * val + 63f / 64f) : (7.5625f * (val -= 0.818181f) * val + 0.9375f))));
        return val;
    }

    protected virtual void OnFinish()
    {
        base.enabled = false;
        //Debug.Log("OnFinish");
    }

    public virtual void SetStartToCurrentValue()
    {

    }

    public virtual void SetEndToCurrentValue()
    {

    }

    public void ResetFactorToBeginning()
    {
        mStarted = false;
        mFactor = ((amountPerDelta < 0f) ? 1f : 0f);
    }

    public void ResetToBeginning()
    {
        mStarted = false;
        mFactor = ((amountPerDelta < 0f) ? 1f : 0f);
        Sample(mFactor, isFinished: false);
    }

    [ContextMenu("Play")]
    public void ResetToBeginningAndPlayForward()
    {
        mStarted = false;
        mFactor = ((amountPerDelta < 0f) ? 1f : 0f);
        Sample(mFactor, isFinished: false);
        PlayForward();
    }

    [ContextMenu("Dump Curve")]
    public void DumpCurve()
    {
        for (int i = 0; i < animationCurve.keys.Length; i++)
        {
            Keyframe frame = animationCurve.keys[i];
            Debug.Log($"{frame.time},{frame.value},{frame.inTangent},{frame.outTangent},{frame.inWeight},{frame.outWeight} {frame.tangentMode} {frame.weightedMode}");
        }
    }

    public void PlayForward()
    {
        Play(forward: true);
    }

    public void PlayReverse()
    {
        Play(forward: false);
    }

    public virtual void Play(bool forward)
    {
        mAmountPerDelta = Mathf.Abs(amountPerDelta);
        if (!forward)
        {
            mAmountPerDelta = 0f - mAmountPerDelta;
        }
        if (!base.enabled)
        {
            base.enabled = true;
            mStarted = false;
        }
        DoUpdate(0.01f);
    }

    public void Stop()
    {
        enabled = false;
    }

    public static T Begin<T>(GameObject go, float duration, float delay = 0f) where T : TweenerBase
    {
        T val = go.GetComponent<T>();
        if ((UnityEngine.Object)val != (UnityEngine.Object)null && val.TweenGroup != 0)
        {
            val = null;
            T[] components = go.GetComponents<T>();
            int i = 0;
            for (int num = components.Length; i < num; i++)
            {
                val = components[i];
                if ((UnityEngine.Object)val != (UnityEngine.Object)null && val.TweenGroup == 0)
                {
                    break;
                }
                val = null;
            }
        }
        if ((UnityEngine.Object)val == (UnityEngine.Object)null)
        {
            val = go.AddComponent<T>();
            if ((UnityEngine.Object)val == (UnityEngine.Object)null)
            {
                UnityEngine.Debug.LogError("Unable to add " + typeof(T) + " to ", go);
                return null;
            }
        }
        val.mStarted = false;
        val.mFactor = 0f;
        val.duration = duration;
        val.mDuration = duration;
        val.delay = delay;
        val.mAmountPerDelta = ((duration > 0f) ? Mathf.Abs(1f / duration) : 1000f);
        val.style = Style.Once;
        val.animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
        val.enabled = true;
        return val;
    }

    public static void PlayAllTween(GameObject go)
    {
        var array = go.GetComponents<TweenerBase>();
        for (int i = 0; i < array.Length; i++)
        {
            array[i].ResetToBeginningAndPlayForward();
        }
    }

    public static void StopAllTween(GameObject go)
    {
        var array = go.GetComponents<TweenerBase>();
        for (int i = 0; i < array.Length; i++)
        {
            array[i].Stop();
        }
    }
}
