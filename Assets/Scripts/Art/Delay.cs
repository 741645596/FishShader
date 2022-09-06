using UnityEngine;
using System.Collections;

public class Delay : MonoBehaviour
{

    public float delayTime = 1.0f;
    private bool isDelayCallBack = false;

    private void OnEnable()
    {
        if (!isDelayCallBack)
        {
            isDelayCallBack = true;
            gameObject.SetActive(false);
            Invoke("DelayFunc", delayTime);
        }
    }

    void DelayFunc()
    {
        gameObject.SetActive(true);
        isDelayCallBack = false;
    }

}
