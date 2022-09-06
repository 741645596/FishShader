using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(XFishInfo))]
public class XAniSpeed2VelocityTest : MonoBehaviour
{
    [Range(0.1f, 3.0f)]
    public float Velocity = 1.0f;

    XFishInfo info;

    private void Start()
    {
        info = GetComponent<XFishInfo>();
    }

    private void Update()
    {
        float halfWidth = CameraUtils.GetWorldSize().x / 2;
        Vector3 pos = transform.position;
        if (transform.position.x > halfWidth)
        {
            pos.x = -halfWidth;
            transform.position = pos;
        }
        pos.x += Velocity * Time.deltaTime;
        transform.position = pos;
        var com = GetComponent<XFish>();
        if (com != null)
        {
            com.SetVelocity(Velocity);
        }
    }
}
