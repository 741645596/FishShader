using UnityEngine;

public class XShakeGameObject : MonoBehaviour
{
    public AnimationCurve Curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.13f, 0.4f), new Keyframe(0.33f, -0.33f), new Keyframe(0.5f, 0.17f), new Keyframe(0.71f, -0.12f), new Keyframe(1f, 0f));
    public Vector3 ShakeDirection = new Vector3(1f, 1f, 0f);
    public int LoopTime = 1;
    public float Duration = 1;

    private bool _isPlay;
    private float timeAdd;
    private Vector3 _curPos;
    private Vector3 _shakePos;
    private int _loopTime;

    private void Update()
    {
        if (_isPlay)
        {
            _curPos = transform.position - _shakePos;
            float eval = Curve.Evaluate(timeAdd / Duration);
            _shakePos = ShakeDirection * eval;
            transform.position = _shakePos + _curPos;
            timeAdd += Time.deltaTime;
            if (_loopTime >= 1 && timeAdd >= Duration)
            {
                _loopTime--;
                timeAdd = 0f;
                if (_loopTime <= 0)
                {
                    _isPlay = false;
                    enabled = false;
                }
            }
        }
    }

    [ContextMenu("Test Play")]
    public void Play()
    {
        _shakePos = Vector3.zero;
        _loopTime = LoopTime;
        _isPlay = true;
        timeAdd = 0f;
        enabled = true;
    }
}