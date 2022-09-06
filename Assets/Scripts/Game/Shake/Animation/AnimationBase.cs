using ShakeLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShakeLibrary
{
    public abstract class AnimationBase
    {
        public float Duration = 1f;

        public float Delay;

        private bool _playAnimation;

        private bool mStarted;

        private float mFactor;

        private float mDuration;

        private float mStartTime;

        private float mAmountPerDelta = 1000f;

        public float amountPerDelta
        {
            get
            {
                if (Duration == 0f)
                {
                    return 1000f;
                }
                if (mDuration != Duration)
                {
                    mDuration = Duration;
                    mAmountPerDelta = Mathf.Abs(1f / Duration) * Mathf.Sign(mAmountPerDelta);
                }
                return mAmountPerDelta;
            }
        }

        protected abstract void OnUpdate(float factor, bool isFinished);

        public AnimationBase()
        {

        }

        public bool IsPlaying()
        {
            return _playAnimation;
        }

        public void Update(float dt)
        {
            DoUpdate(dt);
        }

        protected void DoUpdate(float dt)
        {
            float num = dt;
            float num2 = Time.time;
            if (!mStarted)
            {
                num = 0f;
                mStarted = true;
                mStartTime = num2 + Delay;
            }
            if (num2 < mStartTime)
            {
                return;
            }
            mFactor += ((Duration == 0f) ? 1f : (amountPerDelta * num));
            if (Duration == 0f || mFactor > 1f || mFactor < 0f)
            {
                mFactor = Mathf.Clamp01(mFactor);
                Sample(mFactor, isFinished: true);
                _playAnimation = false;
            }
            else
            {
                Sample(mFactor, isFinished: false);
            }
        }

        public void Sample(float factor, bool isFinished)
        {
            float num = Mathf.Clamp01(factor);
            OnUpdate(num, isFinished);
        }

        public void ResetToBeginning()
        {
            mFactor = 0f;
            _playAnimation = true;
            mStarted = false;
        }
    }
}
