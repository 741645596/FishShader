using System;
using System.Collections.Generic;
using UnityEngine;

public class XBullet : MonoBehaviour
{
    public bool worldSpace; // true 为世界坐标下的位置，false为ui坐标下位置

    protected Vector2 m_Vec = Vector2.zero;
    protected Action<int> m_HitListener;
    protected List<int> m_HitFishes = new List<int>();

    int m_ViewID;
    int m_ConfigID;
    int m_UID;
    float m_Speed;
    float m_FollowDistance; // 跟踪距离
    int m_FollowFishID;

    float m_BulletRadius;
    float m_NetRadius;

    protected virtual void Awake()
    {
        m_BulletRadius = 0.25f;
        m_NetRadius = 1.0f;
    }

    private void OnDestroy()
    {
        m_HitListener = null;
    }

    public void SetBulletRadius(float r)
    {
        m_BulletRadius = r;
    }

    public float GetBulletRadius()
    {
        return m_BulletRadius;
    }

    public void SetNetRadius(float r)
    {
        m_NetRadius = r;
    }

    public float GetNetRadius()
    {
        return m_NetRadius;
    }

    public void Launch(float angle, Vector3 startPos, float speed)
    {
        transform.localPosition = startPos;
        float radian = angle * Mathf.Deg2Rad;
        Vector2 vec = new Vector2(-Mathf.Sin(radian), Mathf.Cos(radian));
        m_Vec = vec * speed;
        RecalcAngle();
        m_Speed = speed;
        m_FollowDistance = -1;
    }

    public void SetViewID(int id)
    {
        m_ViewID = id;
    }

    public int GetViewID()
    {
        return m_ViewID;
    }

    public void SetUID(int id)
    {
        m_UID = id;
    }

    public int GetUID()
    {
        return m_UID;
    }

    public void SetConfigID(int id)
    {
        m_ConfigID = id;
    }

    public int GetConfigID()
    {
        return m_ConfigID;
    }

    public void SetFollowFishID(int id)
    {
        m_FollowFishID = id;
    }

    public void SetHitListener(Action<int> listener)
    {
        m_HitListener = listener;
    }

    public List<int> GetHitFishes()
    {
        return m_HitFishes;
    }

    public void UpdateBullet(float dt)
    {
        if (m_FollowFishID > 0) // 锁定子弹
        {
            ChaseUpdate(dt);
        }
        else
        {
            FlyUpdate(dt);
        }
    }

    // 跟踪子弹
    void ChaseUpdate(float dt)
    {
        XFish fish = XFishManager.Instance.FindFish(m_FollowFishID);
        if (fish == null)
        {
            RemoveFollowBullet();
            return;
        }
        if (!fish.IsInScreen())
        {
            RemoveFollowBullet();
            return;
        }
        Vector2 dst = fish.GetLockPoint();
        Vector2 src = transform.position;
        Vector2 offset = dst - src;
        float magnitude = offset.magnitude;
        if (magnitude < 0.25f || (m_FollowDistance > 0 && magnitude > m_FollowDistance))
        {
            m_HitFishes.Add(m_FollowFishID);
            RemoveFollowBullet();
            return;
        }
        m_FollowDistance = magnitude;
        offset.Normalize();
        var pos = transform.localPosition;
        float s = dt * m_Speed;
        pos.x += offset.x * s;
        pos.y += offset.y * s;
        transform.localPosition = pos;

        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg - 90;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    void RemoveFollowBullet()
    {
        XBulletManager.Instance.RemoveBullet(this);
    }

    // 普通子弹
    void FlyUpdate(float dt)
    {
        CheckEdge();
        var pos = transform.localPosition;
        pos.x += m_Vec.x * dt;
        pos.y += m_Vec.y * dt;
        transform.localPosition = pos;
    }

    // 边界检测
    void CheckEdge()
    {
        bool ret = false;
        Rect rect = worldSpace ? XBulletUtils.GetWorldRect() : XBulletUtils.GetUIRect();
        var pos = transform.localPosition;
        if (pos.x < rect.xMin)
        {
            //pos.y = pos.y - (pos.x - rect.xMin) / m_Vec.x * m_Vec.y;
            pos.x = rect.xMin - (pos.x - rect.xMin);
            m_Vec.x = -m_Vec.x;
            ret = true;
        }
        else if (pos.x > rect.xMax)
        {
            //pos.y = pos.y - (pos.x - rect.xMax) / m_Vec.x * m_Vec.y;
            pos.x = rect.xMax - (pos.x - rect.xMax);
            m_Vec.x = -m_Vec.x;
            ret = true;
        }
        else if (pos.y < rect.yMin)
        {
            //pos.x = pos.x - (pos.y - rect.yMin) / m_Vec.y * m_Vec.x;
            pos.y = rect.yMin - (pos.y - rect.yMin);
            m_Vec.y = -m_Vec.y;
            ret = true;
        }
        else if (pos.y > rect.yMax)
        {
            //pos.x = pos.x - (pos.y - rect.yMax) / m_Vec.y * m_Vec.x;
            pos.y = rect.yMax - (pos.y - rect.yMax);
            m_Vec.y = -m_Vec.y;
            ret = true;
        }
        transform.localPosition = pos;
        if (ret)
        {
            RecalcAngle();
        }
    }

    // 重新计算朝向
    void RecalcAngle()
    {
        float angle = Mathf.Atan2(m_Vec.y, m_Vec.x) * Mathf.Rad2Deg - 90;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public Vector3 GetWorldPosition()
    {
        if (worldSpace)
        {
            return transform.position;
        }
        else
        {
            Vector3 pos = transform.position;
            return pos;
        }
    }

    public void OnHit()
    {
        m_HitListener?.Invoke(m_UID);
    }

    public void Reset()
    {
        m_HitFishes.Clear();
        m_FollowFishID = 0;
        m_FollowDistance = -1;
    }

    public bool NeedCollision()
    {
        return m_FollowFishID <= 0;
    }
}
