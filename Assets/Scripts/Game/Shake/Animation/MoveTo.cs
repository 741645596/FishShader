using UnityEngine;

namespace ShakeLibrary
{
	public class MoveTo : AnimationBase
	{
		public Vector3 From;

		public Vector3 To;

		public Transform cachedTransform;

		public bool WorldSpace;

		private int _playIndex;

		public Vector3 Value
		{
			get
			{
				if (!WorldSpace)
				{
					return cachedTransform.localPosition;
				}
				return cachedTransform.position;
			}
			set
			{
                if (WorldSpace)
                {
                    cachedTransform.position = value;
                }
                else
                {
                    cachedTransform.localPosition = value;
                }
            }
		}

		public MoveTo(Transform target, Vector3 from, Vector3 to, float duration, float delay)
		{
			cachedTransform = target;
			From = from;
			To = to;
			if (duration == 0f)
			{
				duration = 1f;
			}
			Duration = duration;
			Delay = delay;
		}

		protected override void OnUpdate(float factor, bool isFinished)
		{
			Value = From * (1f - factor) + To * factor;
		}
	}
}
