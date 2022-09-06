using System;
using System.Collections.Generic;
using UnityEngine;

public class XMirrorFlipCamera : MonoBehaviour
{

#if UNITY_EDITOR
    bool m_Flip;
    [ContextMenu("Flip")]
    void TestFlip()
    {
        m_Flip = !m_Flip;
        FlipCamera(m_Flip);
    }
#endif

    public void FlipCamera(bool flip)
    {
        var camera = GetComponentInChildren<Camera>();
        if (camera != null)
        {
            Vector3 scale = new Vector3(flip ? -1 : 1, 1, 1);
            camera.projectionMatrix = camera.projectionMatrix * Matrix4x4.Scale(scale);
        }
    }
}
