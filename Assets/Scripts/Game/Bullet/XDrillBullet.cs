using System;
using System.Collections.Generic;
using UnityEngine;

public class XDrillBullet : MonoBehaviour
{
    public bool worldSpace; // true 为世界坐标下的位置，false为ui坐标下位置
    protected Vector2 m_Vec = Vector2.zero;
    protected Action m_HitListener;
    protected Action m_ReAngle;
    protected List<int> m_HitFishes = new List<int>();
    float m_CollisionTimer;
    const float m_CollisionInterval = 1.0f / 20.0f;
    int m_ViewID;
    bool m_enableHit;
    float m_speed;
    float m_range;
    Rect m_rect;
    public bool changeDirection = true;
    float m_BulletRadius;

    protected virtual void Awake()
    {
        m_range = 1.5f;
        m_BulletRadius = 0.15f;
    }

    private void OnDestroy()
    {
        m_HitListener = null;
    }

    public void SetBulletRadius(float r)
    {
        m_BulletRadius = r;
    }

    public void Launch(float angle, Vector3 startPos, float speed)
    {
        transform.localPosition = startPos;
        float radian = angle * Mathf.Deg2Rad;
        Vector2 vec = new Vector2(-Mathf.Sin(radian), Mathf.Cos(radian));
        m_Vec = vec;
        m_speed = speed;
        RecalcAngle();
        
        InitRect();
    }

    public void SetViewID(int id)
    {
        m_ViewID = id;
    }

    public int GetViewID()
    {
        return m_ViewID;
    }

    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }

    public void SetRang(float range)
    {
        m_range = range;
    }
    public void SetHitListener(Action listener)
    {
        m_HitListener = listener;
    }
    public void SetReAngle(Action listener)
    {
        m_ReAngle = listener;
    }

    public List<int> GetHitFishes()
    {
        return m_HitFishes;
    }

    public void SetEnabelHit(bool bo)
    {
        m_enableHit = bo;
    }

    private void Update()
    {
        if (m_enableHit)
        {
            if (m_CollisionTimer > m_CollisionInterval)
            {
                m_CollisionTimer = 0;

                var worldPos = GetWorldPosition();
                if (XBulletUtils.BulletAndFish(worldPos, m_BulletRadius))
                {
                    XBulletUtils.GetCollisionFish(GetHitFishes(), worldPos, m_range);
                    OnHit();
                    m_HitFishes.Clear();
                }
            }
            else
            {
                m_CollisionTimer += Time.deltaTime;
            }
        }

        FlyUpdate(Time.deltaTime);
    }

    // 普通子弹
    void FlyUpdate(float dt)
    {
        CheckEdge();
        var pos = transform.localPosition;
        pos.x += m_Vec.x * dt * m_speed;
        pos.y += m_Vec.y * dt * m_speed;
        transform.localPosition = pos;
    }

    // 边界检测
    void CheckEdge()
    {
        bool ret = false;
        Rect rect = m_rect;
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
            m_ReAngle?.Invoke();
        }
    }
    
    // 重新计算朝向
    void RecalcAngle() {
        if (!changeDirection)
            return;
        
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
        m_HitListener?.Invoke();
    }

    private void InitRect()
    {    
        var rect = worldSpace? XBulletUtils.GetWorldRect() : XBulletUtils.GetUIRect();
        var dis = worldSpace ? 0.25f : 25;
        rect.x += -dis;
        rect.y += -dis;
        rect.width += dis * 2;
        rect.height += dis * 2;

        m_rect = rect;
    }

}
