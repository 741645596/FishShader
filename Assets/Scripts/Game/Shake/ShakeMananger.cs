using System.Collections.Generic;
using UnityEngine;

namespace ShakeLibrary
{
	public class ShakeMananger : MonoBehaviour
	{
        static ShakeMananger sInstance;
        public static ShakeMananger Instance { get { return sInstance; } }

        public List<int> AnimationIndex = new List<int>();

		public List<AnimationShake> AnimationClip = new List<AnimationShake>();

        public Transform ShakeTarget;

#if UNITY_EDITOR
        public int TestId;

		public bool TestPlay;

		public bool TestStop;
        public int AnimationCount;
#endif

        private AnimationShake _curAnimation;

        private void Awake()
        {
            sInstance = this;
        }

		public void PlayAnimation(int id)
		{
            if (ShakeTarget == null)
            {
                LogUtils.W("Not Find Shake Target ");
                return;
            }
			int num = AnimationIndex.IndexOf(id);
			if (num < 0)
			{
				LogUtils.W("Not Find Shake Index " + id);
				return;
			}
			StopAnimation();
			_curAnimation = AnimationClip[num];
            _curAnimation.Target = ShakeTarget;
			_curAnimation.ResetToBeginning();
        }

		public void StopAnimation()
		{
			if (_curAnimation != null)
			{
				_curAnimation.Stop();
				_curAnimation = null;
			}
		}

		private void Update()
		{
#if UNITY_EDITOR
            if (TestPlay)
			{
				PlayAnimation(TestId);
				TestPlay = false;
			}
			if (TestStop)
			{
				StopAnimation();
				TestStop = false;
			}
            AnimationCount = _curAnimation != null ? _curAnimation.GetPlayingCount() : 0;
#endif
            if (_curAnimation != null)
            {
                if (_curAnimation.GetPlayingCount() > 0)
                {
                    _curAnimation.UpdateAnimation(Time.deltaTime);
                }
                else
                {
                    StopAnimation();
                }
            }
        }
	}
}
