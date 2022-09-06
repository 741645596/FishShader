using UnityEngine;
using System.Collections;

public class ZoomShake : MonoBehaviour
{
    public float duration = 2.0f;
    public bool Shaking = false;
    public float minValue = 1.0f;

    protected float time;
    protected Vector3 _initialScale;

    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.5f, 1f, 1f, 1f), new Keyframe(1.0f, 1f, -0.6f, -0.6f));


    protected void Start()
    {
        _initialScale = Vector3.one;
    }

    protected void Update()
    {
        if (!Shaking)
        {
            transform.localScale = _initialScale;
            return;
        }
        else
        {
            float ratio = Mathf.Clamp01((Time.time - time) / duration);
            ratio = animationCurve.Evaluate(ratio) + 1.0f;
            transform.localScale = _initialScale * ratio;
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
            transform.localScale = _initialScale;
            GameObject.Destroy(this);
        }
    }

}
