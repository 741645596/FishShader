using UnityEngine;
using System.Collections;

public class DelayHideParticle : MonoBehaviour
{

    private void Start()
    {
        var psArr = gameObject.GetComponentsInChildren<ParticleSystem>();
        float delayTime = 0;
        for(var i = 0; i < psArr.Length; i++)
        {
            var obj = psArr[i];
            delayTime = obj.main.duration > delayTime ? obj.main.duration : delayTime;
        }

        Invoke("DelayFunc", delayTime+0.1f);
    }

    void DelayFunc()
    {
        gameObject.SetActive(false);
    }
}
