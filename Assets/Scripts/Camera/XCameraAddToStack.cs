using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class XCameraAddToStack : MonoBehaviour
{
    public XCameraDepth Depth = XCameraDepth.MainCamera;
    Camera m_Camera;

    static int Index = 0;
    const float Offset = 150.0f;

    private void Start()
    {
        var camera = CameraUtils.GetMainCamera();
        if (camera != null)
        {
            var data = camera.GetUniversalAdditionalCameraData();
            m_Camera = GetComponentInChildren<Camera>();
            if (m_Camera != null)
            {
                m_Camera.farClipPlane = 100;
                Index++;
                if (Index > 100)
                {
                    Index = 1;
                }
                float offset = Index * Offset + 1000;
                transform.localPosition = new Vector3(offset, offset, 0);
                string name = Depth.ToString();
                var cameraData = m_Camera.GetUniversalAdditionalCameraData();
                cameraData.renderType = CameraRenderType.Overlay;
                var list = data.cameraStack;
                int idx = 0;
                if (Depth != XCameraDepth.MainCamera)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].name == name)
                        {
                            idx = i + 1;
                            break;
                        }
                    }
                }
                list.Insert(idx, m_Camera);
            }
        }
    }

    private void OnDestroy()
    {
        var camera = CameraUtils.GetMainCamera();
        if (camera != null && m_Camera != null)
        {
            var data = camera.GetUniversalAdditionalCameraData();
            data.cameraStack.Remove(m_Camera);
        }
        m_Camera = null;
    }
}
