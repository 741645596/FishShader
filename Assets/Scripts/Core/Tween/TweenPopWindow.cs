using System;
using UnityEngine;

[AddComponentMenu("Tween/Tween Pop Window")]
public class TweenPopWindow : MonoBehaviour
{
    int TweenGroup = 0;
    float delay;
    float duration = 1f;
    bool steeperCurves;
    float timeScale = 1f;
    bool ignoreTimeScale = true;
    AnimationCurve animationCurve;

    private bool mStarted;
    private float mStartTime;
    private float mDuration;
    private float mAmountPerDelta = 1000f;
    private float mFactor;
    private Action mOnFinished;

    Vector3 from = Vector3.one;
    Vector3 to = Vector3.one;
    private Transform mTrans;

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


    void Update()
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
        if ((duration == 0f || mFactor > 1f || mFactor < 0f))
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


    public void Sample(float factor, bool isFinished)
    {
        float num = Mathf.Clamp01(factor);
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

    private void Start()
    {
        delay = 0;
        duration = 0.3f;
        from = new Vector3(0.5f, 0.5f, 1);
        to = new Vector3(1, 1, 1);
        var keyFrame1 = new Keyframe(0, 0, 1, 1, 0, 0.3333333f);
        keyFrame1.tangentMode = 34;
        keyFrame1.weightedMode = WeightedMode.None;
        var keyFrame2 = new Keyframe(1, 1, -0.9973046f, -0.9973046f, 0.4375f, 0);
        keyFrame2.tangentMode = 0;
        keyFrame2.weightedMode = WeightedMode.None;
        animationCurve = new AnimationCurve(keyFrame1, keyFrame2);
        DoUpdate(0.0f);
    }

    

    public Transform cachedTransform
    {
        get
        {
            if (mTrans == null)
            {
                mTrans = base.transform;
            }
            return mTrans;
        }
    }

    public Vector3 value
    {
        get
        {
            return cachedTransform.localScale;
        }
        set
        {
            cachedTransform.localScale = value;
        }
    }

    void OnUpdate(float factor, bool isFinished)
    {
        value = from * (1f - factor) + to * factor;
    }
}
