using UnityEngine;
using System.Collections;

public class PositionShake : MonoBehaviour
{
    public float duration = 2.0f;
    public Vector3 Amplitude = new Vector3(0.5f, 0.55f, 0);
    public float Frequency = 30.0f;
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.5f, 1f, 1f, 1f), new Keyframe(1.0f, 0f, -0.6f, -0.6f));

    protected bool Shaking = false;
    protected float time;
    protected Vector3 _initialPosition;
    protected Vector3 _shakePosition;

    

    protected void Start()
    {
        _initialPosition = transform.localPosition;
    }

    protected void Update()
    {
        if (!Shaking)
        {
            transform.localPosition = _initialPosition;
            return;
        }
        else
        {
            float ratio = Mathf.Clamp01((Time.time - time) / duration);
            ratio = animationCurve.Evaluate(ratio);
            _shakePosition.x = Mathf.PerlinNoise(-(Time.time) * Frequency, Time.time * Frequency) * Amplitude.x - Amplitude.x / 2f;
            _shakePosition.y = Mathf.PerlinNoise(-(Time.time + 0.25f) * Frequency, Time.time * Frequency) * Amplitude.y - Amplitude.y / 2f;
            _shakePosition.z = Mathf.PerlinNoise(-(Time.time + 0.5f) * Frequency, Time.time * Frequency) * Amplitude.z - Amplitude.z / 2f;
            transform.localPosition = _initialPosition + _shakePosition * ratio;
        }
    }

    [ContextMenu("Test")]
    public void TestShake()
    {
        Play(false);
    }

    public void Play(bool removeWhenStopped)
    {
        Shaking = true;
        StartCoroutine(Shake(removeWhenStopped));
    }

    IEnumerator Shake(bool removeWhenStopped)
    {
        time = Time.time;
        Shaking = true;
        yield return new WaitForSeconds(duration);
        Shaking = false;
        if (removeWhenStopped)
        {
            transform.localPosition = _initialPosition;
            GameObject.Destroy(this);
        }
    }

}
