using System;
using UnityEngine;

public class XCameraRaycast: MonoBehaviour
{
    Camera m_Camera;
    
    private void Start()
    {
        m_Camera = GetComponentInChildren<Camera>();
        if (m_Camera != null)
        {
            XBulletUtils.AddCameraRaycast(this);
        }
    }

    private void OnDestroy()
    {
        if (m_Camera != null)
        {
            XBulletUtils.RemoveCameraRaycast(this);
        }
    }

    /*
    RaycastHit[] m_HitInfos = new RaycastHit[48];
    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            LogUtils.V(Input.mousePosition);
            GetColliderResult(Input.mousePosition, 0.25f, m_HitInfos);
        }
    }
    */

    public int GetColliderResult(Vector2 screenPos, float raduis, RaycastHit[] infos)
    {
        Ray ray = m_Camera.ScreenPointToRay(screenPos);
        int count = Physics.SphereCastNonAlloc(ray, raduis, infos);
        return count;
    }

    public Vector3 GetWorldPoint(Vector3 pos)
    {
        if (m_Camera != null)
        {
            Vector2 screenPos = m_Camera.WorldToScreenPoint(pos);
            return CameraUtils.ScreenPointToWorldPoint(screenPos);
        }
        return Vector3.zero;
    }

}