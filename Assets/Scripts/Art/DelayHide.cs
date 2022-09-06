using UnityEngine;
using System.Collections;

public class DelayHide : MonoBehaviour
{

    public float delayTime = 1.0f;

    private void OnEnable()
    {
        CancelInvoke("DelayFunc");
        Invoke("DelayFunc", delayTime);
    }

    void DelayFunc()
    {
        gameObject.SetActive(false);
    }

}
