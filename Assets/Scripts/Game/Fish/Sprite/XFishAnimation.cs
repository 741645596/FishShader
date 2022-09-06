#define USE_SELF_UPDATE
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(SpriteRenderer))]

public class XFishAnimation : MonoBehaviour
{
    public Sprite[] IdleSprites;
    public Sprite[] DeadSprites;
    public int FramePerSecond = 30;
    

    int OriginFramePerSecond;
    private float interval;
    private float mTime;
    private SpriteRenderer mActionSprite;
    private SpriteRenderer mShadow;
    private int mIndex;
    private Sprite[] mCurrentSprites;
    private float m_Speed;

    static int s_SortingOrder;

    public void Awake()
    {
        m_Speed = 1.0f;
        mActionSprite = FindSprite("ActionSprite");
        mShadow = FindSprite("Shadow");
        FramePerSecond = Math.Max(1, FramePerSecond);
        interval = 1.0f / FramePerSecond;
        PlayAnimation("Idle");
        OriginFramePerSecond = FramePerSecond;
    }

    public void Start()
    {
        var info = GetComponent<XFishInfo>();
        if (info != null)
        {
            Vector3 forward = CameraUtils.GetShadowForward();
            if (mShadow != null)
            {
                mShadow.transform.localPosition = forward * info.Distance * 3.0f;
            }
        }
    }

    public void Reset()
    {
        PlayAnimation("Idle");
        m_Speed = 1.0f;
        mTime = 0;
    }

    public void InitSortingOrder(int fishid)
    {
        fishid = (fishid % 1000) * 3;
        s_SortingOrder++;
        if (s_SortingOrder > 999)
        {
            s_SortingOrder = 0;
        }
        if (mActionSprite != null)
        {
            mActionSprite.sortingLayerName = "Fish2D";
            mActionSprite.sortingOrder = fishid * 1000 + s_SortingOrder;
        }
        if (mShadow != null)
        {
            mShadow.sortingLayerName = "Fish2D";
            mShadow.sortingOrder = (fishid - 1) * 1000 + s_SortingOrder;
        }
    }

    public void OnRouteNodeChange(bool isLeftToRight, float vel)
    {
        if (isLeftToRight)
        {
            var localScale = transform.localScale;
            float x = Mathf.Abs(localScale.x);
            localScale.x = x;
            transform.localScale = localScale;
        }
        else
        {
            var localScale = transform.localScale;
            float x = Mathf.Abs(localScale.x);
            localScale.x = -x;
            transform.localScale = localScale;
        }
    }

    public void SetAnimationSpeed(float speed)
    {
        m_Speed = speed;
    }

    public void PlayAnimation(string name, float time = 0)
    {
        //Debug.Log(name);
        if (name == "Dead")
        {
            mIndex = 0;
            mCurrentSprites = DeadSprites;
        }
        else
        {
            mIndex = 0;
            mCurrentSprites = IdleSprites;
        }
    }

    SpriteRenderer FindSprite(string name)
    {
        var go = transform.Find(name);
        if (go != null)
        {
            return go.GetComponent<SpriteRenderer>();
        }
        return null;
    }

    void UpdateAnimation(float dt)
    {
#if UNITY_EDITOR
        if (OriginFramePerSecond != FramePerSecond)
        {
            OriginFramePerSecond = FramePerSecond;
            interval = 1.0f / FramePerSecond;
        }
#endif
        mTime += Time.deltaTime * m_Speed;
        int count = Mathf.FloorToInt(mTime / interval);
        if (count > 0)
        {
            mTime -= count * interval;
            if (mCurrentSprites.Length > 0)
            {
                mIndex += count;
                mIndex %= mCurrentSprites.Length;
                var sprite = mCurrentSprites[mIndex];
                if (mActionSprite != null)
                {
                    mActionSprite.sprite = sprite;
                }
                if (mShadow != null)
                {
                    mShadow.sprite = sprite;
                }
            }
        }
    }

#if USE_SELF_UPDATE
    void Update()
    {
        UpdateAnimation(Time.deltaTime);
    }
#endif
}
