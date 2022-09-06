using System;
using UnityEngine;

[AddComponentMenu("Tween/Tween Relative Rotation")]
public class TweenRelativeRotation : TweenerBase
{
    public float zAngle;

    private Transform mTrans;
    private float zLastAngle;

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


    protected override void OnUpdate(float factor, bool isFinished)
    {
        if (factor < 0.01f)
        {
            zLastAngle = 0;
        }
        var newAngle = Mathf.Lerp(0, zAngle, factor);
        
        float delta = newAngle - zLastAngle;
        
        var localEulerAngles = cachedTransform.localEulerAngles;
        //Debug.Log($"{newAngle} {delta} {localEulerAngles.z}");
        localEulerAngles.z += delta;
        cachedTransform.localEulerAngles = localEulerAngles;

        zLastAngle = newAngle;
    }
}
